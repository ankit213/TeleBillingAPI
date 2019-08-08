using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeleBillingRepository.Repository.Template;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class TemplateController : ControllerBase {

		#region "Private Variable(s)"
		private readonly ITemplateRepository _iTemplateRepository;
		#endregion

		#region "Constructor"
		public TemplateController(ITemplateRepository iTemplateRepository)
		{
			_iTemplateRepository = iTemplateRepository;
		}
		#endregion

		#region Public Method(s)
		
		[HttpGet]
		[Route("list")]
		public async Task<IActionResult> GetTemplateList()
		{
			return Ok(await _iTemplateRepository.GetTemplateList());
		}


		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddTemplate(TemplateDetailAC templateDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTemplateRepository.AddTemplate(Convert.ToInt64(userId), templateDetailAC));
		}

		[HttpPut]
		[Route("edit")]
		public async Task<IActionResult> EditTemplate(TemplateDetailAC templateDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iTemplateRepository.UpdateTemplate(Convert.ToInt64(userId), templateDetailAC));
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetTemplate(long id)
		{
			return Ok(await _iTemplateRepository.GetTemplateById(id));
		}

		#endregion
	}
}