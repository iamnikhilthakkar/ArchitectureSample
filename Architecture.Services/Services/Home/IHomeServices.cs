﻿using Architecture.Core.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Architecture.Services.Services
{
    public interface IHomeServices
    {
        BaseResponse<SearchUserTables> GetTablesByUserId(string userEmailId);
        BaseResponse<Tuple<DataTable, int>> GetTableData(string userEmailId, string tableName);
        BaseResponse<Tuple<DataTable, int>> InsertFile(string userEmailId,string tableName,DataTable excelAsTable);
    }
}
