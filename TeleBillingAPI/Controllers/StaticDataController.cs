using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.StaticData;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StaticDataController : Controller
    {
        #region "Private Variable(s)"
        private readonly IStaticDataRepository _iStaticDataRepository;
        private IHostingEnvironment _hostingEnvironment;
        #endregion

        #region "Constructor"
        public StaticDataController(IStaticDataRepository iStaticDataRepository, IHostingEnvironment hostingEnvironment)
        {
            _iStaticDataRepository = iStaticDataRepository;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region "Public Method(s)"
        [HttpGet]
        [Route("countries")]
        public async Task<IActionResult> GetCountries()
        {
            return Ok(await _iStaticDataRepository.CountryList());
        }


        [HttpGet]
        [Route("servicetypes")]
        public async Task<IActionResult> GetServiceTypes()
        {
            return Ok(await _iStaticDataRepository.ServiceTypeList());
        }


        [HttpGet]
        [Route("providerservices/{providerId}")]
        public async Task<IActionResult> GetProviderServices(long providerId)
        {
            return Ok(await _iStaticDataRepository.ProviderServiceTypeList(providerId));
        }

        [HttpGet]
        [Route("providernotmappedservices/{providerId}")]
        public async Task<IActionResult> GetProviderNotMappedServices(long providerId)
        {
            return Ok(_iStaticDataRepository.ProviderNotMappedServiceTypeList(providerId));
        }

        [HttpGet]
        [Route("providerCommonservices/{providerId}/{mappingId}")]
        public async Task<IActionResult> GetProviderCommonServices(long providerId, long mappingId)
        {
            return Ok(_iStaticDataRepository.ProviderCommonServiceTypeList(providerId, mappingId));
        }

        [HttpGet]
        [Route("costcenters")]
        public async Task<IActionResult> GetCostCenters()
        {
            return Ok(await _iStaticDataRepository.CostCenterList());
        }
        [HttpGet]
        [Route("getcostcenterbybusinessid/{id}")]
        public async Task<IActionResult> GetCostCentersBYBU(long id = 0)
        {
            return Ok(await _iStaticDataRepository.BusinessUnitCostCenterList(id));
        }

        [HttpGet]
        [Route("businessunits")]
        public async Task<IActionResult> GetBusinessUnits()
        {
            return Ok(await _iStaticDataRepository.BusinessUnitList());
        }


        [HttpGet]
        [Route("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            return Ok(await _iStaticDataRepository.DepartmentList());
        }

        [HttpGet]
        [Route("businessunitdepartments/{id}")]
        public async Task<IActionResult> GetBusinessUnitDepartments(long id)
        {
            return Ok(await _iStaticDataRepository.BusinessUnitDepartmentList(id));
        }

        [HttpGet]
        [Route("roles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _iStaticDataRepository.RoleNameList());
        }


        [HttpGet]
        [Route("getdbfieldlist/{servicetypeId}")]
        public async Task<IActionResult> GetDBFieldsList(long servicetypeId)
        {
            return Ok(await _iStaticDataRepository.DBFieldsList(servicetypeId));
        }

        [HttpGet]
        [Route("getdbfieldlistbyorder/{servicetypeId}")]
        public async Task<IActionResult> GetDBFieldsListByOrder(long servicetypeId)
        {
            return Ok(await _iStaticDataRepository.DBFieldsListByOrder(servicetypeId));
        }


        [HttpGet]
        [Route("linetypes")]
        public async Task<IActionResult> GetLineTypeList()
        {
            return Ok(await _iStaticDataRepository.GetLineTypeList());
        }

        [HttpGet]
        [Route("assigntypes")]
        public async Task<IActionResult> GetAssignTypeList()
        {
            return Ok(await _iStaticDataRepository.GetAssignTypeList());
        }

        [HttpGet]
        [Route("getpbxdbfieldlist/{deviceId}")]
        public async Task<IActionResult> GetDeviceDBFieldsList(long deviceId)
        {
            return Ok(await _iStaticDataRepository.DeviceDBFieldsList(deviceId));
        }


        [HttpGet]
        [Route("devices")]
        public async Task<IActionResult> GetDeviceList()
        {
            return Ok(await _iStaticDataRepository.DeviceList());
        }


        [HttpGet]
        [Route("months")]
        public async Task<IActionResult> GetMonthList()
        {
            return Ok(await _iStaticDataRepository.MonthList());
        }

        [HttpGet]
        [Route("activities")]
        public async Task<IActionResult> GetActivityList()
        {
            return Ok(await _iStaticDataRepository.ActionList());
        }

        [HttpGet]
        [Route("years")]
        public async Task<IActionResult> GetYearList()
        {
            return Ok(await _iStaticDataRepository.YearList());
        }

        [HttpGet]
        [Route("colorcodes")]
        public IActionResult GetColorList()
        {
            return Ok( _iStaticDataRepository.ColorList());
        }


        [HttpGet]
        [Route("getprovidertranstypes/{providerid}")]
        public async Task<IActionResult> GetProvidersTranstypeList(long providerid)
        {
            return Ok(await _iStaticDataRepository.GetTransactionTypeByProviderId(providerid));
        }



        [HttpGet]
        [Route("templatetypes")]
        public async Task<IActionResult> GetTemplateTypeList()
        {
            return Ok(await _iStaticDataRepository.GetTemplateTypeList());
        }

        [HttpGet]
        [Route("templatetag")]
        public async Task<IActionResult> GetTemaplteTag()
        {
            return Ok(await _iStaticDataRepository.GetTemaplteTagList());
        }

        [HttpGet]
        [Route("loggedinuser")]
        public async Task<IActionResult> GetLoggedInUserDetail()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iStaticDataRepository.GetLoggedInUserDetail(Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("employeelist/{name}")]
        public async Task<IActionResult> GetEmployeeListByName(string name)
        {
            AutocompleteAC autocomplete = new AutocompleteAC();
            autocomplete.data = await _iStaticDataRepository.GetEmployeeListByName(name);
            return Ok(autocomplete);
        }

        [HttpGet]
        [Route("employeelistforlinemanager/{name}")]
        public async Task<IActionResult> GetLineManagerEmployeeListByName(string name)
        {
            AutocompleteAC autocomplete = new AutocompleteAC();
            autocomplete.data = await _iStaticDataRepository.GetLineManagerEmployeeListByName(name);
            return Ok(autocomplete);
        }

        [HttpGet]
        [Route("numberlist/{number}/{id}")]
        public async Task<IActionResult> GetNumberList(string number, long id)
        {
            AutocompleteNumberAC autocomplete = new AutocompleteNumberAC();
            autocomplete.data = await _iStaticDataRepository.GetNumberList(number, id);
            return Ok(autocomplete);
        }

        [HttpGet]
        [Route("package/list")]
        public async Task<IActionResult> GetPackageList()
        {
            return Ok(await _iStaticDataRepository.GetPackageList());
        }

        [HttpGet]
        [Route("linstatus/list")]
        public async Task<IActionResult> GetLineStatusList()
        {
            return Ok(await _iStaticDataRepository.GetLineStatusList());
        }


        [HttpGet]
        [Route("calltype")]
        public async Task<IActionResult> GetCallTypeList()
        {
            return Ok(await _iStaticDataRepository.GetCallTypeList());
        }

        [HttpGet]
        [Route("formattype")]
        public IActionResult GetFormatFieldList()
        {
            return Ok(_iStaticDataRepository.GetFormatList());
        }

        [HttpGet]
        [Route("transactiontypes/{providerid}")]
        public async Task<IActionResult> GetTransactionTypeByProviderId(long providerId)
        {
            return Ok(await _iStaticDataRepository.GetTransactionTypeByProviderId(providerId));
        }

        [HttpGet]
        [Route("servicetypelist/{providerid}")]
        public async Task<IActionResult> GetServiceTypes(long providerid)
        {
            return Ok(await _iStaticDataRepository.ServiceTypeListByAssigneTypeId(providerid));
        }


        [HttpGet]
        [Route("packagelist/{serviceTypeId}")]
        public async Task<IActionResult> GetServicePackageList(long serviceTypeId)
        {
            return Ok(await _iStaticDataRepository.GetServicePackageListByServiceTypeId(serviceTypeId));
        }


        [HttpGet]
        [Route("packagesbyprovider/{serviceTypeId}/{providerId}")]
        public async Task<IActionResult> GetServicePackagesByProvider(long serviceTypeId, long providerId)
        {
            return Ok(await _iStaticDataRepository.GetServicePackagesByProvider(serviceTypeId, providerId));
        }


        [HttpGet]
        [Route("billstatuslist")]
        public async Task<IActionResult> GetBillStatusList()
        {
            return Ok(await _iStaticDataRepository.GetBillStatusList());
        }

        [HttpGet]
        [Route("internetdevices")]
        public async Task<IActionResult> GetInternetDevices()
        {
            return Ok(await _iStaticDataRepository.GetInternetDevices());
        }

        #region Account Bills
        [HttpGet]
        [Route("bill-list")]
        public async Task<IActionResult> AccountBills()
        {
            return Ok(await _iStaticDataRepository.GetAccountBills());
        }

        [HttpGet]
        [Route("notifications")]
        public IActionResult GetNotifications()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(_iStaticDataRepository.GetNotifications(Convert.ToInt64(userId)));
        }

        [HttpPost]
        [Route("readallnotification")]
        public async Task<IActionResult> ReadAllNotification()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iStaticDataRepository.ReadAllNotification(Convert.ToInt64(userId)));
        }

        #endregion

        #endregion
    }

}