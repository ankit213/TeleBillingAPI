﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Package;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {

        #region "Private Variable(s)"
        private readonly IPackageRepository _iPackageRepository;
        #endregion

        #region "Constructor"
        public PackageController(IPackageRepository iPackageRepository)
        {
            _iPackageRepository = iPackageRepository;
        }
        #endregion

        #region "Public Method(s)"

        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> GetPacakgeList()
        {
            return Ok(await _iPackageRepository.GetPackageList());
        }


        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddPackage(PackageDetailAC packageDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iPackageRepository.AddPackage(Convert.ToInt64(userId), packageDetailAC, fullname));
        }

        [HttpGet]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeletePackage(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iPackageRepository.DeletePackage(Convert.ToInt64(userId), id, fullname));
        }


        [HttpGet]
        [Route("changestatus/{id}")]
        public async Task<IActionResult> ChangePackageStatus(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iPackageRepository.ChangePackageStatus(Convert.ToInt64(userId), id, fullname));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetPackage(long id)
        {
            return Ok(await _iPackageRepository.GetPackageById(id));
        }

        #endregion

    }
}