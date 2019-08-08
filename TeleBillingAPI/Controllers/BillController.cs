using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using TeleBillingRepository.Repository.BillDelegate;
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingRepository.Repository.Employee;
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
        #endregion

        #region "Constructor"
        public BillController(IBillUploadRepository iBillUploadRepository, ILogManagement ilogManagement
            ,IBillDelegateRepository ibillDelegateRepository
            , IEmployeeRepository iEmployeeRepository
            )
        {
            _iBillUploadRepository = iBillUploadRepository;
            _iLogManagement = ilogManagement;
            _ibillDelegateRepository = ibillDelegateRepository;

        }
        #endregion

        #region "Public Method(s)"



        [HttpGet]
        [Route("billuploadedlist")]
        public async Task<IActionResult> GetBillUplaodedList()
        {
            return Ok(await _iBillUploadRepository.GetBillUploadedList());
        }


        [HttpGet]
        [Route("deleteupload/{id}")]
        public async Task<IActionResult> DeleteExcelUpload(long id)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _iBillUploadRepository.DeleteExcelUplaod(Convert.ToInt64(userId), id));
        }

        [HttpPost]
        [Route("uploadnewbillold")]
        public async Task<IActionResult> UploadNewBillOLDBAckup([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        {
            try
            {

                BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);
                var file = billUploadFormDataAC.File;
                ExcelFileAC excelFileAC = new ExcelFileAC();
                ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
                List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();
                var currentUser = HttpContext.User;
                string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;

                #region --> Save File to temp Folder                
                if (file != null)
                {
                    string folderName = "TempUpload";
                    excelFileAC.File = file;
                    excelFileAC.FolderName = folderName;
                    billUploadModel.ExcelFileName1 = file.FileName;
                    exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
                }
                #endregion

                #region --> Get Excel mapping details

                if (billUploadModel != null)
                {
                    mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
                }

                #endregion

                ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
                ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
                ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListKems = new ImportBillDetailAC<InternetServiceUploadListAC>();
                AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();

                if (mappingExcellist != null)
                {
                    #region Read Data Service wise one by one
                    foreach (var mapservice in mappingExcellist)
                    {
                        MappingDetailAC mappingDetail = new MappingDetailAC();
                        if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
                        {
                            #region --> Read Excel file for mobility                           
                            mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.Mobility);
                            ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                            uploadDetailList = mobilityList;

                            #endregion

                            #region --> store in common class allserviceTypeData
                            allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
                            allserviceTypeData.InvalidList1 = mobilityList.UploadData.Data.InvalidMobilityList;
                            #endregion
                        }

                        else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
                        {
                            #region --> Read Excel file for mada GeneralService                           
                            mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
                            ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                            uploadDetailListMada = madaserviceList;
                            #endregion

                            #region --> store in common class allserviceTypeData
                            allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
                            allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
                            #endregion
                        }


                        else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                        {
                            #region --> Read Excel file for Internet service                           
                            mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
                            ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                            uploadDetailListKems = internetserviceList;
                            #endregion

                            #region --> store in common class allserviceTypeData
                            allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
                            allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
                            #endregion
                        }

                    }
                    #endregion
                }
                foreach (var mapservice in mappingExcellist)
                {
                    if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
                    {
                        SaveExcelResponseAC<MobilityExcelUploadDetailStringAC> responeAC = new SaveExcelResponseAC<MobilityExcelUploadDetailStringAC>();
                        responeAC.StatusCode = uploadDetailList.Status;
                        responeAC.Message = uploadDetailList.Message;
                        responeAC.TotalAmount = "0";
                        responeAC.TotalValidCount = "0";
                        if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                            || uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                           )
                        {
                            return Ok(responeAC);
                        }
                        else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.DataInvalid)
                        {
                            responeAC.UploadDataList = uploadDetailList.UploadData.Data.InvalidMobilityList;
                            return Ok(responeAC);
                        }
                        else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.Success
                            || uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid
                           )
                        {
                            #region --> Save Data to DB

                            long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

                            List<ExcelDetail> excelDetailList = uploadDetailList.UploadData.Data.ValidMobilityList;
                            excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

                            bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

                            #endregion
                            if (!IsSaved)
                            {
                                responeAC.Message = "Error occur During Excel Detail Data Saving ";
                                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                            }
                            else
                            {
                                responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
                                responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
                            }

                            responeAC.UploadDataList = uploadDetailList.UploadData.Data.InvalidMobilityList;
                        }
                       
                    }

                    if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
                    {
                        SaveExcelResponseAC<MadaExcelUploadDetailStringAC> responeAC = new SaveExcelResponseAC<MadaExcelUploadDetailStringAC>();
                        responeAC.StatusCode = uploadDetailListMada.Status;
                        responeAC.Message = uploadDetailListMada.Message;
                        responeAC.TotalAmount = "0";
                        responeAC.TotalValidCount = "0";
                        if (uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                            || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                           )
                        {
                            return Ok(responeAC);
                        }
                        else if (uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.DataInvalid)
                        {
                            responeAC.UploadDataList = uploadDetailListMada.UploadData.Data.InvalidList;
                            return Ok(responeAC);
                        }
                        else if (uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.Success
                            || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid
                           )
                        {
                            #region --> Save Data to DB

                            long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

                            List<ExcelDetail> excelDetailList = uploadDetailListMada.UploadData.Data.ValidList;
                            excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

                            bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

                            #endregion
                            if (!IsSaved)
                            {
                                responeAC.Message = "Error occur During Excel Detail Data Saving ";
                                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                            }
                            else
                            {
                                responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.InitialDiscountedMonthlyPriceKd)) + "KD (Initial Discounted Monthly Price)";
                                responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
                            }

                            responeAC.UploadDataList = uploadDetailListMada.UploadData.Data.InvalidList;
                        }
                        return Ok(responeAC);
                    }
                }

                ResponseAC responedefault = new ResponseAC();
                responedefault.Message = "Service function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseAC responeAC = new ResponseAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return Ok(responeAC);
            }
        }


        [HttpPost]
        [Route("uploadnewbillold2")]
        public async Task<IActionResult> UploadNewBillOld2([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        {
            try
            {

                BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);
                var file = billUploadFormDataAC.File;
                ExcelFileAC excelFileAC = new ExcelFileAC();
                ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
                List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();
                var currentUser = HttpContext.User;
                string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;

                #region --> Save File to temp Folder                
                if (file != null)
                {
                    string folderName = "TempUpload";
                    excelFileAC.File = file;
                    excelFileAC.FolderName = folderName;
                    billUploadModel.ExcelFileName1 = file.FileName;
                    exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
                }
                #endregion

                #region --> Get Excel mapping details

                if (billUploadModel != null)
                {
                    mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
                }

                #endregion

                ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
                ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
                ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListInternetservice = new ImportBillDetailAC<InternetServiceUploadListAC>();
                ImportBillDetailAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenter = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();
                ImportBillDetailAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHosting = new ImportBillDetailAC<ManagedHostingServiceUploadListAC>();

                AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();
                SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();

                if (mappingExcellist != null)
                {
                    if (mappingExcellist.Count() == 1)
                    {
                        #region Read Data Service wise one by one for single service selected at once
                        foreach (var mapservice in mappingExcellist)
                        {
                            MappingDetailAC mappingDetail = new MappingDetailAC();
                            if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
                            {
                                #region --> Read Excel file for mobility                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.Mobility);
                                ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailList = mobilityList;

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
                                allserviceTypeData.InvalidList1.AddRange(mobilityList.UploadData.Data.InvalidMobilityList);
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
                                ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListMada = madaserviceList;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
                                ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListInternetservice = internetserviceList;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility);
                                ImportBillDetailAC<DataCenterFacilityUploadListAC> dataCenterserviceList = await _iBillUploadRepository.ReadExcelForDataCenterFacility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListDataCenter = dataCenterserviceList;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(dataCenterserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList4 = dataCenterserviceList.UploadData.Data.InvalidList;
                                #endregion
                            }

                        }
                        #endregion
                    }else if(mappingExcellist.Count() > 1){

                        #region --> Read Data for Multiple service From Same file

                        MappingDetailAC mappingDetail = new MappingDetailAC();
                        int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
                        if (ServiceWithoutTitleCount > 1)
                        {
                            #region --> if mapping is missing for selected services
                            responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
                            responeAC.Message = "Multiple service without title is invalid !";
                            responeAC.TotalValidCount = "0";
                            responeAC.TotalAmount = "0";
                            return Ok(responeAC);
                            #endregion
                        }
                        else 
                        {
                            //Get first Reading for Multiple service in same excel
                            MultiServiceUploadAC multiServiceUploadAC = new MultiServiceUploadAC();
                            try
                            {
                                multiServiceUploadAC =  _iBillUploadRepository.getFirstReadingIndexWithService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingExcellist, billUploadModel);

                                if (multiServiceUploadAC.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                                {

                                }

                            }
                            catch (Exception)
                            {

                            }
                    

                        }
                     #endregion
                        }
                    else
                    {
                        #region --> if mapping is missing for selected services
                        responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                        responeAC.Message = "Mapping Details doesnot exists!";
                        responeAC.TotalValidCount = "0";
                        responeAC.TotalAmount = "0";
                        return Ok(responeAC);
                        #endregion
                    }
                }

                #region --> Save Data for all selected service at once                 

                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                   )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                    responeAC.Message = "Error during Reading";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }

                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                   )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                    responeAC.Message = "File Not Found";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }
                else if (allserviceTypeData.ValidList.Count() == 0)
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
                    responeAC.Message = "All Data Invalid";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }

                else if (allserviceTypeData.InvalidList1.Count() <= 0 && allserviceTypeData.ValidList.Count() > 0
                  && allserviceTypeData.InvalidList3.Count() <= 0 && allserviceTypeData.InvalidList4.Count() <= 0
                  && allserviceTypeData.InvalidList5.Count() <= 0 && allserviceTypeData.InvalidList9.Count() <= 0
                 )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
                    responeAC.Message = "All Uploaded Successfully";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    // return Ok(responeAC);
                }
                else if ((allserviceTypeData.InvalidList1.Count() > 0 || allserviceTypeData.InvalidList3.Count() > 0
                  || allserviceTypeData.InvalidList4.Count() > 0 || allserviceTypeData.InvalidList5.Count() > 0
                  || allserviceTypeData.InvalidList9.Count() > 0) && allserviceTypeData.ValidList.Count() > 0
                 )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.SomeDataInvalid;
                    responeAC.Message = "Some Data Upload With Error!";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    //return Ok(responeAC);
                }
                else if (allserviceTypeData.InvalidList1.Count() == 0 && allserviceTypeData.InvalidList3.Count() == 0
                        && allserviceTypeData.InvalidList4.Count() == 0 && allserviceTypeData.InvalidList5.Count() == 0
                        && allserviceTypeData.InvalidList9.Count() == 0 && allserviceTypeData.ValidList.Count() == 0
                     )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                    responeAC.Message = "Data doesnot exists!";
                    responeAC.TotalValidCount = "0";
                    responeAC.TotalAmount = "0";
                    return Ok(responeAC);
                }

                if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.Success
                  || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid)
                {
                    #region --> Save Data to DB

                    long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

                    List<ExcelDetail> excelDetailList = allserviceTypeData.ValidList;
                    excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

                    bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

                    #endregion
                    if (!IsSaved)
                    {
                        responeAC.Message = "Error occur During Excel Detail Data Saving ";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    }
                    else
                    {
                        responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
                        responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
                    }

                    responeAC.InvalidList1 = allserviceTypeData.InvalidList1;
                    responeAC.InvalidList3 = allserviceTypeData.InvalidList3;
                    responeAC.InvalidList4 = allserviceTypeData.InvalidList4;
                    responeAC.InvalidList5 = allserviceTypeData.InvalidList5;
                    responeAC.InvalidList9 = allserviceTypeData.InvalidList9;
                    return Ok(responeAC);
                }


                #endregion


                ResponseAC responedefault = new ResponseAC();
                responedefault.Message = "Service function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseAC responeAC = new ResponseAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return Ok(responeAC);
            }
        }


        [HttpPost]
        [Route("uploadnewbill")]
        public async Task<IActionResult> UploadNewBill([FromForm]BillUploadFormDataAC billUploadFormDataAC)
        {
            try
            {

                BillUploadAC billUploadModel = JsonConvert.DeserializeObject<BillUploadAC>(billUploadFormDataAC.BillUploadAc);
                var file = billUploadFormDataAC.File;
                ExcelFileAC excelFileAC = new ExcelFileAC();
                ExcelUploadResponseAC exceluploadDetail = new ExcelUploadResponseAC();
                List<MappingDetailAC> mappingExcellist = new List<MappingDetailAC>();
                var currentUser = HttpContext.User;
                string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;

                #region --> Save File to temp Folder                
                if (file != null)
                {
                    string folderName = "TempUpload";
                    excelFileAC.File = file;
                    excelFileAC.FolderName = folderName;
                    billUploadModel.ExcelFileName1 = file.FileName;
                    exceluploadDetail = _iBillUploadRepository.UploadNewExcel(excelFileAC);
                }
                #endregion

                #region --> Get Excel mapping details

                if (billUploadModel != null)
                {
                    mappingExcellist = await _iBillUploadRepository.GetExcelMapping(billUploadModel);
                }

                #endregion

                ImportBillDetailAC<MobilityUploadListAC> uploadDetailList = new ImportBillDetailAC<MobilityUploadListAC>();
                ImportBillDetailAC<MadaUploadListAC> uploadDetailListMada = new ImportBillDetailAC<MadaUploadListAC>();
                ImportBillDetailAC<InternetServiceUploadListAC> uploadDetailListInternetservice = new ImportBillDetailAC<InternetServiceUploadListAC>();
                ImportBillDetailAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenter = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();
                ImportBillDetailAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHosting = new ImportBillDetailAC<ManagedHostingServiceUploadListAC>();


                ImportBillDetailMultipleAC<MobilityUploadListAC> uploadDetailListM = new ImportBillDetailMultipleAC<MobilityUploadListAC>();
                ImportBillDetailMultipleAC<MadaUploadListAC> uploadDetailListMadaM = new ImportBillDetailMultipleAC<MadaUploadListAC>();
                ImportBillDetailMultipleAC<InternetServiceUploadListAC> uploadDetailListInternetserviceM = new ImportBillDetailMultipleAC<InternetServiceUploadListAC>();
                ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC> uploadDetailListDataCenterM = new ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>();
                ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC> uploadDetailListManagedHostingM = new ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>();

                AllServiceTypeDataAC allserviceTypeData = new AllServiceTypeDataAC();
                SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();

                if (mappingExcellist != null)
                {
                    if (mappingExcellist.Count() == 1)
                    {
                        #region Read Data Service wise one by one for single service selected at once
                        foreach (var mapservice in mappingExcellist)
                        {
                            MappingDetailAC mappingDetail = new MappingDetailAC();
                            if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.Mobility)
                            {
                                #region --> Read Excel file for mobility                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.Mobility);
                                ImportBillDetailAC<MobilityUploadListAC> mobilityList = await _iBillUploadRepository.ReadExcelForMobility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailList = mobilityList;

                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(mobilityList.UploadData.Data.ValidMobilityList);
                                allserviceTypeData.InvalidList1.AddRange(mobilityList.UploadData.Data.InvalidMobilityList);
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.GeneralServiceMada);
                                ImportBillDetailAC<MadaUploadListAC> madaserviceList = await _iBillUploadRepository.ReadExcelForMadaService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListMada = madaserviceList;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(madaserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList9 = madaserviceList.UploadData.Data.InvalidList;
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.InternetService);
                                ImportBillDetailAC<InternetServiceUploadListAC> internetserviceList = await _iBillUploadRepository.ReadExcelForInternetService(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListInternetservice = internetserviceList;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(internetserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList3 = internetserviceList.UploadData.Data.InvalidList;
                                #endregion
                            }

                            else if (mapservice.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility)
                            {
                                #region --> Read Excel file for mada GeneralService                           
                                mappingDetail = mappingExcellist.Find(x => x.ServiceTypeId == (long)EnumList.ServiceType.DataCenterFacility);
                                ImportBillDetailAC<DataCenterFacilityUploadListAC> dataCenterserviceList = await _iBillUploadRepository.ReadExcelForDataCenterFacility(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid, mappingDetail, billUploadModel);
                                uploadDetailListDataCenter = dataCenterserviceList;
                                #endregion

                                #region --> store in common class allserviceTypeData
                                allserviceTypeData.ValidList.AddRange(dataCenterserviceList.UploadData.Data.ValidList);
                                allserviceTypeData.InvalidList4 = dataCenterserviceList.UploadData.Data.InvalidList;
                                #endregion
                            }

                        }
                        #endregion
                    }

                    else if (mappingExcellist.Count() > 1)
                    {

                        #region --> Read Data for Multiple service From Same file

                        MappingDetailAC mappingDetail = new MappingDetailAC();

                        int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
                        if (ServiceWithoutTitleCount > 1)
                        {
                            #region --> if excel have two service without title we can not verify each record 
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

                            foreach(var worksheet in mappingExcellist.Select(p => p.WorkSheetNo).Distinct().ToList())
                            {

                                // Get first Reading Index with service
                                MultiServiceUploadAC responseIndexService = new MultiServiceUploadAC();
                                int worksheetNo = (int)worksheet;
                                int readingIndex = 0;
                                List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == (int)worksheet).ToList();
                                string[] ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();

                                try
                                {
                                    responseIndexService = _iBillUploadRepository.getReadingIndexWithServiceFromSingleWorksheet(
                                                                                  exceluploadDetail.FilePath, 
                                                                                  exceluploadDetail.FileNameGuid,
                                                                                  SingleWorksheetservice,
                                                                                  billUploadModel, ServiceTitle,worksheetNo);

                                    if (responseIndexService.ServiceTypeId>0)
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
                                                    allserviceTypeData.ValidList.AddRange(uploadDetailListInternetserviceM.UploadData.Data.ValidList);
                                                    allserviceTypeData.InvalidList3.AddRange(uploadDetailListInternetserviceM.UploadData.Data.InvalidList);
                                                    responseIndexService.ServiceTypeId = uploadDetailListInternetserviceM.ServiceTypeId;
                                                    responseIndexService.ReadingIndex = uploadDetailListInternetserviceM.ReadingIndex;
                                                    readingIndex = (int)responseIndexService.ReadingIndex;
                                                    mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

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
                                                    allserviceTypeData.ValidList.AddRange(uploadDetailListDataCenterM.UploadData.Data.ValidList);
                                                    allserviceTypeData.InvalidList4.AddRange(uploadDetailListDataCenterM.UploadData.Data.InvalidList);
                                                    responseIndexService.ServiceTypeId = uploadDetailListDataCenterM.ServiceTypeId;
                                                    responseIndexService.ReadingIndex = uploadDetailListDataCenterM.ReadingIndex;
                                                    readingIndex = (int)responseIndexService.ReadingIndex;
                                                    mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

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
                                                    allserviceTypeData.ValidList.AddRange(uploadDetailListManagedHostingM.UploadData.Data.ValidList);
                                                    allserviceTypeData.InvalidList5.AddRange(uploadDetailListManagedHostingM.UploadData.Data.InvalidList);
                                                    responseIndexService.ServiceTypeId = uploadDetailListManagedHostingM.ServiceTypeId;
                                                    responseIndexService.ReadingIndex = uploadDetailListManagedHostingM.ReadingIndex;
                                                    readingIndex = (int)responseIndexService.ReadingIndex;
                                                    mappingDetail = SingleWorksheetservice.FirstOrDefault(x => x.ServiceTypeId == responseIndexService.ServiceTypeId);

                                                }
                                            }

                                        }
                                        
                                    }

                                }
                                catch (Exception e)
                                {
                                    ResponseAC respone = new ResponseAC();
                                    respone.Message = "Error while excel reading multiple : " + e.Message;
                                    respone.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                    return Ok(respone);
                                }

                            }

                            #endregion -->  Worksheet No based Loop

                        }
                        #endregion
                    }

                    else
                    {
                        #region --> if mapping is missing for selected services
                        responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                        responeAC.Message = "Mapping Details doesnot exists!";
                        responeAC.TotalValidCount = "0";
                        responeAC.TotalAmount = "0";
                        return Ok(responeAC);
                        #endregion
                    }
                }

                #region --> Save Data for all selected service at once                 

                if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                    || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.ExceptionError
                   )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.ExceptionError;
                    responeAC.Message = "Error during Reading";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }

                else if (uploadDetailList.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListMada.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListInternetservice.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListDataCenter.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                    || uploadDetailListManagedHosting.Status == (int)EnumList.ExcelUploadResponseType.FileNotFound
                   )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.FileNotFound;
                    responeAC.Message = "File Not Found";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }
                else if (allserviceTypeData.ValidList.Count() == 0)
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.DataInvalid;
                    responeAC.Message = "All Data Invalid";
                    responeAC.TotalAmount = "0";
                    responeAC.TotalValidCount = "0";
                    return Ok(responeAC);
                }

                else if (allserviceTypeData.InvalidList1.Count() <= 0 && allserviceTypeData.ValidList.Count() > 0
                  && allserviceTypeData.InvalidList3.Count() <= 0 && allserviceTypeData.InvalidList4.Count() <= 0
                  && allserviceTypeData.InvalidList5.Count() <= 0 && allserviceTypeData.InvalidList9.Count() <= 0
                 )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
                    responeAC.Message = "All Uploaded Successfully";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    // return Ok(responeAC);
                }
                else if ((allserviceTypeData.InvalidList1.Count() > 0 || allserviceTypeData.InvalidList3.Count() > 0
                  || allserviceTypeData.InvalidList4.Count() > 0 || allserviceTypeData.InvalidList5.Count() > 0
                  || allserviceTypeData.InvalidList9.Count() > 0) && allserviceTypeData.ValidList.Count() > 0
                 )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.SomeDataInvalid;
                    responeAC.Message = "Some Data Upload With Error!";
                    responeAC.TotalValidCount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    responeAC.TotalAmount = Convert.ToString(allserviceTypeData.ValidList.Count());
                    //return Ok(responeAC);
                }
                else if (allserviceTypeData.InvalidList1.Count() == 0 && allserviceTypeData.InvalidList3.Count() == 0
                        && allserviceTypeData.InvalidList4.Count() == 0 && allserviceTypeData.InvalidList5.Count() == 0
                        && allserviceTypeData.InvalidList9.Count() == 0 && allserviceTypeData.ValidList.Count() == 0
                     )
                {
                    responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
                    responeAC.Message = "Data doesnot exists!";
                    responeAC.TotalValidCount = "0";
                    responeAC.TotalAmount = "0";
                    return Ok(responeAC);
                }

                if (responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.Success
                  || responeAC.StatusCode == (int)EnumList.ExcelUploadResponseType.SomeDataInvalid)
                {
                    #region --> Save Data to DB

                    long excelUploadLogId = await _iBillUploadRepository.AddExcelUploadLog(billUploadModel, exceluploadDetail.FileNameGuid, Convert.ToInt64(userId));

                    List<ExcelDetail> excelDetailList = allserviceTypeData.ValidList;
                    excelDetailList.ForEach(x => x.ExcelUploadLogId = excelUploadLogId);

                    bool IsSaved = await _iBillUploadRepository.AddExcelDetail(excelDetailList);

                    #endregion
                    if (!IsSaved)
                    {
                        responeAC.Message = "Error occur During Excel Detail Data Saving ";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    }
                    else
                    {
                        responeAC.TotalAmount = Convert.ToString(excelDetailList.Sum(x => x.CallAmount));
                        responeAC.TotalValidCount = Convert.ToString(excelDetailList.Count());
                    }

                    responeAC.InvalidList1 = allserviceTypeData.InvalidList1;
                    responeAC.InvalidList3 = allserviceTypeData.InvalidList3;
                    responeAC.InvalidList4 = allserviceTypeData.InvalidList4;
                    responeAC.InvalidList5 = allserviceTypeData.InvalidList5;
                    responeAC.InvalidList9 = allserviceTypeData.InvalidList9;
                    return Ok(responeAC);
                }


                #endregion


                ResponseAC responedefault = new ResponseAC();
                responedefault.Message = "Service function not found";
                responedefault.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return Ok(responedefault);
            }
            catch (Exception e)
            {
                ResponseAC responeAC = new ResponseAC();
                responeAC.Message = "Error while excel reading : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
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
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
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
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iBillUploadRepository.BillAllocation(billAllocationAC,Convert.ToInt64(userId)));
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
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _ibillDelegateRepository.AddDelegate(DelegateDetailAC, Convert.ToInt64(userId)));
        }


        [HttpPut]
        [Route("delegate/edit")]
        public async Task<IActionResult> EditDelegate(BillDelegatesAC DelegateDetailAC)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _ibillDelegateRepository.EditDelegate(DelegateDetailAC, Convert.ToInt64(userId)));
        }

        [HttpGet]
        [Route("delegate/delete/{id}")]
        public async Task<IActionResult> DeleteDelegates(long id)
        {
            var currentUser = HttpContext.User;
            string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(await _ibillDelegateRepository.DeleteDelegate(id, Convert.ToInt64(userId)));
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
		
		#endregion
	}
}