using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
		#endregion
		
		#region "Constructor"
		public BillProcessController(IBillProcessRepository iBillProcessRepository) {
			_iBillProcessRepository = iBillProcessRepository;
		}

		#endregion

		#region Public Method(s)

		[HttpGet]
		[Route("currentbills")]
		public async Task<IActionResult> GetCurrentBills() {
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.GetCurrentBills(Convert.ToInt64(userId)));
		}

		[HttpGet]
		[Route("viewbilldetails/{employeebillmasterid}")]
		public async Task<IActionResult> GetViewBillDetailsByEmpId(long employeebillmasterid) {
			return Ok(await _iBillProcessRepository.GetViewBillDetails(employeebillmasterid));
		}

		[HttpPost]
		[Route("billidentificationsave")]
		public async Task<IActionResult> BillIdentificationSave(BillIdentificationAC billIdentificationAC) {
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.BillIdentificationSave(billIdentificationAC,Convert.ToInt64(userId)));
		}


		[HttpPost]
		[Route("billprocess")]
		public async Task<IActionResult> BillProcess(BillIdentificationAC billIdentificationAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.BillProcess(billIdentificationAC, Convert.ToInt64(userId)));
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
			return Ok(await _iBillProcessRepository.LineManagerApproval(lineManagerApprovalAC, Convert.ToInt64(userId)));
		}


		#endregion

		#region My Staff Bills

		[HttpGet]
		[Route("mystaffbills")]
		public async Task<IActionResult> GetMyStaffBills()
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.GetMyStaffBills(Convert.ToInt64(userId)));
		}

		#endregion

		#region Previous Period Bills 

		[HttpGet]
		[Route("previousperiodbills")]
		public async Task<IActionResult> GetPreviousPeriodBills()
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.GetPreviousPeriodBills(Convert.ToInt64(userId)));
		}


		[HttpGet]
		[Route("reidentificationrequest/{employeebillmasterid}")]
		public async Task<IActionResult> ReIdentificationRequest(long employeebillmasterid) {
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillProcessRepository.ReIdentificationRequest(Convert.ToInt64(userId),employeebillmasterid));
		}

		#endregion

		#region Re-Imburse Request
			[HttpPost]
			[Route("reimbursementrequest")]
			public async Task<IActionResult> ReImbursementRequest(ReImbursementRequestAC reImbursementRequestAC)
			{
				var currentUser = HttpContext.User;
				string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
				return Ok(await _iBillProcessRepository.ReImbursementRequest(Convert.ToInt64(userId), reImbursementRequestAC));
			}

			[HttpGet]
			[Route("reimbursebills")]
			public async Task<IActionResult> GetReImburseBills()
			{
				return Ok(await _iBillProcessRepository.GetReImburseBills());
			}

			[HttpPost]
			[Route("reimbursebillapproval")]
			public async Task<IActionResult> GetReImburseBills(ReImburseBillApprovalAC reImburseBillApprovalAC)
			{
				var currentUser = HttpContext.User;
				string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
				return Ok(await _iBillProcessRepository.ReImburseBillApproval(Convert.ToInt64(userId), reImburseBillApprovalAC));
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
			return Ok(await _iBillProcessRepository.ChangeBillStatus(changeBillStatusACs,Convert.ToInt64(userId)));
		}



		#endregion


		#endregion

	}
}