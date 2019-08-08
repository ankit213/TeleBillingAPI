using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.BillMemo;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class BillMemoController : ControllerBase {

		#region "Private Variable(s)"
		private readonly IBillMemoRepository _iBillMemoRepository;
		private IHostingEnvironment _hostingEnvironment;
		#endregion

		#region "Constructor"
		public BillMemoController(IBillMemoRepository iBillMemoRepository, IHostingEnvironment hostingEnvironment) {
			_iBillMemoRepository = iBillMemoRepository;
			_hostingEnvironment = hostingEnvironment;
		}

		#endregion


		#region "Public Method(s)"
		
		[HttpGet]
		[Route("list")]
		public async Task<IActionResult> GetMemoList()
		{
			return Ok(await _iBillMemoRepository.GetMemoList());
		}



		[HttpGet]
		[Route("memobills/{month}/{year}/{providerid}")]
		public async Task<IActionResult> GetMemoBills(int month,int year,int providerid)
		{
			return Ok(await _iBillMemoRepository.GetMemoBills(month,year,providerid));
		}


		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddMemo(MemoAC memoAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillMemoRepository.AddMemo(memoAC,Convert.ToInt64(userId)));
		}

		[HttpGet]
		[Route("delete/{id}")]
		public async Task<IActionResult> DeleteMemoBills(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillMemoRepository.DeleteMemoBills(id, Convert.ToInt64(userId)));
		}

		[HttpGet]
		[Route("approvallist")]
		public async Task<IActionResult> GetApprvoalMemoList()
		{
			return Ok(await _iBillMemoRepository.GetApprvoalMemoList());
		}

		[HttpPost]
		[Route("memoapproval")]
		public async Task<IActionResult> MemoApproval(MemoApprovalAC memoApprovalAC) {
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillMemoRepository.MemoApproval(memoApprovalAC,Convert.ToInt64(userId)));
		}

		
		[HttpGet]
		[Route("file/{id}")]
		public async Task<IActionResult> GetFile(int id)	{
			if(id == 0)
			{
				return File("~/TempUpload/1234568790123456789.Pdf", "application/pdf");
			}
			else if(id == 1)
			{
				return File("~/TempUpload/123456789.xlsx","application/vnd.ms-excel");
			}
			else
				return File("~/TempUpload/123456879.docx", "application/vnd.ms-word");
		}


		#region Account Memo List

		[HttpGet]
		[Route("memo-list")]
		public async Task<IActionResult> GetAccountMemoList() {
			return Ok(await _iBillMemoRepository.GetAccountMemoList());
		}


		#endregion

		#endregion
	}
}