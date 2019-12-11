using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Repository.Operator;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class OperatorController : ControllerBase
	{

		#region Private Variable(s)"
		private readonly IOperatorRepository _iOperatorRepository;
		private readonly IBillUploadRepository _iBillUploadRepository;
		#endregion

		#region Constructor
		public OperatorController(IOperatorRepository iOperatorRepository, IBillUploadRepository iBillUploadRepository)
		{
			_iOperatorRepository = iOperatorRepository;
			_iBillUploadRepository = iBillUploadRepository;
		}
		#endregion

		#region Public Method(s)
		[HttpGet]
		[Route("list")]
		public async Task<IActionResult> OperatorCallLogList()
		{
			return Ok(await _iOperatorRepository.OperatorCallLogList());
		}

		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddOperatorCallLog(OperatorCallLogDetailAC operatorCallLogDetailAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			operatorCallLogDetailAC.CallDate = operatorCallLogDetailAC.CallDate.AddDays(1);
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iOperatorRepository.AddOperatorCallLog(Convert.ToInt64(userId), operatorCallLogDetailAC, fullname));
		}


		[HttpPut]
		[Route("edit")]
		public async Task<IActionResult> EditOperatorCallLog(OperatorCallLogDetailAC operatorCallLogDetailAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			operatorCallLogDetailAC.CallDate = operatorCallLogDetailAC.CallDate.AddDays(1);
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iOperatorRepository.EditOperatorCallLog(Convert.ToInt64(userId), operatorCallLogDetailAC, fullname));
		}

		[HttpGet]
		[Route("delete/{id}")]
		public async Task<IActionResult> DeleteOperatorCallLog(long id)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iOperatorRepository.DeleteOperatorCallLog(Convert.ToInt64(userId), id, fullname));
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetOperatorCallLog(long id)
		{
			return Ok(await _iOperatorRepository.GetOperatorCallLog(id));
		}

		[HttpPost]
		[Route("bulkuploadoperatorcalllog")]
		public async Task<IActionResult> BulkUploadOperatorCallLog()
		{
			ExcelFileAC excelFileAC = new ExcelFileAC();
			IFormFile file = Request.Form.Files[0];
			excelFileAC.File = file;
			excelFileAC.FolderName = "TempUpload";
			ExcelUploadResponseAC exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iOperatorRepository.BulkUploadOperatorCallLog(Convert.ToInt64(userId), exceluploadDetail, fullname));
		}
		#endregion

	}
}