
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.BillDelegate;
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Repository.Employee;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class BillController : Controller
    {

        #region "Private Variable(s)"
        private readonly IBillUploadRepository _iBillUploadRepository;
        private readonly IBillDelegateRepository _ibillDelegateRepository;
        private readonly ILogManagement _iLogManagement;
        private readonly Logger _logger = LogManager.GetLogger("logger");
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion

        #region "Constructor"
        public BillController(IBillUploadRepository iBillUploadRepository, ILogManagement ilogManagement
            ,IBillDelegateRepository ibillDelegateRepository
            , IEmployeeRepository iEmployeeRepository
            ,IHostingEnvironment hostingEnvironment
            )
        {
            _iBillUploadRepository = iBillUploadRepository;
            _iLogManagement = ilogManagement;
            _ibillDelegateRepository = ibillDelegateRepository;
            _hostingEnvironment = hostingEnvironment;

        }
        #endregion

        #region "Public Method(s)"



        [HttpGet]
        [Route("billuploadedlist")]
        public async Task<IActionResult> GetBillUplaodedList()
        {
            //string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            //string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            //LogManager.Configuration.Variables["user"] = fullname + '('+ userId + ')';
            //LogManager.Configuration.Variables["stepno"] = "0-list";
            //_logger.Info("Request bill uplaoded list");

            return Ok(await _iBillUploadRepository.GetBillUploadedList());
        }

        [HttpGet]
        [Route("pbxbilluploadedlist")]
        public async Task<IActionResult> GetPbxBillUplaodedList()
        {
            return Ok(await _iBillUploadRepository.GetPbxBillUploadedList());
        }


        [HttpGet]
        [Route("deleteupload/{id}")]
        public async Task<IActionResult> DeleteExcelUpload(long id)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillUploadRepository.DeleteExcelUplaod(Convert.ToInt64(userId), id, fullname));
        }

        [HttpGet]
        [Route("approveupload/{id}")]
        public async Task<IActionResult> ApproveExcelUpload(long id)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillUploadRepository.ApproveExcelUploadLog(Convert.ToInt64(userId), id, fullname));
        }

        [HttpGet]
        [Route("approveuploadpbx/{id}")]
        public async Task<IActionResult> ApproveExcelUploadPbx(long id)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillUploadRepository.ApproveExcelUploadPbxLog(Convert.ToInt64(userId), id, fullname));
        }

        [HttpGet]
        [Route("checkisbillallocated/{id}")]
        public IActionResult CheckIsBillAllocated(long id)
        {
            return Ok( _iBillUploadRepository.CheckIsBillAllocated(id));
        }

        [HttpGet]
        [Route("deletepbxupload/{id}")]
        public async Task<IActionResult> DeletePbxExcelUpload(long id)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillUploadRepository.DeletePbxExcelUplaod(Convert.ToInt64(userId), id, fullname));
        }


        [HttpGet]
        [Route("checkBillExists/{providerid}/{month}/{year}")]
        public async Task<IActionResult> ChekBillMergeId(long providerid, int month, int year)
        {
            return Ok(await _iBillUploadRepository.ChekBillMergeId(providerid, month, year));
        }

        [HttpGet]
        [Route("checkBillExistspbx/{deviceid}/{month}/{year}")]
        public async Task<IActionResult> ChekBillMergeIdPbx(long deviceid, int month, int year)
        {
            return Ok(await _iBillUploadRepository.ChekBillMergeIdPbx(deviceid, month, year));
        }


        [HttpPost]
        [Route("checkuploadbillmerge")]
        public async Task<IActionResult> CheckUploadBillMerge([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        {
            long ExcelBilluploadId = 0;
            try
            {
                ResponseDataIdAC responedefault = new ResponseDataIdAC();

                BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);

                if (billUploadModel != null)
                {
                    if (billUploadModel.ServiceTypes.Count() > 0)
                    {
                        ExcelBilluploadId = await _iBillUploadRepository.CheckBillCanMerged(billUploadModel);
                    }

                    if (ExcelBilluploadId > 0)
                    {
                        responedefault.Message = "We found same bill is already uploaded for selected criteria. Do you want to merge it?";
                        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        responedefault.Id = ExcelBilluploadId;
                        return Ok(responedefault);
                    }
                    else if (ExcelBilluploadId == -1)
                    {
                        responedefault.Message = "Sorry ! We found same bill is already uploaded and approved for selected criteria.";
                        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        responedefault.Id = -1;
                        return Ok(responedefault);
                    }
                    else
                    {
                        responedefault.Message = "No need to merge.";
                        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        responedefault.Id = ExcelBilluploadId;
                        return Ok(responedefault);

                    }
                }

                
                responedefault.Message = "Service function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                responedefault.Id = ExcelBilluploadId;
                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseDataIdAC responeAC = new ResponseDataIdAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                responeAC.Id = ExcelBilluploadId;
                return Ok(responeAC);
            }
        }


        [HttpPost]
        [Route("checkuploadbillpbxmerge")]
        public async Task<IActionResult> CheckUploadBillPBXMerge([FromForm]PbxBillUploadFormDataAC billUploadFormDataAC)
        {
            long ExcelBilluploadId = 0;
            try
            {
                ResponseDataIdAC responedefault = new ResponseDataIdAC();

                PbxBillUploadAC billUploadModel = JsonConvert.DeserializeObject<PbxBillUploadAC>(billUploadFormDataAC.PbxBillUploadAc);

                if (billUploadModel != null)
                {
                    if (billUploadModel.DeviceId > 0)
                    {
                        ExcelBilluploadId = await _iBillUploadRepository.CheckPBXBillCanMerged(billUploadModel);
                    }

                    if (ExcelBilluploadId > 0)
                    {
                        responedefault.Message = "We found same bill is already uploaded for selected criteria. Do you want to merge it?";
                        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        responedefault.Id = ExcelBilluploadId;
                        return Ok(responedefault);
                    }
                    else
                    {
                        responedefault.Message = "No need to merge.";
                        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        responedefault.Id = ExcelBilluploadId;
                        return Ok(responedefault);

                    }
                }


                responedefault.Message = "Device function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                responedefault.Id = ExcelBilluploadId;
                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseDataIdAC responeAC = new ResponseDataIdAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                responeAC.Id = ExcelBilluploadId;
                return Ok(responeAC);
            }
        }



        [HttpPost]
        [Route("uploadnewbill")]
        public async Task<IActionResult> UploadNewBill([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        {
            long before_excelUploadLogId = 0;
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
            LogManager.Configuration.Variables["user"] = fullname + '(' + userId + ')';
            LogManager.Configuration.Variables["stepno"] = "1";
            _logger.Info("Start : Upload New Bill ---" + JsonConvert.SerializeObject(billUploadFormDataAC.BillUploadAc));

            long TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
            string logDescription = string.Empty;          
            string ServiceSelected = String.Empty;
            bool IsSkypeData = false;
            bool iserrorDataSaved = false;
            string FileNameGuid = string.Empty;
            try
            {
                SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();
               

                if (billUploadFormDataAC.File==null)
                {
                    #region --> Please select file if null.
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                    responeAC.Message = "Please select file.";
                    responeAC.TotalValidCount = "0";
                    responeAC.TotalAmount = "0";
                    return Ok(responeAC);
                    #endregion
                }

                BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);



                var file = billUploadFormDataAC.File;
                ExcelFileAC excelFileAC = new ExcelFileAC();
                ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
                List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();
             

                #region --> Get Excel mapping details

                if (billUploadModel != null) 
                {
                    mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
                    LogManager.Configuration.Variables["user"] = fullname + '(' + userId + ')';
                    LogManager.Configuration.Variables["stepno"] = "2";
                    _logger.Info("Get excel mapping");

                    if (billUploadModel.ServiceTypes.Count() > 0)
                    {
                        ServiceSelected = string.Join(",", billUploadModel.ServiceTypes.Select(x=>x.Name).ToArray());
                    }
                   
                }

                if (mappingExcellist != null)
                {
                    // Check Mapping is exists for all selected Services

                    if (billUploadModel.ServiceTypes.Count() != mappingExcellist.Count())
                    {
                        #region --> if mapping is missing for selected services
                        responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                        responeAC.Message = "Mapping details does not exist!"; 
                        responeAC.TotalValidCount = "0";
                        responeAC.TotalAmount = "0";
                        return Ok(responeAC);
                        #endregion
                    }
                }
                #endregion                   
                               
                #region --> Save File to temp Folder                
                if (file != null)
                {
                    string folderName = "TempUpload";
                    excelFileAC.File = file;
                    excelFileAC.FolderName = folderName;
                    billUploadModel.ExcelFileName1 = file.FileName;
                    exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
                    if (exceluploadDetail != null)
                    {
                        logDescription = " File Name :" + exceluploadDetail.FileName + " , FileNameGuid : " + exceluploadDetail.FileNameGuid + " , ";
                    }

                    LogManager.Configuration.Variables["user"] = fullname + '(' + userId + ')';
                    LogManager.Configuration.Variables["stepno"] = "3";
                    _logger.Info("File Saved : " + logDescription);

                    FileNameGuid = exceluploadDetail.FileNameGuid;
                    responeAC.FileGuidNo = FileNameGuid;
                }
                #endregion

                #region --> File EXtension , WorkSheet No Check  || 17/10/2019
                if (file != null)
                {
                    IFormFile FileData = file;
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Split(".")[1].Trim('"');
                    if (!string.IsNullOrEmpty(fileExtension.ToLower()))
                    {
                        if (fileExtension.ToLower() != "xls" && fileExtension.ToLower() != "xlsx" && fileExtension.ToLower() != "csv")
                        {
                            #region --> file format is not valid
                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                            responeAC.Message = "File format not valid!";
                            responeAC.TotalValidCount = "0";
                            responeAC.TotalAmount = "0";
                            return Ok(responeAC);
                            #endregion
                        }

                        if (mappingExcellist != null)
                        {
                            long MaxWorkSheetNo = mappingExcellist.Max(x => x.WorkSheetNo);
                            if (MaxWorkSheetNo > 1)
                            {
                                if (fileExtension.ToLower() == "xls" || fileExtension.ToLower() == "xlsx")
                                {
                                    responeAC = await _iBillUploadRepository.CheckMappingWithFileFormat(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, MaxWorkSheetNo);
                                     if(responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.NoDataFound)
                                      {
                                        return Ok(responeAC);
                                      }                                
                                }
                            }
                        }
                    }
                }
                #endregion


                ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
                // Added On 03/10/2019
                ImportBillDetailAC<StaticIPUploadListAC> uploadDetailListStaticIP = new ImportBillDetailAC<StaticIPUploadListAC>();
                ImportBillDetailAC<VoiceOnlyUploadListAC> uploadDetailListVoiceOnly = new ImportBillDetailAC<VoiceOnlyUploadListAC>();
                //----------------------

                ImportBillDetailAC<VoipUploadListAC> uploadDetailListVoip = new ImportBillDetailAC<VoipUploadListAC>();

                ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
                ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListInternetservice = new ImportBillDetailAC<InternetServiceUploadListAC>();
                ImportBillDetailAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenter = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();
                ImportBillDetailAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHosting = new ImportBillDetailAC<ManagedHostingServiceUploadListAC>();


                ImportBillDetailMultipleAC<MobilityUploadListAC> uploadDetailListM = new ImportBillDetailMultipleAC<MobilityUploadListAC>();
                // Added On 03/10/2019
                ImportBillDetailMultipleAC<StaticIPUploadListAC> uploadDetailListStaticIPM = new ImportBillDetailMultipleAC<StaticIPUploadListAC>();
                ImportBillDetailMultipleAC<VoiceOnlyUploadListAC> uploadDetailListVoiceOnlyM = new ImportBillDetailMultipleAC<VoiceOnlyUploadListAC>();
                //----------------------

                ImportBillDetailMultipleAC<MadaUploadListAC> uploadDetailListMadaM = new ImportBillDetailMultipleAC<MadaUploadListAC>();
                ImportBillDetailMultipleAC<InternetServiceUploadListAC> uploadDetailListInternetserviceM = new ImportBillDetailMultipleAC<InternetServiceUploadListAC>();
                ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenterM = new ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>();
                ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHostingM = new ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>();

                ImportBillDetailMultipleAC<VoipUploadListAC> uploadDetailListVoipM = new ImportBillDetailMultipleAC<VoipUploadListAC>();

                AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();
                

                if (mappingExcellist != null)
                {
                    LogManager.Configuration.Variables["user"] = fullname + '(' + userId + ')';
                    LogManager.Configuration.Variables["stepno"] = "4";
                    _logger.Info("Find service based on mapping");

                    #region ---> Add New code fpr optimization  | Save file log before detail upload | ADD | 29 Nov 2019
                     before_excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));
                    #endregion


                    if (mappingExcellist.Count() == 1)
                    {
                        #region Read Data Service wise one by one for single service selected at once
                        foreach (var mapservice in mappingExcellist)
                        {
                            MappingDetailAC mappingDetail = new MappingDetailAC();
                            if ( mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility
                                || mapservice.ServiceTypeId == (long)EnumList.ServiceType.StaticIP
                                || mapservice.ServiceTypeId == (long)EnumList.ServiceType.VoiceOnly
                                || mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetPlanDeviceOffer
                               )
                            {
                                #region --> Read Excel file for mobility                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == mapservice.ServiceTypeId);
                                ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailList = mobilityList;
                                responeAC.mode = 1;

                                #region --> Handel If Exception Found
                                if (mobilityList.Status==(long)EnumList.ExcelUploadResponseType.ExceptionError)
                                {                                

                                    #region Transaction Log Entry 
                                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + mobilityList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = mobilityList.Message;
                                    responeAC.TotalValidCount = "0";
                                    responeAC.TotalAmount = "0";
                                    return Ok(responeAC);
                                 
                                }

                                #endregion

                                                               
                                if (mobilityList.Status == (long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
                                {

                                    #region Transaction Log Entry 
                                    logDescription += " Service : "+ ServiceSelected + " , Status Code : " 
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.MultipleServiceFound) 
                                                        + " , Respnse Message : " + mobilityList.Message + ".";

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion


                                    responeAC.Message = mobilityList.Message;
                                    responeAC.StatusCode = (long)EnumList.ExcelUploadResponseType.MultipleServiceFound;
                                    return Ok(responeAC);
                                }
                            
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
                                allserviceTypeData.InvalidList1.AddRange(mobilityList.UploadData.Data.InvalidMobilityList);
                                allserviceTypeData.InvalidListAllDB.AddRange(mobilityList.UploadData.Data.InvalidListAllDB);
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.VOIP)
                            {
                                #region --> Read Excel file for Voip                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.VOIP);
                                ImportBillDetailAC<VoipUploadListAC> voipList = await _iBillUploadRepository.ReadExcelForVoip(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListVoip = voipList;
                                responeAC.mode = 7;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidListSkype.AddRange(voipList.UploadData.Data.ValidVoipList);
                                allserviceTypeData.InvalidList7.AddRange(voipList.UploadData.Data.InvalidVoipList);
                                #endregion
                            }
                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
                                ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListMada = madaserviceList;
                                responeAC.mode = 9;
                                #endregion

                                #region --> Handel If Exception Found
                                if (madaserviceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                {

                                    #region Transaction Log Entry 
                                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + madaserviceList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = madaserviceList.Message;
                                    responeAC.TotalValidCount = "0";
                                    responeAC.TotalAmount = "0";
                                    return Ok(responeAC);

                                }

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
                                allserviceTypeData.InvalidListAllDB.AddRange(madaserviceList.UploadData.Data.InvalidListAllDB);
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
                                ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListInternetservice = internetserviceList;
                                responeAC.mode = 345;
                                #endregion

                                #region --> Handel If Exception Found
                                if (internetserviceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                {

                                    #region Transaction Log Entry 
                                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + internetserviceList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = internetserviceList.Message;
                                    responeAC.TotalValidCount = "0";
                                    responeAC.TotalAmount = "0";
                                    return Ok(responeAC);

                                }

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
                                allserviceTypeData.InvalidListAllDB.AddRange(internetserviceList.UploadData.Data.InvalidListAllDB);
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility);
                                ImportBillDetailAC<DataCenterFacilityUploadListAC> dataCenterserviceList = await _iBillUploadRepository.ReadExcelForDataCenterFacility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListDataCenter = dataCenterserviceList;
                                responeAC.mode = 345;
                                #endregion

                                #region --> Handel If Exception Found
                                if (dataCenterserviceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                {

                                    #region Transaction Log Entry 
                                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + dataCenterserviceList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = dataCenterserviceList.Message;
                                    responeAC.TotalValidCount = "0";
                                    responeAC.TotalAmount = "0";
                                    return Ok(responeAC);

                                }

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(dataCenterserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList4 = dataCenterserviceList.UploadData.Data.InvalidList;
                                allserviceTypeData.InvalidListAllDB.AddRange(dataCenterserviceList.UploadData.Data.InvalidListAllDB);
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.ManagedHostingService)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.ManagedHostingService);
                                ImportBillDetailAC<ManagedHostingServiceUploadListAC> managedHostingServiceList = await _iBillUploadRepository.ReadExcelForManagedHostingService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListManagedHosting = managedHostingServiceList;
                                responeAC.mode = 345;
                                #endregion

                                #region --> Handel If Exception Found
                                if (managedHostingServiceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                {

                                    #region Transaction Log Entry 
                                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + managedHostingServiceList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = managedHostingServiceList.Message;
                                    responeAC.TotalValidCount = "0";
                                    responeAC.TotalAmount = "0";
                                    return Ok(responeAC);

                                }

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(managedHostingServiceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList5 = managedHostingServiceList.UploadData.Data.InvalidList;
                                allserviceTypeData.InvalidListAllDB.AddRange(managedHostingServiceList.UploadData.Data.InvalidListAllDB);
                                #endregion
                            }

                        }
                        #endregion
                    }
                    else if (mappingExcellist.Count() > 1)
                    {
                        #region --> If Multi Service From Mobility,StaticIP,VoiceONly,InternetPlanDeviceOffer

                        // call common function for all 1.
                        List<MappingDetailAC> mappingExcellistWithoutCommon = new List<MappingDetailAC>();


                        mappingExcellistWithoutCommon = mappingExcellist.Where(x => x.IsCommonMapped !=true).ToList();

                        if (mappingExcellistWithoutCommon != null)
                        {
                            if(mappingExcellistWithoutCommon.Count() == 1)
                            {

                                #region Read Data Service wise one by one for single service selected at once
                                foreach (var mapservice in mappingExcellistWithoutCommon)
                                {
                                    MappingDetailAC mappingDetails = new MappingDetailAC();
                                    if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility
                                        || mapservice.ServiceTypeId == (long)EnumList.ServiceType.StaticIP
                                        || mapservice.ServiceTypeId == (long)EnumList.ServiceType.VoiceOnly
                                        || mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetPlanDeviceOffer
                                       )
                                    {
                                        #region --> Read Excel file for mobility                           
                                        mappingDetails = mappingExcellist.Find(x => x.ServiceTypeId == mapservice.ServiceTypeId);
                                        ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetails, billUploadModel);
                                        uploadDetailList = mobilityList;
                                        responeAC.mode = 1;

                                        #region --> Handel If Exception Found
                                        if (mobilityList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                        {

                                            #region Transaction Log Entry 
                                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                + " , Respnse Message : " + mobilityList.Message;

                                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                            #endregion

                                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                            responeAC.Message = mobilityList.Message;
                                            responeAC.TotalValidCount = "0";
                                            responeAC.TotalAmount = "0";
                                            return Ok(responeAC);

                                        }

                                        #endregion

                                        if (mobilityList.Status == (long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
                                        {

                                            #region Transaction Log Entry 
                                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
                                                                + " , Respnse Message : " + mobilityList.Message + ".";

                                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                            #endregion


                                            responeAC.Message = mobilityList.Message;
                                            responeAC.StatusCode = (long)EnumList.ExcelUploadResponseType.MultipleServiceFound;
                                            return Ok(responeAC);
                                        }

                                        #endregion

                                        #region --> store in common class allserviceTypeData
                                        allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
                                        allserviceTypeData.InvalidList1.AddRange(mobilityList.UploadData.Data.InvalidMobilityList);
                                        allserviceTypeData.InvalidListAllDB.AddRange(mobilityList.UploadData.Data.InvalidListAllDB);
                                        #endregion
                                    }


                                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
                                    {
                                        #region --> Read Excel file for mada GeneralService                           
                                        mappingDetails = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
                                        ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetails, billUploadModel);
                                        uploadDetailListMada = madaserviceList;
                                        responeAC.mode = 9;
                                        #endregion

                                        #region --> Handel If Exception Found
                                        if (madaserviceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                        {

                                            #region Transaction Log Entry 
                                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                + " , Respnse Message : " + madaserviceList.Message;

                                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                            #endregion

                                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                            responeAC.Message = madaserviceList.Message;
                                            responeAC.TotalValidCount = "0";
                                            responeAC.TotalAmount = "0";
                                            return Ok(responeAC);

                                        }
                                        #endregion

                                        #region --> store in common class allserviceTypeData
                                        allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
                                        allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
                                        allserviceTypeData.InvalidListAllDB.AddRange(madaserviceList.UploadData.Data.InvalidListAllDB);
                                        #endregion
                                    }

                                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                                    {
                                        #region --> Read Excel file for mada GeneralService                           
                                        mappingDetails = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
                                        ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetails, billUploadModel);
                                        uploadDetailListInternetservice = internetserviceList;
                                        responeAC.mode = 345;
                                        #endregion

                                        #region --> Handel If Exception Found
                                        if (internetserviceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                        {

                                            #region Transaction Log Entry 
                                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                + " , Respnse Message : " + internetserviceList.Message;

                                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                            #endregion

                                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                            responeAC.Message = internetserviceList.Message;
                                            responeAC.TotalValidCount = "0";
                                            responeAC.TotalAmount = "0";
                                            return Ok(responeAC);

                                        }
                                        #endregion


                                        #region --> store in common class allserviceTypeData
                                        allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
                                        allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
                                        allserviceTypeData.InvalidListAllDB.AddRange(internetserviceList.UploadData.Data.InvalidListAllDB);
                                        #endregion
                                    }

                                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
                                    {
                                        #region --> Read Excel file for mada GeneralService                           
                                        mappingDetails = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility);
                                        ImportBillDetailAC<DataCenterFacilityUploadListAC> dataCenterserviceList = await _iBillUploadRepository.ReadExcelForDataCenterFacility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetails, billUploadModel);
                                        uploadDetailListDataCenter = dataCenterserviceList;
                                        responeAC.mode = 345;
                                        #endregion

                                        #region --> Handel If Exception Found
                                        if (dataCenterserviceList.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                        {

                                            #region Transaction Log Entry 
                                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                + " , Respnse Message : " + dataCenterserviceList.Message;

                                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                            #endregion

                                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                            responeAC.Message = dataCenterserviceList.Message;
                                            responeAC.TotalValidCount = "0";
                                            responeAC.TotalAmount = "0";
                                            return Ok(responeAC);

                                        }
                                        #endregion

                                        #region --> store in common class allserviceTypeData
                                        allserviceTypeData.ValidList.AddRange(dataCenterserviceList.UploadData.Data.ValidList);
                                        allserviceTypeData.InvalidList4 = dataCenterserviceList.UploadData.Data.InvalidList;
                                        allserviceTypeData.InvalidListAllDB.AddRange(dataCenterserviceList.UploadData.Data.InvalidListAllDB);
                                        #endregion
                                    }

                                }
                                #endregion
                            }
                        }

                        // call common function for all 1.

                        #endregion

                        if (mappingExcellistWithoutCommon != null)
                        {
                            if (mappingExcellistWithoutCommon.Count()>1)
                            {


                                #region --> Read Data for Multiple service From Same file

                                MappingDetailAC mappingDetail = new MappingDetailAC();

                                int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
                                if (ServiceWithoutTitleCount > 1)
                                {
                                    #region --> if excel have two service without title we can not verify each record 


                                    #region Transaction Log Entry 
                                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.DataInvalid)
                                                        + " , Respnse Message : " + "Multiple service without title is invalid !";

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
                                    responeAC.Message = "Multiple service without title is invalid !";
                                    responeAC.TotalValidCount = "0";
                                    responeAC.TotalAmount = "0";
                                    return Ok(responeAC);
                                    #endregion
                                }
                                else  // if mapping details are accurate than 
                                {

                                    #region --> loop based on the worksheetNo provided by the mapping list

                                    foreach (var worksheet in mappingExcellist.Select(p => p.WorkSheetNo).Distinct().ToList())
                                    {

                                        // Get first Reading Index with service
                                        MultiServiceUploadAC responseIndexService = new MultiServiceUploadAC();
                                        int worksheetNo = (int)worksheet;
                                        int readingIndex = 0;
                                        List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == (int)worksheet && x.IsCommonMapped == false).ToList();
                                        int serviceCountWithTitle = 0;

                                        if (SingleWorksheetservice != null)
                                        {
                                            serviceCountWithTitle = SingleWorksheetservice.Where(x => x.HaveTitle && x.TitleName != null).Count();
                                        }

                                        // Pending Case : 15/10/2019  Sweta Patel for multi worksheet service without Title case Error Handling

                                        string[] ServiceTitle = null;

                                        try
                                        {

                                            if (serviceCountWithTitle > 0)
                                            {
                                                 ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();
                                                responseIndexService = _iBillUploadRepository.getReadingIndexWithServiceFromSingleWorksheet(
                                                                                         exceluploadDetail.FilePath,
                                                                                         exceluploadDetail.FileNameGuid,
                                                                                         SingleWorksheetservice,
                                                                                         billUploadModel, ServiceTitle, worksheetNo);
                                            }else if(serviceCountWithTitle == 0)
                                            {
                                                if (SingleWorksheetservice.Count() == 1)
                                                {
                                                    responseIndexService.ServiceTypeId = SingleWorksheetservice.Select(x => x.ServiceTypeId).FirstOrDefault();
                                                    responseIndexService.ReadingIndex = Convert.ToInt64(SingleWorksheetservice.Select(x => x.ExcelReadingColumn).FirstOrDefault());
                                                }
                                                else
                                                {

                                                    List<long> BusinessServiceID = new List<long>() { 3,4,5 };
                                                    if (SingleWorksheetservice.Any(w => BusinessServiceID.Contains(w.ServiceTypeId))) {

                                                        #region Transaction Log Entry 
                                                        logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                            + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                            + " , Respnse Message : " + "Sorry! We could not upload multiple services with this complex mapping in single file.";

                                                        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                                        #endregion

                                                        ResponseAC respone = new ResponseAC();
                                                        respone.Message = "Sorry! We could not upload multiple services with this complex mapping in single file.";
                                                        respone.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                                        return Ok(respone);

                                                    }


                                                    List<long> MobilityCommonServiceId = new List<long>() {1,2,6,12 };
                                                    // Get Unique Mapped Service List & REmove Common Mapped service 
                                                    List<MappingDetailAC> SingleWorksheetserviceUniqueMapped = new List<MappingDetailAC>();
                                                    SingleWorksheetserviceUniqueMapped = SingleWorksheetservice.Where(x => x.IsCommonMapped == false).ToList();
                                                    if (SingleWorksheetserviceUniqueMapped != null)
                                                    {
                                                        if (SingleWorksheetserviceUniqueMapped.Any(w => MobilityCommonServiceId.Contains(w.ServiceTypeId)))
                                                        {
                                                            responseIndexService.ServiceTypeId = SingleWorksheetserviceUniqueMapped.Select(x => x.ServiceTypeId).FirstOrDefault();
                                                            responseIndexService.ReadingIndex = Convert.ToInt64(SingleWorksheetserviceUniqueMapped.Select(x => x.ExcelReadingColumn).FirstOrDefault());
                                                        }
                                                    }
                                                  
                                                }
                                                
                                            }

                                            if (responseIndexService.ServiceTypeId > 0)
                                            {
                                                readingIndex = (int)responseIndexService.ReadingIndex;


                                                while (responseIndexService.ServiceTypeId > 0)
                                                {
                                                    mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

                                                    if (responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                                                    {

                                                        List<string> list = new List<string>(ServiceTitle);
                                                        list.Remove(mappingDetail.TitleName.ToLower().Trim());

                                                        string[] ServiceTitleSelected = list.ToArray();

                                                        uploadDetailListInternetserviceM = await _iBillUploadRepository.ReadExcelForInternetServiceMultiple
                                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
                                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
                                                                                                 readingIndex, ServiceTitle, worksheetNo);

                                                        if (uploadDetailListInternetserviceM != null)
                                                        {
                                                            #region --> Handel If Exception Found
                                                            if (uploadDetailListInternetserviceM.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                            {

                                                                #region Transaction Log Entry 
                                                                logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                                    + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                                    + " , Respnse Message : " + uploadDetailListInternetserviceM.Message;

                                                                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                                                #endregion

                                                                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                                                responeAC.Message = uploadDetailListInternetserviceM.Message;
                                                                responeAC.TotalValidCount = "0";
                                                                responeAC.TotalAmount = "0";
                                                                return Ok(responeAC);

                                                            }
                                                            #endregion

                                                            allserviceTypeData.ValidList.AddRange(uploadDetailListInternetserviceM.UploadData.Data.ValidList);
                                                            allserviceTypeData.InvalidList3.AddRange(uploadDetailListInternetserviceM.UploadData.Data.InvalidList);
                                                            allserviceTypeData.InvalidListAllDB.AddRange(uploadDetailListInternetserviceM.UploadData.Data.InvalidListAllDB);
                                                            responseIndexService.ServiceTypeId = uploadDetailListInternetserviceM.ServiceTypeId;
                                                            responseIndexService.ReadingIndex = uploadDetailListInternetserviceM.ReadingIndex;
                                                            readingIndex = (int)responseIndexService.ReadingIndex;
                                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);
                                                            responeAC.mode = 345;

                                                        }

                                                    }

                                                    if (responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
                                                    {
                                                        List<string> list = new List<string>(ServiceTitle);
                                                        list.Remove(mappingDetail.TitleName.ToLower().Trim());

                                                        string[] ServiceTitleSelected = list.ToArray();

                                                        uploadDetailListDataCenterM = await _iBillUploadRepository.ReadExcelForDataCenterFacilityMultiple
                                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
                                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
                                                                                                 readingIndex, ServiceTitleSelected, worksheetNo);

                                                        if (uploadDetailListDataCenterM != null)
                                                        {
                                                            #region --> Handel If Exception Found
                                                            if (uploadDetailListDataCenterM.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                            {

                                                                #region Transaction Log Entry 
                                                                logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                                    + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                                    + " , Respnse Message : " + uploadDetailListDataCenterM.Message;

                                                                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                                                #endregion

                                                                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                                                responeAC.Message = uploadDetailListDataCenterM.Message;
                                                                responeAC.TotalValidCount = "0";
                                                                responeAC.TotalAmount = "0";
                                                                return Ok(responeAC);

                                                            }
                                                            #endregion

                                                            allserviceTypeData.ValidList.AddRange(uploadDetailListDataCenterM.UploadData.Data.ValidList);
                                                            allserviceTypeData.InvalidList4.AddRange(uploadDetailListDataCenterM.UploadData.Data.InvalidList);
                                                            allserviceTypeData.InvalidListAllDB.AddRange(uploadDetailListDataCenterM.UploadData.Data.InvalidListAllDB);
                                                            responseIndexService.ServiceTypeId = uploadDetailListDataCenterM.ServiceTypeId;
                                                            responseIndexService.ReadingIndex = uploadDetailListDataCenterM.ReadingIndex;
                                                            readingIndex = (int)responseIndexService.ReadingIndex;
                                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);
                                                            responeAC.mode = 345;
                                                        }
                                                    }

                                                    if (responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.ManagedHostingService)
                                                    {
                                                        List<string> list = new List<string>(ServiceTitle);
                                                        list.Remove(mappingDetail.TitleName.ToLower().Trim());

                                                        string[] ServiceTitleSelected = list.ToArray();

                                                        uploadDetailListManagedHostingM = await _iBillUploadRepository.ReadExcelForManagedHostingServiceMultiple
                                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
                                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
                                                                                                 readingIndex, ServiceTitle, worksheetNo);

                                                        if (uploadDetailListManagedHostingM != null)
                                                        {
                                                            #region --> Handel If Exception Found
                                                            if (uploadDetailListManagedHostingM.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                            {

                                                                #region Transaction Log Entry 
                                                                logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                                    + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                                    + " , Respnse Message : " + uploadDetailListManagedHostingM.Message;

                                                                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                                                #endregion

                                                                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                                                responeAC.Message = uploadDetailListManagedHostingM.Message;
                                                                responeAC.TotalValidCount = "0";
                                                                responeAC.TotalAmount = "0";
                                                                return Ok(responeAC);

                                                            }
                                                            #endregion

                                                            allserviceTypeData.ValidList.AddRange(uploadDetailListManagedHostingM.UploadData.Data.ValidList);
                                                            allserviceTypeData.InvalidList5.AddRange(uploadDetailListManagedHostingM.UploadData.Data.InvalidList);
                                                            allserviceTypeData.InvalidListAllDB.AddRange(uploadDetailListManagedHostingM.UploadData.Data.InvalidListAllDB);
                                                            responseIndexService.ServiceTypeId = uploadDetailListManagedHostingM.ServiceTypeId;
                                                            responseIndexService.ReadingIndex = uploadDetailListManagedHostingM.ReadingIndex;
                                                            readingIndex = (int)responseIndexService.ReadingIndex;
                                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);
                                                            responeAC.mode = 345;
                                                        }
                                                    }


                                                    if (  responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.Mobility
                                                         || responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.StaticIP
                                                         || responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.VoiceOnly
                                                         || responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.InternetPlanDeviceOffer
                                                        )
                                                    {

                                                        if (ServiceTitle != null)
                                                        {
                                                            List<string> list = new List<string>(ServiceTitle);
                                                            list.Remove(mappingDetail.TitleName.ToLower().Trim());

                                                            string[] ServiceTitleSelected = list.ToArray();
                                                           
                                                        }
                                                        uploadDetailListM = await _iBillUploadRepository.ReadExcelForMobilityServiceMultiple
                                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
                                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
                                                                                                 readingIndex, ServiceTitle, worksheetNo, responseIndexService.ServiceTypeId);

                                                        if (uploadDetailListM!= null)
                                                        {
                                                            #region --> Handel If Exception Found
                                                            if (uploadDetailListM.Status == (long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                            {

                                                                #region Transaction Log Entry 
                                                                logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                                    + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                                    + " , Respnse Message : " + uploadDetailListM.Message;

                                                                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                                                #endregion

                                                                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                                                responeAC.Message = uploadDetailListM.Message;
                                                                responeAC.TotalValidCount = "0";
                                                                responeAC.TotalAmount = "0";
                                                                return Ok(responeAC);

                                                            }
                                                            #endregion

                                                            allserviceTypeData.ValidList.AddRange(uploadDetailListM.UploadData.Data.ValidMobilityList);
                                                            allserviceTypeData.InvalidList1.AddRange(uploadDetailListM.UploadData.Data.InvalidMobilityList);
                                                            allserviceTypeData.InvalidListAllDB.AddRange(uploadDetailListM.UploadData.Data.InvalidListAllDB);
                                                            responseIndexService.ServiceTypeId = uploadDetailListM.ServiceTypeId;
                                                            responseIndexService.ReadingIndex = uploadDetailListM.ReadingIndex;
                                                            readingIndex = (int)responseIndexService.ReadingIndex;
                                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);
                                                            responeAC.mode = 1;
                                                        }
                                                    }

                                                }

                                            }
                                           
                                        }
                                        catch (Exception e)
                                        {
                                            #region Transaction Log Entry 
                                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                                + " , Respnse Message : " + "Error while excel reading multiple: " + e.Message;

                                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                            #endregion

                                            ResponseAC respone = new ResponseAC();
                                            respone.Message = "Error while excel reading multiple .";//: " + e.Message;
                                            respone.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                            return Ok(respone);
                                        }

                                    }

                                    #endregion -->  Worksheet No based Loop

                                }
                                #endregion


                            }
                        }


                            
                    }

                    else
                    {
                        #region --> if mapping is missing for selected services

                        #region Transaction Log Entry 
                        logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                            + Convert.ToString((long)EnumList.ExcelUploadResponseType.NoDataFound)
                                            + " , Respnse Message : " + "Mapping details does not exist!";

                        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                        #endregion

                        responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                        responeAC.Message = "Mapping details does not exist!";
                        responeAC.TotalValidCount = "0";
                        responeAC.TotalAmount = "0";
                        return Ok(responeAC);
                        #endregion
                    }
                }

                #region --> Save Data for all selected service at once                 

                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListVoip.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                   )
                {

                   

                    #region Transaction Log Entry 
                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                        + " , Respnse Message : " + "Error during reading";

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion


                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                    responeAC.Message = "Error during reading";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    bool isRemove = await _iBillUploadRepository.RemoveExcelUploadLog(before_excelUploadLogId);

                    return Ok(responeAC);
                }

                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListVoip.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                   )
                {

                    #region Transaction Log Entry 
                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.FileNotFound)
                                        + " , Respnse Message : " + "File not found";

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion

                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                    responeAC.Message = "File not found";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    bool isRemove = await _iBillUploadRepository.RemoveExcelUploadLog(before_excelUploadLogId);
                    return Ok(responeAC);
                }               
                else if (allserviceTypeData.InvalidList1.Count() <= 0
                  && (allserviceTypeData.ValidList.Count() > 0 || allserviceTypeData.ValidListSkype.Count() > 0)
                  && allserviceTypeData.InvalidList3.Count() <= 0 && allserviceTypeData.InvalidList4.Count() <= 0
                  && allserviceTypeData.InvalidList5.Count() <= 0 && allserviceTypeData.InvalidList9.Count() <= 0
                  && allserviceTypeData.InvalidList7.Count() <= 0
                 )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
                    responeAC.Message = "Bill uploaded successfully!";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    decimal? amount = 0;
                    amount = allserviceTypeData.ValidList.Sum(x => x.CallAmount)??0;
                    responeAC.TotalAmount = Convert.ToString(amount);
                    // return Ok(responeAC);
                }
                else if ((
                     allserviceTypeData.InvalidList1.Count() > 0 || allserviceTypeData.InvalidList3.Count() > 0
                  || allserviceTypeData.InvalidList4.Count() > 0 || allserviceTypeData.InvalidList5.Count() > 0
                  || allserviceTypeData.InvalidList9.Count() > 0 || allserviceTypeData.InvalidList7.Count() > 0)
                  && (allserviceTypeData.ValidList.Count() > 0 || allserviceTypeData.ValidListSkype.Count() > 0))

                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.SomeDataInvalid;
                    responeAC.Message = "Some data upload with error!";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    decimal? amount = 0;
                    amount = allserviceTypeData.ValidList.Sum(x => x.CallAmount) ?? 0;
                    responeAC.TotalAmount = Convert.ToString(amount);
                    //return Ok(responeAC);
                }
                else if (allserviceTypeData.InvalidList1.Count() == 0 && allserviceTypeData.InvalidList3.Count() == 0
                        && allserviceTypeData.InvalidList4.Count() == 0 && allserviceTypeData.InvalidList5.Count() == 0
                        && allserviceTypeData.InvalidList9.Count() == 0 && allserviceTypeData.ValidList.Count() == 0
                        && allserviceTypeData.InvalidList7.Count() == 0 && allserviceTypeData.ValidListSkype.Count() == 0
                     )
                {
                   

                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                    responeAC.Message = "Data doesnot exists!";
                    responeAC.TotalValidCount = "0";
                    responeAC.TotalAmount = "0";


                    #region Transaction Log Entry 
                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                        + Convert.ToString(responeAC.StatusCode)
                                        + " , Respnse Message : " + responeAC.Message
                                        + ", TotalAmount : " + responeAC.TotalAmount
                                        + ", TotalValidCount : " + responeAC.TotalValidCount;

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion

                    bool isRemove = await _iBillUploadRepository.RemoveExcelUploadLog(before_excelUploadLogId);
                    return Ok(responeAC);
                }
                else if (allserviceTypeData.ValidList.Count() == 0 && allserviceTypeData.ValidListSkype.Count() == 0)
                {

                    #region Transaction Log Entry 
                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.DataInvalid)
                                        + " , Respnse Message : " + "All data invalid";

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion

                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
                    responeAC.Message = "All data invalid";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";


                    if (allserviceTypeData.InvalidList1.Count() > 0)
                    {
                        responeAC.InvalidList1 = allserviceTypeData.InvalidList1.Take(100).ToList();
                    }
                    if (allserviceTypeData.InvalidList3.Count() > 0)
                    {
                        responeAC.InvalidList3 = allserviceTypeData.InvalidList3.Take(100).ToList();
                    }
                    if (allserviceTypeData.InvalidList4.Count() > 0)
                    {
                        responeAC.InvalidList4 = allserviceTypeData.InvalidList4.Take(100).ToList();
                    }
                    if (allserviceTypeData.InvalidList5.Count() > 0)
                    {
                        responeAC.InvalidList5 = allserviceTypeData.InvalidList5.Take(100).ToList();
                    }
                    if (allserviceTypeData.InvalidList7.Count() > 0)
                    {
                        responeAC.InvalidList7 = allserviceTypeData.InvalidList7.Take(100).ToList();
                    }
                    if (allserviceTypeData.InvalidList9.Count() > 0)
                    {
                        responeAC.InvalidList9 = allserviceTypeData.InvalidList9.Take(100).ToList();
                    }
                    if (allserviceTypeData.InvalidListpbx.Count() > 0)
                    {
                        responeAC.InvalidListPbx = allserviceTypeData.InvalidListpbx.Take(100).ToList();
                    }

                    iserrorDataSaved = await _iBillUploadRepository.AddExcelDetailError(allserviceTypeData, exceluploadDetail.FileNameGuid);

                    #region Transaction Log Entry 
                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                        + Convert.ToString(responeAC.StatusCode)
                                        + " , Respnse Message : " + responeAC.Message
                                        + ", TotalAmount : " + responeAC.TotalAmount
                                        + ", TotalValidCount : " + responeAC.TotalValidCount;

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion
                    bool isRemove = await _iBillUploadRepository.RemoveExcelUploadLog(before_excelUploadLogId);
                    return Ok(responeAC);
                }
                
                #region Save Error Data In Database for temp | Add | 02122019 | sweta patel

                if (   responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid
                    || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.DataInvalid && !iserrorDataSaved)
                {
                     iserrorDataSaved = await _iBillUploadRepository.AddExcelDetailError(allserviceTypeData, exceluploadDetail.FileNameGuid);                  
                }
                #endregion

                if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.Success
                  || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid)
                {                   

                    #region --> Save Data to DB

                    LogManager.Configuration.Variables["user"] = "";
                    LogManager.Configuration.Variables["stepno"] = "8";
                    _logger.Info("Start : Data Save Into database");
                  //  long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));
                    if(before_excelUploadLogId > 0)
                    {
                        responeAC.ExcelUploadId = before_excelUploadLogId;
                    }                   
                    else
                    {
                        ResponseAC responeACd = new ResponseAC();
                        responeACd.Message = "Error while excel upload saving!";
                        responeACd.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

                        #region Transaction Log Entry 
                        logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                            + Convert.ToString(responeAC.StatusCode)
                                            + " , Respnse Message : " + responeAC.Message;
                                        
                        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                        #endregion

                        return Ok(responeACd);
                    }

                    List<Exceldetail> excelDetailList = allserviceTypeData.ValidList;
                    List<Skypeexceldetail> skypeExcelDetailList = allserviceTypeData.ValidListSkype;

                    bool IsSaved = false;
                    if (allserviceTypeData.ValidListSkype != null)
                    {
                        if (allserviceTypeData.ValidListSkype.Count() > 0)
                        {
                            LogManager.Configuration.Variables["user"] = "";
                            LogManager.Configuration.Variables["stepno"] = "9";
                            _logger.Info("Start : Call details  Save Into database");
                            skypeExcelDetailList.ForEach(x => x.ExcelUploadLogId = before_excelUploadLogId);
                            IsSaved = await _iBillUploadRepository.AddSkypeExcelDetail(skypeExcelDetailList);
                            IsSkypeData = true;
                            LogManager.Configuration.Variables["user"] = "";
                            LogManager.Configuration.Variables["stepno"] = "10";
                            _logger.Info("END : Call details  Saved Into database");
                        }
                    }

                    if (allserviceTypeData.ValidList != null)
                    {
                        if (allserviceTypeData.ValidList.Count() > 0)
                        {
                            if (before_excelUploadLogId > 0)
                            {

                                if (billUploadModel.MergedWithId > 0)
                                {
                                    excelDetailList.ForEach(u =>
                                    {
                                        //u.ExcelUploadLogId = excelUploadLogId;
                                        //u.MergeExcelUploadId = billUploadModel.MergedWithId;
                                        u.ExcelUploadLogId = billUploadModel.MergedWithId;
                                        u.MergeExcelUploadId = before_excelUploadLogId;

                                    });
                                }
                                else
                                {
                                    excelDetailList.ForEach(x => x.ExcelUploadLogId = before_excelUploadLogId);
                                }
                                LogManager.Configuration.Variables["user"] = "";
                                LogManager.Configuration.Variables["stepno"] = "9";
                                _logger.Info("Start : Call details  Save Into database");
                                IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);
                                IsSkypeData = false;
                                LogManager.Configuration.Variables["user"] = "";
                                LogManager.Configuration.Variables["stepno"] = "10";
                                _logger.Info("END : Call details  Saved Into database");
                            }                          
                           
                        }
                    }

                    #endregion

                    if (!IsSaved)
                    {
                        responeAC.Message = "Error occur during excel detail data saving ";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    }
                    else
                    {
                        if (allserviceTypeData.ValidList.Count() > 0)
                        {

                            responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
                            responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());

                        }
                        else if (allserviceTypeData.ValidListSkype.Count() > 0)
                        {
                            responeAC.TotalAmount = Convert.ToString(skypeExcelDetailList.Sum(x => x.CallAmount));
                            responeAC.TotalValidCount = Convert.ToString(skypeExcelDetailList.Count());
                        }

                        #region --> Update Amount & Count in excelupload log
                        Exceluploadlog updateExcelLog = await _iBillUploadRepository.UpdateExcelUploadLog(before_excelUploadLogId, Convert.ToInt64(userId), Convert.ToInt32(responeAC.TotalValidCount), Convert.ToDecimal(responeAC.TotalAmount), IsSkypeData);
                        if (updateExcelLog != null)
                        {
                            responeAC.TotalAmount = Convert.ToString(updateExcelLog.TotalImportedBillAmount);
                            responeAC.TotalValidCount = Convert.ToString(updateExcelLog.TotalRecordImportCount);
                        }                  

                        #endregion

                        #region --- > Audit Log 
                        //string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;

                        await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.UploadNewBill, fullname,Convert.ToInt64(userId),"New bill(" + exceluploadDetail.FileName + ")",(int)EnumList.ActionTemplateTypes.Upload, responeAC.ExcelUploadId);

						#endregion


					}

					responeAC.InvalidList1 = allserviceTypeData.InvalidList1.Take(100).ToList();
                    responeAC.InvalidList3 = allserviceTypeData.InvalidList3.Take(100).ToList();
                    responeAC.InvalidList4 = allserviceTypeData.InvalidList4.Take(100).ToList();
                    responeAC.InvalidList5 = allserviceTypeData.InvalidList5.Take(100).ToList();
                    responeAC.InvalidList9 = allserviceTypeData.InvalidList9.Take(100).ToList();
                    responeAC.InvalidList7 = allserviceTypeData.InvalidList7.Take(100).ToList();

                    #region Transaction Log Entry 
                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                        + Convert.ToString(responeAC.StatusCode)
                                        + " , Respnse Message : " + responeAC.Message
                                        + ", TotalAmount : " + responeAC.TotalAmount
                                        + ", TotalValidCount : " + responeAC.TotalValidCount;

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion


                    LogManager.Configuration.Variables["user"] = fullname +'('+userId+')';
                    LogManager.Configuration.Variables["stepno"] = "11";
                    _logger.Info("END :Bill Upload Process. Status Message : "+ responeAC.Message);

                    return Ok(responeAC);
                }


                #endregion


                ResponseAC responedefault = new ResponseAC();
                responedefault.Message = "Service function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

                #region Transaction Log Entry 
                logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                    + Convert.ToString(responedefault.StatusCode)
                                    + " , Respnse Message : " + responedefault.Message;                       

                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                #endregion

                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseAC responeAC = new ResponseAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

                #region Transaction Log Entry 
                logDescription += " Service : " + ServiceSelected + " , Status Code : "
                                    + Convert.ToString(responeAC.StatusCode)
                                    + " , Respnse Message : " + responeAC.Message;

                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                #endregion


                return Ok(responeAC);
            }
        }


        [HttpPost]       
        //[Route("exporterrorlist/{fileguidno}/{mode}")]
        [Route("exporterrorlist")]
        public IActionResult ExportErrorList(ErrorExportAc errorExportAc)
        {
            string fileGuidNo = errorExportAc.fileGuidNo;
            int mode = errorExportAc.Mode;
            if (mode == 1)
            {
                List<MobilityExcelUploadDetailStringAC> datalistError = new List<MobilityExcelUploadDetailStringAC>();
                datalistError.Add(new MobilityExcelUploadDetailStringAC()
                {
                    CallDate= "Mobility Error List",
                    ErrorMessage = "Mobility Error List"
                });
               var result  = _iBillUploadRepository.ExportMobilityErrorList(fileGuidNo);             
               if(result!=null && result.Count() > 0)
                {
                    datalistError.AddRange(result);
                }
                string fileName = "ExcelUploadMobilityError.xlsx";
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
                    var worksheet = package.Workbook.Worksheets.Add("ErrorList");
                    worksheet.Row(1).Height = 20;
                    worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(1).Style.Font.Bold = true;
                   
                    worksheet.Cells.LoadFromCollection(datalistError, true);
                    if (datalistError.Count() > 0)
                    {
                       
                        worksheet.Cells["A2:O2"].Merge = true;
                        worksheet.Row(2).Height = 30;
                        worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Row(2).Style.Font.Bold = true;
                        worksheet.Row(2).Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                        worksheet.Row(2).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Row(2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                    }


                    package.Save();
                }
            }           
            
            return Ok();
        }

        [HttpPost]
        [Route("uploadnewbillpbx")]
        public async Task<IActionResult> UploadNewBillPbx([FromForm]PbxBillUploadFormDataAC billUploadFormDataAC)
        {
            long TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
            string logDescription = string.Empty;
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            string DeviceSelected = String.Empty;
            try
            {
                SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();
                AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();

                if (billUploadFormDataAC.File == null)
                {
                    #region --> Please select file if null.
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                    responeAC.Message = "Please select file.";
                    responeAC.TotalValidCount = "0";
                    responeAC.TotalAmount = "0";
                    return Ok(responeAC);
                    #endregion
                }

      
                PbxBillUploadAC billUploadModel = JsonConvert.DeserializeObject<PbxBillUploadAC>(billUploadFormDataAC.PbxBillUploadAc);
                var file = billUploadFormDataAC.File;
                ExcelFileAC excelFileAC = new ExcelFileAC();
                ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
                List<MappingDetailPbxAC> mappingExcellist = new List<MappingDetailPbxAC>();
                if (billUploadModel != null)
                {
                    DeviceSelected = billUploadModel.Device;
                }

                #region --> Get Excel mapping details

                if (billUploadModel != null)
                {
                    mappingExcellist = await _iBillUploadRepository.GetPbxExcelMapping(billUploadModel);
                }

                if (mappingExcellist != null)                {
                    // Check Mapping is exists for all selected Services

                    if ( mappingExcellist.Count() == 0)
                    {
                        #region --> if mapping is missing for selected device
                        responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                        responeAC.Message = "Mapping details does not exist!";
                        responeAC.TotalValidCount = "0";
                        responeAC.TotalAmount = "0";
                        return Ok(responeAC);
                        #endregion
                    }
                }
                #endregion
                
                #region --> Save File to temp Folder                
                if (file != null)
                {
                    string folderName = "TempUpload";
                    excelFileAC.File = file;
                    excelFileAC.FolderName = folderName;
                    billUploadModel.ExcelFileName1 = file.FileName;
                    exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
                    if (exceluploadDetail != null)
                    {
                        logDescription = " File Name :" + exceluploadDetail.FileName + " , FileNameGuid : " + exceluploadDetail.FileName + " , ";
                    }
                }
                #endregion

                #region --> File Extension , WorkSheet No Check  || 17/10/2019
                if (file != null)
                {
                    IFormFile FileData = file;
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Split(".")[1].Trim('"');
                    if (!string.IsNullOrEmpty(fileExtension.ToLower()))
                    {
                        if (fileExtension.ToLower() != "xls" && fileExtension.ToLower() != "xlsx" && fileExtension.ToLower() != "csv")
                        {
                            #region --> file format is not valid
                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                            responeAC.Message = "File format not valid!";
                            responeAC.TotalValidCount = "0";
                            responeAC.TotalAmount = "0";
                            return Ok(responeAC);
                            #endregion
                        }

                        if (mappingExcellist != null)
                        {
                            long MaxWorkSheetNo = mappingExcellist.Max(x => x.WorkSheetNo);
                            if (MaxWorkSheetNo > 1)
                            {
                                if (fileExtension.ToLower() == "xls" || fileExtension.ToLower() == "xlsx")
                                {
                                    responeAC = await _iBillUploadRepository.CheckMappingWithFileFormat(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, MaxWorkSheetNo);
                                    if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.NoDataFound)
                                    {
                                        return Ok(responeAC);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                

                ImportBillDetailAC<PbxUploadListAC> uploadDetailList = new ImportBillDetailAC<PbxUploadListAC>();
                ImportBillDetailMultipleAC<PbxUploadListAC> uploadDetailListM = new ImportBillDetailMultipleAC<PbxUploadListAC>();
                
               

                if (mappingExcellist != null)
                {
                    if (mappingExcellist.Count() == 1)
                    {
                        #region Read Data Service wise one by one for single service selected at once
                        foreach (var mapservice in mappingExcellist)
                        {
                            MappingDetailPbxAC mappingDetail = new MappingDetailPbxAC();
                            if (mapservice.DeviceId == (long)EnumList.DeviceType.Cisco)
                            {
                                #region --> Read Excel file for Cisco                           
                                mappingDetail = mappingExcellist.Find(x => x.DeviceId == (long)EnumList.DeviceType.Cisco);
                                ImportBillDetailAC<PbxUploadListAC> pbxList = await _iBillUploadRepository.ReadExcelForPbx(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailList = pbxList;

                                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError)
                                {
                                    #region Transaction Log Entry 
                                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + "Error during reading" + uploadDetailList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion


                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = uploadDetailList.Message;
                                    responeAC.TotalAmount = "0";
                                    responeAC.TotalValidCount = "0";
                                    return Ok(responeAC);
                                }

                                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound)
                                {
                                    #region Transaction Log Entry 
                                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.FileNotFound)
                                                        + " , Respnse Message : " + "File not found";

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                                    responeAC.Message = "File not found";
                                    responeAC.TotalAmount = "0";
                                    responeAC.TotalValidCount = "0";
                                    return Ok(responeAC);
                                }

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidListPbx.AddRange(pbxList.UploadData.Data.ValidPbxList);
                                allserviceTypeData.InvalidListpbx.AddRange(pbxList.UploadData.Data.InvalidPbxList);
                                #endregion
                            }
                           else if (mapservice.DeviceId == (long)EnumList.DeviceType.Avaya)
                            {
                                #region --> Read Excel file for Avaya                           
                                mappingDetail = mappingExcellist.Find(x => x.DeviceId == (long)EnumList.DeviceType.Avaya);
                                ImportBillDetailAC<PbxUploadListAC> pbxList = await _iBillUploadRepository.ReadExcelForPbx(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailList = pbxList;

                                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError)
                                {
                                    #region Transaction Log Entry 
                                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                                        + " , Respnse Message : " + "Error during reading" + uploadDetailList.Message;

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion


                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                                    responeAC.Message = uploadDetailList.Message;
                                    responeAC.TotalAmount = "0";
                                    responeAC.TotalValidCount = "0";
                                    return Ok(responeAC);
                                }

                                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound)
                                {
                                    #region Transaction Log Entry 
                                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.FileNotFound)
                                                        + " , Respnse Message : " + "File not found";

                                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                                    #endregion

                                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                                    responeAC.Message = "File not found";
                                    responeAC.TotalAmount = "0";
                                    responeAC.TotalValidCount = "0";
                                    return Ok(responeAC);
                                }

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidListPbx.AddRange(pbxList.UploadData.Data.ValidPbxList);
                                allserviceTypeData.InvalidListpbx.AddRange(pbxList.UploadData.Data.InvalidPbxList);
                                #endregion
                            }
                        }
                        #endregion
                    }             

                    else
                    {
                        #region --> if mapping is missing for selected services

                        #region Transaction Log Entry 
                        logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                            + Convert.ToString((long)EnumList.ExcelUploadResponseType.NoDataFound)
                                            + " , Respnse Message : " + "Mapping details does not exist!";

                        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                        #endregion

                        responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                        responeAC.Message = "Mapping details does not exist!";
                        responeAC.TotalValidCount = "0";
                        responeAC.TotalAmount = "0";
                        return Ok(responeAC);                        
                        #endregion
                    }
                }

                #region --> Save Data for all selected service at once                 

                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError)
                {
                    #region Transaction Log Entry 
                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
                                        + " , Respnse Message : " + "Error during reading" + uploadDetailList.Message;

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion


                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                    responeAC.Message = uploadDetailList.Message;
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }

                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound)
                {
                    #region Transaction Log Entry 
                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.FileNotFound)
                                        + " , Respnse Message : " + "File not found";

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion

                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                    responeAC.Message = "File not found";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }
                else if (allserviceTypeData.ValidListPbx.Count() == 0 )
                {
                    #region Transaction Log Entry 
                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.DataInvalid)
                                        + " , Respnse Message : " + "All data invalid";

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion

                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
                    responeAC.Message = "All data invalid";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";

                    if (allserviceTypeData.InvalidListpbx.Count() > 0)
                    {
                        responeAC.InvalidListPbx = allserviceTypeData.InvalidListpbx;
                    }

                    return Ok(responeAC);
                }

                else if (allserviceTypeData.InvalidListpbx.Count() <= 0 && allserviceTypeData.ValidListPbx.Count() > 0)
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
                    responeAC.Message = "Bill uploaded successfully";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    // return Ok(responeAC);
                }
                else if (allserviceTypeData.InvalidListpbx.Count() > 0 && allserviceTypeData.ValidListPbx.Count() > 0)
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.SomeDataInvalid;
                    responeAC.Message = "Some data upload with error!";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    //return Ok(responeAC);
                }
                else if (allserviceTypeData.InvalidListpbx.Count() == 0 && allserviceTypeData.ValidListPbx.Count() == 0)
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                    responeAC.Message = "Data doesnot exists!";
                    responeAC.TotalValidCount = "0";
                    responeAC.TotalAmount = "0";

                    #region Transaction Log Entry 
                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                        + Convert.ToString(responeAC.StatusCode)
                                        + " , Respnse Message : " + responeAC.Message
                                        + ", TotalAmount : " + responeAC.TotalAmount
                                        + ", TotalValidCount : " + responeAC.TotalValidCount;

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion


                    return Ok(responeAC);
                }

                if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.Success
                  || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid)
                {
                    #region --> Save Data to DB

                    long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLogPbx(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));
                    List<Exceldetailpbx> excelDetailList = allserviceTypeData.ValidListPbx;
                    if(excelUploadLogId >0)                   
                    {
                        responeAC.ExcelUploadId = excelUploadLogId;
                    }
                    else
                    {
                        ResponseAC responeACd = new ResponseAC();
                        responeACd.Message = "Error while excel upload saving!";
                        responeACd.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

                        #region Transaction Log Entry 
                        logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                            + Convert.ToString(responeAC.StatusCode)
                                            + " , Respnse Message : " + responeAC.Message;

                        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                        #endregion

                        return Ok(responeACd);
                    }


                    bool IsSaved = false;
                    if (allserviceTypeData.ValidListPbx != null)
                    {
                        if (allserviceTypeData.ValidListPbx.Count() > 0)
                        {
                            #region --> Merge PBX Codding is commented 
                            //if (excelUploadLogId > 0)
                            //{
                            //    if (billUploadModel.MergedWithId > 0)
                            //    {
                            //        excelDetailList.ForEach(u =>
                            //        {
                            //            u.ExcelUploadLogId = billUploadModel.MergedWithId;
                            //            u.MergeExcelUploadId = excelUploadLogId;

                            //        });
                            //    }
                            //    else
                            //    {
                            //        excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);
                            //    }

                            //    IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);
                            //}
                            #endregion

                            excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);
                            IsSaved = await _iBillUploadRepository.AddPbxExcelDetail(excelDetailList);
                        }
                    }                   

                    #endregion

                    if (!IsSaved)
                    {
                        responeAC.Message = "Error occur during excel detail data saving ";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    }
                    else
                    {
                        responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
                        responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());


						#region --- > Audit Log 
						string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;

						await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.UploadNewPBXBill, fullname, Convert.ToInt64(userId), "New pbx bill(" + exceluploadDetail.FileName + ")", (int)EnumList.ActionTemplateTypes.Upload, responeAC.ExcelUploadId);

						#endregion

					}

					responeAC.InvalidListPbx = allserviceTypeData.InvalidListpbx;

                    #region Transaction Log Entry 
                    logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                        + Convert.ToString(responeAC.StatusCode)
                                        + " , Respnse Message : " + responeAC.Message
                                        + ", TotalAmount : " + responeAC.TotalAmount
                                        + ", TotalValidCount : " + responeAC.TotalValidCount;

                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                    #endregion

                    return Ok(responeAC);
                }


                #endregion


                ResponseAC responedefault = new ResponseAC();
                responedefault.Message = "Service function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

                #region Transaction Log Entry 
                logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                    + Convert.ToString(responedefault.StatusCode)
                                    + " , Respnse Message : " + responedefault.Message;

                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                #endregion

                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseAC responeAC = new ResponseAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);


                #region Transaction Log Entry 
                logDescription += " Device : " + DeviceSelected + " , Status Code : "
                                    + Convert.ToString(responeAC.StatusCode)
                                    + " , Respnse Message : " + responeAC.Message;

                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
                #endregion

                return Ok(responeAC);
            }
        }

        

        #region Bill Allocation

        [HttpPost]
        [Route("billallocationlist")]
        public async Task<IActionResult> GetBillAllocationList(BillAllocationAC billAllocationAC)
        {
            return Ok(await _iBillUploadRepository.GetBillAllocationList(billAllocationAC));
        }


		[HttpPost]
		[Route("assignebill")]
		public async Task<IActionResult> AssgineBill(BillAssigneAC billAssigneAC) {
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillUploadRepository.AssigneBills(billAssigneAC,userId));
		}


		[HttpGet]
		[Route("assignedbills/{employeeId}/{exceluploadlogid}/{businessunitId}")]
		public async Task<IActionResult> AssignedBillList(long employeeId, long exceluploadlogid, long businessunitId){
			return Ok(await _iBillUploadRepository.GetAssignedBillList(employeeId,exceluploadlogid,businessunitId));
		}


		[HttpPost]
		[Route("unassignecalllog")]
		public async Task<IActionResult> UnAssgineCallLogs([FromBody]List<UnAssignedBillAC> unAssignedBillACs) {
			return Ok(await _iBillUploadRepository.UnAssgineCallLogs(unAssignedBillACs));
		}


		[HttpPost]
		[Route("billallocation")]
		public async Task<IActionResult> BillAllocation(BillAllocationAC billAllocationAC)
		{
			string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _iBillUploadRepository.BillAllocation(billAllocationAC,Convert.ToInt64(userId),fullname));
		}


        #endregion

        
        #region --> Bill delegates
        [HttpGet]
        [Route("delegatelist")]
        public async Task<IActionResult> GetDelegateList()
        {
            return Ok(await _ibillDelegateRepository.GetDelegates());
        }


        [HttpGet]
        [Route("delegate/{id}")]
        public async Task<IActionResult> GetDelegate(long id)
        {
            return Ok(await _ibillDelegateRepository.GetDelegateById(id));
        }

        [HttpPost]
        [Route("delegate/add")]
        public async Task<IActionResult> AddDelegate(BillDelegatesAC DelegateDetailAC)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _ibillDelegateRepository.AddDelegate(DelegateDetailAC, Convert.ToInt64(userId),fullname));
        }


        [HttpPut]
        [Route("delegate/edit")]
        public async Task<IActionResult> EditDelegate(BillDelegatesAC DelegateDetailAC)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _ibillDelegateRepository.EditDelegate(DelegateDetailAC, Convert.ToInt64(userId), fullname));
        }

        [HttpGet]
        [Route("delegate/delete/{id}")]
        public async Task<IActionResult> DeleteDelegates(long id)
        {
            string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			string fullname =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "fullname").Value;
			return Ok(await _ibillDelegateRepository.DeleteDelegate(id, Convert.ToInt64(userId), fullname));
        }

        [HttpPost]
        [Route("delegate/check")]
        public async Task<IActionResult> CheckDelegate(BillDelegatesAC delegateDetailAC)
        {
            if (delegateDetailAC.Employee.UserId > 0 && delegateDetailAC.DelegateEmployee.UserId > 0)
            {
                return Ok(await _ibillDelegateRepository.checkDelegatePair(delegateDetailAC.Employee, delegateDetailAC.DelegateEmployee, delegateDetailAC.Id));
            }
            else if (delegateDetailAC.Employee.UserId > 0 && delegateDetailAC.DelegateEmployee.UserId == 0)
            {
                return Ok(await _ibillDelegateRepository.checkIsEmployeeCanDelegated(delegateDetailAC.Employee, delegateDetailAC.Id));
            }
            else if (delegateDetailAC.DelegateEmployee.UserId > 0)
            {
                return Ok(await _ibillDelegateRepository.checkIsEmployeeNotDelegatedToOther(delegateDetailAC.DelegateEmployee));
            }
            else
            {
                ResponseAC responeAC = new ResponseAC();
                responeAC.Message = "Delegate details not found.";
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return Ok(responeAC);

            }

        }

        #endregion

        #region --> OLd Backup Of Upload Bill Functions

        //[HttpPost]
        //[Route("uploadnewbillold")]
        //public async Task<IActionResult> UploadNewBillOLDBAckup([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        //{
        //    try
        //    {

        //        BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);
        //        var file = billUploadFormDataAC.File;
        //        ExcelFileAC excelFileAC = new ExcelFileAC();
        //        ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
        //        List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();
        //        var  HttpContext.User = HttpContext.User;
        //        string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;

        //        #region --> Save File to temp Folder                
        //        if (file != null)
        //        {
        //            string folderName = "TempUpload";
        //            excelFileAC.File = file;
        //            excelFileAC.FolderName = folderName;
        //            billUploadModel.ExcelFileName1 = file.FileName;
        //            exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
        //        }
        //        #endregion

        //        #region --> Get Excel mapping details

        //        if (billUploadModel != null)
        //        {
        //            mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
        //        }

        //        #endregion

        //        ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
        //        ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
        //        ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListKems = new ImportBillDetailAC<InternetServiceUploadListAC>();
        //        AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();

        //        if (mappingExcellist != null)
        //        {
        //            #region Read Data Service wise one by one
        //            foreach (var mapservice in mappingExcellist)
        //            {
        //                MappingDetailAC mappingDetail = new MappingDetailAC();
        //                if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
        //                {
        //                    #region --> Read Excel file for mobility                           
        //                    mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.Mobility);
        //                    ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                    uploadDetailList = mobilityList;

        //                    #endregion

        //                    #region --> store in common class allserviceTypeData
        //                    allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
        //                    allserviceTypeData.InvalidList1 = mobilityList.UploadData.Data.InvalidMobilityList;
        //                    #endregion
        //                }

        //                else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
        //                {
        //                    #region --> Read Excel file for mada GeneralService                           
        //                    mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
        //                    ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                    uploadDetailListMada = madaserviceList;
        //                    #endregion

        //                    #region --> store in common class allserviceTypeData
        //                    allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
        //                    allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
        //                    #endregion
        //                }


        //                else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
        //                {
        //                    #region --> Read Excel file for Internet service                           
        //                    mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
        //                    ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                    uploadDetailListKems = internetserviceList;
        //                    #endregion

        //                    #region --> store in common class allserviceTypeData
        //                    allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
        //                    allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
        //                    #endregion
        //                }

        //            }
        //            #endregion
        //        }
        //        foreach (var mapservice in mappingExcellist)
        //        {
        //            if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
        //            {
        //                SaveExcelResponseAC<MobilityExcelUploadDetailStringAC> responeAC = new SaveExcelResponseAC<MobilityExcelUploadDetailStringAC>();
        //                responeAC.StatusCode = uploadDetailList.Status;
        //                responeAC.Message = uploadDetailList.Message;
        //                responeAC.TotalAmount = "0";
        //                responeAC.TotalValidCount = "0";
        //                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //                    || uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //                   )
        //                {
        //                    return Ok(responeAC);
        //                }
        //                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.DataInvalid)
        //                {
        //                    responeAC.UploadDataList = uploadDetailList.UploadData.Data.InvalidMobilityList;
        //                    return Ok(responeAC);
        //                }
        //                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.Success
        //                    || uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid
        //                   )
        //                {
        //                    #region --> Save Data to DB

        //                    long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

        //                    List<Exceldetail> excelDetailList = uploadDetailList.UploadData.Data.ValidMobilityList;
        //                    excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

        //                    bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

        //                    #endregion
        //                    if (!IsSaved)
        //                    {
        //                        responeAC.Message = "Error occur during excel detail data saving ";
        //                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //                    }
        //                    else
        //                    {
        //                        responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
        //                        responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
        //                    }

        //                    responeAC.UploadDataList = uploadDetailList.UploadData.Data.InvalidMobilityList;
        //                }

        //            }

        //            if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
        //            {
        //                SaveExcelResponseAC<MadaExcelUploadDetailStringAC> responeAC = new SaveExcelResponseAC<MadaExcelUploadDetailStringAC>();
        //                responeAC.StatusCode = uploadDetailListMada.Status;
        //                responeAC.Message = uploadDetailListMada.Message;
        //                responeAC.TotalAmount = "0";
        //                responeAC.TotalValidCount = "0";
        //                if (uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //                   )
        //                {
        //                    return Ok(responeAC);
        //                }
        //                else if (uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.DataInvalid)
        //                {
        //                    responeAC.UploadDataList = uploadDetailListMada.UploadData.Data.InvalidList;
        //                    return Ok(responeAC);
        //                }
        //                else if (uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.Success
        //                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid
        //                   )
        //                {
        //                    #region --> Save Data to DB

        //                    long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

        //                    List<Exceldetail> excelDetailList = uploadDetailListMada.UploadData.Data.ValidList;
        //                    excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

        //                    bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

        //                    #endregion
        //                    if (!IsSaved)
        //                    {
        //                        responeAC.Message = "Error occur during excel detail data saving ";
        //                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //                    }
        //                    else
        //                    {
        //                        responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.InitialDiscountedMonthlyPriceKd)) + "KD (Initial Discounted Monthly Price)";
        //                        responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
        //                    }

        //                    responeAC.UploadDataList = uploadDetailListMada.UploadData.Data.InvalidList;
        //                }
        //                return Ok(responeAC);
        //            }
        //        }

        //        ResponseAC responedefault = new ResponseAC();
        //        responedefault.Message = "Service function not found";
        //        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //        return Ok(responedefault);
        //    }
        //    catch (Exception e)
        //    {
        //        ResponseAC responeAC = new ResponseAC();
        //        responeAC.Message = "Error while excel reading : " + e.Message;
        //        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //        return Ok(responeAC);
        //    }
        //}


        //[HttpPost]
        //[Route("uploadnewbillold2")]
        //public async Task<IActionResult> UploadNewBillOld2([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        //{
        //    try
        //    {

        //        BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);
        //        var file = billUploadFormDataAC.File;
        //        ExcelFileAC excelFileAC = new ExcelFileAC();
        //        ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
        //        List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();
        //        var  HttpContext.User = HttpContext.User;
        //        string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;

        //        #region --> Save File to temp Folder                
        //        if (file != null)
        //        {
        //            string folderName = "TempUpload";
        //            excelFileAC.File = file;
        //            excelFileAC.FolderName = folderName;
        //            billUploadModel.ExcelFileName1 = file.FileName;
        //            exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
        //        }
        //        #endregion

        //        #region --> Get Excel mapping details

        //        if (billUploadModel != null)
        //        {
        //            mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
        //        }

        //        #endregion

        //        ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
        //        ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
        //        ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListInternetservice = new ImportBillDetailAC<InternetServiceUploadListAC>();
        //        ImportBillDetailAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenter = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();
        //        ImportBillDetailAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHosting = new ImportBillDetailAC<ManagedHostingServiceUploadListAC>();

        //        AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();
        //        SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();

        //        if (mappingExcellist != null)
        //        {
        //            if (mappingExcellist.Count() == 1)
        //            {
        //                #region Read Data Service wise one by one for single service selected at once
        //                foreach (var mapservice in mappingExcellist)
        //                {
        //                    MappingDetailAC mappingDetail = new MappingDetailAC();
        //                    if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
        //                    {
        //                        #region --> Read Excel file for mobility                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.Mobility);
        //                        ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailList = mobilityList;

        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
        //                        allserviceTypeData.InvalidList1.AddRange(mobilityList.UploadData.Data.InvalidMobilityList);
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
        //                    {
        //                        #region --> Read Excel file for mada GeneralService                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
        //                        ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListMada = madaserviceList;
        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
        //                        allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
        //                    {
        //                        #region --> Read Excel file for mada GeneralService                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
        //                        ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListInternetservice = internetserviceList;
        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
        //                        allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
        //                    {
        //                        #region --> Read Excel file for mada GeneralService                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility);
        //                        ImportBillDetailAC<DataCenterFacilityUploadListAC> dataCenterserviceList = await _iBillUploadRepository.ReadExcelForDataCenterFacility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListDataCenter = dataCenterserviceList;
        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(dataCenterserviceList.UploadData.Data.ValidList);
        //                        allserviceTypeData.InvalidList4 = dataCenterserviceList.UploadData.Data.InvalidList;
        //                        #endregion
        //                    }

        //                }
        //                #endregion
        //            }
        //            else if (mappingExcellist.Count() > 1)
        //            {

        //                #region --> Read Data for Multiple service From Same file

        //                MappingDetailAC mappingDetail = new MappingDetailAC();
        //                int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
        //                if (ServiceWithoutTitleCount > 1)
        //                {
        //                    #region --> if mapping is missing for selected services
        //                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
        //                    responeAC.Message = "Multiple service without title is invalid !";
        //                    responeAC.TotalValidCount = "0";
        //                    responeAC.TotalAmount = "0";
        //                    return Ok(responeAC);
        //                    #endregion
        //                }
        //                else
        //                {
        //                    //Get first Reading for Multiple service in same excel
        //                    MultiServiceUploadAC multiServiceUploadAC = new MultiServiceUploadAC();
        //                    try
        //                    {
        //                        multiServiceUploadAC = _iBillUploadRepository.getFirstReadingIndexWithService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingExcellist, billUploadModel);

        //                        if (multiServiceUploadAC.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
        //                        {

        //                        }

        //                    }
        //                    catch (Exception)
        //                    {

        //                    }


        //                }
        //                #endregion
        //            }
        //            else
        //            {
        //                #region --> if mapping is missing for selected services
        //                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
        //                responeAC.Message = "Mapping details does not exists!";
        //                responeAC.TotalValidCount = "0";
        //                responeAC.TotalAmount = "0";
        //                return Ok(responeAC);
        //                #endregion
        //            }
        //        }

        //        #region --> Save Data for all selected service at once                 

        //        if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //           )
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
        //            responeAC.Message = "Error during reading";
        //            responeAC.TotalAmount = "0";
        //            responeAC.TotalValidCount = "0";
        //            return Ok(responeAC);
        //        }

        //        else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //           )
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
        //            responeAC.Message = "File not found";
        //            responeAC.TotalAmount = "0";
        //            responeAC.TotalValidCount = "0";
        //            return Ok(responeAC);
        //        }
        //        else if (allserviceTypeData.ValidList.Count() == 0)
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
        //            responeAC.Message = "All data invalid";
        //            responeAC.TotalAmount = "0";
        //            responeAC.TotalValidCount = "0";
        //            return Ok(responeAC);
        //        }

        //        else if (allserviceTypeData.InvalidList1.Count() <= 0 && allserviceTypeData.ValidList.Count() > 0
        //          && allserviceTypeData.InvalidList3.Count() <= 0 && allserviceTypeData.InvalidList4.Count() <= 0
        //          && allserviceTypeData.InvalidList5.Count() <= 0 && allserviceTypeData.InvalidList9.Count() <= 0
        //         )
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
        //            responeAC.Message = "All uploaded successfully";
        //            responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            // return Ok(responeAC);
        //        }
        //        else if ((allserviceTypeData.InvalidList1.Count() > 0 || allserviceTypeData.InvalidList3.Count() > 0
        //          || allserviceTypeData.InvalidList4.Count() > 0 || allserviceTypeData.InvalidList5.Count() > 0
        //          || allserviceTypeData.InvalidList9.Count() > 0) && allserviceTypeData.ValidList.Count() > 0
        //         )
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.SomeDataInvalid;
        //            responeAC.Message = "Some data upload with error!";
        //            responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            //return Ok(responeAC);
        //        }
        //        else if (allserviceTypeData.InvalidList1.Count() == 0 && allserviceTypeData.InvalidList3.Count() == 0
        //                && allserviceTypeData.InvalidList4.Count() == 0 && allserviceTypeData.InvalidList5.Count() == 0
        //                && allserviceTypeData.InvalidList9.Count() == 0 && allserviceTypeData.ValidList.Count() == 0
        //             )
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
        //            responeAC.Message = "Data doesnot exists!";
        //            responeAC.TotalValidCount = "0";
        //            responeAC.TotalAmount = "0";
        //            return Ok(responeAC);
        //        }

        //        if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.Success
        //          || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid)
        //        {
        //            #region --> Save Data to DB

        //            long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

        //            List<Exceldetail> excelDetailList = allserviceTypeData.ValidList;
        //            excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

        //            bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

        //            #endregion
        //            if (!IsSaved)
        //            {
        //                responeAC.Message = "Error occur during excel detail data saving ";
        //                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //            }
        //            else
        //            {
        //                responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
        //                responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
        //            }

        //            responeAC.InvalidList1 = allserviceTypeData.InvalidList1;
        //            responeAC.InvalidList3 = allserviceTypeData.InvalidList3;
        //            responeAC.InvalidList4 = allserviceTypeData.InvalidList4;
        //            responeAC.InvalidList5 = allserviceTypeData.InvalidList5;
        //            responeAC.InvalidList9 = allserviceTypeData.InvalidList9;
        //            return Ok(responeAC);
        //        }


        //        #endregion


        //        ResponseAC responedefault = new ResponseAC();
        //        responedefault.Message = "Service function not found";
        //        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //        return Ok(responedefault);
        //    }
        //    catch (Exception e)
        //    {
        //        ResponseAC responeAC = new ResponseAC();
        //        responeAC.Message = "Error while excel reading : " + e.Message;
        //        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //        return Ok(responeAC);
        //    }
        //}

        //[HttpPost]
        //[Route("uploadnewbillold3")]
        //public async Task<IActionResult> UploadNewBillOld3([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        //{
        //    long TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
        //    string logDescription = string.Empty;
        //    var  HttpContext.User = HttpContext.User;
        //    string userId =  HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
        //    string ServiceSelected = String.Empty;
        //    try
        //    {
        //        SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();


        //        if (billUploadFormDataAC.File == null)
        //        {
        //            #region --> if mapping is missing for selected services
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
        //            responeAC.Message = "Please select file.";
        //            responeAC.TotalValidCount = "0";
        //            responeAC.TotalAmount = "0";
        //            return Ok(responeAC);
        //            #endregion
        //        }

        //        BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);



        //        var file = billUploadFormDataAC.File;
        //        ExcelFileAC excelFileAC = new ExcelFileAC();
        //        ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
        //        List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();


        //        #region --> Get Excel mapping details

        //        if (billUploadModel != null)
        //        {
        //            mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
        //            if (billUploadModel.ServiceTypes.Count() > 0)
        //            {
        //                ServiceSelected = string.Join(",", billUploadModel.ServiceTypes.Select(x => x.Name).ToArray());
        //            }

        //        }

        //        if (mappingExcellist != null)
        //        {
        //            // Check Mapping is exists for all selected Services

        //            if (billUploadModel.ServiceTypes.Count() != mappingExcellist.Count())
        //            {
        //                #region --> if mapping is missing for selected services
        //                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
        //                responeAC.Message = "Mapping details do not exist for selected services!";
        //                responeAC.TotalValidCount = "0";
        //                responeAC.TotalAmount = "0";
        //                return Ok(responeAC);
        //                #endregion
        //            }
        //        }
        //        #endregion



        //        #region --> Save File to temp Folder                
        //        if (file != null)
        //        {
        //            string folderName = "TempUpload";
        //            excelFileAC.File = file;
        //            excelFileAC.FolderName = folderName;
        //            billUploadModel.ExcelFileName1 = file.FileName;
        //            exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
        //            if (exceluploadDetail != null)
        //            {
        //                logDescription = " File Name :" + exceluploadDetail.FileName + " , FileNameGuid : " + exceluploadDetail.FileName + " , ";
        //            }

        //        }
        //        #endregion



        //        ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
        //        // Added On 03/10/2019
        //        ImportBillDetailAC<StaticIPUploadListAC> uploadDetailListStaticIP = new ImportBillDetailAC<StaticIPUploadListAC>();
        //        ImportBillDetailAC<VoiceOnlyUploadListAC> uploadDetailListVoiceOnly = new ImportBillDetailAC<VoiceOnlyUploadListAC>();
        //        //----------------------

        //        ImportBillDetailAC<VoipUploadListAC> uploadDetailListVoip = new ImportBillDetailAC<VoipUploadListAC>();

        //        ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
        //        ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListInternetservice = new ImportBillDetailAC<InternetServiceUploadListAC>();
        //        ImportBillDetailAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenter = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();
        //        ImportBillDetailAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHosting = new ImportBillDetailAC<ManagedHostingServiceUploadListAC>();


        //        ImportBillDetailMultipleAC<MobilityUploadListAC> uploadDetailListM = new ImportBillDetailMultipleAC<MobilityUploadListAC>();
        //        // Added On 03/10/2019
        //        ImportBillDetailMultipleAC<StaticIPUploadListAC> uploadDetailListStaticIPM = new ImportBillDetailMultipleAC<StaticIPUploadListAC>();
        //        ImportBillDetailMultipleAC<VoiceOnlyUploadListAC> uploadDetailListVoiceOnlyM = new ImportBillDetailMultipleAC<VoiceOnlyUploadListAC>();
        //        //----------------------

        //        ImportBillDetailMultipleAC<MadaUploadListAC> uploadDetailListMadaM = new ImportBillDetailMultipleAC<MadaUploadListAC>();
        //        ImportBillDetailMultipleAC<InternetServiceUploadListAC> uploadDetailListInternetserviceM = new ImportBillDetailMultipleAC<InternetServiceUploadListAC>();
        //        ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenterM = new ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>();
        //        ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHostingM = new ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>();

        //        ImportBillDetailMultipleAC<VoipUploadListAC> uploadDetailListVoipM = new ImportBillDetailMultipleAC<VoipUploadListAC>();

        //        AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();


        //        if (mappingExcellist != null)
        //        {

        //            if (mappingExcellist.Count() == 1)
        //            {
        //                #region Read Data Service wise one by one for single service selected at once
        //                foreach (var mapservice in mappingExcellist)
        //                {
        //                    MappingDetailAC mappingDetail = new MappingDetailAC();
        //                    if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
        //                    {
        //                        #region --> Read Excel file for mobility                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.Mobility);
        //                        ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailList = mobilityList;

        //                        if (mobilityList.Status == (long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
        //                        {

        //                            #region Transaction Log Entry 
        //                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
        //                                                + " , Respnse Message : " + mobilityList.Message + ".";

        //                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                            #endregion


        //                            responeAC.Message = mobilityList.Message;
        //                            responeAC.StatusCode = (long)EnumList.ExcelUploadResponseType.MultipleServiceFound;
        //                            return Ok(responeAC);
        //                        }

        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
        //                        allserviceTypeData.InvalidList1.AddRange(mobilityList.UploadData.Data.InvalidMobilityList);
        //                        #endregion
        //                    }
        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.StaticIP)
        //                    {
        //                        #region --> Read Excel file for StaticIP                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.StaticIP);
        //                        ImportBillDetailAC<StaticIPUploadListAC> staticipList = await _iBillUploadRepository.ReadExcelForStaticIP(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListStaticIP = staticipList;

        //                        if (staticipList.Status == (long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
        //                        {

        //                            #region Transaction Log Entry 
        //                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
        //                                                + " , Respnse Message : " + staticipList.Message + ".";

        //                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                            #endregion


        //                            responeAC.Message = staticipList.Message;
        //                            responeAC.StatusCode = (long)EnumList.ExcelUploadResponseType.MultipleServiceFound;
        //                            return Ok(responeAC);
        //                        }

        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(staticipList.UploadData.Data.ValidStaticIPList);
        //                        allserviceTypeData.InvalidList6.AddRange(staticipList.UploadData.Data.InvalidStaticIPList);
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.VoiceOnly)
        //                    {
        //                        #region --> Read Excel file for VoiceOnly                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.VoiceOnly);
        //                        ImportBillDetailAC<VoiceOnlyUploadListAC> voiceOnlyList = await _iBillUploadRepository.ReadExcelForVoiceOnly(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListVoiceOnly = voiceOnlyList;

        //                        if (voiceOnlyList.Status == (long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
        //                        {

        //                            #region Transaction Log Entry 
        //                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.MultipleServiceFound)
        //                                                + " , Respnse Message : " + voiceOnlyList.Message + ".";

        //                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                            #endregion


        //                            responeAC.Message = voiceOnlyList.Message;
        //                            responeAC.StatusCode = (long)EnumList.ExcelUploadResponseType.MultipleServiceFound;
        //                            return Ok(responeAC);
        //                        }

        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(voiceOnlyList.UploadData.Data.ValidVoiceOnlyList);
        //                        allserviceTypeData.InvalidList2.AddRange(voiceOnlyList.UploadData.Data.InvalidVoiceOnlyList);
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.VOIP)
        //                    {
        //                        #region --> Read Excel file for Voip                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.VOIP);
        //                        ImportBillDetailAC<VoipUploadListAC> voipList = await _iBillUploadRepository.ReadExcelForVoip(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListVoip = voipList;

        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidListSkype.AddRange(voipList.UploadData.Data.ValidVoipList);
        //                        allserviceTypeData.InvalidList7.AddRange(voipList.UploadData.Data.InvalidVoipList);
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
        //                    {
        //                        #region --> Read Excel file for mada GeneralService                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
        //                        ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListMada = madaserviceList;
        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
        //                        allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
        //                    {
        //                        #region --> Read Excel file for mada GeneralService                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
        //                        ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListInternetservice = internetserviceList;
        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
        //                        allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
        //                        #endregion
        //                    }

        //                    else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
        //                    {
        //                        #region --> Read Excel file for mada GeneralService                           
        //                        mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility);
        //                        ImportBillDetailAC<DataCenterFacilityUploadListAC> dataCenterserviceList = await _iBillUploadRepository.ReadExcelForDataCenterFacility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
        //                        uploadDetailListDataCenter = dataCenterserviceList;
        //                        #endregion

        //                        #region --> store in common class allserviceTypeData
        //                        allserviceTypeData.ValidList.AddRange(dataCenterserviceList.UploadData.Data.ValidList);
        //                        allserviceTypeData.InvalidList4 = dataCenterserviceList.UploadData.Data.InvalidList;
        //                        #endregion
        //                    }

        //                }
        //                #endregion
        //            }
        //            else if (mappingExcellist.Count() > 1)
        //            {

        //                #region --> Read Data for Multiple service From Same file

        //                MappingDetailAC mappingDetail = new MappingDetailAC();

        //                int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
        //                if (ServiceWithoutTitleCount > 1)
        //                {
        //                    #region --> if excel have two service without title we can not verify each record 


        //                    #region Transaction Log Entry 
        //                    logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                        + Convert.ToString((long)EnumList.ExcelUploadResponseType.DataInvalid)
        //                                        + " , Respnse Message : " + "Multiple service without title is invalid !";

        //                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                    #endregion

        //                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
        //                    responeAC.Message = "Multiple service without title is invalid !";
        //                    responeAC.TotalValidCount = "0";
        //                    responeAC.TotalAmount = "0";
        //                    return Ok(responeAC);
        //                    #endregion
        //                }
        //                else  // if mapping details are accurate than 
        //                {

        //                    #region --> loop based on the worksheetNo provided by the mapping list

        //                    foreach (var worksheet in mappingExcellist.Select(p => p.WorkSheetNo).Distinct().ToList())
        //                    {

        //                        // Get first Reading Index with service
        //                        MultiServiceUploadAC responseIndexService = new MultiServiceUploadAC();
        //                        int worksheetNo = (int)worksheet;
        //                        int readingIndex = 0;
        //                        List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == (int)worksheet).ToList();
        //                        string[] ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();

        //                        try
        //                        {
        //                            responseIndexService = _iBillUploadRepository.getReadingIndexWithServiceFromSingleWorksheet(
        //                                                                          exceluploadDetail.FilePath,
        //                                                                          exceluploadDetail.FileNameGuid,
        //                                                                          SingleWorksheetservice,
        //                                                                          billUploadModel, ServiceTitle, worksheetNo);

        //                            if (responseIndexService.ServiceTypeId > 0)
        //                            {
        //                                readingIndex = (int)responseIndexService.ReadingIndex;


        //                                while (responseIndexService.ServiceTypeId > 0)
        //                                {
        //                                    mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

        //                                    if (responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
        //                                    {

        //                                        List<string> list = new List<string>(ServiceTitle);
        //                                        list.Remove(mappingDetail.TitleName.ToLower().Trim());

        //                                        string[] ServiceTitleSelected = list.ToArray();

        //                                        uploadDetailListInternetserviceM = await _iBillUploadRepository.ReadExcelForInternetServiceMultiple
        //                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
        //                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
        //                                                                                 readingIndex, ServiceTitle, worksheetNo);

        //                                        if (uploadDetailListInternetserviceM != null)
        //                                        {
        //                                            allserviceTypeData.ValidList.AddRange(uploadDetailListInternetserviceM.UploadData.Data.ValidList);
        //                                            allserviceTypeData.InvalidList3.AddRange(uploadDetailListInternetserviceM.UploadData.Data.InvalidList);
        //                                            responseIndexService.ServiceTypeId = uploadDetailListInternetserviceM.ServiceTypeId;
        //                                            responseIndexService.ReadingIndex = uploadDetailListInternetserviceM.ReadingIndex;
        //                                            readingIndex = (int)responseIndexService.ReadingIndex;
        //                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

        //                                        }

        //                                    }

        //                                    if (responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
        //                                    {
        //                                        List<string> list = new List<string>(ServiceTitle);
        //                                        list.Remove(mappingDetail.TitleName.ToLower().Trim());

        //                                        string[] ServiceTitleSelected = list.ToArray();

        //                                        uploadDetailListDataCenterM = await _iBillUploadRepository.ReadExcelForDataCenterFacilityMultiple
        //                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
        //                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
        //                                                                                 readingIndex, ServiceTitleSelected, worksheetNo);

        //                                        if (uploadDetailListDataCenterM != null)
        //                                        {
        //                                            allserviceTypeData.ValidList.AddRange(uploadDetailListDataCenterM.UploadData.Data.ValidList);
        //                                            allserviceTypeData.InvalidList4.AddRange(uploadDetailListDataCenterM.UploadData.Data.InvalidList);
        //                                            responseIndexService.ServiceTypeId = uploadDetailListDataCenterM.ServiceTypeId;
        //                                            responseIndexService.ReadingIndex = uploadDetailListDataCenterM.ReadingIndex;
        //                                            readingIndex = (int)responseIndexService.ReadingIndex;
        //                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

        //                                        }
        //                                    }

        //                                    if (responseIndexService.ServiceTypeId == (long)EnumList.ServiceType.ManagedHostingService)
        //                                    {
        //                                        List<string> list = new List<string>(ServiceTitle);
        //                                        list.Remove(mappingDetail.TitleName.ToLower().Trim());

        //                                        string[] ServiceTitleSelected = list.ToArray();

        //                                        uploadDetailListManagedHostingM = await _iBillUploadRepository.ReadExcelForManagedHostingServiceMultiple
        //                                                                                (exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid,
        //                                                                                 mappingDetail, SingleWorksheetservice, billUploadModel,
        //                                                                                 readingIndex, ServiceTitle, worksheetNo);

        //                                        if (uploadDetailListManagedHostingM != null)
        //                                        {
        //                                            allserviceTypeData.ValidList.AddRange(uploadDetailListManagedHostingM.UploadData.Data.ValidList);
        //                                            allserviceTypeData.InvalidList5.AddRange(uploadDetailListManagedHostingM.UploadData.Data.InvalidList);
        //                                            responseIndexService.ServiceTypeId = uploadDetailListManagedHostingM.ServiceTypeId;
        //                                            responseIndexService.ReadingIndex = uploadDetailListManagedHostingM.ReadingIndex;
        //                                            readingIndex = (int)responseIndexService.ReadingIndex;
        //                                            mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

        //                                        }
        //                                    }

        //                                }

        //                            }

        //                        }
        //                        catch (Exception e)
        //                        {
        //                            #region Transaction Log Entry 
        //                            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
        //                                                + " , Respnse Message : " + "Error while excel reading multiple: " + e.Message;

        //                            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                            #endregion

        //                            ResponseAC respone = new ResponseAC();
        //                            respone.Message = "Error while excel reading multiple .";//: " + e.Message;
        //                            respone.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //                            return Ok(respone);
        //                        }

        //                    }

        //                    #endregion -->  Worksheet No based Loop

        //                }
        //                #endregion
        //            }

        //            else
        //            {
        //                #region --> if mapping is missing for selected services

        //                #region Transaction Log Entry 
        //                logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                    + Convert.ToString((long)EnumList.ExcelUploadResponseType.NoDataFound)
        //                                    + " , Respnse Message : " + "Mapping details does not exists!";

        //                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                #endregion

        //                responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
        //                responeAC.Message = "Mapping details does not exists!";
        //                responeAC.TotalValidCount = "0";
        //                responeAC.TotalAmount = "0";
        //                return Ok(responeAC);
        //                #endregion
        //            }
        //        }

        //        #region --> Save Data for all selected service at once                 

        //        if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListVoip.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //            || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
        //           )
        //        {



        //            #region Transaction Log Entry 
        //            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.ExceptionError)
        //                                + " , Respnse Message : " + "Error during reading";

        //            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //            #endregion


        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
        //            responeAC.Message = "Error during reading";
        //            responeAC.TotalAmount = "0";
        //            responeAC.TotalValidCount = "0";
        //            return Ok(responeAC);
        //        }

        //        else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListVoip.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //            || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
        //           )
        //        {

        //            #region Transaction Log Entry 
        //            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.FileNotFound)
        //                                + " , Respnse Message : " + "File not found";

        //            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //            #endregion

        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
        //            responeAC.Message = "File not found";
        //            responeAC.TotalAmount = "0";
        //            responeAC.TotalValidCount = "0";
        //            return Ok(responeAC);
        //        }
        //        else if (allserviceTypeData.ValidList.Count() == 0 && allserviceTypeData.ValidListSkype.Count() == 0)
        //        {

        //            #region Transaction Log Entry 
        //            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                + Convert.ToString((long)EnumList.ExcelUploadResponseType.DataInvalid)
        //                                + " , Respnse Message : " + "All data invalid";

        //            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //            #endregion

        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
        //            responeAC.Message = "All data invalid";
        //            responeAC.TotalAmount = "0";

        //            if (allserviceTypeData.InvalidList1.Count() > 0)
        //            {
        //                responeAC.InvalidList1 = allserviceTypeData.InvalidList1;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidList1.Count());
        //            }
        //            else if (allserviceTypeData.InvalidList3.Count() > 0)
        //            {
        //                responeAC.InvalidList3 = allserviceTypeData.InvalidList3;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidList3.Count());
        //            }
        //            else if (allserviceTypeData.InvalidList4.Count() > 0)
        //            {
        //                responeAC.InvalidList4 = allserviceTypeData.InvalidList4;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidList4.Count());
        //            }
        //            else if (allserviceTypeData.InvalidList5.Count() > 0)
        //            {
        //                responeAC.InvalidList5 = allserviceTypeData.InvalidList5;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidList5.Count());
        //            }
        //            else if (allserviceTypeData.InvalidList7.Count() > 0)
        //            {
        //                responeAC.InvalidList7 = allserviceTypeData.InvalidList7;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidList7.Count());
        //            }
        //            else if (allserviceTypeData.InvalidList9.Count() > 0)
        //            {
        //                responeAC.InvalidList9 = allserviceTypeData.InvalidList9;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidList9.Count());
        //            }
        //            else if (allserviceTypeData.InvalidListpbx.Count() > 0)
        //            {
        //                responeAC.InvalidListPbx = allserviceTypeData.InvalidListpbx;
        //                responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.InvalidListpbx.Count());
        //            }
        //            else
        //            {
        //                responeAC.TotalValidCount = "0";
        //            }

        //            #region Transaction Log Entry 
        //            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                + Convert.ToString(responeAC.StatusCode)
        //                                + " , Respnse Message : " + responeAC.Message
        //                                + ", TotalAmount : " + responeAC.TotalAmount
        //                                + ", TotalValidCount : " + responeAC.TotalValidCount;

        //            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //            #endregion

        //            return Ok(responeAC);
        //        }

        //        else if (allserviceTypeData.InvalidList1.Count() <= 0
        //          && (allserviceTypeData.ValidList.Count() > 0 || allserviceTypeData.ValidListSkype.Count() > 0)
        //          && allserviceTypeData.InvalidList3.Count() <= 0 && allserviceTypeData.InvalidList4.Count() <= 0
        //          && allserviceTypeData.InvalidList5.Count() <= 0 && allserviceTypeData.InvalidList9.Count() <= 0
        //          && allserviceTypeData.InvalidList7.Count() <= 0
        //         )
        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
        //            responeAC.Message = "Bill uploaded successfully!";
        //            responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            // return Ok(responeAC);
        //        }
        //        else if ((
        //             allserviceTypeData.InvalidList1.Count() > 0 || allserviceTypeData.InvalidList3.Count() > 0
        //          || allserviceTypeData.InvalidList4.Count() > 0 || allserviceTypeData.InvalidList5.Count() > 0
        //          || allserviceTypeData.InvalidList9.Count() > 0 || allserviceTypeData.InvalidList7.Count() > 0)
        //          && (allserviceTypeData.ValidList.Count() > 0 || allserviceTypeData.ValidListSkype.Count() > 0))

        //        {
        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.SomeDataInvalid;
        //            responeAC.Message = "Some data upload with error!";
        //            responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
        //            //return Ok(responeAC);
        //        }
        //        else if (allserviceTypeData.InvalidList1.Count() == 0 && allserviceTypeData.InvalidList3.Count() == 0
        //                && allserviceTypeData.InvalidList4.Count() == 0 && allserviceTypeData.InvalidList5.Count() == 0
        //                && allserviceTypeData.InvalidList9.Count() == 0 && allserviceTypeData.ValidList.Count() == 0
        //                && allserviceTypeData.InvalidList7.Count() == 0 && allserviceTypeData.ValidListSkype.Count() == 0
        //             )
        //        {


        //            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
        //            responeAC.Message = "Data doesnot exists!";
        //            responeAC.TotalValidCount = "0";
        //            responeAC.TotalAmount = "0";


        //            #region Transaction Log Entry 
        //            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                + Convert.ToString(responeAC.StatusCode)
        //                                + " , Respnse Message : " + responeAC.Message
        //                                + ", TotalAmount : " + responeAC.TotalAmount
        //                                + ", TotalValidCount : " + responeAC.TotalValidCount;

        //            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //            #endregion

        //            return Ok(responeAC);
        //        }

        //        if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.Success
        //          || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid)
        //        {
        //            #region --> Save Data to DB

        //            long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));
        //            if (excelUploadLogId > 0)
        //            {
        //                responeAC.ExcelUploadId = excelUploadLogId;
        //            }
        //            else
        //            {
        //                ResponseAC responeACd = new ResponseAC();
        //                responeACd.Message = "Error while excel upload saving!";
        //                responeACd.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

        //                #region Transaction Log Entry 
        //                logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                    + Convert.ToString(responeAC.StatusCode)
        //                                    + " , Respnse Message : " + responeAC.Message;

        //                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //                #endregion

        //                return Ok(responeACd);
        //            }

        //            List<Exceldetail> excelDetailList = allserviceTypeData.ValidList;
        //            List<Skypeexceldetail> skypeExcelDetailList = allserviceTypeData.ValidListSkype;

        //            bool IsSaved = false;
        //            if (allserviceTypeData.ValidListSkype != null)
        //            {
        //                if (allserviceTypeData.ValidListSkype.Count() > 0)
        //                {
        //                    skypeExcelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);
        //                    IsSaved = await _iBillUploadRepository.AddSkypeExcelDetail(skypeExcelDetailList);
        //                }
        //            }

        //            if (allserviceTypeData.ValidList != null)
        //            {
        //                if (allserviceTypeData.ValidList.Count() > 0)
        //                {
        //                    if (excelUploadLogId > 0)
        //                    {
        //                        if (billUploadModel.MergedWithId > 0)
        //                        {
        //                            excelDetailList.ForEach(u =>
        //                            {
        //                                //u.ExcelUploadLogId = excelUploadLogId;
        //                                //u.MergeExcelUploadId = billUploadModel.MergedWithId;
        //                                u.ExcelUploadLogId = billUploadModel.MergedWithId;
        //                                u.MergeExcelUploadId = excelUploadLogId;

        //                            });
        //                        }
        //                        else
        //                        {
        //                            excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);
        //                        }

        //                        IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);
        //                    }

        //                }
        //            }

        //            #endregion

        //            if (!IsSaved)
        //            {
        //                responeAC.Message = "Error occur during excel detail data saving ";
        //                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
        //            }
        //            else
        //            {
        //                if (allserviceTypeData.ValidList.Count() > 0)
        //                {
        //                    responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
        //                    responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());

        //                }
        //                else if (allserviceTypeData.ValidListSkype.Count() > 0)
        //                {
        //                    responeAC.TotalAmount = Convert.ToString(skypeExcelDetailList.Sum(x => x.CallAmount));
        //                    responeAC.TotalValidCount = Convert.ToString(skypeExcelDetailList.Count());
        //                }
        //            }

        //            responeAC.InvalidList1 = allserviceTypeData.InvalidList1;
        //            responeAC.InvalidList3 = allserviceTypeData.InvalidList3;
        //            responeAC.InvalidList4 = allserviceTypeData.InvalidList4;
        //            responeAC.InvalidList5 = allserviceTypeData.InvalidList5;
        //            responeAC.InvalidList9 = allserviceTypeData.InvalidList9;
        //            responeAC.InvalidList7 = allserviceTypeData.InvalidList7;

        //            #region Transaction Log Entry 
        //            logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                                + Convert.ToString(responeAC.StatusCode)
        //                                + " , Respnse Message : " + responeAC.Message
        //                                + ", TotalAmount : " + responeAC.TotalAmount
        //                                + ", TotalValidCount : " + responeAC.TotalValidCount;

        //            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //            #endregion


        //            return Ok(responeAC);
        //        }


        //        #endregion


        //        ResponseAC responedefault = new ResponseAC();
        //        responedefault.Message = "Service function not found";
        //        responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

        //        #region Transaction Log Entry 
        //        logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                            + Convert.ToString(responedefault.StatusCode)
        //                            + " , Respnse Message : " + responedefault.Message;

        //        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //        #endregion

        //        return Ok(responedefault);
        //    }
        //    catch (Exception e)
        //    {
        //        ResponseAC responeAC = new ResponseAC();
        //        responeAC.Message = "Error while excel reading : " + e.Message;
        //        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);

        //        #region Transaction Log Entry 
        //        logDescription += " Service : " + ServiceSelected + " , Status Code : "
        //                            + Convert.ToString(responeAC.StatusCode)
        //                            + " , Respnse Message : " + responeAC.Message;

        //        await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(TransactionId), Convert.ToInt64(userId), Convert.ToInt64(EnumList.TransactionTraseLog.ExcelFileUpload), logDescription);
        //        #endregion


        //        return Ok(responeAC);
        //    }
        //}



        #endregion

        #endregion
    }
}