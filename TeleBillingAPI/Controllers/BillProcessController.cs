using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.BillProcess;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class BillProcessController : ControllerBase
	{
		#region "Private Variable(s)"
		private readonly IBillProcessRepository _iBillProcessRepository;
		private readonly IHostingEnvironment _hostingEnviorment;
		#endregion

		#region "Constructor"
		public BillProcessController(IBillProcessRepository iBillProcessRepository, IHostingEnvironment hostingEnviorment)
		{
			_iBillProcessRepository = iBillProcessRepository;
			_hostingEnviorment = hostingEnviorment;
		}

		#endregion

		#region Public Method(s)

		[HttpGet]
		[Route("currentbills")]
		public async Task<IActionResult> GetCurrentBills()
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.GetCurrentBills(Convert.ToInt64(userId)));
		}

		[HttpGet]
		[Route("viewbilldetails/{employeebillmasterid}")]
		public async Task<IActionResult> GetViewBillDetailsByEmpId(long employeebillmasterid)
		{
			return Ok(await _iBillProcessRepository.GetViewBillDetails(employeebillmasterid));
		}

		[HttpPost]
		[Route("billidentificationsave")]
		public async Task<IActionResult> BillIdentificationSave(BillIdentificationAC billIdentificationAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.BillIdentificationSave(billIdentificationAC, Convert.ToInt64(userId), fullname));
		}


		[HttpPost]
		[Route("billprocess")]
		public async Task<IActionResult> BillProcess(BillIdentificationAC billIdentificationAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.BillProcess(billIdentificationAC, Convert.ToInt64(userId), fullname));
		}


		#region Line Manager Approval


		[HttpGet]
		[Route("linemanageapprovallist")]
		public async Task<IActionResult> GetLineManagerApprovalList()
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.GetLineManagerApprovalList(Convert.ToInt64(userId)));
		}

		[HttpPost]
		[Route("linemanageapproval")]
		public async Task<IActionResult> LineManagerApproval(LineManagerApprovalAC lineManagerApprovalAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.LineManagerApproval(lineManagerApprovalAC, Convert.ToInt64(userId), fullname));
		}


		#endregion

		#region My Staff Bills

		[HttpPost]
		[Route("mystaffbills")]
		public async Task<IActionResult> GetMyStaffBills([FromBody]JqueryDataWithExtraParameterAC param)
		{
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			var results = await _iBillProcessRepository.GetMyStaffBills(param, Convert.ToInt64(userId));
			return new JsonResult(new JqueryDataTablesResult<CurrentBillAC>
			{
				Draw = param.DataTablesParameters.Draw,
				Data = results.Items,
				RecordsFiltered = results.TotalSize,
				RecordsTotal = results.TotalSize
			});
		}



		[HttpPost]
		[Route("exportmystaffbills")]
		public IActionResult ExportMyStaffBillsList(SearchMyStaffAC searchMyStaffAC)
		{
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			var results = _iBillProcessRepository.GetMyStaffExportList(searchMyStaffAC, Convert.ToInt64(userId));
			string fileName = "MyStaffBills.xlsx";
			string folderPath = Path.Combine(_hostingEnviorment.WebRootPath, "TempUploadTelePhone");
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
				var workSheet = package.Workbook.Worksheets.Add("MyStaffBills");
				workSheet.Cells.LoadFromCollection(results, true);
				package.Save();
			}
			return Ok();
		}


		#endregion

		#region Previous Period Bills 

		[HttpPost]
		[Route("previousperiodbills")]
		public async Task<IActionResult> GetPreviousPeriodBills([FromBody]JqueryDataWithExtraParameterAC param)
		{
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			var results = await _iBillProcessRepository.GetPreviousPeriodBills(param, Convert.ToInt64(userId));
			return new JsonResult(new JqueryDataTablesResult<CurrentBillAC>
			{
				Draw = param.DataTablesParameters.Draw,
				Data = results.Items,
				RecordsFiltered = results.TotalSize,
				RecordsTotal = results.TotalSize
			});
		}


		[HttpPost]
		[Route("exportpreviousperiodbills")]
		public IActionResult ExportPreviousPeriodBillList(SearchMyStaffAC searchMyStaffAC)
		{
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			var results = _iBillProcessRepository.GetExportPreviousPeriodBills(searchMyStaffAC, Convert.ToInt64(userId));
			string fileName = "PreviousPeriodBills.xlsx";
			string folderPath = Path.Combine(_hostingEnviorment.WebRootPath, "TempUploadTelePhone");
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
				var workSheet = package.Workbook.Worksheets.Add("PreviousPeriodBills");
				workSheet.Cells.LoadFromCollection(results, true);
				package.Save();
			}
			return Ok();
		}

		[HttpGet]
		[Route("reidentificationrequest/{employeebillmasterid}")]
		public async Task<IActionResult> ReIdentificationRequest(long employeebillmasterid)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.ReIdentificationRequest(Convert.ToInt64(userId), employeebillmasterid, fullname));
		}

		#endregion

		#region Re-Imburse Request
		[HttpPost]
		[Route("reimbursementrequest")]
		public async Task<IActionResult> ReImbursementRequest(ReImbursementRequestAC reImbursementRequestAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.ReImbursementRequest(Convert.ToInt64(userId), reImbursementRequestAC, fullname));
		}

		[HttpGet]
		[Route("reimbursebills")]
		public async Task<IActionResult> GetReImburseBills()
		{
			return Ok(await _iBillProcessRepository.GetReImburseBills());
		}

		[HttpPost]
		[Route("reimbursebillapproval")]
		public async Task<IActionResult> ReImburseBillApproval(ReImburseBillApprovalAC reImburseBillApprovalAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.ReImburseBillApproval(Convert.ToInt64(userId), reImburseBillApprovalAC, fullname));
		}

		#endregion

		#region Change Bill Status

		[HttpGet]
		[Route("changebillstatuslist")]
		public async Task<IActionResult> GetChangeBillStatusList()
		{
			return Ok(await _iBillProcessRepository.GetChangeBillStatusList());
		}



		[HttpPost]
		[Route("changestatus")]
		public async Task<IActionResult> ChangeBillStatus([FromBody]List<ChangeBillStatusAC> changeBillStatusACs)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = currentUser.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillProcessRepository.ChangeBillStatus(changeBillStatusACs, Convert.ToInt64(userId), fullname));
		}



		#endregion

		#endregion

	}
}