using Architecture.Core.Model;
using Architecture.Repositories.Repositories;
using Architecture.Services.Services;
using Architecture.Web.Helper;
using Architecture.Web.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Architecture.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeServices _iHomeServices;

        public HomeController(ILogger<HomeController> logger, IHomeServices iHomeServices)
        {
            _logger = logger;
            _iHomeServices = iHomeServices;
        }

        public IActionResult Index()
        {
            var tableList = _iHomeServices.GetTablesByUserId("nikhil@mywork.com");
            ViewBag.TableList = tableList.DataList.GetDistinctList("TableName", "TableName");
            return View();
        }

        public IActionResult GetTableData(string tableName)
        {
            var response = _iHomeServices.GetTableData("nikhil@mywork.com", tableName);
            if (response.Status)
            {
                return PartialView("_TableData", response.Data);
            }
            else
            {
                return Json(new { Status = false, message = response.Message });
            }
        }

        [HttpPost]
        public ActionResult ImportFile(string tableName)
        {
            BaseResponse<Tuple<DataTable, int>> result = new BaseResponse<Tuple<DataTable, int>>();
            DataTable excelAsTable = new DataTable();

            try
            {
                string extenstion = Request.Form.Files[0].Name.Split(".").LastOrDefault();
                Stream file = Request.Form.Files[0].OpenReadStream();
                using (Stream stream = new MemoryStream())
                {
                    IExcelDataReader reader = null;

                    //Configuration is set to true for use excel header as datatable header and not as an value of row.
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };

                    //Must check file extension to adjust the reader to the excel file type
                    if (extenstion == "xls")
                        reader = ExcelReaderFactory.CreateBinaryReader(file);
                    else if (extenstion == "xlsx")
                        reader = ExcelReaderFactory.CreateOpenXmlReader(file);
                    else if (extenstion == "csv")
                        reader = ExcelReaderFactory.CreateCsvReader(file);

                    if (reader != null)
                    {
                        //Fill DataSet
                        DataSet content = reader.AsDataSet(conf);
                        excelAsTable = content.Tables[0];
                        result = _iHomeServices.InsertFile("nikhil@mywork.com", tableName, excelAsTable);
                    }
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                result.Exception = ex;
            }
            if (result.Status)
            {
                return PartialView("_TableData", result.Data);
            }
            else
            {
                return Json(new { Status = false, message = result.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
