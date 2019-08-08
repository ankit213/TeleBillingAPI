using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeleBillingRepository.Repository.Provider;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Models;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase {

		#region "Private Variable(s)"
		private readonly IProviderRepository _iProviderRepository;
		#endregion

		#region "Constructor"
		public ProviderController(IProviderRepository iProviderRepository)
		{
			_iProviderRepository = iProviderRepository;
		}
        #endregion

		#region "Public Method(s)"
		[HttpGet]
		[Route("list")]
		public async Task<IActionResult> GetProviderList()
		{
			return Ok(await _iProviderRepository.GetProviders());
		}

		[HttpGet]
		[Route("providerlist")]
		public async Task<IActionResult> ProviderList()
		{
			return Ok(await _iProviderRepository.ProviderList());
		}

		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddProvider(ProviderAC providerAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iProviderRepository.AddProvider(Convert.ToInt64(userId), providerAC));
		}


		[HttpPut]
		[Route("edit")]
		public async Task<IActionResult> EditProvider(ProviderAC providerAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iProviderRepository.UpdateProvider(Convert.ToInt64(userId), providerAC));
		}

		[HttpGet]
		[Route("delete/{id}")]
		public async Task<IActionResult> DeleteProvider(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iProviderRepository.DeleteProvider(Convert.ToInt64(userId), id));
		}


		[HttpGet]
		[Route("changestatus/{id}")]
		public async Task<IActionResult> ChangeProviderStatus(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iProviderRepository.ChangeProviderStatus(Convert.ToInt64(userId), id));
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetProvider(long id)
		{
			return Ok(await _iProviderRepository.GetProviderById(id));
		}

		#endregion
	}


}