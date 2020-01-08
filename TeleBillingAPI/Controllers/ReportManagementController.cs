using DinkToPdf;
using DinkToPdf.Contracts;
using IronPdf;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TeleBillingRepository.Repository.Report;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportManagementController : Controller
    {

        #region Private Variable(s)"
        private readonly IReportRepository _iReportRepository;
        private readonly ILogManagement _iLogManagement;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IConverter _converter;
        #endregion

        #region Constructor
        public ReportManagementController(IReportRepository iReportRepository, ILogManagement ilogManagement, IHostingEnvironment hostingEnvironment, IConverter converter)
        {
            _iLogManagement = ilogManagement;
            _iReportRepository = iReportRepository;
            _hostingEnvironment = hostingEnvironment;
            _converter = converter;
        }
        #endregion

        #region --> Operator Call Logs Report
        [HttpPost]
        [Route("operatorlogreport")]
        public IActionResult GetOperatorLogsReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetOperatorCallLogSearchReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<OperatorLogReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exportoperatorcallloglist")]
        public IActionResult ExportOperatorCallLogList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportOperatorLogReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "OperatorCallLogsList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("OperatorCallLogsList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        #endregion

        #region --> Provider Bill Report
        [HttpPost]
        [Route("providerbillreport")]
        public IActionResult GetProviderBillReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetProviderBillSearchReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<ProviderBillReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exportproviderbill")]
        public IActionResult ExportProviderBillList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportProviderBillReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "ProviderBillList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("ProviderBillList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        #endregion

        #region --> Audit Logs Report
        [HttpPost]
        [Route("auditlogreport")]
        public IActionResult GetAuditLogsReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetAuditActivitySearchReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<AuditLogReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exportauditloglist")]
        public IActionResult ExportAuditLogList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportAuditLogReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "AuditLogReportList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("AuditLogReportList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        #endregion

        #region --> Reimbursement Bill Report
        [HttpPost]
        [Route("reimbursementbillreport")]
        public IActionResult GetReimbursementBillReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetReimbursementBillSearchReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<ReimbursementBillReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exportreimbursementbill")]
        public IActionResult ExportReimbursementBillList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportReimbursementBillReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "ReimbursementBillList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("ReimbursementBillList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        #endregion

        #region --> Account Bills Report
        [HttpPost]
        [Route("accountbillsreport")]
        public IActionResult GetAccountBillReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetAccountBillSearchReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<AccountBillReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }

        [HttpPost]
        [Route("exportaccountbill")]
        public IActionResult ExportAccountBillList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportAccountBillReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "AccountBillList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("AccountBillList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        [HttpGet]
        [Route("viewaccountbilldetails/{empbillid}")]
        public IActionResult ViewAccountBillDetails(long empBillId)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            List<AccountBillDetailsAC> resultlist = new List<AccountBillDetailsAC>();
            resultlist = _iReportRepository.ViewAccountBillDetails(empBillId, Convert.ToInt64(userId));
            if (resultlist != null)
                return Ok(resultlist);
            else
                return Ok(new List<AccountBillDetailsAC>());
        }

        #endregion

        #region Memo Bills Report
        [HttpPost]
        [Route("accountmemosreport")]
        public IActionResult GetAccountMemoReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            var results = _iReportRepository.GetAccountMemoReport(param);
            return new JsonResult(new JqueryDataTablesResult<MemoReportListAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exportaccountmemo")]
        public IActionResult ExportAccountMemoList(SearchReportAC searchReportAC)
        {
            var results = _iReportRepository.ExportAccountMemoReportList(searchReportAC);
            string fileName = "MemoList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("MemoList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }


        [HttpGet]
        [Route("viewmemobillsdetail/{memoId}")]
        public IActionResult ViewMemoBillsDetail(long memoId)
        {
            return Ok(_iReportRepository.ViewMemoBillsDetail(memoId));
        }

        #endregion

        #region Transferred-deactivated Report

        [HttpPost]
        [Route("transferreddeactivated")]
        public IActionResult TransferredDeactivatedReport([FromBody]JqueryDataTablesParameters param)
        {
            var results = _iReportRepository.GetTransferredDeactivatedReport(param);
            return new JsonResult(new JqueryDataTablesResult<TransferDeActivatedReportAC>
            {
                Draw = param.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exporttransferreddeactivated")]
        public IActionResult ExportTransferredDeactivatedList(SearchReportAC searchReportAC)
        {
            var results = _iReportRepository.ExportTransferredDeactivatedList(searchReportAC);
            string fileName = "TransferredDeactivatedList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("TransferredDeactivatedList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }


        #endregion

        #region Vendor Wise Package Report

        [HttpPost]
        [Route("vendorwisepackagereport")]
        public IActionResult VendorWisePackageDetailReport([FromBody]JqueryDataTablesParameters param)
        {
            var results = _iReportRepository.VendorWisePackageDetailReport(param);
            return new JsonResult(new JqueryDataTablesResult<VendorWisePackageDetailsAC>
            {
                Draw = param.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exportvendorwisepackages")]
        public IActionResult ExportVendorWisePackageDetailList(SearchReportAC searchReportAC)
        {
            var results = _iReportRepository.ExportVendorWisePackageDetailList(searchReportAC);
            string fileName = "VendorWisePackageDetailList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("VendorWisePackageDetailList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }
        #endregion

        #region -->Users Highest Consumption Report
        [HttpPost]
        [Route("usershighestconsumptionreport")]
        public IActionResult UsersHighestConsumptionReport(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(_iReportRepository.GetUsersHighestConsumptionReport(searchReportAC, Convert.ToInt64(userId)));
        }

        #endregion

        #region --> User Consumption Report
        [HttpPost]
        [Route("usersconsumptionreport")]
        public IActionResult GetUsersConsumptionReport([FromBody]JqueryDataWithExtraParameterAC param)
        {


            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetUserConsumptionSearchReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<UserConsumptionReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }

        [HttpPost]
        [Route("exportusersconsumptionlist")]
        public IActionResult ExportUserConsumptionLogList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportUserConsumptionReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "UsersConsumptionReportList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("UsersConsumptionList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }


        [HttpGet]
        [Route("viewusersconsumptiondetail/{empbillid}")]
        public IActionResult ViewUserConsumptionDetails(long empbillid)
        {

            UserConsumptionDetailAC _UserConsumptionDetail = new UserConsumptionDetailAC();
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            _UserConsumptionDetail = _iReportRepository.ViewUserConsumptionReportDetail(empbillid, Convert.ToInt64(userId));

            return Ok(_UserConsumptionDetail);
        }


        [HttpGet]
        [Route("exportusersconsumptionbill/{empbillid}")]
        public IActionResult ExportUserConsumptionBill(long empbillid)
        {
            long TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
            try
            {


                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "try to create PDF");
                #endregion

                UserConsumptionDetailAC _UserConsumptionDetail = new UserConsumptionDetailAC();
                string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
                _UserConsumptionDetail = _iReportRepository.ViewUserConsumptionReportDetail(empbillid, Convert.ToInt64(userId));


                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "API CALLED :");
                #endregion

                #region --> Save PDF 
                HtmlToPdf Renderer = new HtmlToPdf();
                string fileName = "UsersConsumptionBill.pdf";
                string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
                string filePath = Path.Combine(folderPath, fileName);
                try
                {
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                }
                catch (Exception e)
                {
                    string dataJson = JsonConvert.SerializeObject(_UserConsumptionDetail);
                    Renderer.RenderHtmlAsPdf(dataJson).SaveAs(filePath);
                }

                #region --> design HTML 
                string _htmlString = "<html><header><title>User Consumption Report </title></header>";

                _htmlString += "<body>";

                _htmlString += "<table width='100%' style='font-size: 11px; border: solid #9c9a9a;'><thead><tr><th colspan='7'></th></tr></thead>";

                _htmlString += "<tbody background-color: #edeff1;>";

                //  ----- Sub Section Row--------------
                _htmlString += "<tr>";
                _htmlString += "<td colspan='7'></td>";
                _htmlString += "</tr>";

                // ----- Sub section Title------------
                _htmlString += "<tr>";
                _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 15px;' colspan='7'>User Bill Details</td>";
                _htmlString += "</tr>";

                //  ----- Sub Section Row--------------
                _htmlString += "<tr>";
                _htmlString += "<td colspan='7'></td>";
                _htmlString += "</tr>";

                _htmlString += "<tr>";
                // _htmlString += "<td></td>";
                _htmlString += "<td><b>Employee</b></td>" + "<td colspan='2' >" + _UserConsumptionDetail.FullName + "</td>";
                _htmlString += "<td><b>Business Unit</b></td>" + "<td>" + _UserConsumptionDetail.BusinessUnit + "</td>";
                _htmlString += "<td><b>Bill Month</b></td>" + "<td>" + _UserConsumptionDetail.BillDate + "</td>";
                _htmlString += "</tr>";

                _htmlString += "<tr>";
                //   _htmlString += "<td></td>";
                _htmlString += "<td><b>Employee</b></td>" + "<td colspan='2'>" + _UserConsumptionDetail.EmailId + "</td>";
                _htmlString += "<td><b>Cost Center</b></td>" + "<td>" + _UserConsumptionDetail.CostCenter + "</td>";
                _htmlString += "<td><b>Total Amount</b></td>" + "<td>" + _UserConsumptionDetail.BillAmount + "</td>";
                _htmlString += "</tr>";


                _htmlString += "<tr>";
                //  _htmlString += "<td></td>";
                _htmlString += "<td><b>PF Number</b></td>" + "<td colspan='2'>" + _UserConsumptionDetail.EmpPFNumber + "</td>";
                _htmlString += "<td><b>Provider</b></td>" + "<td>" + _UserConsumptionDetail.Provider + "</td>";
                _htmlString += "<td><b>Deductable</b></td>" + "<td>" + _UserConsumptionDetail.DeductionAmount + "</td>";
                _htmlString += "</tr>";


                _htmlString += "<tr>";
                // _htmlString += "<td></td>";
                _htmlString += "<td><b>Extension Number</b></td>" + "<td colspan='2'>" + _UserConsumptionDetail.EmpPFNumber + "</td>";
                _htmlString += "<td><b>Mobile No</b></td>" + "<td>" + _UserConsumptionDetail.Provider + "</td>";
                _htmlString += "<td><b>Emp.Bill Status</b></td>" + "<td>" + _UserConsumptionDetail.EmpBillStatus + "</td>";
                _htmlString += "</tr>";

                //  ----- Sub Section Row--------------
                _htmlString += "<tr>";
                _htmlString += "<td colspan='7'></td>";
                _htmlString += "</tr>";

                // ----- Sub section Title------------
                _htmlString += "<tr>";
                _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 18px;' colspan='7'>Package Details</td>";
                _htmlString += "</tr>";

                //  ----- Sub Section Row--------------
                _htmlString += "<tr>";
                _htmlString += "<td colspan='7'></td>";
                _htmlString += "</tr>";

                if (_UserConsumptionDetail.userPacakgeDetailList != null && _UserConsumptionDetail.userPacakgeDetailList.Count > 0)
                {
                    _htmlString += "<tr>";
                    _htmlString += "<td colspan='3' ><b>Package Detail</b></td>";
                    _htmlString += "<td colspan='2' ><b>Package Amount</b></td>";
                    _htmlString += "<td colspan='2' ><b>User Consumption Amount</b></td>";
                    _htmlString += "</tr>";

                    foreach (var pkg in _UserConsumptionDetail.userPacakgeDetailList)
                    {
                        _htmlString += "<tr>";
                        _htmlString += "<td colspan='3' >" + pkg.Package + "<small>(" + pkg.ServiceType + ")</small>" + "</td>";
                        _htmlString += "<td colspan='2' >" + pkg.PackageAmount + "</td>";
                        _htmlString += "<td colspan='2' ><b>" + pkg.UserConsumptionAmount + "</td>";
                        _htmlString += "</tr>";


                        //    _htmlString += "<tr>";
                        //// _htmlString += "<td></td>";
                        //_htmlString += "<td><b>Package Detail</b></td>" + "<td colspan='2'>" + pkg.Package + "<small>(" + pkg.ServiceType + ")</small>" + "</td>";
                        //_htmlString += "<td><b>Package Amount</b></td>" + "<td>" + pkg.PackageAmount + "</td>";
                        //_htmlString += "<td><b>User Consumption Amount</b></td>" + "<td>" + pkg.UserConsumptionAmount + "</td>";
                        //_htmlString += "</tr>";
                    }
                }
                //  ----- Sub Section Row--------------
                _htmlString += "<tr>";
                _htmlString += "<td colspan='7'></td>";
                _htmlString += "</tr>";

                // ----- Sub section Title------------
                _htmlString += "<tr>";
                _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 18px;' colspan='7'>Call Detail List</td>";
                _htmlString += "</tr>";

                //  ----- Sub Section Row--------------
                _htmlString += "<tr>";
                _htmlString += "<td colspan='7'></td>";
                _htmlString += "</tr>";


                if (_UserConsumptionDetail.userCallDetailList != null && _UserConsumptionDetail.userCallDetailList.Count > 0)
                {
                    foreach (var item in _UserConsumptionDetail.userCallDetailList)
                    {
                        // ----- Sub section Title------------
                        _htmlString += "<tr>";
                        _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='7'></td>";
                        _htmlString += "</tr>";


                        _htmlString += "<tr class='sub-header-row'>";
                        _htmlString += "<td><b>Trans Type</b></td>";
                        _htmlString += "<td><b>Package</b></td>";
                        _htmlString += "<td><b>Call Date</b></td>";
                        _htmlString += "<td><b>Duration</b></td>";
                        _htmlString += "<td><b>Description</b></td>";
                        _htmlString += "<td><b>CallType</b></td>";
                        _htmlString += "<td><b>Amount</b></td>";
                        _htmlString += "</tr>";

                        foreach (var subitem in item.CallDetailsList)
                        {
                            _htmlString += "<tr>";
                            _htmlString += "<td>" + subitem.TransType + "</td>";
                            _htmlString += "<td>" + subitem.Package + "</td>";
                            _htmlString += "<td>" + subitem.CallDate + "</td>";
                            _htmlString += "<td>" + subitem.CallDuration + "</td>";
                            _htmlString += "<td>" + subitem.Description + "</td>";
                            _htmlString += "<td>" + subitem.CallType + "</td>";
                            _htmlString += "<td>" + subitem.CallAmount + "</td>";
                            _htmlString += "</tr>";
                        }

                        //  ----- Sub Section Row--------------
                        _htmlString += "<tr>";
                        _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='7'></td>";
                        _htmlString += "</tr>";
                        //------ Sub total Row -------------
                        _htmlString += "<tr>";
                        _htmlString += "<td><b>" + item.TransType + "</b></td>";
                        _htmlString += "<td></td>";
                        _htmlString += "<td></td>";
                        _htmlString += "<td></td>";
                        _htmlString += "<td></td>";
                        _htmlString += "<td><b>Sub Total</b></td>";
                        _htmlString += "<td>" + item.subtotal + "</td>";
                        _htmlString += "</tr>";

                        //  ----- Sub Section Row--------------
                        _htmlString += "<tr>";
                        _htmlString += "<td colspan='7'></td>";
                        _htmlString += "</tr>";

                    }
                }

                _htmlString += "</tbody> </table>";
                _htmlString += "</body></html>";
                #endregion



                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "HTML CReated :" + _htmlString);
                #endregion

                string tempPath = Path.Combine(folderPath, "Temp");

                Renderer.RenderHtmlAsPdf(_htmlString, tempPath, null).SaveAs(filePath);




                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "File Path PDF :" + filePath);
                #endregion
                #endregion

                return Ok();
            }
            catch (Exception e)
            {
                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "File Path PDF :" + e.InnerException.Message);
                #endregion
                return Ok();
            }
        }


        [HttpGet]
        [Route("exportusersbill/{empbillid}")]
        public IActionResult CreatePDF(long empbillid)
        {
            long TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
            try
            {
                string fileName = "UsersBill.pdf";
                string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
                string filePath = Path.Combine(folderPath, fileName);

                string reportTime = DateTime.Now.ToString("dddd, dd MMMM yyyy");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "User Consumption Bill Report",
                    Out = filePath
                };




                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = GetHTMLString(empbillid),
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css") },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "User Consumption Bill Report  - " + reportTime }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                _converter.Convert(pdf);

                return Ok("Successfully created PDF document.");

            }
            catch (Exception e)
            {
                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "File Path PDF :" + e.InnerException.Message);
                #endregion
                return Ok();
            }
        }


        private string GetHTMLString(long empbillid)
        {

            UserConsumptionDetailAC _UserConsumptionDetail = new UserConsumptionDetailAC();
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            _UserConsumptionDetail = _iReportRepository.ViewUserConsumptionReportDetail(empbillid, Convert.ToInt64(userId));

            #region --> design HTML 
            string _htmlString = "<html><header><title>User Consumption Report </title></header>";

            _htmlString += "<body>";

            _htmlString += "<table width='100%' style='font-size: 11px; border: solid #9c9a9a;'><thead><tr><th colspan='7'></th></tr></thead>";

            _htmlString += "<tbody background-color: #edeff1;>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='7'></td>";
            _htmlString += "</tr>";

            // ----- Sub section Title------------
            _htmlString += "<tr>";
            _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 15px;' colspan='7'>User Bill Details</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='7'></td>";
            _htmlString += "</tr>";

            _htmlString += "<tr>";
            // _htmlString += "<td></td>";
            _htmlString += "<td><b>Employee</b></td>" + "<td colspan='2' >" + _UserConsumptionDetail.FullName + "</td>";
            _htmlString += "<td><b>Business Unit</b></td>" + "<td>" + _UserConsumptionDetail.BusinessUnit + "</td>";
            _htmlString += "<td><b>Bill Month</b></td>" + "<td>" + _UserConsumptionDetail.BillDate + "</td>";
            _htmlString += "</tr>";

            _htmlString += "<tr>";
            //   _htmlString += "<td></td>";
            _htmlString += "<td><b>Employee</b></td>" + "<td colspan='2'>" + _UserConsumptionDetail.EmailId + "</td>";
            _htmlString += "<td><b>Cost Center</b></td>" + "<td>" + _UserConsumptionDetail.CostCenter + "</td>";
            _htmlString += "<td><b>Total Amount</b></td>" + "<td>" + _UserConsumptionDetail.BillAmount + "</td>";
            _htmlString += "</tr>";


            _htmlString += "<tr>";
            //  _htmlString += "<td></td>";
            _htmlString += "<td><b>PF Number</b></td>" + "<td colspan='2'>" + _UserConsumptionDetail.EmpPFNumber + "</td>";
            _htmlString += "<td><b>Provider</b></td>" + "<td>" + _UserConsumptionDetail.Provider + "</td>";
            _htmlString += "<td><b>Deductable</b></td>" + "<td>" + _UserConsumptionDetail.DeductionAmount + "</td>";
            _htmlString += "</tr>";


            _htmlString += "<tr>";
            // _htmlString += "<td></td>";
            _htmlString += "<td><b>Extension Number</b></td>" + "<td colspan='2'>" + _UserConsumptionDetail.EmpPFNumber + "</td>";
            _htmlString += "<td><b>Mobile No</b></td>" + "<td>" + _UserConsumptionDetail.Provider + "</td>";
            _htmlString += "<td><b>Emp.Bill Status</b></td>" + "<td>" + _UserConsumptionDetail.EmpBillStatus + "</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='7'></td>";
            _htmlString += "</tr>";

            // ----- Sub section Title------------
            _htmlString += "<tr>";
            _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 18px;' colspan='7'>Package Details</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='7'></td>";
            _htmlString += "</tr>";

            if (_UserConsumptionDetail.userPacakgeDetailList != null && _UserConsumptionDetail.userPacakgeDetailList.Count > 0)
            {
                _htmlString += "<tr>";
                _htmlString += "<td colspan='3' ><b>Package Detail</b></td>";
                _htmlString += "<td colspan='2' ><b>Package Amount</b></td>";
                _htmlString += "<td colspan='2' ><b>User Consumption Amount</b></td>";
                _htmlString += "</tr>";

                foreach (var pkg in _UserConsumptionDetail.userPacakgeDetailList)
                {
                    _htmlString += "<tr>";
                    _htmlString += "<td colspan='3' >" + pkg.Package + "<small>(" + pkg.ServiceType + ")</small>" + "</td>";
                    _htmlString += "<td colspan='2' >" + pkg.PackageAmount + "</td>";
                    _htmlString += "<td colspan='2' ><b>" + pkg.UserConsumptionAmount + "</td>";
                    _htmlString += "</tr>";


                    //    _htmlString += "<tr>";
                    //// _htmlString += "<td></td>";
                    //_htmlString += "<td><b>Package Detail</b></td>" + "<td colspan='2'>" + pkg.Package + "<small>(" + pkg.ServiceType + ")</small>" + "</td>";
                    //_htmlString += "<td><b>Package Amount</b></td>" + "<td>" + pkg.PackageAmount + "</td>";
                    //_htmlString += "<td><b>User Consumption Amount</b></td>" + "<td>" + pkg.UserConsumptionAmount + "</td>";
                    //_htmlString += "</tr>";
                }
            }
            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='7'></td>";
            _htmlString += "</tr>";

            // ----- Sub section Title------------
            _htmlString += "<tr>";
            _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 18px;' colspan='7'>Call Detail List</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='7'></td>";
            _htmlString += "</tr>";


            if (_UserConsumptionDetail.userCallDetailList != null && _UserConsumptionDetail.userCallDetailList.Count > 0)
            {
                foreach (var item in _UserConsumptionDetail.userCallDetailList)
                {
                    // ----- Sub section Title------------
                    _htmlString += "<tr>";
                    _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='7'></td>";
                    _htmlString += "</tr>";


                    _htmlString += "<tr class='sub-header-row'>";
                    _htmlString += "<td><b>Trans Type</b></td>";
                    _htmlString += "<td><b>Package</b></td>";
                    _htmlString += "<td><b>Call Date</b></td>";
                    _htmlString += "<td><b>Duration</b></td>";
                    _htmlString += "<td><b>Description</b></td>";
                    _htmlString += "<td><b>CallType</b></td>";
                    _htmlString += "<td><b>Amount</b></td>";
                    _htmlString += "</tr>";

                    foreach (var subitem in item.CallDetailsList)
                    {
                        _htmlString += "<tr>";
                        _htmlString += "<td>" + subitem.TransType + "</td>";
                        _htmlString += "<td>" + subitem.Package + "</td>";
                        _htmlString += "<td>" + subitem.CallDate + "</td>";
                        _htmlString += "<td>" + subitem.CallDuration + "</td>";
                        _htmlString += "<td>" + subitem.Description + "</td>";
                        _htmlString += "<td>" + subitem.CallType + "</td>";
                        _htmlString += "<td>" + subitem.CallAmount + "</td>";
                        _htmlString += "</tr>";
                    }

                    //  ----- Sub Section Row--------------
                    _htmlString += "<tr>";
                    _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='7'></td>";
                    _htmlString += "</tr>";
                    //------ Sub total Row -------------
                    _htmlString += "<tr>";
                    _htmlString += "<td><b>" + item.TransType + "</b></td>";
                    _htmlString += "<td></td>";
                    _htmlString += "<td></td>";
                    _htmlString += "<td></td>";
                    _htmlString += "<td></td>";
                    _htmlString += "<td><b>Sub Total</b></td>";
                    _htmlString += "<td>" + item.subtotal + "</td>";
                    _htmlString += "</tr>";

                    //  ----- Sub Section Row--------------
                    _htmlString += "<tr>";
                    _htmlString += "<td colspan='7'></td>";
                    _htmlString += "</tr>";

                }
            }

            _htmlString += "</tbody> </table>";
            _htmlString += "</body></html>";
            #endregion

            var sb = new StringBuilder();
            sb.Append(_htmlString);
            //sb.Append(@"
            //            <html>
            //                <head>
            //                </head>
            //                <body>
            //                    <div class='header'><h1>This is the generated PDF report!!!</h1></div>
            //                    <table align='center'>
            //                        <tr>
            //                            <th>Name</th>
            //                            <th>LastName</th>
            //                            <th>Age</th>
            //                            <th>Gender</th>
            //                        </tr>");

            //foreach (var emp in _UserConsumptionDetail.userCallDetailList)
            //{
            //    sb.AppendFormat(@"<tr>
            //                        <td>{0}</td>
            //                        <td>{1}</td>
            //                        <td>{2}</td>
            //                        <td>{3}</td>
            //                      </tr>", emp.TransType, emp.subtotal, emp.EmpBillId, emp.TransactionTypeId);
            //}

            //sb.Append(@"
            //                    </table>
            //                </body>
            //            </html>");

            return sb.ToString();
        }

        #endregion

        #region -->Get Multiple lines User Report
        [HttpPost]
        [Route("multiplelinesusersreport")]
        public IActionResult GetMultipleLinesUsersReport([FromBody]JqueryDataWithExtraParameterAC param)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.GetMultipleLinesUserReportList(param, Convert.ToInt64(userId));
            return new JsonResult(new JqueryDataTablesResult<MultipleLinesUserListReportAC>
            {
                Draw = param.DataTablesParameters.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }

        [HttpPost]
        [Route("exportmultiplelinesusers")]
        public IActionResult ExportMultipleLinesUserListReportList(SearchReportAC searchReportAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            var results = _iReportRepository.ExportAuditLogReportList(searchReportAC, Convert.ToInt64(userId));
            string fileName = "MultipleLinesUserListReportList.xlsx";
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
            string filePath = Path.Combine(folderPath, fileName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("MultipleLinesUserListReportList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        [HttpGet]
        [Route("Viewusermobilepackagedetails/{employeeid}")]
        public IActionResult ViewUserMobilePackageDetails(long employeeid)
        {
            UserMobilePackageDetailReportAC _userMobilePackageDetail = new UserMobilePackageDetailReportAC();
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            _userMobilePackageDetail = _iReportRepository.ViewUserMobilePackageDetail(employeeid, Convert.ToInt64(userId));
            return Ok(_userMobilePackageDetail);
        }


        [HttpGet]
        [Route("exportusersmultiplelines/{employeeid}")]
        public IActionResult CreatePDFOfUserMultiplelines(long employeeid)
        {
            long TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
            try
            {
                string fileName = "UserMultiplelines.pdf";
                string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
                string filePath = Path.Combine(folderPath, fileName);

                string reportTime = DateTime.Now.ToString("dddd, dd MMMM yyyy");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "User Multiple lines Report",
                    Out = filePath
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = GetHTMLStringOfUserMultipleLines(employeeid),
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css") },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "User Multiple lines Report  - " + reportTime }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                _converter.Convert(pdf);

                return Ok("Successfully created PDF document.");

            }
            catch (Exception e)
            {
                #region Transaction Log Entry           

                _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(1), Convert.ToInt64(0), "File Path PDF :" + e.InnerException.Message);
                #endregion
                return Ok();
            }
        }

        private string GetHTMLStringOfUserMultipleLines(long employeeid)
        {

            UserMobilePackageDetailReportAC _userMobilePackageDetail = new UserMobilePackageDetailReportAC();
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            _userMobilePackageDetail = _iReportRepository.ViewUserMobilePackageDetail(employeeid, Convert.ToInt64(userId));

            #region --> design HTML 
            string _htmlString = "<html><header><title>User's Multiple Lines Report </title></header>";

            _htmlString += "<body>";

            _htmlString += "<table width='100%' style='font-size: 11px; border: solid #9c9a9a;'><thead><tr><th colspan='4'></th></tr></thead>";

            _htmlString += "<tbody background-color: #edeff1;>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='4'></td>";
            _htmlString += "</tr>";

            // ----- Sub section Title------------
            _htmlString += "<tr>";
            _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 15px;' colspan='4'>User's Multiple Lines Details</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='4'></td>";
            _htmlString += "</tr>";

            _htmlString += "<tr>";
            // _htmlString += "<td></td>";         
            _htmlString += "<td><b>Employee</b></td>" + "<td>" + _userMobilePackageDetail.EmployeeName + "</td>";
            _htmlString += "<td><b>Department</b></td>" + "<td>" + _userMobilePackageDetail.Department + "</td>";
            _htmlString += "</tr>";

            _htmlString += "<tr>";
            // _htmlString += "<td></td>";         
            _htmlString += "<td><b>PF Number</b></td>" + "<td>" + _userMobilePackageDetail.EmpPFNumber + "</td>";
            _htmlString += "<td><b>Extension Number</b></td>" + "<td>" + _userMobilePackageDetail.ExtensionNumber + "</td>";
            _htmlString += "</tr>";

            _htmlString += "<tr>";
            // _htmlString += "<td></td>";         
            _htmlString += "<td><b>Business Unit</b></td>" + "<td>" + _userMobilePackageDetail.BusinessUnit + "</td>";
            _htmlString += "<td><b>Cost Center</b></td>" + "<td>" + _userMobilePackageDetail.CostCenter + "</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='4'></td>";
            _htmlString += "</tr>";

            // ----- Sub section Title------------
            _htmlString += "<tr>";
            _htmlString += "<td style='border: 1px solid #ddd;padding: 5px;padding-top: 2px;padding-bottom: 2px;text-align: left;background-color: #a0dae0;color: black;text-align: center;font-weight: bold;font-size: 18px;' colspan='4'>Mobile Package Detail List</td>";
            _htmlString += "</tr>";

            //  ----- Sub Section Row--------------
            _htmlString += "<tr>";
            _htmlString += "<td colspan='4'></td>";
            _htmlString += "</tr>";

            if (_userMobilePackageDetail.userMobileDetailList != null && _userMobilePackageDetail.userMobileDetailList.Count > 0)
            {


                foreach (var mobilePkg in _userMobilePackageDetail.userMobileDetailList)
                {
                    // ----- Sub section Title------------                    _
                    _htmlString += "<tr>";
                    _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='4'></td>";
                    _htmlString += "</tr>";

                    _htmlString += "<tr>";
                    _htmlString += "<td ><b>Mobile</b></td>";
                    _htmlString += "<td ><b>Provider</b></td>";
                    _htmlString += "<td ><b>Assign Type</b></td>";
                    _htmlString += "<td ><b>Line Status</b></td>";
                    _htmlString += "</tr>";


                    _htmlString += "<tr>";
                    _htmlString += "<td><b>" + mobilePkg.TelephoneNumber + "</b></td>";
                    _htmlString += "<td><b>" + mobilePkg.Provider + "</b></td>";
                    _htmlString += "<td><b>" + mobilePkg.AssignType + "</b></td>";
                    _htmlString += "<td><b>" + mobilePkg.LineStatus + "</b></td>";
                    _htmlString += "</tr>";

                    _htmlString += "<tr>";
                    _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='4'></td>";
                    _htmlString += "</tr>";

                    _htmlString += "<tr>";
                    _htmlString += "<td><b>Package</b></td>";
                    _htmlString += "<td><b>Package Amount</b></td>";
                    _htmlString += "<td><b>Start Date</b></td>";
                    _htmlString += "<td><b>End Date</b></td>";
                    _htmlString += "</tr>";

                    _htmlString += "<tr>";
                    _htmlString += "<td style='background-color:#9c9a9a; border-bottom:solid 1px black;' colspan='4'></td>";
                    _htmlString += "</tr>";

                    foreach (var subitem in mobilePkg.userMobilePackageDetailLists)
                    {
                        _htmlString += "<tr>";
                        _htmlString += "<td>" + subitem.PackageName + "<small>(" + subitem.ServiceName + ")</small></td>";
                        _htmlString += "<td>" + subitem.PackageAmount + "</td>";
                        _htmlString += "<td>" + subitem.StartDate + "</td>";
                        _htmlString += "<td>" + subitem.EndDate + "</td>";
                        _htmlString += "</tr>";
                    }


                }
            }

            _htmlString += "</tbody> </table>";
            _htmlString += "</body></html>";
            #endregion

            var sb = new StringBuilder();
            sb.Append(_htmlString);

            return sb.ToString();
        }

        #endregion

    }
}