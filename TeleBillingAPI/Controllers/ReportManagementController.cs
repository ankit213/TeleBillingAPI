using ClosedXML.Excel;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.BillProcess;
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
		#endregion

		#region Constructor
		public ReportManagementController(IReportRepository iReportRepository, ILogManagement ilogManagement, IHostingEnvironment hostingEnvironment)
		{
			_iLogManagement = ilogManagement;
			_iReportRepository = iReportRepository;
			_hostingEnvironment = hostingEnvironment;
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

	}
}