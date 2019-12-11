using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Employee;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class EmployeeController : Controller
	{
		#region Private Variable(s)"

		private readonly IEmployeeRepository _iEmployeeRepository;
		private readonly ILogManagement _iLogManagement;
		private readonly IHostingEnvironment _hostingEnvironment;
		#endregion

		#region Constructor
		public EmployeeController(IEmployeeRepository iEmployeeRepository, ILogManagement ilogManagement, IHostingEnvironment hostingEnvironment)
		{
			_iLogManagement = ilogManagement;
			_iEmployeeRepository = iEmployeeRepository;
			_hostingEnvironment = hostingEnvironment;
		}
		#endregion

		#region "Public Method(s)"

		[HttpGet]
		[Route("getprofile")]
		public async Task<IActionResult> GetUserProfile()
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iEmployeeRepository.GetUserProfile(Convert.ToInt64(userId)));
		}

		[HttpPost]
		[Route("resetpassword")]
		public async Task<IActionResult> ResetPassword(EmployeeProfileDetailAC employeeProfile)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iEmployeeRepository.resetPassword(employeeProfile, Convert.ToInt64(userId)));
		}

		[HttpPost]
		[Route("getemployeelist")]
		public IActionResult GetEmployeeList([FromBody]JqueryDataTablesParameters param)
		{
			var results = _iEmployeeRepository.GetEmployeeList(param);
			return new JsonResult(new JqueryDataTablesResult<EmployeeProfileDetailAC>
			{
				Draw = param.Draw,
				Data = results.Items,
				RecordsFiltered = results.TotalSize,
				RecordsTotal = results.TotalSize
			});
		}


		[HttpPost]
		[Route("exportemployeelist")]
		public IActionResult ExportEmployeeList()
		{

			var results = _iEmployeeRepository.GetExportEmployeeList();
			string fileName = "EmployeeList.xlsx";
			string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "TempUploadTelePhone");
			string filePath = Path.Combine(folderPath, fileName);
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}
			FileInfo file = new FileInfo(Path.Combine(folderPath, fileName));
			using (var package = new ExcelPackage(file))
			{
				var workSheet = package.Workbook.Worksheets.Add("EmployeeList");
				workSheet.Cells.LoadFromCollection(results, true);
				package.Save();
			}
			return Ok();
		}

		[HttpGet]
		[Route("get/{id}")]
		public async Task<IActionResult> GetEmployee(long id)
		{
			return Ok(await _iEmployeeRepository.GetEmployeeById(id));
		}

		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddEmployee(MstEmployeeAC mstEmployeeAc)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iEmployeeRepository.AddEmployee(mstEmployeeAc, Convert.ToInt64(userId), fullname));
		}


		[HttpPost]
		[Route("edit")]
		public async Task<IActionResult> EditEmployee(MstEmployeeAC mstEmployeeAc)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iEmployeeRepository.EditEmployee(mstEmployeeAc, Convert.ToInt64(userId), fullname));
		}

		[HttpGet]
		[Route("delete/{id}")]
		public async Task<IActionResult> DeleteEmployee(long id)
		{
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iEmployeeRepository.DeleteEmployee(id, Convert.ToInt64(userId), fullname));
		}

		[HttpGet]
		[Route("changestatus/{id}")]
		public async Task<IActionResult> ChangeEmployeeStatus(long id)
		{
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iEmployeeRepository.ChangeEmployeeStatus(id, Convert.ToInt64(userId), fullname));
		}


		[HttpPost]
		[Route("check")]
		public async Task<IActionResult> CheckEmployee(MstEmployeeAC mstEmployeeAc)
		{
			bool isValid = true;
			ResponseAC responeAC = new ResponseAC();
			if (mstEmployeeAc.EmailId != null && mstEmployeeAc.EmailId.Length > 0)
			{
				isValid = await _iEmployeeRepository.checkEmailUnique(mstEmployeeAc.EmailId, mstEmployeeAc.UserId);
				if (!isValid)
				{
					responeAC.Message = "Email is already exists!";
					responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
					return Ok(responeAC);
				}
			}

			if (mstEmployeeAc.EmpPFNumber != null && mstEmployeeAc.EmpPFNumber.Length > 0)
			{
				isValid = await _iEmployeeRepository.checkPFNumberUnique(mstEmployeeAc.EmpPFNumber, mstEmployeeAc.UserId);
				if (isValid)
				{
					responeAC.Message = "PFNumber is already exists!";
					responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
					return Ok(responeAC);
				}
			}
			else
			{
				responeAC.Message = "Employee details not found.";
				responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
				return Ok(responeAC);
			}


			responeAC.Message = "Employee is Valid";
			responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Success);
			return Ok(responeAC);


		}

		[HttpGet]
		[Route("checkpfnumber/{emppfnumber}/{id}")]
		public async Task<IActionResult> CheckPFNumber(string empPFNumber, long id = 0)
		{
			ResponseAC responeAC = new ResponseAC();
			if (empPFNumber != null && empPFNumber.Length > 0)
			{
				if (await _iEmployeeRepository.checkPFNumberUnique(empPFNumber, id))
				{
					responeAC.Message = "PFNumber is already exists!";
					responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
					return Ok(responeAC);
				}
			}
			responeAC.Message = "PFNumber is valid";
			responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Success);
			return Ok(responeAC);
		}

		[HttpGet]
		[Route("checkemailid/{emailid}/{id}")]
		public async Task<IActionResult> Checkemailid(string emailid, long id = 0)
		{
			bool isValid = true;
			ResponseAC responeAC = new ResponseAC();
			if (emailid != null && emailid.Length > 0)
			{
				isValid = await _iEmployeeRepository.checkEmailUnique(emailid, id);

				if (!isValid)
				{
					responeAC.Message = "Email is already exists!";
					responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Error);
					return Ok(responeAC);
				}
			}

			responeAC.Message = "Email is valid";
			responeAC.StatusCode = Convert.ToInt16(TeleBillingUtility.Helpers.Enums.EnumList.ResponseType.Success);
			return Ok(responeAC);


		}


		[HttpGet]
		[Route("logout")]
		public async Task<IActionResult> LogOut() {
			string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iEmployeeRepository.LogOutUser(Convert.ToInt64(userId), fullname));
		}

		#endregion
	}
}