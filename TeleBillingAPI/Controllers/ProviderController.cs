using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Provider;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {

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

        [HttpGet]
        [Route("allproviderlist")]
        public async Task<IActionResult> AllProviderList()
        {
            return Ok(await _iProviderRepository.AllProviderList());
        }

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddProvider(ProviderAC providerAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iProviderRepository.AddProvider(Convert.ToInt64(userId), providerAC, fullname));
        }


        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> EditProvider(ProviderAC providerAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iProviderRepository.UpdateProvider(Convert.ToInt64(userId), providerAC, fullname));
        }

        [HttpGet]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteProvider(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iProviderRepository.DeleteProvider(Convert.ToInt64(userId), id, fullname));
        }


        [HttpGet]
        [Route("changestatus/{id}")]
        public async Task<IActionResult> ChangeProviderStatus(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iProviderRepository.ChangeProviderStatus(Convert.ToInt64(userId), id, fullname));
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