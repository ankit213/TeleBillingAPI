using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Repository.Telephone;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class TelephoneController : ControllerBase
    {

		#region "Private Variable(s)"
		private readonly ITelephoneRepository _iTelephoneRepository;
		private readonly IBillUploadRepository _iBillUploadRepository;
		#endregion

		#region "Constructor"
		public TelephoneController(ITelephoneRepository iTelephoneRepository,IBillUploadRepository iBillUploadRepository) {
			_iTelephoneRepository = iTelephoneRepository;
			_iBillUploadRepository = iBillUploadRepository;
		}
		#endregion

		#region "Public Method(s)"

		#region Telphone Management

		[HttpGet]
		[Route("list")]
		public async Task<IActionResult> GetTelephoneList()
		{
			return Ok(await _iTelephoneRepository.GetTelephoneList());
		}


		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddTelephone(TelephoneDetailAC telephoneDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.AddTelephone(Convert.ToInt64(userId), telephoneDetailAC));
		}

		[HttpPut]
		[Route("edit")]
		public async Task<IActionResult> EditTelephone(TelephoneDetailAC telephoneDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.UpdateTelephone(Convert.ToInt64(userId), telephoneDetailAC));
		}

		[HttpGet]
		[Route("delete/{id}")]
		public async Task<IActionResult> DeleteTelephone(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.DeleteTelphone(Convert.ToInt64(userId), id));
		}


		[HttpGet]
		[Route("changestatus/{id}")]
		public async Task<IActionResult> ChangeTelephoneStatus(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.ChangeTelephoneStatus(Convert.ToInt64(userId), id));
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetTelephone(long id)
		{
			return Ok(await _iTelephoneRepository.GetTelephoneById(id));
		}
		#endregion


		#region AssignTelephone Management

		[HttpGet]
		[Route("assignedtelephone/list")]
		public async Task<IActionResult> GetAssignedTelephoneList()
		{
			return Ok(await _iTelephoneRepository.GetAssignedTelephoneList());
		}

		[HttpPost]
		[Route("assignedtelephone/add")]
		public async Task<IActionResult> AddAssignedTelephone(AssignTelephoneDetailAC assignTelephoneDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.AddAssignedTelephone(Convert.ToInt64(userId), assignTelephoneDetailAC));
		}

		[HttpPut]
		[Route("assignedtelephone/edit")]
		public async Task<IActionResult> EditAssignedTelephone(AssignTelephoneDetailAC assignTelephoneDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.UpdateAssignedTelephone(Convert.ToInt64(userId), assignTelephoneDetailAC));
		}

		[HttpGet]
		[Route("assignedtelephone/{id}")]
		public async Task<IActionResult> GetAssignedTelephone(long id)
		{
			return Ok(await _iTelephoneRepository.GetAssignedTelephoneById(id));
		}

		[HttpPost]
		[Route("bulkassgintelephone")]
		public async Task<IActionResult> BulkAssginTelePhone() {
			ExcelFileAC excelFileAC = new ExcelFileAC();
			IFormFile file = Request.Form.Files[0];
			excelFileAC.File = file;
			excelFileAC.FolderName = "TempUpload";
			ExcelUploadResponseAC exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTelephoneRepository.UploadBulkAssignTelePhone(Convert.ToInt64(userId),exceluploadDetail));
		}
		#endregion
		

		#endregion

	}
}