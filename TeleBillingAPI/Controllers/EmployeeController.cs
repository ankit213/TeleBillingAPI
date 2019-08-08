using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingUtility.Helpers;
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
        #endregion

        #region Constructor
        public EmployeeController(IEmployeeRepository iEmployeeRepository, ILogManagement ilogManagement)
        {
            _iLogManagement = ilogManagement;
            _iEmployeeRepository = iEmployeeRepository;
        }
        #endregion

        #region "Public Method(s)"

        [HttpGet]
        [Route("getprofile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var currentUser = HttpContext.User;
            string userId =  currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok( await _iEmployeeRepository.GetUserProfile(Convert.ToInt64(userId)));
        }

        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword(EmployeeProfileDetailAC employeeProfile)
        {
            var currentUser = HttpContext.User;
            string userId =  currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iEmployeeRepository.resetPassword(employeeProfile,Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("getemployeelist")]
        public  IActionResult GetEmployeeList()
        {
            return Ok( _iEmployeeRepository.GetEmployeeList());
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
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iEmployeeRepository.AddEmployee(mstEmployeeAc, Convert.ToInt64(userId)));
        }


        [HttpPost]
        [Route("edit")]
        public async Task<IActionResult> EditEmployee(MstEmployeeAC mstEmployeeAc)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iEmployeeRepository.EditEmployee(mstEmployeeAc, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteEmployee(long id)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iEmployeeRepository.DeleteEmployee(id, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("changestatus/{id}")]
        public async Task<IActionResult> ChangeEmployeeStatus(long id)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iEmployeeRepository.ChangeEmployeeStatus(id, Convert.ToInt64(userId)));
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

            if (mstEmployeeAc.EmpPFNumber!=null && mstEmployeeAc.EmpPFNumber.Length > 0)
            {
                isValid = await _iEmployeeRepository.checkPFNumberUnique(mstEmployeeAc.EmpPFNumber,mstEmployeeAc.UserId);

                if (!isValid)
                {
                    responeAC.Message = "PFNUmber is already exists!";
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
        public async Task<IActionResult> CheckPFNumber(string empPFNumber,long id=0)
        {
            bool isValid = true;
            ResponseAC responeAC = new ResponseAC();
            if (empPFNumber != null && empPFNumber.Length > 0)
            {
                isValid = await _iEmployeeRepository.checkPFNumberUnique(empPFNumber, id);

                if (!isValid)
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

        #endregion
    }
}