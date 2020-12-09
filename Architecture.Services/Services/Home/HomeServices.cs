using Architecture.Core.Model;
using Architecture.Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Architecture.Services.Services
{
    public class HomeServices : IHomeServices
    {
        private IHomeRepository _iHomeRepository;
        public HomeServices(IHomeRepository iHomeRepository)
        {
            _iHomeRepository = iHomeRepository;
        }

        public BaseResponse<SearchUserTables> GetTablesByUserId(string userId)
        {
            BaseResponse<SearchUserTables> response = new BaseResponse<SearchUserTables>();
            try
            {
                response = _iHomeRepository.GetTablesByUserId(userId);
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
            try
            {
                response = _iHomeRepository.GetTableData(userEmailId, tableName);
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public BaseResponse<Tuple<DataTable, int>> InsertFile(string userEmailId, string tableName, DataTable excelAsTable)
        {
            BaseResponse<Tuple<DataTable, int>> response = new BaseResponse<Tuple<DataTable, int>>();
            try
            {
                response = _iHomeRepository.InsertFile(userEmailId, tableName, excelAsTable);
            }
            catch (Exception ex)
            {

            }
            return response;
        }
    }
}
