using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Master.HandsetManagement;
using TeleBillingRepository.Repository.Master.InternetDevice;
using TeleBillingRepository.Repository.Master.RoleManagement;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {

        #region "Private Variable(s)"
        private readonly IRoleRepository _iRoleRepository;
        private readonly IHandsetRepository _iHandsetRepository;
        private readonly IinternetDeviceRepositoy _iInternetDeviceRepositoy;
        #endregion

        #region "Constructor"
        public MasterController(IRoleRepository iRoleRepository, IHandsetRepository iHandsetRepository, IinternetDeviceRepositoy iInternetDeviceRepositoy)
        {
            _iRoleRepository = iRoleRepository;
            _iHandsetRepository = iHandsetRepository;
            _iInternetDeviceRepositoy = iInternetDeviceRepositoy;
        }
        #endregion

        #region "Public Method(s)"

        #region Menu Binding
        [HttpGet]
        [Route("menulist")]
        public async Task<IActionResult> GetMenuList()
        {
            string roleId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "role_id").Value;
            long newRoleId = roleId != "" ? Convert.ToInt64(roleId) : 0;
            return Ok(await _iRoleRepository.GetMenuListByRoleId(newRoleId));
        }
        #endregion

        #region  Role Management

        [HttpGet]
        [Route("roles")]
        public async Task<IActionResult> GetRoleList()
        {
            return Ok(await _iRoleRepository.GetRoleList());
        }

        [HttpGet]
        [Route("role/{roleId}")]
        public async Task<IActionResult> GetRole(long roleId)
        {
            return Ok(await _iRoleRepository.GetRoleById(roleId));
        }

        [HttpPost]
        [Route("role/add")]
        public async Task<IActionResult> AddRole(RoleAC roleAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iRoleRepository.AddRole(roleAC, Convert.ToInt64(userId), fullname));
        }


        [HttpPut]
        [Route("role/edit")]
        public async Task<IActionResult> EditRole(RoleAC roleAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iRoleRepository.EditRole(roleAC, Convert.ToInt64(userId), fullname));
        }

        [HttpGet]
        [Route("role/delete/{roleId}")]
        public async Task<IActionResult> DeleteRole(long roleId)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iRoleRepository.DeleteRole(roleId, Convert.ToInt64(userId), fullname));
        }


        [HttpGet]
        [Route("role/changestatus/{roleId}")]
        public async Task<IActionResult> ChangeRoleStatus(long roleId)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iRoleRepository.ChangeRoleStatus(roleId, Convert.ToInt64(userId), fullname));
        }
        #endregion

        #region Role Rights Management
        [HttpGet]
        [Route("rolerights/{roleId}")]
        public async Task<IActionResult> GetRoleRights(long roleId)
        {
            return Ok(await _iRoleRepository.GetRoleRights(roleId));
        }


        [HttpPost]
        [Route("rolerights")]
        public async Task<IActionResult> UpdateRoleRights([FromBody] List<RoleRightsAC> roleRightsAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iRoleRepository.UpdateRoleRights(Convert.ToInt64(userId), roleRightsAC, fullname));
        }


        #endregion

        #region Handset Management
        [HttpGet]
        [Route("handsets")]
        public async Task<IActionResult> GetHandsetList()
        {
            return Ok(await _iHandsetRepository.GetHandsetList());
        }


        [HttpGet]
        [Route("handset/list")]
        public async Task<IActionResult> GetHandsets()
        {
            return Ok(await _iHandsetRepository.GetHandsets());
        }


        [HttpGet]
        [Route("handset/{id}")]
        public async Task<IActionResult> GetHandset(long id)
        {
            return Ok(await _iHandsetRepository.GetHandsetById(id));
        }

        [HttpPost]
        [Route("handset/add")]
        public async Task<IActionResult> AddHandset(HandsetDetailAC handsetDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iHandsetRepository.AddHandset(handsetDetailAC, Convert.ToInt64(userId), fullname));
        }


        [HttpPut]
        [Route("handset/edit")]
        public async Task<IActionResult> EditHandset(HandsetDetailAC handsetDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iHandsetRepository.EditHandset(handsetDetailAC, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("handset/delete/{id}")]
        public async Task<IActionResult> DeleteHandsets(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iHandsetRepository.DeleteHandset(id, Convert.ToInt64(userId), fullname));
        }
        #endregion

        #region Internet Device Management

        [HttpGet]
        [Route("internetdevices")]
        public async Task<IActionResult> GetInternetDevices()
        {
            return Ok(await _iInternetDeviceRepositoy.GetInternetDevices());
        }


        [HttpGet]
        [Route("internetdevice/{id}")]
        public async Task<IActionResult> GetInternetDevice(long id)
        {
            return Ok(await _iInternetDeviceRepositoy.GetInternetDeviceById(id));
        }

        [HttpPost]
        [Route("internetdevice/add")]
        public async Task<IActionResult> AddInternetDevice(InternetDeviceAC internetDeviceAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iInternetDeviceRepositoy.AddInternetDevice(internetDeviceAC, Convert.ToInt64(userId), fullname));
        }


        [HttpPut]
        [Route("internetdevice/edit")]
        public async Task<IActionResult> EditInternetDevice(InternetDeviceAC internetDeviceAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iInternetDeviceRepositoy.EditInternetDevice(internetDeviceAC, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("internetdevice/delete/{id}")]
        public async Task<IActionResult> DeleteInternetDevice(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iInternetDeviceRepositoy.DeleteInternetDevice(id, Convert.ToInt64(userId), fullname));
        }
        #endregion

        #region Service Type List
        [HttpGet]
        [Route("servicetypes")]
        public async Task<IActionResult> GetServiceTypes()
        {
            return Ok(await _iRoleRepository.GetServiceTypes());
        }

        #endregion

        #endregion
    }

}