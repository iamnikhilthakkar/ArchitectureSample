using Architecture.Core.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Architecture.Repositories.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly IConfiguration configuration;
        private static IDbConnection DbContext;
        public HomeRepository(IConfiguration _configuration)
        {
            configuration = _configuration;
            DbContext = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        #region '-- Action --'
        public BaseResponse<SearchUserTables> GetTablesByUserId(string userId)
        {
            BaseResponse<SearchUserTables> response = new BaseResponse<SearchUserTables>();
            DynamicParameters parameters = new DynamicParameters();

            try
            {
                parameters.Add("@EmailId", userId, DbType.String);
                response.DataList = DbContext.Query<SearchUserTables>("GetTablesByUserId", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Exception = ex;
                response.Message = ex.Message;
            }
            return response;
        }

        public BaseResponse<Tuple<DataTable, int>> GetTableData(string userEmailId, string tableName)
        {
            BaseResponse<Tuple<DataTable, int>> response = new BaseResponse<Tuple<DataTable, int>>();
            IEnumerable<dynamic> list = new List<dynamic>();
            string condition = "";
            string query = "";
            string colNames = "";
            int count = 0;

            condition = "TOP 100";
            var columnNames = GetColumnName(tableName, true);
            colNames = GetColumnsAsString(columnNames);

            query = "SELECT " + condition + " " + colNames + " FROM " + tableName;
            var countQuery = "SELECT COUNT(*) FROM " + tableName;

            try
            {
                list = DbContext.Query<dynamic>(query);
                count = DbContext.Query<int>(countQuery).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Exception = ex;
                response.Message = ex.Message;
            }

            response.Data = new Tuple<DataTable, int>(ToDataTable(list, columnNames), count);
            return response;
        }

        public BaseResponse<Tuple<DataTable, int>> InsertFile(string userEmailId, string tableName, DataTable data)
        {

            #region 'Parameter Declaration'
            int count = 0;
            string colNames = "";
            IEnumerable<dynamic> list = new List<dynamic>();
            BaseResponse bulkInsertResponse = new BaseResponse();
            DynamicParameters parameters = new DynamicParameters();
            BaseResponse<Tuple<DataTable, int>> response = new BaseResponse<Tuple<DataTable, int>>();
            #endregion

            try
            {
                #region 'Get Available Columns of the table'
                var columnDetails = GetColumnName(tableName);
                var columnNames = columnDetails;
                string columnNameString = "";
                columnNameString = GetColumnsAsString(columnNames);
                #endregion

                #region 'Add Where clause according to the company code and email id fields'
                string whereCondition = " Where 1 = 1";

                #endregion

                #region 'Delete existing and Insert new Data in database'
                bulkInsertResponse = InsertInBulk(tableName, columnNameString, data, userEmailId);

                if (!bulkInsertResponse.Status)
                {
                    response.Status = false;
                    response.Message = bulkInsertResponse.Message;
                    response.Exception = bulkInsertResponse.Exception;
                }
                #endregion

                #region 'Fetch Data After Insertion to display in table'

                columnNames = GetColumnName(tableName, true);
                colNames = GetColumnsAsString(columnNames);

                var query = "SELECT TOP 100 " + colNames + " FROM " + tableName + whereCondition;
                var countQuery = "SELECT COUNT(*) FROM " + tableName + whereCondition;
                list = DbContext.Query<dynamic>(query);
                count = DbContext.Query<int>(countQuery).FirstOrDefault();
                response.Data = new Tuple<DataTable, int>(ToDataTable(list, columnNames), count);
                #endregion

            }
            catch (Exception e)
            {
                response.Status = false;
                response.Message = e.Message;
                response.Exception = e;
            }
            return response;
        }

        #endregion



        #region '-- Private Methods --'

        private BaseResponse InsertInBulk(string tableName, string columnNameString, DataTable data, string userEmailId)
        {
            return BulkInsert(tableName, data, columnNameString, userEmailId);
        }

        private BaseResponse BulkInsert(string tableName, DataTable datatable, string columnNameString, string userEmailId, int batchSize = 5000, int bulkCopyTimeout = 90)
        {
            BaseResponse response = new BaseResponse();
            try
            {
                using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy.batchsize?view=dotnet-plat-ext-3.1
                    connection.Open();
                    //Truncate all the data in the table.
                    //var deleteAllQuery = "truncate table " + tableName;
                    var deleteAllQuery = "DELETE FROM " + tableName;

                    var deleteExistingRecords = DbContext.Execute(deleteAllQuery);
                    var trans = connection.BeginTransaction("BulkInsertTransaction");
                    var tempToBeInserted = $"#TempInsert_{tableName}".Replace(".", string.Empty);
                    var numberOfBatches = (int)Math.Ceiling((double)datatable.Rows.Count / batchSize);


                    for (int i = 0; i < numberOfBatches; i++)
                    {
                        connection.Execute($@"SELECT TOP 0 {columnNameString} INTO {tempToBeInserted} FROM {FormatTableName(tableName)} target WITH(NOLOCK);", null, trans);

                        // Quick load with bulk insert to then merge fast
                        using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, trans))
                        {
                            bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                            bulkCopy.BatchSize = batchSize;
                            bulkCopy.DestinationTableName = tempToBeInserted;
                            var batchDatatable = datatable.AsEnumerable().Skip(i * batchSize).Take(batchSize).CopyToDataTable();
                            bulkCopy.WriteToServer(batchDatatable.CreateDataReader());
                        }
                        connection.Execute($@"
                            INSERT INTO {FormatTableName(tableName)}({columnNameString}) 
                            SELECT {columnNameString} FROM {tempToBeInserted}
                            DROP TABLE {tempToBeInserted};", null, trans);
                    }
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Exception = ex;
                response.Status = false;
            }
            return response;
        }
        private static string FormatTableName(string table)
        {
            if (string.IsNullOrEmpty(table))
            {
                return table;
            }

            var parts = table.Split('.');

            if (parts.Length == 1)
            {
                return $"[{table}]";
            }

            return $"[{parts[0]}].[{parts[1]}]";
        }

        private List<ColumnDetails> GetColumnName(string tableName, bool IsIDRequired = false)
        {

            IEnumerable<ColumnDetails> datalist = new List<ColumnDetails>();
            try
            {
                //Pass parameters to Stored Procedure.
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@table_name", tableName, DbType.String);
                datalist = DbContext.Query<ColumnDetails>("sys.sp_columns", parameters, commandType: CommandType.StoredProcedure);
                if (!IsIDRequired)
                {
                    datalist = datalist.Except(datalist.Where(w => w.TYPE_NAME.Contains("identity")).ToList());
                }
            }
            catch (Exception ex)
            {

            }

            return datalist.ToList();
        }

        private string GetColumnsAsString(List<ColumnDetails> columnNames)
        {
            string colNames = "";
            if (columnNames.Count() > 1)
            {
                colNames = "[" + string.Join("],[", columnNames.Select(s => s.COLUMN_NAME)) + "]";
            }
            else
            {
                colNames = "[" + string.Join(",", columnNames.Select(s => s.COLUMN_NAME)) + "]";
            }

            return colNames;
        }

        private DataTable ToDataTable(IEnumerable<dynamic> items, List<ColumnDetails> columnNames)
        {
            var dt = new DataTable();
            if (items == null)
                return null;
            var data = items.ToArray();
            if (data.Length == 0)
            {
                foreach (var item in columnNames)
                {
                    dt.Columns.Add(item.COLUMN_NAME);
                }
            }
            else
            {
                foreach (var pair in ((IDictionary<string, object>)data[0]))
                {
                    dt.Columns.Add(pair.Key, (pair.Value ?? string.Empty).GetType());
                }
                foreach (var d in data)
                {
                    dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
                }
            }
            return dt;
        }
    }
    #endregion
}
