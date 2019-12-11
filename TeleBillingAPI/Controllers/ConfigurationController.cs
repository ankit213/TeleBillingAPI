using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Repository.Configuration;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ConfigurationController : ControllerBase
	{

		#region Private Variable(s)"
		private readonly IConfigurationRepository _iConfigurationRepository;
		private readonly IBillUploadRepository _iBillUploadRepository;
		#endregion

		#region Constructor
		public ConfigurationController(IConfigurationRepository iConfigurationRepository, IBillUploadRepository iBillUploadRepository)
		{
			_iConfigurationRepository = iConfigurationRepository;
			_iBillUploadRepository = iBillUploadRepository;
		}
		#endregion

		#region Public Method(s)

		#region Notification & Reminder
		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddConfiguration(TeleBillingUtility.Models.Configuration configuration)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.AddConfiguration(Convert.ToInt64(userId), configuration, fullname));
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetConfiguration()
		{
			return Ok(await _iConfigurationRepository.GetConfiguration());
		}

		#endregion

		#region Provider Wise Transaction 

		[HttpGet]
		[Route("transaction/list")]
		public async Task<IActionResult> GetProviderWiseTransaction()
		{
			return Ok(await _iConfigurationRepository.GetProviderWiseTransaction());
		}

		[HttpPost]
		[Route("transaction/add")]
		public async Task<IActionResult> AddProviderWiseTransaction(ProviderWiseTransactionAC providerWiseTransactionAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.AddProviderWiseTransaction(Convert.ToInt64(userId), providerWiseTransactionAC, fullname));
		}


		[HttpPut]
		[Route("transaction/edit")]
		public async Task<IActionResult> EditProviderWiseTransaction(ProviderWiseTransactionAC providerWiseTransactionAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.UpdateProviderWiseTransaction(Convert.ToInt64(userId), providerWiseTransactionAC, fullname));
		}

		[HttpGet]
		[Route("transaction/delete/{id}")]
		public async Task<IActionResult> DeleteProviderWiseTransaction(long id)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.DeleteProviderWiseTransaction(Convert.ToInt64(userId), id, fullname));
		}

		[HttpGet]
		[Route("transaction/changestatus/{id}")]
		public async Task<IActionResult> ChangeProviderWiseTransactionStatus(long id)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.ChangeProviderWiseTransactionStatus(id, Convert.ToInt64(userId), fullname));
		}

		[HttpGet]
		[Route("transaction/{id}")]
		public async Task<IActionResult> GetProviderWiseTransaction(long id)
		{
			return Ok(await _iConfigurationRepository.GetProviderWiseTransactionById(id));
		}


		[HttpPost]
		[Route("bulkuploadproviderwisetrans")]
		public async Task<IActionResult> BulkUploadProviderWiseTrans([FromForm]string providerId)
		{
			ExcelFileAC excelFileAC = new ExcelFileAC();
			IFormFile file = Request.Form.Files[0];
			excelFileAC.File = file;
			excelFileAC.FolderName = "TempUpload";
			ExcelUploadResponseAC exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.BulkUploadProviderWiseTrans(Convert.ToInt64(userId), exceluploadDetail, Convert.ToInt64(providerId), fullname));
		}


		[HttpPost]
		[Route("transactiontypesetting/update")]
		public async Task<IActionResult> UpdateTransactionTypeSetting(ProviderWiseTransactionAC providerWiseTransactionAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iConfigurationRepository.UpdateTransactionTypeSetting(Convert.ToInt64(userId), providerWiseTransactionAC, fullname));
		}

		#endregion

		#endregion
	}
}