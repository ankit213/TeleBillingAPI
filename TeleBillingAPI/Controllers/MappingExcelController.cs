using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iExcelMappingRepository.DeleteExcelMapping(Convert.ToInt64(userId), id, fullname));
		}


		[HttpPost]
		[Route("checkexcelmappingexistsforservices")]
		public async Task<IActionResult> CheckExcelMappingExists(ExcelMappingAC excelMappingAC)
		{
			bool isExists = false;
			ResponseAC responeAC = new ResponseAC();
			if (excelMappingAC != null || (excelMappingAC.ProviderId > 0 && excelMappingAC.ServiceTypeId > 0))
			{
				isExists = await _iExcelMappingRepository.checkExcelMappingExistsForServices(excelMappingAC);

				if (isExists)
				{
					responeAC.Message = "excel mapping is already exists";
					responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
					return Ok(responeAC);
				}
			}

			responeAC.Message = "excel mapping is valid";
			responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Success);
			return Ok(responeAC);

		}


		[HttpGet]
		[Route("checkexcelmappingexists/{providerid}/{servicetypeid}")]
		public async Task<IActionResult> CheckExcelMappingExists(long providerid, long servicetypeid)
		{
			bool isExists = false;
			ResponseAC responeAC = new ResponseAC();
			if (providerid > 0 && servicetypeid > 0)
			{
				isExists = await _iExcelMappingRepository.checkExcelMappingExists(providerid, servicetypeid);

				if (isExists)
				{
					responeAC.Message = "excel mapping is already exists";
					responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
					return Ok(responeAC);
				}
			}

			responeAC.Message = "excel mapping is valid";
			responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Success);
			return Ok(responeAC);

		}


		[HttpPost]
		[Route("excelmapping/add")]
		public async Task<IActionResult> AddExcelMapping(ExcelMappingAC excelMappingAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iExcelMappingRepository.AddExcelMapping(excelMappingAC, Convert.ToInt64(userId), fullname));
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
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iExcelMappingRepository.EditExcelMapping(excelMappingAC, Convert.ToInt64(userId), fullname));
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
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iExcelMappingRepository.DeletePbxExcelMapping(Convert.ToInt64(userId), id, fullname));
		}

		[HttpPost]
		[Route("pbxexcelmapping/add")]
		public async Task<IActionResult> AddPbxExcelMapping(PbxExcelMappingAC excelMappingAC)
		{
			string userId = "2";//  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iExcelMappingRepository.AddPbxExcelMapping(excelMappingAC, Convert.ToInt64(userId), fullname));
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
			string userId = "2";//  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iExcelMappingRepository.EditPbxExcelMapping(excelMappingAC, Convert.ToInt64(userId), fullname));
		}

		#endregion
	}
}