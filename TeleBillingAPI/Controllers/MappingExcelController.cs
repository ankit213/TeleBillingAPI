using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Master.ExcelMapping;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
   // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MappingExcelController : Controller
    {
        #region "Private Variable(s)"
        private readonly IExcelMappingRepository _iExcelMappingRepository;

        #endregion

        #region "Constructor"
        public MappingExcelController(IExcelMappingRepository iExcelMappingRepository)
        {
            _iExcelMappingRepository = iExcelMappingRepository;

        }
        #endregion

        #region "Public Method(s)"

      
        [HttpGet]
        [Route("excelmappinglist")]
        public async Task<IActionResult> GetExcelMappingList()
        {
            return Ok(await _iExcelMappingRepository.GetExcelMappingList());
        }


        [HttpGet]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteExcelMapping(long id)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iExcelMappingRepository.DeleteExcelMapping(Convert.ToInt64(userId), id));
        }

        [HttpPost]
        [Route("excelmapping/add")]
        public async Task<IActionResult> AddExcelMapping(ExcelMappingAC excelMappingAC)
        {
            var currentUser = HttpContext.User;
            string userId = "2";// currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iExcelMappingRepository.AddExcelMapping(excelMappingAC, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("excelmapping/{id}")]
        public async Task<IActionResult> GetExcelMapping(long id)
        {
            return Ok(await _iExcelMappingRepository.GetExcelMappingById(id));
        }
        [HttpPost]
        [Route("excelmapping/edit")]
        public async Task<IActionResult> EditExcelMapping(ExcelMappingAC excelMappingAC)
        {
            var currentUser = HttpContext.User;
            string userId = "2";// currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iExcelMappingRepository.EditExcelMapping(excelMappingAC, Convert.ToInt64(userId)));
        }


        [HttpGet]
        [Route("pbxexcelmappinglist")]
        public async Task<IActionResult> GetPbxExcelMappingList()
        {
            return Ok(await _iExcelMappingRepository.GetPbxExcelMappingList());
        }


        [HttpGet]
        [Route("pbxdelete/{id}")]
        public async Task<IActionResult> DeletePbxExcelMapping(long id)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iExcelMappingRepository.DeletePbxExcelMapping(Convert.ToInt64(userId), id));
        }

        [HttpPost]
        [Route("pbxexcelmapping/add")]
        public async Task<IActionResult> AddPbxExcelMapping(PbxExcelMappingAC excelMappingAC)
        {
            var currentUser = HttpContext.User;
            string userId = "2";// currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iExcelMappingRepository.AddPbxExcelMapping(excelMappingAC, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("pbxexcelmapping/{id}")]
        public async Task<IActionResult> GetPbxExcelMapping(long id)
        {
            return Ok(await _iExcelMappingRepository.GetPbxExcelMappingById(id));
        }
        [HttpPost]
        [Route("pbxexcelmapping/edit")]
        public async Task<IActionResult> EditPbxExcelMapping(PbxExcelMappingAC excelMappingAC)
        {
            var currentUser = HttpContext.User;
            string userId = "2";// currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iExcelMappingRepository.EditPbxExcelMapping(excelMappingAC, Convert.ToInt64(userId)));
        }

        #endregion
    }
}