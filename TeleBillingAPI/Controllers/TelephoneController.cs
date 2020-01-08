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
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Repository.Telephone;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TelephoneController : ControllerBase
    {

        #region "Private Variable(s)"
        private readonly ITelephoneRepository _iTelephoneRepository;
        private readonly IBillUploadRepository _iBillUploadRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion

        #region "Constructor"
        public TelephoneController(ITelephoneRepository iTelephoneRepository, IBillUploadRepository iBillUploadRepository, IHostingEnvironment hostingEnvironment)
        {
            _iTelephoneRepository = iTelephoneRepository;
            _hostingEnvironment = hostingEnvironment;
            _iBillUploadRepository = iBillUploadRepository;
        }
        #endregion

        #region "Public Method(s)"

        #region Telphone Management

        [HttpPost]
        [Route("list")]
        public async Task<IActionResult> GetTelephoneList([FromBody]JqueryDataTablesParameters param)
        {
            var results = await _iTelephoneRepository.GetTelephoneList(param);
            return new JsonResult(new JqueryDataTablesResult<TelephoneAC>
            {
                Draw = param.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }


        [HttpPost]
        [Route("exporttelphonelist")]
        public IActionResult ExportTelephoneList()
        {

            var results = _iTelephoneRepository.GetTelephoneExportList();
            string fileName = "TelePhoneList.xlsx";
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
                var workSheet = package.Workbook.Worksheets.Add("TelePhoneList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }


        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddTelephone(TelephoneDetailAC telephoneDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.AddTelephone(Convert.ToInt64(userId), telephoneDetailAC, fullname));
        }

        [HttpPut]
        [Route("edit")]
        public async Task<IActionResult> EditTelephone(TelephoneDetailAC telephoneDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.UpdateTelephone(Convert.ToInt64(userId), telephoneDetailAC, fullname));
        }

        [HttpGet]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteTelephone(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.DeleteTelphone(Convert.ToInt64(userId), id, fullname));
        }


        [HttpGet]
        [Route("changestatus/{id}")]
        public async Task<IActionResult> ChangeTelephoneStatus(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.ChangeTelephoneStatus(Convert.ToInt64(userId), id, fullname));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetTelephone(long id)
        {
            return Ok(await _iTelephoneRepository.GetTelephoneById(id));
        }
        #endregion


        #region AssignTelephone Management

        [HttpPost]
        [Route("assignedtelephone/list")]
        public IActionResult GetAssignedTelephoneList([FromBody]JqueryDataTablesParameters param)
        {
            var results = _iTelephoneRepository.GetAssignedTelephoneList(param);
            return new JsonResult(new JqueryDataTablesResult<AssignTelePhoneAC>
            {
                Draw = param.Draw,
                Data = results.Items,
                RecordsFiltered = results.TotalSize,
                RecordsTotal = results.TotalSize
            });
        }



        [HttpPost]
        [Route("exportassignedtelephonelist")]
        public IActionResult ExportAssignedTelephoneList()
        {
            var results = _iTelephoneRepository.GetAssignedTelephoneExportList();
            string fileName = "AssignedTelephoneList.xlsx";
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
                var workSheet = package.Workbook.Worksheets.Add("AssignedTelephoneList");
                workSheet.Cells.LoadFromCollection(results, true);
                package.Save();
            }
            return Ok();
        }

        [HttpGet]
        [Route("telephonepackagedetils/list/{id}")]
        public async Task<IActionResult> GetAssignedTelephonePackageList(long id)
        {
            return Ok(await _iTelephoneRepository.GetAssignedTelephonePackageList(id));
        }

        [HttpGet]
        [Route("deleteassignedtelephone/{id}")]
        public async Task<IActionResult> DeleteAssignedTelephone(long id)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.DeleteAssignedTelephone(Convert.ToInt64(userId), id, fullname));
        }

        [HttpPost]
        [Route("assignedtelephone/add")]
        public async Task<IActionResult> AddAssignedTelephone(AssignTelephoneDetailAC assignTelephoneDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.AddAssignedTelephone(Convert.ToInt64(userId), assignTelephoneDetailAC, fullname));
        }

        [HttpPut]
        [Route("assignedtelephone/edit")]
        public async Task<IActionResult> EditAssignedTelephone(AssignTelephoneDetailAC assignTelephoneDetailAC)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.UpdateAssignedTelephone(Convert.ToInt64(userId), assignTelephoneDetailAC, fullname));
        }

        [HttpGet]
        [Route("assignedtelephone/{id}")]
        public async Task<IActionResult> GetAssignedTelephone(long id)
        {
            return Ok(await _iTelephoneRepository.GetAssignedTelephoneById(id));
        }

        [HttpPost]
        [Route("bulkassgintelephone")]
        public async Task<IActionResult> BulkAssginTelePhone()
        {
            ExcelFileAC excelFileAC = new ExcelFileAC();
            IFormFile file = Request.Form.Files[0];
            excelFileAC.File = file;
            excelFileAC.FolderName = "TempUpload";
            ExcelUploadResponseAC exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            return Ok(await _iTelephoneRepository.UploadBulkAssignTelePhone(Convert.ToInt64(userId), exceluploadDetail, fullname));
        }
        #endregion


        #endregion

    }
}