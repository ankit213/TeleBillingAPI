using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Template;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {

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
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTemplateRepository.AddTemplate(Convert.ToInt64(userId), templateDetailAC, fullname));
        }

        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> EditTemplate(TemplateDetailAC templateDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTemplateRepository.UpdateTemplate(Convert.ToInt64(userId), templateDetailAC, fullname));
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