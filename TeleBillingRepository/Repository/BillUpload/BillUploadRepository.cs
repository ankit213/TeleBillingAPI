using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;

namespace TeleBillingRepository.Repository.BillUpload
{
    public class BillUploadRepository : IBillUploadRepository
    {
        #region "Private Variable(s)"
        private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;
        #endregion

        #region "Constructor"

        public BillUploadRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
            , ILogManagement ilogManagement, IHostingEnvironment ihostingEnvironment)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _mapper = mapper;
            _iLogManagement = ilogManagement;
            _hostingEnvironment = ihostingEnvironment;
        }
        #endregion

        #region Public Method(s)

        public async Task<List<BillUploadListAC>> GetBillUploadedList()
        {
            List<BillUploadListAC> billUploadLists = new List<BillUploadListAC>();
            List<ExcelUploadLog> exceluploadlist = await _dbTeleBilling_V01Context.ExcelUploadLog.Where(x => !x.IsDelete).Include(x => x.Provider).OrderByDescending(x => x.UploadDate).ToListAsync();
            foreach (var item in exceluploadlist)
            {
                BillUploadListAC billUploadListAC = new BillUploadListAC();
                billUploadListAC = _mapper.Map<BillUploadListAC>(item);
                billUploadLists.Add(billUploadListAC);
            }
            return billUploadLists;
        }

        public async Task<bool> DeleteExcelUplaod(long userId, long id)
        {
            ExcelUploadLog excelUploadLog = await _dbTeleBilling_V01Context.ExcelUploadLog.FirstOrDefaultAsync(x => x.Id == id);
            if (excelUploadLog != null)
            {
                excelUploadLog.IsDelete = true;
                excelUploadLog.UpdatedBy = userId;
                excelUploadLog.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(excelUploadLog);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                ExcelUploadLogServiceType excelUploadServices = await _dbTeleBilling_V01Context.ExcelUploadLogServiceType.FirstOrDefaultAsync(x => x.ExceluploadLogId == id);

                if (excelUploadServices != null)
                {
                    excelUploadServices.IsDelete = true;
                    excelUploadServices.UpdatedBy = userId;
                    excelUploadServices.UpdatedDate = DateTime.Now;
                    _dbTeleBilling_V01Context.Update(excelUploadServices);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                    return true;
                }

                return true;
            }
            return false;
        }

        public async Task<List<MappingDetailAC>> GetExcelMapping(BillUploadAC billUploadModel)
        {
            List<MappingExcel> mappingExcel = new List<MappingExcel>();
            List<MappingDetailAC> mappingDetails = new List<MappingDetailAC>();
            if (billUploadModel != null && billUploadModel.ProviderId > 0)
            {
                if (billUploadModel.ServiceTypes.Count > 0)
                {
                    List<long> serviceTypeIds = new List<long>();
                    serviceTypeIds = billUploadModel.ServiceTypes.Select(x => x.Id).ToList();
                    mappingExcel = await _dbTeleBilling_V01Context.MappingExcel.Where(x => x.ProviderId == billUploadModel.ProviderId && serviceTypeIds.Contains(x.ServiceTypeId) && !x.IsDelete && x.IsActive == true).Include(x => x.MappingExcelColumn).ToListAsync();
                }
            }

            if (mappingExcel != null && mappingExcel.Count > 0)
            {
                mappingDetails = _mapper.Map<List<MappingDetailAC>>(mappingExcel);
                foreach (var m in mappingDetails)
                {
                    var mapDBData = await _dbTeleBilling_V01Context.MappingExcelColumn
                                            .Where(x => x.MappingExcelId == m.Id)
                                            .Include(x => x.MappingServiceTypeField)
                                            .Select(x => new DBFiledMappingAC
                                            {
                                                MappingDetailId = x.MappingExcelId,
                                                DBColumnName = x.MappingServiceTypeField.DbcolumnName,
                                                ExcelcolumnName = x.ExcelcolumnName,
                                                FormatField = x.FormatField
                                            })
                                            .ToListAsync();
                    m.DBFiledMappingList = mapDBData.ToList();
                }
            }
            return mappingDetails;
        }

        public ExcelUploadResponseAC UploadNewExcel(ExcelFileAC excelFileAC)
        {
            ExcelUploadResponseAC excelUploadResponse = new ExcelUploadResponseAC();
            try
            {
                #region --> Save File to temp Folder

                var file = excelFileAC.File;
                string folderName = excelFileAC.FolderName;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                string excelfilenameGuid = string.Empty;
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
                if (file.Length > 0)
                {
                    excelUploadResponse.FileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    excelfilenameGuid = Guid.NewGuid().ToString() + "." + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Split(".")[1].Trim('"');
                    // excelfilenameGuid = Guid.NewGuid().ToString() + ".xlsx";

                    string fullPath = Path.Combine(newPath, excelfilenameGuid);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }

                excelUploadResponse.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                excelUploadResponse.Message = "File Uplaoded Successfully";
                excelUploadResponse.FileNameGuid = excelfilenameGuid;
                excelUploadResponse.FilePath = newPath;
                #endregion
            }
            catch (Exception e)
            {
                excelUploadResponse.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                excelUploadResponse.Message = "Error while excel uploading :" + e.Message;
                return excelUploadResponse;
            }

            return excelUploadResponse;
        }

        public string getAddress(List<DBFiledMappingAC> DBFiledMappingList, string dbColumn, int index)
        {
            string cellAddress = string.Empty;
            if (DBFiledMappingList != null)
            {
                cellAddress = DBFiledMappingList.Where(x => x.DBColumnName == dbColumn).Select(x => x.ExcelcolumnName).FirstOrDefault();
                cellAddress = Convert.ToString(cellAddress) + Convert.ToString(index);
            }
            return cellAddress;
        }

        public string getFormatField(List<DBFiledMappingAC> DBFiledMappingList, string dbColumn)
        {
            string formatField = string.Empty;
            if (DBFiledMappingList != null)
            {
                formatField = DBFiledMappingList.Where(x => x.DBColumnName == dbColumn).Select(x => x.FormatField).FirstOrDefault();
                formatField = Convert.ToString(formatField);
            }
            return formatField;
        }

        public string checkPriceValidation(string dbColumn, string Value, Type valueType, bool isRequired)
        {
            string ErrorMessage = "valid";
            string PriceValueStr = Value;
            try
            {
                if (!string.IsNullOrEmpty(PriceValueStr))
                {

                    if ((valueType == typeof(string)))
                    {
                        decimal number1;
                        double number2;
                        long number3;
                        if ((!decimal.TryParse(PriceValueStr, out number1)) && (!double.TryParse(PriceValueStr, out number2)) && (!long.TryParse(PriceValueStr, out number3)))
                        {
                            ErrorMessage = dbColumn + " must be numeric ! ";
                        }
                    }
                    else
                    {
                        if ((valueType != typeof(decimal)) && (valueType != typeof(double)) && (valueType != typeof(long)))
                        {
                            ErrorMessage = dbColumn + " must be numeric ! ";
                        }

                    }
                }
                else
                {
                    if (isRequired)
                        ErrorMessage = dbColumn + " doesnot exists ! ";

                }
                return ErrorMessage;
            }
            catch (Exception)
            {
                ErrorMessage = dbColumn + " must be numeric ! ";
                return ErrorMessage;
            }

        }

        public async Task<bool> GetServiceChargeType(long serviceTypeId)
        {
            bool IsBusiness = await _dbTeleBilling_V01Context.FixServiceType.Where(x => x.Id == serviceTypeId).Select(x => x.IsBusinessOnly).FirstOrDefaultAsync();

            if (IsBusiness)
                return true;
            else
                return false;
        }

        public async Task<BillAllocationListAC> GetBillAllocationList(long providerId, int month, int year)
        {
            BillAllocationListAC billAllocationResponse = new BillAllocationListAC();
            ExcelUploadLog excelUploadLog = await _dbTeleBilling_V01Context.ExcelUploadLog.FirstOrDefaultAsync(x => x.Month == month && x.Year == year && x.ProviderId == providerId);
            if (excelUploadLog != null)
            {



            }
            return billAllocationResponse;
        }

        public async Task<long> AddExcelUploadLog(BillUploadAC billUploadModel, string fileNameGuid, long userId)
        {
            ExcelUploadLog excelUploadLog = new ExcelUploadLog();
            excelUploadLog.ProviderId = billUploadModel.ProviderId;
            excelUploadLog.Month = billUploadModel.MonthId;
            excelUploadLog.Year = billUploadModel.YearId;
            excelUploadLog.DeviceId = billUploadModel.DeviceId;
            excelUploadLog.IsPbxupload = billUploadModel.DeviceId > 0 ? true : false;
            excelUploadLog.ExcelFileName = billUploadModel.ExcelFileName1 ?? fileNameGuid;
            excelUploadLog.FileNameGuid = fileNameGuid;
            excelUploadLog.UploadBy = userId;
            excelUploadLog.UploadDate = DateTime.Now;
            excelUploadLog.IsDelete = false;
            excelUploadLog.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

            await _dbTeleBilling_V01Context.AddAsync(excelUploadLog);
            await _dbTeleBilling_V01Context.SaveChangesAsync();

            List<ExcelUploadLogServiceType> excelUploadLogServiceType = new List<ExcelUploadLogServiceType>();
            foreach (var services in billUploadModel.ServiceTypes)
            {
                excelUploadLogServiceType.Add(new ExcelUploadLogServiceType
                {
                    ServiceTypeId = services.Id,
                    ExceluploadLogId = excelUploadLog.Id
                });
            }

            await _dbTeleBilling_V01Context.AddRangeAsync(excelUploadLogServiceType);
            await _dbTeleBilling_V01Context.SaveChangesAsync();

            return excelUploadLog.Id;
        }

        public async Task<bool> AddExcelDetail(List<ExcelDetail> excelDetailList)
        {
            try
            {
                await _dbTeleBilling_V01Context.AddRangeAsync(excelDetailList);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<ImportBillDetailAC<MobilityUploadListAC>> ReadExcelForMobilityold(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<MobilityUploadListAC> importBillDetail = new ImportBillDetailAC<MobilityUploadListAC>();

            try
            {
                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.Mobility);

                MobilityUploadListAC mobilityUploadListAC = new MobilityUploadListAC();
                #region --> Read Excel file

                FileInfo fileinfo = new FileInfo(Path.Combine(filepath, filename));
                using (ExcelPackage package = new ExcelPackage(fileinfo))
                {

                    #region get Mapping settings
                    int worksheetno = 0;
                    int readingIndex = 0;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);

                    }
                    #endregion
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[worksheetno];
                    int totalRows = workSheet.Dimension.Rows;
                    List<ExcelDetail> datalist = new List<ExcelDetail>();
                    List<MobilityExcelUploadDetailStringAC> datalistInvalid = new List<MobilityExcelUploadDetailStringAC>();

                    for (int i = readingIndex; i <= totalRows; i++)
                    {
                        try
                        {
                            bool IsFullValid = true;
                            string ErrorMessageSummary = string.Empty;

                            #region --> Call Date Required and Format Validation Part

                            string CallDateStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDate", i)].Value.ToString()));
                            string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
                            if (!string.IsNullOrEmpty(CallDateStr))
                            {
                                DateTime dt;
                                string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss" };
                                if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

                            }
                            #endregion

                            #region --> Call Time Required and Format Validation Part

                            string CallTimeStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallTime", i)].Value);
                            string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
                            if (!string.IsNullOrEmpty(CallTimeStr))
                            {
                                DateTime dt;
                                string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss", "dd-MM-yyyy HH:mm:ss" };
                                if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";

                            }
                            #endregion

                            #region --> Call Duration Required and Format Validation Part
                            long DuractionSeconds = 0;
                            string CallDurationStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDuration", i)].Value);

                            string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
                            if (!string.IsNullOrEmpty(CallDurationStr))
                            {

                                if (durationFormat == "seconds")
                                {
                                    long number;
                                    if (!long.TryParse(CallDurationStr, out number))
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
                                    }
                                }
                                else
                                {
                                    if (workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDuration", i)].Value.GetType() == typeof(DateTime) || workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDuration", i)].Value.GetType() == typeof(TimeSpan))
                                    {
                                        DateTime dt1 = DateTime.Parse(CallDurationStr);
                                        CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
                                    }
                                    //string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

                                    DateTime dt;
                                    string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
                                    if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
                                    }
                                    else
                                    {
                                        TimeSpan ts = TimeSpan.Parse(CallDurationStr);
                                        double totalSeconds = ts.TotalSeconds;
                                        DuractionSeconds = Convert.ToInt64(totalSeconds);
                                    }
                                }

                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";

                            }
                            #endregion

                            #region --> Call Amount Required and Format Validation Part

                            string CallAmountStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value.ToString());
                            if (!string.IsNullOrEmpty(CallAmountStr))
                            {
                                decimal number;
                                if (!decimal.TryParse(CallAmountStr, out number))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
                            }
                            #endregion

                            #region --> Call Number Required and Format Validation Part

                            string CallNumberStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value.ToString());
                            if (!string.IsNullOrEmpty(CallNumberStr))
                            {

                                if (!(CallNumberStr.Length > 5))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
                            }
                            #endregion

                            if (IsFullValid)
                            {
                                ExcelDetail data = new ExcelDetail();
                                // --> Required Field Data--------------------
                                data.CallerName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerName", i)].Value);
                                string callTransactionType = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallType", i)].Value);
                                data.CallTransactionTypeId = (await _dbTeleBilling_V01Context.TransactionTypeSetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == callTransactionType.Trim().ToLower()))?.Id;
                                data.TransType = callTransactionType;
                                data.CallerNumber = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value);
                                data.CallAmount = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value));
                                data.CallDate = Convert.ToDateTime(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDate", i)].Value));
                                data.CallTime = Convert.ToDateTime(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallTime", i)].Value)).TimeOfDay;
                                // Call duration hh:mm:ss to long convert and stored
                                data.CallDuration = DuractionSeconds;// Convert.ToInt64(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDuration", i)].Value.ToString());

                                // --> Optional Field Data--------------------
                                if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                {
                                    data.Description = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Description", i)].Value);

                                }
                                if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                {
                                    data.SubscriptionType = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SubscriptionType", i)].Value);

                                }

                                data.CallWithinGroup = false;
                                data.SiteName = string.Empty;
                                data.GroupDetail = string.Empty;
                                data.Bandwidth = string.Empty;
                                data.MonthlyPrice = null;
                                data.CommentOnPrice = string.Empty;
                                data.CommentOnBandwidth = string.Empty;
                                data.ReceiverNumber = string.Empty;
                                data.ReceiverName = string.Empty;

                                data.BusinessUnitId = null;
                                data.EmployeeId = (await _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstOrDefaultAsync(x => x.TelephoneNumber == data.CallerNumber && !x.IsDelete && x.IsActive))?.EmployeeId;
                                data.ServiceTypeId = (long)EnumList.ServiceType.Mobility;
                                data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                data.ExcelUploadLogId = 0;



                                data.ExcelUploadLogId = 0;
                                data.GroupId = null;
                                data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                data.AssignType = null;

                                if (isBusinessOnly)
                                {
                                    data.AssignType = (long)EnumList.AssignType.Employee;
                                }
                                else
                                {
                                    #region Get Assign Type Logic                            
                                    if (data.CallTransactionTypeId > 0)
                                    {
                                        long? ChargeType = (await _dbTeleBilling_V01Context.TransactionTypeSetting.FindAsync(data.CallTransactionTypeId))?.SetTypeAs;
                                        if (ChargeType > 0)
                                        {
                                            data.AssignType = ChargeType;
                                        }
                                        else
                                        {
                                            if (data.EmployeeId > 0)
                                            {
                                                data.AssignType = (long)EnumList.AssignType.Employee;
                                            }
                                            else if (data.BusinessUnitId > 0)
                                            {
                                                data.AssignType = (long)EnumList.AssignType.Business;
                                            }
                                        }
                                    }
                                    #endregion
                                }

                                datalist.Add(data);
                            }
                            else
                            {
                                string dDescription = string.Empty;
                                // --> Optional Field Data--------------------
                                if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                {
                                    dDescription = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Description", i)].Value);

                                }

                                datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
                                {
                                    CallerName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerName", i)].Value),
                                    CallType = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallType", i)].Value),
                                    Description = dDescription,//Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Description", i)].Value),
                                    CallerNumber = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value),
                                    CallAmount = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value),
                                    CallDate = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDate", i)].Value),
                                    CallTime = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallTime", i)].Value),
                                    CallDuration = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDuration", i)].Value),
                                    ErrorMessage = ErrorMessageSummary,

                                });
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.GetType() != typeof(System.NullReferenceException))
                            {
                                string dDescription = string.Empty;
                                // --> Optional Field Data--------------------
                                if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                {
                                    dDescription = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Description", i)].Value);

                                }
                                datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
                                {
                                    CallerName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerName", i)].Value),
                                    CallType = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallType", i)].Value),
                                    Description = dDescription,// Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Description", i)].Value),
                                    CallerNumber = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value),
                                    CallAmount = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value),
                                    CallDate = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDate", i)].Value),
                                    CallTime = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallTime", i)].Value),
                                    CallDuration = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallDuration", i)].Value),
                                    ErrorMessage = "Error :" + e.Message
                                });
                            }
                        }
                    }

                    mobilityUploadListAC.InvalidMobilityList = datalistInvalid;
                    mobilityUploadListAC.ValidMobilityList = datalist;
                    ResponseDynamicDataAC<MobilityUploadListAC> responseData = new ResponseDynamicDataAC<MobilityUploadListAC>();
                    responseData.Data = mobilityUploadListAC;
                    importBillDetail.UploadData = responseData;

                    if (datalistInvalid.Count > 0 && datalist.Count > 0)
                    {
                        importBillDetail.Message = "Some Data Upload With Error!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                    }
                    else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                    {
                        importBillDetail.Message = "All Data With Error!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                    }
                    if (datalistInvalid.Count == 0 && datalist.Count > 0)
                    {
                        importBillDetail.Message = "All Data Upload!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                    }

                    #region --> Delete File Upload after reading Successful

                    if (File.Exists(Path.Combine(filepath, filename)))
                        File.Delete(Path.Combine(filepath, filename));

                    #endregion

                    return importBillDetail;
                }
                #endregion                
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }
        }

        public async Task<ImportBillDetailAC<MadaUploadListAC>> ReadExcelForMadaServiceold(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<MadaUploadListAC> importBillDetail = new ImportBillDetailAC<MadaUploadListAC>();

            try
            {
                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.GeneralServiceMada);


                MadaUploadListAC madaUploadListAC = new MadaUploadListAC();
                #region --> Read Excel file

                FileInfo fileinfo = new FileInfo(Path.Combine(filepath, filename));
                using (ExcelPackage package = new ExcelPackage(fileinfo))
                {

                    #region get Mapping settings
                    int worksheetno = 0;
                    int readingIndex = 0;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);

                    }
                    #endregion
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[worksheetno];

                    int totalRows = workSheet.Dimension.Rows;
                    List<ExcelDetail> datalist = new List<ExcelDetail>();
                    List<MadaExcelUploadDetailStringAC> datalistInvalid = new List<MadaExcelUploadDetailStringAC>();

                    for (int i = readingIndex; i <= totalRows; i++)
                    {
                        try
                        {
                            bool IsFullValid = true;
                            string ErrorMessageSummary = string.Empty;

                            #region --> Site Name Required and Format Validation Part

                            string SiteNameStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value));
                            if (!string.IsNullOrEmpty(SiteNameStr))
                            {
                                if (workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value.GetType() != typeof(string))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                            }
                            #endregion

                            #region --> Bandwidth Required and Format Validation Part

                            string BandwidthStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value));
                            Type valueTypeofBandwidth = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value.GetType();
                            if (!string.IsNullOrEmpty(BandwidthStr))
                            {
                                // if ((valueTypeofBandwidth != typeof(decimal)) && (valueTypeofBandwidth != typeof(double)) && (valueTypeofBandwidth != typeof(long)))
                                if ((valueTypeofBandwidth != typeof(string)))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " Bandwidth Name doesnot exists ! ";

                            }
                            #endregion

                            #region --> Price Validatition Required and Format Validation Part
                            string PriceStr = string.Empty;
                            string Message = string.Empty;
                            Type valueType;

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value.GetType();
                            Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", i)].Value.GetType();
                            Message = checkPriceValidation("FinalAnnualChargesKD", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", i)].Value.GetType();
                            Message = checkPriceValidation("InitialDiscountedMonthlyPriceKD", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", i)].Value.GetType();
                            Message = checkPriceValidation("InitialDiscountedAnnualPriceKD", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", i)].Value.GetType();
                            Message = checkPriceValidation("InitialDiscountedSavingMonthlyKD", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", i)].Value.GetType();
                            Message = checkPriceValidation("InitialDiscountedSavingYearlyKD", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }

                            #endregion

                            if (IsFullValid)
                            {
                                ExcelDetail data = new ExcelDetail();
                                data.SiteName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value);
                                data.ServiceDetail = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", i)].Value);
                                data.Bandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value);
                                data.CostCentre = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CostCentre", i)].Value);



                                data.InitialDiscountedAnnualPriceKd = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", i)].Value));
                                data.InitialDiscountedMonthlyPriceKd = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", i)].Value));
                                data.InitialDiscountedSavingMonthlyKd = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", i)].Value));
                                data.InitialDiscountedSavingYearlyKd = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", i)].Value));
                                data.MonthlyPrice = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value));
                                data.FinalAnnualChargesKd = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", i)].Value));

                                data.ExcelUploadLogId = 0;
                                data.ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada;
                                data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                datalist.Add(data);
                            }
                            else
                            {
                                datalistInvalid.Add(new MadaExcelUploadDetailStringAC
                                {
                                    SiteName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value),
                                    ServiceDetail = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", i)].Value),
                                    Bandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value),
                                    CostCentre = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CostCentre", i)].Value),

                                    InitialDiscountedAnnualPriceKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", i)].Value),
                                    InitialDiscountedMonthlyPriceKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", i)].Value),
                                    InitialDiscountedSavingMonthlyKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", i)].Value),
                                    InitialDiscountedSavingYearlyKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", i)].Value),
                                    MonthlyPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value),
                                    FinalAnnualChargesKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", i)].Value),

                                    ErrorMessage = ErrorMessageSummary
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.GetType() != typeof(System.NullReferenceException))
                            {
                                datalistInvalid.Add(new MadaExcelUploadDetailStringAC
                                {
                                    SiteName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value),
                                    ServiceDetail = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", i)].Value),
                                    Bandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value),
                                    CostCentre = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CostCentre", i)].Value),

                                    InitialDiscountedAnnualPriceKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKd", i)].Value),
                                    InitialDiscountedMonthlyPriceKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKd", i)].Value),
                                    InitialDiscountedSavingMonthlyKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKd", i)].Value),
                                    InitialDiscountedSavingYearlyKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKd", i)].Value),
                                    MonthlyPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value),
                                    FinalAnnualChargesKd = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKd", i)].Value),

                                    ErrorMessage = "Error : " + e.Message
                                });
                            }
                        }
                    }

                    madaUploadListAC.InvalidList = datalistInvalid;
                    madaUploadListAC.ValidList = datalist;
                    ResponseDynamicDataAC<MadaUploadListAC> responseData = new ResponseDynamicDataAC<MadaUploadListAC>();
                    responseData.Data = madaUploadListAC;
                    importBillDetail.UploadData = responseData;

                    if (datalistInvalid.Count > 0 && datalist.Count > 0)
                    {
                        importBillDetail.Message = "Some Data Upload With Error!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                    }
                    else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                    {
                        importBillDetail.Message = "All Data With Error!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                    }
                    if (datalistInvalid.Count == 0 && datalist.Count > 0)
                    {
                        importBillDetail.Message = "All Data Upload!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                    }

                    #region --> Delete File Upload after reading Successful

                    if (File.Exists(Path.Combine(filepath, filename)))
                        File.Delete(Path.Combine(filepath, filename));

                    #endregion

                    return importBillDetail;
                }
                #endregion                
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }
        }

        public async Task<ImportBillDetailAC<InternetServiceUploadListAC>> ReadExcelForInternetServiceold(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<InternetServiceUploadListAC> importBillDetail = new ImportBillDetailAC<InternetServiceUploadListAC>();

            try
            {
                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.InternetService);

                InternetServiceUploadListAC UploadListAC = new InternetServiceUploadListAC();

                #region --> Read Excel file

                FileInfo fileinfo = new FileInfo(Path.Combine(filepath, filename));
                using (ExcelPackage package = new ExcelPackage(fileinfo))
                {

                    #region get Mapping settings
                    int worksheetno = 0;
                    int readingIndex = 0;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);

                    }
                    #endregion
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[worksheetno];

                    int totalRows = workSheet.Dimension.Rows;
                    List<ExcelDetail> datalist = new List<ExcelDetail>();
                    List<InternetServiceExcelUploadDetailStringAC> datalistInvalid = new List<InternetServiceExcelUploadDetailStringAC>();

                    for (int i = readingIndex; i <= totalRows; i++)
                    {
                        try
                        {
                            bool IsFullValid = true;
                            string ErrorMessageSummary = string.Empty;

                            #region --> Site Name Required and Format Validation Part

                            string SiteNameStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value));
                            if (!string.IsNullOrEmpty(SiteNameStr))
                            {
                                if (workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value.GetType() != typeof(string))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                }
                            }
                            else
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                            }
                            #endregion

                            #region --> Bandwidth Required and Format Validation Part

                            string BandwidthStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value));
                            Type valueTypeofBandwidth = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value.GetType();
                            if (!string.IsNullOrEmpty(BandwidthStr))
                            {
                                if ((valueTypeofBandwidth != typeof(string)))
                                {
                                    IsFullValid = false;
                                    ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                }
                            }

                            #endregion

                            #region --> Price Validatition Required and Format Validation Part
                            string PriceStr = string.Empty;
                            string Message = string.Empty;
                            Type valueType;

                            PriceStr = Convert.ToString((workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value));
                            valueType = workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value.GetType();
                            Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                            if (Message != "valid")
                            {
                                IsFullValid = false;
                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                            }


                            #endregion

                            if (IsFullValid)
                            {
                                ExcelDetail data = new ExcelDetail();
                                data.SiteName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value);
                                data.GroupDetail = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", i)].Value);
                                data.Bandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value);
                                data.BusinessUnit = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", i)].Value);
                                data.MonthlyPrice = Convert.ToDecimal(Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value));
                                data.CommentOnPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", i)].Value);
                                data.CommentOnBandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", i)].Value);

                                data.ExcelUploadLogId = 0;
                                data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
                                data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                //data.AssignType = (long)EnumList.AssignType.Business;
                                data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                datalist.Add(data);
                            }
                            else
                            {
                                datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
                                {
                                    ServiceName = "Internet Service",
                                    SiteName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value),
                                    GroupDetail = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", i)].Value),
                                    Bandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value),
                                    BusinessUnit = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", i)].Value),
                                    MonthlyPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value),
                                    CommentOnPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", i)].Value),
                                    CommentOnBandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", i)].Value),
                                    ErrorMessage = ErrorMessageSummary
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.GetType() != typeof(System.NullReferenceException))
                            {
                                datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
                                {
                                    ServiceName = "Internet Service",
                                    SiteName = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "SiteName", i)].Value),
                                    GroupDetail = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", i)].Value),
                                    Bandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", i)].Value),
                                    BusinessUnit = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", i)].Value),
                                    MonthlyPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", i)].Value),
                                    CommentOnPrice = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", i)].Value),
                                    CommentOnBandwidth = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", i)].Value),

                                    ErrorMessage = "Error : " + e.Message
                                });
                            }
                        }
                    }

                    UploadListAC.InvalidList = datalistInvalid;
                    UploadListAC.ValidList = datalist;
                    ResponseDynamicDataAC<InternetServiceUploadListAC> responseData = new ResponseDynamicDataAC<InternetServiceUploadListAC>();
                    responseData.Data = UploadListAC;
                    importBillDetail.UploadData = responseData;

                    if (datalistInvalid.Count > 0 && datalist.Count > 0)
                    {
                        importBillDetail.Message = "Some Data Upload With Error!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                    }
                    else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                    {
                        importBillDetail.Message = "All Data With Error!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                    }
                    if (datalistInvalid.Count == 0 && datalist.Count > 0)
                    {
                        importBillDetail.Message = "All Data Upload!";
                        importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                    }

                    #region --> Delete File Upload after reading Successful

                    if (File.Exists(Path.Combine(filepath, filename)))
                        File.Delete(Path.Combine(filepath, filename));

                    #endregion

                    return importBillDetail;
                }
                #endregion                
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }
        }

        //  new Methods based on Npoi packagefor .xls and excel reader
        public async Task<ImportBillDetailAC<MobilityUploadListAC>> ReadExcelForMobility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<MobilityUploadListAC> importBillDetail = new ImportBillDetailAC<MobilityUploadListAC>();
            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<MobilityExcelUploadDetailStringAC> datalistInvalid = new List<MobilityExcelUploadDetailStringAC>();

            try
            {
                MobilityUploadListAC mobilityUploadListAC = new MobilityUploadListAC();
                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.Mobility);

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = 0;
                    int readingIndex = 0;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
                    }

                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    IRow headerRow = sheet.GetRow(0);

                    if (headerRow != null)
                    {
                        int cellCount = headerRow.LastCellNum;
                        int rowcount = sheet.LastRowNum;
                        for (int j = readingIndex; j <= rowcount + 1; j++)
                        {
                            int intIndex = 0;
                            intIndex = (j > 0 ? j - 1 : j);
                            IRow row = sheet.GetRow(intIndex);

                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    bool IsFullValid = true;
                                    string ErrorMessageSummary = string.Empty;

                                    #region --> Call Date Required and Format Validation Part

                                    var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
                                    string CallDateStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
                                    string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
                                    if (!string.IsNullOrEmpty(CallDateStr))
                                    {
                                        DateTime dt;
                                        string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy" };
                                        if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Call Time Required and Format Validation Part

                                    var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
                                    string CallTimeStr = getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
                                    string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
                                    if (!string.IsNullOrEmpty(CallTimeStr))
                                    {
                                        DateTime dt;
                                        string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
                                                             "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
                                        if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Call Duration Required and Format Validation Part
                                    long DuractionSeconds = 0;
                                    string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
                                    var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

                                    string CallDurationStr = getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));

                                    if (!string.IsNullOrEmpty(CallDurationStr))
                                    {

                                        if (durationFormat == "seconds")
                                        {
                                            long number;
                                            if (!long.TryParse(CallDurationStr, out number))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
                                            }
                                        }
                                        else
                                        {
                                            if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
                                            {
                                                DateTime dt1 = DateTime.Parse(CallDurationStr);
                                                CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
                                            }
                                            //string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

                                            DateTime dt;
                                            string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
                                            if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
                                            }
                                            else
                                            {
                                                TimeSpan ts = TimeSpan.Parse(CallDurationStr);
                                                double totalSeconds = ts.TotalSeconds;
                                                DuractionSeconds = Convert.ToInt64(totalSeconds);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Call Amount Required and Format Validation Part

                                    string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
                                    //string CallAmountStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value.ToString());
                                    if (!string.IsNullOrEmpty(CallAmountStr))
                                    {
                                        decimal number;
                                        if (!decimal.TryParse(CallAmountStr, out number))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
                                    }
                                    #endregion


                                    #region --> Call Number Required and Format Validation Part
                                    string CallNumberStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
                                    //string CallNumberStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value.ToString());
                                    if (!string.IsNullOrEmpty(CallNumberStr))
                                    {

                                        if (!(CallNumberStr.Length > 5))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid";
                                        }
                                        else
                                        {
                                            long? EmployeeId = (await _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstOrDefaultAsync(x => x.TelephoneNumber == CallNumberStr && !x.IsDelete && x.IsActive))?.EmployeeId;

                                            if (EmployeeId == null || EmployeeId <= 0)
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not allocated to employee!";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
                                    }
                                    #endregion

                                    //---------------------
                                    #region --> TransType Validation 
                                    string CallTransTypeStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(CallTransTypeStr))
                                    {
                                        long? CallTransactionTypeId = (await _dbTeleBilling_V01Context.TransactionTypeSetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == CallTransTypeStr.Trim().ToLower()))?.Id;

                                        if (CallTransactionTypeId == null || CallTransactionTypeId <= 0)
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + "Trans Type is not defined!";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + "Trans Type doesnot exists ! ";
                                    }

                                    #endregion

                                    //-------------------------

                                    if (IsFullValid)
                                    {
                                        ExcelDetail data = new ExcelDetail();
                                        // --> Required Field Data--------------------
                                        data.CallerName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet, (long)EnumList.SupportDataType.String);
                                        string callTransactionType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.CallTransactionTypeId = (await _dbTeleBilling_V01Context.TransactionTypeSetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == callTransactionType.Trim().ToLower()))?.Id;
                                        data.TransType = callTransactionType;
                                        data.CallerNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.CallDate = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date));
                                        data.CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
                                        // Call duration hh:mm:ss to long convert and stored
                                        data.CallDuration = DuractionSeconds;

                                        // --> Optional Field Data--------------------
                                        if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                        {
                                            data.Description = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet, (long)EnumList.SupportDataType.String);
                                        }
                                        if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                        {
                                            data.SubscriptionType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SubscriptionType", j), sheet, (long)EnumList.SupportDataType.String);
                                        }
                                        data.CallWithinGroup = false;
                                        data.SiteName = string.Empty;
                                        data.GroupDetail = string.Empty;
                                        data.Bandwidth = string.Empty;
                                        data.MonthlyPrice = null;
                                        data.CommentOnPrice = string.Empty;
                                        data.CommentOnBandwidth = string.Empty;
                                        data.ReceiverNumber = string.Empty;
                                        data.ReceiverName = string.Empty;

                                        data.EmployeeId = (await _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstOrDefaultAsync(x => x.TelephoneNumber == data.CallerNumber && !x.IsDelete && x.IsActive))?.EmployeeId;
                                        data.ServiceTypeId = (long)EnumList.ServiceType.Mobility;
                                        data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                        data.ExcelUploadLogId = 0;
                                        MstEmployee mstemp = new MstEmployee();
                                        mstemp = (await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == data.EmployeeId));
                                        if (mstemp != null)
                                        {
                                            data.BusinessUnitId = mstemp.BusinessUnitId;
                                            data.CostCenterId = mstemp.CostCenterId;
                                        }
                                        data.ExcelUploadLogId = 0;
                                        data.GroupId = null;
                                        data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                        data.AssignType = null;

                                        if (isBusinessOnly)
                                        {
                                            data.AssignType = (long)EnumList.AssignType.Business;
                                        }
                                        else
                                        {
                                            //if anount is negative than it will be always assigned to Business charge 
                                            if (data.CallAmount < 0)
                                            {
                                                data.AssignType = (long)EnumList.AssignType.Business;
                                            }
                                            else
                                            {
                                                #region Get Assign Type Logic from Trans Type
                                                if (data.CallTransactionTypeId > 0)
                                                {
                                                    long? ChargeType = (await _dbTeleBilling_V01Context.TransactionTypeSetting.FindAsync(data.CallTransactionTypeId))?.SetTypeAs;
                                                    if (ChargeType > 0)
                                                    {
                                                        data.AssignType = ChargeType;
                                                    }
                                                    else
                                                    {
                                                        if (data.EmployeeId > 0)
                                                        {
                                                            data.AssignType = (long)EnumList.AssignType.Employee;
                                                        }
                                                        else if (data.BusinessUnitId > 0)
                                                        {
                                                            data.AssignType = (long)EnumList.AssignType.Business;
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                        }



                                        datalist.Add(data);
                                    }
                                    else
                                    {
                                        string dDescription = string.Empty;
                                        // --> Optional Field Data--------------------
                                        if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                        {
                                            dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);

                                        }

                                        datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
                                        {
                                            CallerName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet),
                                            CallType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet),
                                            Description = dDescription,
                                            CallerNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
                                            CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
                                            CallDate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
                                            CallTime = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
                                            CallDuration = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
                                            ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                        });
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (e.GetType() != typeof(System.NullReferenceException))
                                    {
                                        string dDescription = string.Empty;
                                        // --> Optional Field Data--------------------
                                        if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
                                        {
                                            dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);
                                        }

                                        datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
                                        {
                                            CallerName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet),
                                            CallType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet),
                                            Description = dDescription,
                                            CallerNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
                                            CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
                                            CallDate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
                                            CallTime = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
                                            CallDuration = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
                                            ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                        });
                                    }
                                }
                            }


                        }
                    }

                }

                mobilityUploadListAC.InvalidMobilityList = datalistInvalid;
                mobilityUploadListAC.ValidMobilityList = datalist;
                ResponseDynamicDataAC<MobilityUploadListAC> responseData = new ResponseDynamicDataAC<MobilityUploadListAC>();
                responseData.Data = mobilityUploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                #endregion





                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }
        }

        public async Task<ImportBillDetailAC<MadaUploadListAC>> ReadExcelForMadaService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<MadaUploadListAC> importBillDetail = new ImportBillDetailAC<MadaUploadListAC>();
            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<MadaExcelUploadDetailStringAC> datalistInvalid = new List<MadaExcelUploadDetailStringAC>();

            try
            {
                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.GeneralServiceMada);

                MadaUploadListAC madaUploadListAC = new MadaUploadListAC();

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = 0;
                    int readingIndex = 0;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
                    }

                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    IRow headerRow = sheet.GetRow(0);

                    if (headerRow != null)
                    {
                        int cellCount = headerRow.LastCellNum;
                        int rowcount = sheet.LastRowNum;
                        for (int j = readingIndex; j <= rowcount + 1; j++)
                        {
                            int intIndex = 0;
                            intIndex = (j > 0 ? j - 1 : j);
                            IRow row = sheet.GetRow(intIndex);

                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    bool IsFullValid = true;
                                    string ErrorMessageSummary = string.Empty;


                                    #region --> Site Name Required and Format Validation Part

                                    var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
                                    string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(SiteNameStr))
                                    {
                                        if (SiteNameStr.GetType() != typeof(string))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Bandwidth  Validation Part

                                    var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
                                    string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(BandwidthStr))
                                    {
                                        if (BandwidthStr.GetType() != typeof(string))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " Bandwidth doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Price Validatition Required and Format Validation Part
                                    string PriceStr = string.Empty;
                                    string Message = string.Empty;
                                    Type valueType;

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("FinalAnnualChargesKD", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("FinalAnnualChargesKD", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("InitialDiscountedAnnualPriceKD", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("InitialDiscountedMonthlyPriceKD", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("InitialDiscountedSavingMonthlyKD", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    Message = checkPriceValidation("InitialDiscountedSavingYearlyKD", PriceStr, valueType, false);
                                    if (Message != "valid")
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                    }

                                    #endregion





                                    if (IsFullValid)
                                    {
                                        ExcelDetail data = new ExcelDetail();
                                        // --> Required Field Data--------------------
                                        data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String);

                                        data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.InitialDiscountedAnnualPriceKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.InitialDiscountedMonthlyPriceKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.InitialDiscountedSavingMonthlyKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.InitialDiscountedSavingYearlyKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.FinalAnnualChargesKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number));

                                        data.ExcelUploadLogId = 0;
                                        data.ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada;
                                        data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                        data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                        //  data.AssignType = (long)EnumList.AssignType.Business;
                                        data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);

                                        datalist.Add(data);
                                    }
                                    else
                                    {

                                        datalistInvalid.Add(new MadaExcelUploadDetailStringAC
                                        {
                                            SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                            ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                            Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                            CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String),

                                            MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedAnnualPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedMonthlyPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedSavingMonthlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedSavingYearlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            FinalAnnualChargesKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number)),

                                            ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                        });
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (e.GetType() != typeof(System.NullReferenceException))
                                    {
                                        datalistInvalid.Add(new MadaExcelUploadDetailStringAC
                                        {
                                            SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                            ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                            Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                            CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String),

                                            MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedAnnualPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedMonthlyPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedSavingMonthlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            InitialDiscountedSavingYearlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            FinalAnnualChargesKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number)),

                                            ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                        });

                                    }
                                }
                            }


                        }
                    }

                }

                madaUploadListAC.InvalidList = datalistInvalid;
                madaUploadListAC.ValidList = datalist;
                ResponseDynamicDataAC<MadaUploadListAC> responseData = new ResponseDynamicDataAC<MadaUploadListAC>();
                responseData.Data = madaUploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                #endregion


                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }

        }

        public async Task<ImportBillDetailAC<InternetServiceUploadListAC>> ReadExcelForInternetService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<InternetServiceUploadListAC> importBillDetail = new ImportBillDetailAC<InternetServiceUploadListAC>();

            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<InternetServiceExcelUploadDetailStringAC> datalistInvalid = new List<InternetServiceExcelUploadDetailStringAC>();

            try
            {
                InternetServiceUploadListAC UploadListAC = new InternetServiceUploadListAC();

                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.InternetService);

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = 0;
                    int readingIndex = 0;
                    string TitleHeader = string.Empty;
                    string TitleReadingColumn = string.Empty;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
                        TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
                        TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
                    }

                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    IRow headerRow = sheet.GetRow(0);


                    if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
                    {
                        if (TitleReadingColumn.All(Char.IsLetter))
                        {
                            int rowcount = sheet.LastRowNum;
                            for (int j = readingIndex; j <= rowcount + 1; j++)
                            {
                                int intIndex = 0;
                                intIndex = (j > 0 ? j - 1 : j);
                                IRow row = sheet.GetRow(intIndex);
                                int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

                                if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

                                else
                                {
                                    string strValue = Convert.ToString(row.GetCell(ColNumber));
                                    if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                    {
                                        readingIndex = intIndex + 1;
                                        break;
                                    }
                                }
                            }

                        }
                        else if (TitleReadingColumn.All(Char.IsLetterOrDigit) && TitleReadingColumn.Length>1)
                        {
                            var dynCR = new CellReference(TitleReadingColumn);
                            IRow row = sheet.GetRow(dynCR.Row);
                            var cell = row.GetCell(dynCR.Col);
                            string strValue = Convert.ToString(row.GetCell(dynCR.Col));
                            if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                            {
                                readingIndex = dynCR.Row + 1;
                            }
                        }                      

                    }

                    if (headerRow != null)
                    {

                        int rowcount = sheet.LastRowNum;
                        for (int j = readingIndex; j <= rowcount + 1; j++)
                        {
                            int intIndex = j;
                            // intIndex = (j > 0 ? j - 1 : j);
                            IRow row = sheet.GetRow(intIndex);

                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    bool IsFullValid = true;
                                    string ErrorMessageSummary = string.Empty;


                                    #region --> Site Name Required and Format Validation Part

                                    var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
                                    string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(SiteNameStr))
                                    {
                                        if (SiteNameStr.GetType() != typeof(string))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Bandwidth  Validation Part

                                    var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
                                    string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(BandwidthStr))
                                    {
                                        if (BandwidthStr.GetType() != typeof(string))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                        }
                                    }

                                    #endregion

                                    #region --> Price Validatition Required and Format Validation Part
                                    string PriceStr = string.Empty;
                                    string Message = string.Empty;
                                    Type valueType;

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    if (!string.IsNullOrEmpty(PriceStr))
                                    {
                                        Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                                        if (Message != "valid")
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

                                    }



                                    #endregion


                                    if (IsFullValid)
                                    {
                                        ExcelDetail data = new ExcelDetail();
                                        // --> Required Field Data--------------------
                                        data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

                                        data.ExcelUploadLogId = 0;
                                        data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
                                        data.CurrencyId = 1;
                                        var icid = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                        data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                        // data.AssignType = (long)EnumList.AssignType.Business;
                                        data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                        datalist.Add(data);
                                    }
                                    else
                                    {

                                        datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
                                        {
                                            // --> Required Field Data--------------------
                                            ServiceName = _iStringConstant.InternetService,
                                            SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                            GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                            Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                            BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                            MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                            CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                            ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                        });
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (e.GetType() != typeof(System.NullReferenceException))
                                    {
                                        datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
                                        {
                                            // --> Required Field Data--------------------
                                            ServiceName = _iStringConstant.InternetService,
                                            SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                            GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                            Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                            BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                            MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                            CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                            ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                        });
                                    }
                                }
                            }


                        }
                    }

                }

                UploadListAC.InvalidList = datalistInvalid;
                UploadListAC.ValidList = datalist;
                ResponseDynamicDataAC<InternetServiceUploadListAC> responseData = new ResponseDynamicDataAC<InternetServiceUploadListAC>();
                responseData.Data = UploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                #endregion


                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }

        }

        public async Task<ImportBillDetailAC<DataCenterFacilityUploadListAC>> ReadExcelForDataCenterFacility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
        {
            ImportBillDetailAC<DataCenterFacilityUploadListAC> importBillDetail = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();

            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<DataCenterFacilityExcelUploadDetailStringAC> datalistInvalid = new List<DataCenterFacilityExcelUploadDetailStringAC>();

            try
            {
                DataCenterFacilityUploadListAC UploadListAC = new DataCenterFacilityUploadListAC();
                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.DataCenterFacility);

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = 0;
                    int readingIndex = 0;
                    string TitleHeader = string.Empty;
                    string TitleReadingColumn = string.Empty;
                    if (mappingExcel != null)
                    {
                        worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
                        readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
                        TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
                        TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
                    }

                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    IRow headerRow = sheet.GetRow(0);


                    if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
                    {
                        if (TitleReadingColumn.All(Char.IsLetter))
                        {
                            int rowcount = sheet.LastRowNum;
                            for (int j = readingIndex; j <= rowcount + 1; j++)
                            {
                                int intIndex = 0;
                                intIndex = (j > 0 ? j - 1 : j);
                                IRow row = sheet.GetRow(intIndex);
                                int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

                                if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

                                else
                                {
                                    string strValue = Convert.ToString(row.GetCell(ColNumber));
                                    if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                    {
                                        readingIndex = intIndex + 1;
                                        break;
                                    }
                                }
                            }

                        }
                        else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
                        {
                            var dynCR = new CellReference(TitleReadingColumn);
                            if (dynCR.Row >= 0)
                            {
                                IRow row = sheet.GetRow(dynCR.Row);
                                var cell = row.GetCell(dynCR.Col);
                                string strValue = Convert.ToString(row.GetCell(dynCR.Col));
                                if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                {
                                    readingIndex = dynCR.Row + 1;
                                }
                            }

                        }


                    }

                    if (headerRow != null)
                    {

                        int rowcount = sheet.LastRowNum;
                        for (int j = readingIndex; j <= rowcount + 1; j++)
                        {
                            int intIndex = j;
                            // intIndex = (j > 0 ? j - 1 : j);
                            IRow row = sheet.GetRow(intIndex);

                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }
                            else
                            {
                                try
                                {
                                    bool IsFullValid = true;
                                    string ErrorMessageSummary = string.Empty;


                                    #region --> Site Name Required and Format Validation Part

                                    var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
                                    string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(SiteNameStr))
                                    {
                                        if (SiteNameStr.GetType() != typeof(string))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                                    }
                                    #endregion

                                    #region --> Bandwidth  Validation Part

                                    var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
                                    string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
                                    if (!string.IsNullOrEmpty(BandwidthStr))
                                    {
                                        if (BandwidthStr.GetType() != typeof(string))
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                        }
                                    }

                                    #endregion

                                    #region --> Price Validatition Required and Format Validation Part
                                    string PriceStr = string.Empty;
                                    string Message = string.Empty;
                                    Type valueType;

                                    PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
                                    valueType = PriceStr.GetType();
                                    if (!string.IsNullOrEmpty(PriceStr))
                                    {
                                        Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                                        if (Message != "valid")
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                        }
                                    }
                                    else
                                    {
                                        IsFullValid = false;
                                        ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

                                    }



                                    #endregion


                                    if (IsFullValid)
                                    {
                                        ExcelDetail data = new ExcelDetail();
                                        // --> Required Field Data--------------------
                                        data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
                                        data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
                                        data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

                                        data.ExcelUploadLogId = 0;
                                        data.ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility;
                                        data.CurrencyId = 1;
                                        var icid = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                        data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                        // data.AssignType = (long)EnumList.AssignType.Business;
                                        data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                        datalist.Add(data);
                                    }
                                    else
                                    {

                                        datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
                                        {
                                            // --> Required Field Data--------------------
                                            ServiceName = _iStringConstant.DataCenterFacility,
                                            SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                            GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                            Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                            BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                            MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                            CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                            ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                        });
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (e.GetType() != typeof(System.NullReferenceException))
                                    {
                                        datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
                                        {
                                            // --> Required Field Data--------------------
                                            ServiceName = _iStringConstant.DataCenterFacility,
                                            SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                            GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                            Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                            BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                            MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                            CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                            CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                            ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                        });
                                    }
                                }
                            }


                        }
                    }

                }

                UploadListAC.InvalidList = datalistInvalid;
                UploadListAC.ValidList = datalist;
                ResponseDynamicDataAC<DataCenterFacilityUploadListAC> responseData = new ResponseDynamicDataAC<DataCenterFacilityUploadListAC>();
                responseData.Data = UploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                #endregion


                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }

        }

        private string getValueFromExcel(string dynamicRef, ISheet sheet, long type = 0)
        {
            var dynCR = new CellReference(dynamicRef);
            var row = sheet.GetRow(dynCR.Row);
            var cell = row.GetCell(dynCR.Col);
            string strValueofmyname = string.Empty;
            // IRow rows = sheet.GetRow(dynCR.Row);
            try
            {
                if (row == null)
                {
                    strValueofmyname = "Invalid Type";
                }
                if (row.Cells.All(d => d.CellType == CellType.Blank))
                {
                    strValueofmyname = "";
                }
                else if (cell.CellType == CellType.Formula)
                {
                    if (cell.CachedFormulaResultType == CellType.Numeric)
                    {
                        strValueofmyname = Convert.ToString(cell.NumericCellValue);
                    }
                    else if (cell.CachedFormulaResultType == CellType.String)
                    {
                        strValueofmyname = Convert.ToString(cell.StringCellValue);
                    }
                    else if (cell.CachedFormulaResultType == CellType.Boolean)
                    {
                        strValueofmyname = Convert.ToString(cell.BooleanCellValue);
                    }
                    else if (cell.CachedFormulaResultType == CellType.Blank)
                    {
                        strValueofmyname = string.Empty;
                    }
                    else if (cell.CachedFormulaResultType == CellType.Error)
                    {
                        strValueofmyname = Convert.ToString(cell.ErrorCellValue);
                    }
                    else if (cell.CachedFormulaResultType == CellType.Unknown)
                    {
                        strValueofmyname = Convert.ToString(row.GetCell(dynCR.Col));
                    }
                }
                else
                {
                    strValueofmyname = Convert.ToString(row.GetCell(dynCR.Col));

                    if (type == (long)EnumList.SupportDataType.Date) // date
                    {
                        strValueofmyname = Convert.ToString(cell.DateCellValue);
                    }
                    if (type == (long)EnumList.SupportDataType.Time) //  time
                    {
                        strValueofmyname = Convert.ToString(cell.DateCellValue);
                        DateTime dt1 = DateTime.Parse(strValueofmyname);
                        strValueofmyname = String.Format("{0:HH:mm:ss}", dt1);
                    }
                    if (type == (long)EnumList.SupportDataType.Number) // numeric amount any
                    {
                        strValueofmyname = Convert.ToString(cell.NumericCellValue);
                    }
                }

            }
            catch (Exception)
            {
                strValueofmyname = Convert.ToString(row.GetCell(dynCR.Col));
            }

            return strValueofmyname;
        }

        public MultiServiceUploadAC getFirstReadingIndexWithService(string filepath, string filename, List<MappingDetailAC> mappingExcellist, BillUploadAC billUploadAC)
        {
            MultiServiceUploadAC readingData = new MultiServiceUploadAC();
            try
            {
                int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
                MappingDetailAC mappingDetail = new MappingDetailAC();

                #region --> If we got1 service without title than call function for that service

                if (ServiceWithoutTitleCount == 1)
                {
                    mappingDetail = mappingExcellist.FirstOrDefault(x => !x.HaveTitle && x.TitleName == "");
                    readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

                    if (mappingDetail != null)
                    {
                        if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
                        {
                            readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
                        }
                        else
                        {
                            readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
                        }

                    }

                    return readingData;
                }  // End of  if (ServiceWithoutTitleCount == 1)

                #endregion

                #region --> If all service have Title than get first minimum worksheet no's service details

                if (ServiceWithoutTitleCount == 0)
                {

                    int worksheetno = 0;


                    if (mappingExcellist != null)
                    {
                        mappingDetail = mappingExcellist.OrderBy(x => x.WorkSheetNo).FirstOrDefault();
                        worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);

                        List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == worksheetno).ToList();

                        #region --> Only One service in single worksheet 
                        if (SingleWorksheetservice.Count() == 1)
                        {
                            mappingDetail = SingleWorksheetservice.FirstOrDefault();

                            if (mappingDetail != null)
                            {
                                readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

                                if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
                                {
                                    readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
                                }
                                else if (mappingDetail.HaveHeader)
                                {
                                    readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
                                }
                                else if (mappingDetail.HaveTitle)
                                {
                                    #region--> read excel and get Title Index Row                                    
                                    ISheet sheet;
                                    string sFileExtension = Path.GetExtension(filename).ToLower();
                                    string fullPath = Path.Combine(filepath, filename);
                                    int readingIndex = 0;
                                    using (var stream = new FileStream(fullPath, FileMode.Open))
                                    {

                                        string TitleHeader = string.Empty;
                                        string TitleReadingColumn = string.Empty;
                                        if (mappingDetail != null)
                                        {
                                            worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);
                                            readingIndex = (mappingDetail.HaveHeader ? 2 : 1);
                                            TitleHeader = (mappingDetail.HaveTitle ? mappingDetail.TitleName : "");
                                            TitleReadingColumn = (mappingDetail.ExcelColumnNameForTitle);
                                        }

                                        stream.Position = 0;
                                        if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                                        {
                                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                                            sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                        }
                                        else
                                        {
                                            XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                                            sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                        }
                                        IRow headerRow = sheet.GetRow(0);

                                        if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
                                        {
                                            if (TitleReadingColumn.All(Char.IsLetter))
                                            {
                                                int rowcount = sheet.LastRowNum;
                                                for (int j = readingIndex; j <= rowcount + 1; j++)
                                                {
                                                    int intIndex = 0;
                                                    intIndex = (j > 0 ? j - 1 : j);
                                                    IRow row = sheet.GetRow(intIndex);
                                                    int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

                                                    if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

                                                    else
                                                    {
                                                        string strValue = Convert.ToString(row.GetCell(ColNumber));
                                                        if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                                        {
                                                            readingIndex = intIndex + 1;
                                                            break;
                                                        }
                                                    }
                                                }

                                            }
                                            else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
                                            {
                                                var dynCR = new CellReference(TitleReadingColumn);
                                                if (dynCR.Row >= 0)
                                                {
                                                    IRow row = sheet.GetRow(dynCR.Row);
                                                    var cell = row.GetCell(dynCR.Col);
                                                    string strValue = Convert.ToString(row.GetCell(dynCR.Col));
                                                    if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                                    {
                                                        readingIndex = dynCR.Row + 1;
                                                    }
                                                }
                                            }

                                        }

                                    }
                                    #endregion

                                    if (mappingDetail.HaveTitle && mappingDetail.HaveHeader)
                                    {
                                        readingData.ReadingIndex = readingIndex + 1; //Add one for Header
                                    }
                                    else
                                    {
                                        readingData.ReadingIndex = readingIndex;
                                    }
                                }

                                return readingData;
                            }

                        } // end of if (SingleWorksheetservice.Count() == 1)

                        #endregion

                        #region --> Multiple service in  single worksheet 

                        if (SingleWorksheetservice.Count() > 1)
                        {
                            // Get All service Title in list to find first index
                            string[] ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();

                            #region--> read excel and get Title Index Row                                    
                            ISheet sheet;
                            string sFileExtension = Path.GetExtension(filename).ToLower();
                            string fullPath = Path.Combine(filepath, filename);
                            int readingIndex = 0;
                            using (var stream = new FileStream(fullPath, FileMode.Open))
                            {

                                if (mappingDetail != null)
                                {
                                    worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);
                                    readingIndex = 1; // as we dont know which service will be first so will start from 1.                                    
                                }
                                stream.Position = 0;
                                if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                                {
                                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                                    sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                }
                                else
                                {
                                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                                    sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                }
                                IRow headerRow = sheet.GetRow(0);

                                if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
                                {
                                    int rowcount = sheet.LastRowNum;
                                    for (int j = readingIndex; j <= rowcount + 1; j++)
                                    {
                                        int intIndex = 0;
                                        intIndex = (j > 0 ? j - 1 : j);
                                        IRow row = sheet.GetRow(intIndex);

                                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                                        else
                                        {
                                            int TitleColumNo = 0;
                                            // find service title nd column wise first index
                                            foreach (var service in SingleWorksheetservice
                                                .Where(x => x.WorkSheetNo == worksheetno)
                                                .OrderBy(x => x.ExcelColumnNameForTitle).ToList())
                                            {

                                                if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
                                                {
                                                    TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
                                                    if (row == null || row.GetCell(TitleColumNo).CellType == CellType.Blank) continue;
                                                    else
                                                    {
                                                        string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
                                                        if (!string.IsNullOrEmpty(getStrVal))
                                                        {
                                                            bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                            if (ServiceTitle.Any(getStrVal.Contains))
                                                            {
                                                                readingIndex = intIndex + 1;
                                                                readingData.ServiceTypeId = service.ServiceTypeId;
                                                                readingData.ReadingIndex = readingIndex;
                                                                return readingData;
                                                                //  break;
                                                            }
                                                            else
                                                            {
                                                                continue;
                                                            }
                                                        }
                                                    }

                                                }

                                                else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
                                                {
                                                    var dynCR = new CellReference(service.ExcelColumnNameForTitle);
                                                    if (dynCR.Row >= 0)
                                                    {
                                                        IRow rowR = sheet.GetRow(dynCR.Row);
                                                        var cell = rowR.GetCell(dynCR.Col);
                                                        string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
                                                        if (!string.IsNullOrEmpty(getStrVal))
                                                        {
                                                            bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                            if (ServiceTitle.Any(getStrVal.Contains))
                                                            {
                                                                readingIndex = dynCR.Row + 1;
                                                                readingData.ServiceTypeId = service.ServiceTypeId;
                                                                readingData.ReadingIndex = readingIndex;
                                                                return readingData;
                                                                //  break;
                                                            }
                                                            else
                                                            {
                                                                continue;
                                                            }
                                                        }

                                                    }
                                                }

                                            } // end of for loop

                                        } // end of else


                                    } // end of forloop of reading index
                                } // end of if  (ServiceTitle != null)

                            } // end of using 

                        } // end of  if (SingleWorksheetservice.Count() > 1)

                        #endregion


                    }

                    #endregion

                } // end of   if (mappingExcellist != null)

                #endregion
            }
            catch (Exception)
            {
                readingData.ReadingIndex = 0;
                return readingData;
            }
            return readingData;
        }



        #region --> Multiple Service functions 

        public MultiServiceUploadAC getReadingIndexWithServiceFromSingleWorksheet(string filepath, string filename, List<MappingDetailAC> mappingExcellist, BillUploadAC billUploadAC, string[] ServiceTitle, int worksheetNo = 1)
        {
            MultiServiceUploadAC readingData = new MultiServiceUploadAC();
            try
            {
                int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
                MappingDetailAC mappingDetail = new MappingDetailAC();

                #region --> If we got1 service without title than call function for that service

                if (ServiceWithoutTitleCount == 1)
                {
                    mappingDetail = mappingExcellist.FirstOrDefault(x => !x.HaveTitle && x.TitleName == "");
                    readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

                    if (mappingDetail != null)
                    {
                        if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
                        {
                            readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
                        }
                        else
                        {
                            readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
                        }

                    }

                    return readingData;
                }  // End of  if (ServiceWithoutTitleCount == 1)

                #endregion

                #region --> If all service have Title than get first minimum worksheet no's service details

                if (ServiceWithoutTitleCount == 0)
                {

                    int worksheetno = 0;
                    worksheetno = worksheetNo;

                    if (mappingExcellist != null)
                    {
                        // mappingDetail = mappingExcellist.OrderBy(x => x.WorkSheetNo).FirstOrDefault();
                        // worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);

                        List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == worksheetno).ToList();

                        #region --> Only One service in single worksheet 
                        if (SingleWorksheetservice.Count() == 1)
                        {
                            mappingDetail = SingleWorksheetservice.FirstOrDefault();

                            if (mappingDetail != null)
                            {
                                readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

                                if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
                                {
                                    readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
                                }
                                else if (mappingDetail.HaveHeader)
                                {
                                    readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
                                }
                                else if (mappingDetail.HaveTitle)
                                {
                                    #region--> read excel and get Title Index Row                                    
                                    ISheet sheet;
                                    string sFileExtension = Path.GetExtension(filename).ToLower();
                                    string fullPath = Path.Combine(filepath, filename);
                                    int readingIndex = 0;
                                    using (var stream = new FileStream(fullPath, FileMode.Open))
                                    {

                                        string TitleHeader = string.Empty;
                                        string TitleReadingColumn = string.Empty;
                                        if (mappingDetail != null)
                                        {
                                            worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);
                                            readingIndex = (mappingDetail.HaveHeader ? 2 : 1);
                                            TitleHeader = (mappingDetail.HaveTitle ? mappingDetail.TitleName : "");
                                            TitleReadingColumn = (mappingDetail.ExcelColumnNameForTitle);
                                        }

                                        stream.Position = 0;
                                        if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                                        {
                                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                                            sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                        }
                                        else
                                        {
                                            XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                                            sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                        }
                                        IRow headerRow = sheet.GetRow(0);

                                        if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
                                        {
                                            if (TitleReadingColumn.All(Char.IsLetter))
                                            {
                                                int rowcount = sheet.LastRowNum;
                                                for (int j = readingIndex; j <= rowcount + 1; j++)
                                                {
                                                    int intIndex = 0;
                                                    intIndex = (j > 0 ? j - 1 : j);
                                                    IRow row = sheet.GetRow(intIndex);
                                                    int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

                                                    if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

                                                    else
                                                    {
                                                        string strValue = Convert.ToString(row.GetCell(ColNumber));
                                                        if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                                        {
                                                            readingIndex = intIndex + 1;
                                                            break;
                                                        }
                                                    }
                                                }

                                            }
                                            else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
                                            {
                                                var dynCR = new CellReference(TitleReadingColumn);
                                                if (dynCR.Row >= 0)
                                                {
                                                    IRow row = sheet.GetRow(dynCR.Row);
                                                    var cell = row.GetCell(dynCR.Col);
                                                    string strValue = Convert.ToString(row.GetCell(dynCR.Col));
                                                    if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
                                                    {
                                                        readingIndex = dynCR.Row + 1;
                                                    }
                                                }
                                            }

                                        }

                                    }
                                    #endregion

                                    if (mappingDetail.HaveTitle && mappingDetail.HaveHeader)
                                    {
                                        readingData.ReadingIndex = readingIndex + 1; //Add one for Header
                                    }
                                    else
                                    {
                                        readingData.ReadingIndex = readingIndex;
                                    }
                                }

                                return readingData;
                            }

                        } // end of if (SingleWorksheetservice.Count() == 1)

                        #endregion

                        #region --> Multiple service in  single worksheet 

                        if (SingleWorksheetservice.Count() > 1)
                        {
                            // Get All service Title in list to find first index
                            // string[] ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();

                            #region--> read excel and get Title Index Row                                    
                            ISheet sheet;
                            string sFileExtension = Path.GetExtension(filename).ToLower();
                            string fullPath = Path.Combine(filepath, filename);
                            int readingIndex = 0;
                            using (var stream = new FileStream(fullPath, FileMode.Open))
                            {
                                    readingIndex = 0;                                   
                                    worksheetno = worksheetNo;
                                
                                stream.Position = 0;
                                if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                                {
                                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                                    sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                }
                                else
                                {
                                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                                    sheet = hssfwb.GetSheetAt(worksheetno - 1);
                                }
                                IRow headerRow = sheet.GetRow(0);

                                if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
                                {
                                    int rowcount = sheet.LastRowNum;
                                    for (int j = readingIndex; j <= rowcount + 1; j++)
                                    {
                                        int intIndex = j;
                                       // intIndex = (j > 0 ? j - 1 : j);
                                        IRow row = sheet.GetRow(intIndex);

                                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                                        else
                                        {
                                            int TitleColumNo = 0;
                                            // find service title nd column wise first index
                                            foreach (var service in SingleWorksheetservice
                                                .Where(x => x.WorkSheetNo == worksheetno)
                                                .OrderBy(x => x.ExcelColumnNameForTitle).ToList())
                                            {

                                                if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
                                                {
                                                    TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
                                                    if (row == null || row.GetCell(TitleColumNo).CellType == CellType.Blank) continue;
                                                    else
                                                    {
                                                        string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
                                                        if (!string.IsNullOrEmpty(getStrVal))
                                                        {
                                                            bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                            if (containsAny)
                                                            {
                                                                readingIndex = intIndex + 1;
                                                                readingData.ServiceTypeId = SingleWorksheetservice.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;
                                                                readingData.ReadingIndex = readingIndex;
                                                                return readingData;
                                                                //  break;
                                                            }
                                                            else
                                                            {
                                                                continue;
                                                            }
                                                        }
                                                    }

                                                }

                                                else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
                                                {
                                                    var dynCR = new CellReference(service.ExcelColumnNameForTitle);
                                                    if (dynCR.Row >= 0)
                                                    {
                                                        IRow rowR = sheet.GetRow(dynCR.Row);
                                                        var cell = rowR.GetCell(dynCR.Col);
                                                        string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
                                                        if (!string.IsNullOrEmpty(getStrVal))
                                                        {
                                                            bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                            if (containsAny)
                                                            {
                                                                readingIndex = dynCR.Row + 1;
                                                                readingData.ServiceTypeId = service.ServiceTypeId;
                                                                readingData.ServiceTypeId = SingleWorksheetservice.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                                readingData.ReadingIndex = readingIndex;
                                                                return readingData;
                                                                //  break;
                                                            }
                                                            else
                                                            {
                                                                continue;
                                                            }
                                                        }

                                                    }
                                                }

                                            } // end of for loop

                                        } // end of else


                                    } // end of forloop of reading index
                                } // end of if  (ServiceTitle != null)

                            } // end of using 

                        } // end of  if (SingleWorksheetservice.Count() > 1)

                        #endregion
                    }
                    #endregion
                } // end of   if (mappingExcellist != null)

                #endregion
            }
            catch (Exception e)
            {
                readingData.ReadingIndex = 0;
                return readingData;
            }
            return readingData;
        }


        public async Task<ImportBillDetailMultipleAC<InternetServiceUploadListAC>> ReadExcelForInternetServiceMultiple
                        (string filepath, string filename,
                         MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
                         BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1)
        {
            ImportBillDetailMultipleAC<InternetServiceUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<InternetServiceUploadListAC>();

            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<InternetServiceExcelUploadDetailStringAC> datalistInvalid = new List<InternetServiceExcelUploadDetailStringAC>();

            try
            {
                InternetServiceUploadListAC UploadListAC = new InternetServiceUploadListAC();

                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.InternetService);

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = worksheetNo;
                    int readingIndex = ReadingIndex;
                    string TitleHeader = string.Empty;
                    string TitleReadingColumn = string.Empty;
                    if (mappingExcel != null)
                    {
                        #region --> set reading index for current Service type
                        TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
                        if (ReadingIndex < 0)
                        {
                            readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
                          
                            TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
                        }
                        else
                        {
                            readingIndex = (int)ReadingIndex;
                        }

                        #endregion
                    }

                    #region --> Read  .xls or .xlsx File Using NPOI Package Class
                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    #endregion

                    if (sheet.LastRowNum > 0)
                    {
                        int rowcount = sheet.LastRowNum +1;
                        bool IsServiceTitleRow = false;
                        for (int j = readingIndex; j <= rowcount; j++)
                        {
                            IsServiceTitleRow = false;
                            int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
                            IRow row = sheet.GetRow(intIndex);
                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }

                            #region --> Check next row having any title for next service or not 
                            else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
                            {
                                int TitleColumNo = 0;
                                // find service title nd column wise first index
                                #region --> Forloop forget title from each mapped column at current row
                                foreach (var service in singleWorksheetserviceList
                                               .Where(x => x.WorkSheetNo == worksheetno)
                                                .OrderBy(x => x.ExcelColumnNameForTitle).ToList())
                                {

                                    if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
                                    {
                                        TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
                                        if (row == null || row.GetCell(TitleColumNo).CellType == CellType.Blank) continue;
                                        else
                                        {
                                            string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
                                            if (!string.IsNullOrEmpty(getStrVal))
                                            {
                                                if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
                                                {
                                                    bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                    if (containsAny)
                                                    {
                                                        readingIndex = intIndex + 1;
                                                        importBillDetail.ServiceTypeId = service.ServiceTypeId;
                                                        importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                        importBillDetail.ReadingIndex = readingIndex;
                                                        rowcount = 0; // to stop the loop
                                                        IsServiceTitleRow = true;
                                                        break;
                                                        //return importBillDetail;

                                                    }
                                                }
                                                else
                                                {
                                                    IsServiceTitleRow = true; // just to avoid error of Totle row
                                                    break;
                                                }
                                                
                                            }
                                        }

                                    }

                                    else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
                                    {
                                        var dynCR = new CellReference(service.ExcelColumnNameForTitle);
                                        if (dynCR.Row >= 0)
                                        {
                                            IRow rowR = sheet.GetRow(dynCR.Row);
                                            var cell = rowR.GetCell(dynCR.Col);
                                            string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
                                            if (!string.IsNullOrEmpty(getStrVal))
                                            {
                                                if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
                                                {
                                                    bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                    if (containsAny)
                                                    {
                                                        readingIndex = dynCR.Row + 1;
                                                        importBillDetail.ServiceTypeId = service.ServiceTypeId;
                                                        importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                        importBillDetail.ReadingIndex = readingIndex;
                                                        rowcount = 0;// to stop the loop
                                                        IsServiceTitleRow = true;
                                                        break;
                                                        // return importBillDetail;                                             
                                                    }
                                                }
                                                else
                                                {
                                                    IsServiceTitleRow = true; // just to avoid error of Totle row
                                                    break;
                                                }

                                            }

                                        }
                                    }

                                } // end of for loop
                                #endregion

                            } // end of if  (ServiceTitle != null)

                            #endregion

                            if (!IsServiceTitleRow)
                            {
                                if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
                                {
                                    try
                                    {
                                        bool IsFullValid = true;
                                        string ErrorMessageSummary = string.Empty;

                                        #region --> Site Name Required and Format Validation Part

                                        var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
                                        string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
                                        if (!string.IsNullOrEmpty(SiteNameStr))
                                        {
                                            if (SiteNameStr.GetType() != typeof(string))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                            }
                                        }
                                        else
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                                        }
                                        #endregion

                                        #region --> Bandwidth  Validation Part

                                        var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
                                        string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
                                        if (!string.IsNullOrEmpty(BandwidthStr))
                                        {
                                            if (BandwidthStr.GetType() != typeof(string))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                            }
                                        }

                                        #endregion

                                        #region --> Price Validatition Required and Format Validation Part
                                        string PriceStr = string.Empty;
                                        string Message = string.Empty;
                                        Type valueType;

                                        PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
                                        valueType = PriceStr.GetType();
                                        if (!string.IsNullOrEmpty(PriceStr))
                                        {
                                            Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                                            if (Message != "valid")
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                            }
                                        }
                                        else
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

                                        }



                                        #endregion

                                        if (IsFullValid)
                                        {
                                            ExcelDetail data = new ExcelDetail();
                                            // --> Required Field Data--------------------
                                            data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
                                            data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

                                            data.ExcelUploadLogId = 0;
                                            data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
                                            data.CurrencyId = 1;
                                            var icid = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                            data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                            // data.AssignType = (long)EnumList.AssignType.Business;
                                            data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                            datalist.Add(data);
                                        }
                                        else
                                        {

                                            datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
                                            {
                                                // --> Required Field Data--------------------
                                                ServiceName = _iStringConstant.InternetService,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                                ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        if (e.GetType() != typeof(System.NullReferenceException))
                                        {
                                            datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
                                            {
                                                // --> Required Field Data--------------------
                                                ServiceName = _iStringConstant.InternetService,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                                ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
                                    }
                                }
                            } // end of if (!IsServiceTitleRow)

                        }
                    }

                }

                UploadListAC.InvalidList = datalistInvalid;
                UploadListAC.ValidList = datalist;
                ResponseDynamicDataAC<InternetServiceUploadListAC> responseData = new ResponseDynamicDataAC<InternetServiceUploadListAC>();
                responseData.Data = UploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

               // if (File.Exists(Path.Combine(filepath, filename)))
                //    File.Delete(Path.Combine(filepath, filename));

                #endregion


                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }

        }



        public async Task<ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>> ReadExcelForDataCenterFacilityMultiple
                        (string filepath, string filename,
                         MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
                         BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1)
        {
            ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>();

            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<DataCenterFacilityExcelUploadDetailStringAC> datalistInvalid = new List<DataCenterFacilityExcelUploadDetailStringAC>();

            try
            {
                DataCenterFacilityUploadListAC UploadListAC = new DataCenterFacilityUploadListAC();

                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.DataCenterFacility);

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = worksheetNo;
                    int readingIndex = ReadingIndex;
                    string TitleHeader = string.Empty;
                    string TitleReadingColumn = string.Empty;
                    if (mappingExcel != null)
                    {
                        #region --> set reading index for current Service type
                        TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
                        if (ReadingIndex < 0)
                        {
                            readingIndex = (mappingExcel.HaveHeader ? 2 : 1);                           
                            TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
                        }
                        else
                        {
                            readingIndex = (int)ReadingIndex;
                        }

                        #endregion
                    }

                    #region --> Read  .xls or .xlsx File Using NPOI Package Class
                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    #endregion

                    if (sheet.LastRowNum > 0)
                    {
                        int rowcount = sheet.LastRowNum + 1;
                        bool IsServiceTitleRow = false;
                        for (int j = readingIndex; j <= rowcount; j++)
                        {
                            IsServiceTitleRow = false;
                            int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
                            IRow row = sheet.GetRow(intIndex);
                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }

                            #region --> Check next row having any title for next service or not 
                            else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
                            {
                                int TitleColumNo = 0;
                                // find service title nd column wise first index
                                #region --> Forloop forget title from each mapped column at current row
                                foreach (var service in singleWorksheetserviceList
                                               .Where(x => x.WorkSheetNo == worksheetno)
                                                .OrderBy(x => x.ExcelColumnNameForTitle).ToList())
                                {

                                    if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
                                    {
                                        TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
                                        if (row == null || row.GetCell(TitleColumNo).CellType == CellType.Blank) continue;
                                        else
                                        {
                                            string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
                                            if (!string.IsNullOrEmpty(getStrVal))
                                            {
                                                if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
                                                {
                                                    bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                    if (containsAny)
                                                    {
                                                        readingIndex = intIndex + 1;
                                                        importBillDetail.ServiceTypeId = service.ServiceTypeId;
                                                        importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                        importBillDetail.ReadingIndex = readingIndex;
                                                        rowcount = 0;// to stop the loop
                                                        IsServiceTitleRow = true;
                                                        break;
                                                        //return importBillDetail;

                                                    }
                                                }
                                                else
                                                {
                                                    IsServiceTitleRow = true;
                                                    break;
                                                }

                                            }
                                           
                                        }

                                    }

                                    else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
                                    {
                                        var dynCR = new CellReference(service.ExcelColumnNameForTitle);
                                        if (dynCR.Row >= 0)
                                        {
                                            IRow rowR = sheet.GetRow(dynCR.Row);
                                            var cell = rowR.GetCell(dynCR.Col);
                                            string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
                                            if (!string.IsNullOrEmpty(getStrVal))
                                            {
                                                if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
                                                {
                                                    bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                    if (containsAny)
                                                    {
                                                        readingIndex = dynCR.Row + 1;
                                                        importBillDetail.ServiceTypeId = service.ServiceTypeId;
                                                        importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                        importBillDetail.ReadingIndex = readingIndex;
                                                        rowcount = 0;// to stop the loop
                                                        IsServiceTitleRow = true;
                                                        break;
                                                        // return importBillDetail;                                             
                                                    }
                                                }
                                                else
                                                {
                                                    IsServiceTitleRow = true;
                                                    break;
                                                }

                                            }

                                        }
                                    }

                                } // end of for loop
                                #endregion

                            } // end of if  (ServiceTitle != null)

                            #endregion
                            if (!IsServiceTitleRow)
                            {
                                if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
                                {
                                    try
                                    {
                                        bool IsFullValid = true;
                                        string ErrorMessageSummary = string.Empty;

                                        #region --> Site Name Required and Format Validation Part

                                        var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
                                        string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
                                        if (!string.IsNullOrEmpty(SiteNameStr))
                                        {
                                            if (SiteNameStr.GetType() != typeof(string))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                            }
                                        }
                                        else
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                                        }
                                        #endregion

                                        #region --> Bandwidth  Validation Part

                                        var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
                                        string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
                                        if (!string.IsNullOrEmpty(BandwidthStr))
                                        {
                                            if (BandwidthStr.GetType() != typeof(string))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                            }
                                        }

                                        #endregion

                                        #region --> Price Validatition Required and Format Validation Part
                                        string PriceStr = string.Empty;
                                        string Message = string.Empty;
                                        Type valueType;

                                        PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
                                        valueType = PriceStr.GetType();
                                        if (!string.IsNullOrEmpty(PriceStr))
                                        {
                                            Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                                            if (Message != "valid")
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                            }
                                        }
                                        else
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

                                        }



                                        #endregion

                                        if (IsFullValid)
                                        {
                                            ExcelDetail data = new ExcelDetail();
                                            // --> Required Field Data--------------------
                                            data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
                                            data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

                                            data.ExcelUploadLogId = 0;
                                            data.ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility;
                                            data.CurrencyId = 1;
                                            var icid = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                            data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                            // data.AssignType = (long)EnumList.AssignType.Business;
                                            data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                            datalist.Add(data);
                                        }
                                        else
                                        {

                                            datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
                                            {
                                                // --> Required Field Data--------------------
                                                ServiceName = _iStringConstant.DataCenterFacility,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                                ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        if (e.GetType() != typeof(System.NullReferenceException))
                                        {
                                            datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
                                            {
                                                // --> Required Field Data--------------------
                                                ServiceName = _iStringConstant.DataCenterFacility,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                                ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
                                    }
                                }
                            }

                        }
                    }

                }

                UploadListAC.InvalidList = datalistInvalid;
                UploadListAC.ValidList = datalist;
                ResponseDynamicDataAC<DataCenterFacilityUploadListAC> responseData = new ResponseDynamicDataAC<DataCenterFacilityUploadListAC>();
                responseData.Data = UploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

               // if (File.Exists(Path.Combine(filepath, filename)))
                //    File.Delete(Path.Combine(filepath, filename));

                #endregion


                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }

        }


        public async Task<ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>> ReadExcelForManagedHostingServiceMultiple
                        (string filepath, string filename,
                         MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
                         BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1)
        {
            ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>();

            List<ExcelDetail> datalist = new List<ExcelDetail>();
            List<ManagedHostingServiceExcelUploadDetailStringAC> datalistInvalid = new List<ManagedHostingServiceExcelUploadDetailStringAC>();

            try
            {
                ManagedHostingServiceUploadListAC UploadListAC = new ManagedHostingServiceUploadListAC();

                bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.ManagedHostingService);

                #region --> Read Excel file   
                ISheet sheet;
                string sFileExtension = Path.GetExtension(filename).ToLower();
                string fullPath = Path.Combine(filepath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    int worksheetno = worksheetNo;
                    int readingIndex = ReadingIndex;
                    string TitleHeader = string.Empty;
                    string TitleReadingColumn = string.Empty;
                    if (mappingExcel != null)
                    {
                        #region --> set reading index for current Service type
                        TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
                        if (ReadingIndex < 0)
                        {
                            readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
                            TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
                        }
                        else
                        {
                            readingIndex = (int)ReadingIndex;
                        }

                        #endregion
                    }

                    #region --> Read  .xls or .xlsx File Using NPOI Package Class
                    stream.Position = 0;
                    if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    else //This will read 2007 Excel format    
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(worksheetno - 1);
                    }
                    #endregion

                    if (sheet.LastRowNum > 0)
                    {
                        int rowcount = sheet.LastRowNum + 1;
                        bool IsServiceTitleRow = false;
                        for (int j = readingIndex; j <= rowcount; j++)
                        {
                            IsServiceTitleRow = false;
                            int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
                            IRow row = sheet.GetRow(intIndex);
                            if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
                            {
                                continue;
                            }

                            #region --> Check next row having any title for next service or not 
                            else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
                            {
                                int TitleColumNo = 0;
                                // find service title nd column wise first index
                                #region --> Forloop forget title from each mapped column at current row
                                foreach (var service in singleWorksheetserviceList
                                               .Where(x => x.WorkSheetNo == worksheetno)
                                                .OrderBy(x => x.ExcelColumnNameForTitle).ToList())
                                {

                                    if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
                                    {
                                        TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
                                        if (row == null || row.GetCell(TitleColumNo).CellType == CellType.Blank) continue;
                                        else
                                        {
                                            string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
                                            if (!string.IsNullOrEmpty(getStrVal))
                                            {
                                                if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
                                                {
                                                    bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                    if (containsAny)
                                                    {
                                                        readingIndex = intIndex + 1;
                                                        importBillDetail.ServiceTypeId = service.ServiceTypeId;
                                                        importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                        importBillDetail.ReadingIndex = readingIndex;
                                                        rowcount = 0;// to stop the loop
                                                        IsServiceTitleRow = true;
                                                        break;
                                                        //return importBillDetail;

                                                    }
                                                }
                                                else
                                                {
                                                    IsServiceTitleRow = true;
                                                    break;
                                                }

                                            }

                                        }

                                    }

                                    else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
                                    {
                                        var dynCR = new CellReference(service.ExcelColumnNameForTitle);
                                        if (dynCR.Row >= 0)
                                        {
                                            IRow rowR = sheet.GetRow(dynCR.Row);
                                            var cell = rowR.GetCell(dynCR.Col);
                                            string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
                                            if (!string.IsNullOrEmpty(getStrVal))
                                            {
                                                if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
                                                {
                                                    bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
                                                    if (containsAny)
                                                    {
                                                        readingIndex = dynCR.Row + 1;
                                                        importBillDetail.ServiceTypeId = service.ServiceTypeId;
                                                        importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

                                                        importBillDetail.ReadingIndex = readingIndex;
                                                        rowcount = 0;// to stop the loop
                                                        IsServiceTitleRow = true;
                                                        break;
                                                        // return importBillDetail;                                             
                                                    }
                                                }
                                                else
                                                {
                                                    IsServiceTitleRow = true;
                                                    break;
                                                }

                                            }

                                        }
                                    }

                                } // end of for loop
                                #endregion

                            } // end of if  (ServiceTitle != null)

                            #endregion
                            if (!IsServiceTitleRow)
                            {
                                if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
                                {
                                    try
                                    {
                                        bool IsFullValid = true;
                                        string ErrorMessageSummary = string.Empty;

                                        #region --> Site Name Required and Format Validation Part

                                        var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
                                        string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
                                        if (!string.IsNullOrEmpty(SiteNameStr))
                                        {
                                            if (SiteNameStr.GetType() != typeof(string))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
                                            }
                                        }
                                        else
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

                                        }
                                        #endregion

                                        #region --> Bandwidth  Validation Part

                                        var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
                                        string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
                                        if (!string.IsNullOrEmpty(BandwidthStr))
                                        {
                                            if (BandwidthStr.GetType() != typeof(string))
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
                                            }
                                        }

                                        #endregion

                                        #region --> Price Validatition Required and Format Validation Part
                                        string PriceStr = string.Empty;
                                        string Message = string.Empty;
                                        Type valueType;

                                        PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
                                        valueType = PriceStr.GetType();
                                        if (!string.IsNullOrEmpty(PriceStr))
                                        {
                                            Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
                                            if (Message != "valid")
                                            {
                                                IsFullValid = false;
                                                ErrorMessageSummary = ErrorMessageSummary + " " + Message;
                                            }
                                        }
                                        else
                                        {
                                            IsFullValid = false;
                                            ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

                                        }



                                        #endregion

                                        if (IsFullValid)
                                        {
                                            ExcelDetail data = new ExcelDetail();
                                            // --> Required Field Data--------------------
                                            data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
                                            data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
                                            data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

                                            data.ExcelUploadLogId = 0;
                                            data.ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService;
                                            data.CurrencyId = 1;
                                            var icid = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                            data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
                                            // data.AssignType = (long)EnumList.AssignType.Business;                                            
                                            data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
                                            datalist.Add(data);
                                        }
                                        else
                                        {

                                            datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
                                            {
                                                // --> Required Field Data--------------------
                                                ServiceName = _iStringConstant.ManagedHostingService,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                                ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        if (e.GetType() != typeof(System.NullReferenceException))
                                        {
                                            datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
                                            {
                                                // --> Required Field Data--------------------
                                                ServiceName = _iStringConstant.ManagedHostingService,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

                                                ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
                                    }
                                }
                            }

                        }
                    }

                }

                UploadListAC.InvalidList = datalistInvalid;
                UploadListAC.ValidList = datalist;
                ResponseDynamicDataAC<ManagedHostingServiceUploadListAC> responseData = new ResponseDynamicDataAC<ManagedHostingServiceUploadListAC>();
                responseData.Data = UploadListAC;
                importBillDetail.UploadData = responseData;

                if (datalistInvalid.Count > 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "Some Data Upload With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

                }
                else if (datalistInvalid.Count > 0 && datalist.Count == 0)
                {
                    importBillDetail.Message = "All Data With Error!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
                }
                if (datalistInvalid.Count == 0 && datalist.Count > 0)
                {
                    importBillDetail.Message = "All Data Upload!";
                    importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
                }

                #region --> Delete File Upload after reading Successful

                // if (File.Exists(Path.Combine(filepath, filename)))
                //    File.Delete(Path.Combine(filepath, filename));

                #endregion


                return importBillDetail;

                #endregion
            }
            catch (Exception e)
            {
                if (File.Exists(Path.Combine(filepath, filename)))
                    File.Delete(Path.Combine(filepath, filename));

                importBillDetail.Message = "Error during Reading : " + e.Message;
                importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
                return importBillDetail;
            }

        }

        #endregion




        #region Bill Allocation

        public async Task<BillAllocationListAC> GetBillAllocationList(BillAllocationAC billAllocationAC)
        {
            BillAllocationListAC billAllocationResponse = new BillAllocationListAC();
            billAllocationResponse.Month = billAllocationAC.Month;
            billAllocationResponse.ToAssigneType = billAllocationAC.ToAssigneType;
            billAllocationResponse.Year = billAllocationAC.Year;
            billAllocationResponse.ServiceTypes = billAllocationAC.ServiceTypes;
            billAllocationResponse.ProviderId = billAllocationAC.ProviderId;
            billAllocationResponse.AssignedBillList = new List<AssignedBillAC>();
            billAllocationResponse.BusinessAssignedBillList = new List<BusinessAssignedBillAC>();
            billAllocationResponse.UnAssignedBillList = new List<UnAssignedBillAC>();

            List<ExcelUploadLog> excelUploadLogList = await _dbTeleBilling_V01Context.ExcelUploadLog.Where(x => x.Month == billAllocationAC.Month && x.Year == billAllocationAC.Year && x.ProviderId == billAllocationAC.ProviderId && !x.IsDelete).Include(x => x.Provider).ToListAsync();
            if (excelUploadLogList.Any())
            {
                foreach (var excelUploadLog in excelUploadLogList)
                {
                    long assignTypeForEmployee = Convert.ToInt16(EnumList.AssignType.Employee);
                    long assignTypeForBusiness = Convert.ToInt16(EnumList.AssignType.Business);

                    List<ExcelDetail> lstExcelDetailForEmployee = new List<ExcelDetail>();
                    List<ExcelDetail> lstExcelDetailForBusiness = new List<ExcelDetail>();
                    List<ExcelDetail> lstForUnAssigned = new List<ExcelDetail>();
                    foreach (var serviceType in billAllocationAC.ServiceTypes)
                    {
                        if (_dbTeleBilling_V01Context.ExcelUploadLogServiceType.Include(x => x.ServiceType).Any(x => x.ServiceTypeId == serviceType.Id && !x.IsAllocated && !x.IsDelete && x.ExceluploadLogId == excelUploadLog.Id && !x.ServiceType.IsBusinessOnly))
                        {

                            lstExcelDetailForEmployee.AddRange(await _dbTeleBilling_V01Context.ExcelDetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == true && x.AssignType == assignTypeForEmployee && x.ServiceTypeId == serviceType.Id).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.ServiceType).ToListAsync());
                            lstExcelDetailForBusiness.AddRange(await _dbTeleBilling_V01Context.ExcelDetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == true && x.AssignType == assignTypeForBusiness && x.ServiceTypeId == serviceType.Id).Include(x => x.Currency).Include(x => x.BusinessUnitNavigation).Include(x => x.ServiceType).ToListAsync());
                            lstForUnAssigned.AddRange(await _dbTeleBilling_V01Context.ExcelDetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == false && x.ServiceTypeId == serviceType.Id && !x.ServiceType.IsBusinessOnly).Include(x => x.Currency).Include(x => x.ServiceType).ToListAsync());
                        }
                    }

                    if (lstExcelDetailForEmployee.Any())
                    {
                        var lstofEmployee = lstExcelDetailForEmployee.GroupBy(x => x.EmployeeId).ToList();
                        foreach (var item in lstofEmployee.ToList())
                        {

                            #region Get Service Types
                            var lstOfServiceGroup = item.ToList().GroupBy(x => x.ServiceType.Name).ToList();
                            string serviceTypes = string.Empty;
                            foreach (var serviceItem in lstOfServiceGroup)
                            {
                                serviceTypes += serviceItem.Key + ",";
                            }
                            #endregion

                            AssignedBillAC assignedBillAC = new AssignedBillAC();
                            assignedBillAC.ServiceType = serviceTypes.Substring(0, serviceTypes.Length - 1);
                            assignedBillAC.AssignType = EnumList.AssignType.Employee.ToString();
                            assignedBillAC.ExcelUploadLogId = excelUploadLog.Id;
                            assignedBillAC.Currency = lstExcelDetailForEmployee[0].Currency.Code;
                            EnumList.Month monthEnum = (EnumList.Month)billAllocationAC.Month;
                            assignedBillAC.BillDate = monthEnum.ToString() + " " + billAllocationAC.Year.ToString();
                            assignedBillAC.Provider = excelUploadLog.Provider.Name;
                            assignedBillAC.EmployeeName = item.ToList()[0].Employee.FullName;
                            assignedBillAC.EmployeeId = Convert.ToInt64(item.Key);
                            foreach (var subitem in item.ToList())
                            {
                                assignedBillAC.BillAmount += (subitem.CallAmount != null ? Convert.ToDecimal(subitem.CallAmount) : 0);
                            }
                            billAllocationResponse.AssignedBillList.Add(assignedBillAC);
                        }
                    }

                    if (lstExcelDetailForBusiness.Any())
                    {
                        var lstOfBusinessUnit = lstExcelDetailForBusiness.GroupBy(x => x.BusinessUnitId).ToList();
                        foreach (var item in lstOfBusinessUnit.ToList())
                        {
                            #region Get Service Types
                            var lstOfServiceGroup = item.ToList().GroupBy(x => x.ServiceType.Name).ToList();
                            string serviceTypes = string.Empty;
                            foreach (var serviceItem in lstOfServiceGroup)
                            {
                                serviceTypes += serviceItem.Key + ",";
                            }
                            #endregion

							BusinessAssignedBillAC businessAssignedBillAC = new BusinessAssignedBillAC();
							businessAssignedBillAC.ServiceType = serviceTypes.Substring(0, serviceTypes.Length - 1);
							businessAssignedBillAC.ExcelUploadLogId = excelUploadLog.Id;
							businessAssignedBillAC.AssignType = EnumList.AssignType.Business.ToString();
							businessAssignedBillAC.Currency = lstExcelDetailForBusiness[0].Currency.Code;
							EnumList.Month monthEnum = (EnumList.Month)billAllocationAC.Month;
							businessAssignedBillAC.BillDate = monthEnum.ToString() + " " + billAllocationAC.Year.ToString();
							businessAssignedBillAC.Provider = excelUploadLog.Provider.Name;
							businessAssignedBillAC.BusinessUnit = item.ToList()[0].BusinessUnitNavigation.Name;
							businessAssignedBillAC.BusinessUnitId = Convert.ToInt64(item.Key);
							foreach (var subitem in item.ToList())
							{
								businessAssignedBillAC.BillAmount += (subitem.CallAmount != null ? Convert.ToDecimal(subitem.CallAmount) : 0);
							}
							billAllocationResponse.BusinessAssignedBillList.Add(businessAssignedBillAC);
						}
					}

                    if (lstForUnAssigned.Any())
                    {
                        billAllocationResponse.UnAssignedBillList.AddRange(_mapper.Map<List<UnAssignedBillAC>>(lstForUnAssigned));
                    }

                }
            }
            return billAllocationResponse;
        }

        public async Task<ResponseAC> AssigneBills(BillAssigneAC billAssigneAC, string userId)
        {
            ResponseAC responseAC = new ResponseAC();
            List<ExcelDetail> excelDetails = new List<ExcelDetail>();
            foreach (var item in billAssigneAC.UnAssignedBillList)
            {
                ExcelDetail excelDetail = await _dbTeleBilling_V01Context.ExcelDetail.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId && x.ExcelUploadLogId == item.ExcelUploadLogId);
                excelDetail.IsAssigned = true;
                excelDetail.AssignType = billAssigneAC.AssigneType;
                if (billAssigneAC.AssigneType == Convert.ToInt16(EnumList.AssignType.Business))
                    excelDetail.BusinessUnitId = billAssigneAC.BusinessUnitId;
                else
                    excelDetail.EmployeeId = billAssigneAC.EmployeeData.UserId;

                excelDetails.Add(excelDetail);
            }
            _dbTeleBilling_V01Context.UpdateRange(excelDetails);
            await _dbTeleBilling_V01Context.SaveChangesAsync();

            responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            responseAC.Message = _iStringConstant.BillAssignedSuccesfully;
            return responseAC;
        }

        public async Task<List<UnAssignedBillAC>> GetAssignedBillList(long employeeId, long exceluploadlogid, long businessunitId)
        {
            long assignTypeForEmployee = Convert.ToInt16(EnumList.AssignType.Employee);
            long assignTypeForBusiness = Convert.ToInt16(EnumList.AssignType.Business);

            List<ExcelDetail> lstForExcelDetail = new List<ExcelDetail>();
            if (employeeId != 0)
                lstForExcelDetail = await _dbTeleBilling_V01Context.ExcelDetail.Where(x => x.ExcelUploadLogId == exceluploadlogid && x.IsAssigned == true && x.AssignType == assignTypeForEmployee && x.EmployeeId == employeeId).Include(x => x.Currency).Include(x => x.ServiceType).ToListAsync();
            else if (businessunitId != 0)
                lstForExcelDetail = await _dbTeleBilling_V01Context.ExcelDetail.Where(x => x.ExcelUploadLogId == exceluploadlogid && x.IsAssigned == true && x.AssignType == assignTypeForBusiness && x.BusinessUnitId == businessunitId).Include(x => x.Currency).Include(x => x.ServiceType).ToListAsync();

            return _mapper.Map<List<UnAssignedBillAC>>(lstForExcelDetail);
        }

        public async Task<bool> UnAssgineCallLogs(List<UnAssignedBillAC> unAssignedBillACs)
        {
            List<ExcelDetail> lstExcelDetail = new List<ExcelDetail>();
            foreach (var item in unAssignedBillACs)
            {
                ExcelDetail excelDetail = await _dbTeleBilling_V01Context.ExcelDetail.FirstAsync(x => x.Id == item.ExcelDetailId);
                excelDetail.BusinessUnitId = null;
                excelDetail.IsAssigned = false;
                excelDetail.AssignType = null;
                lstExcelDetail.Add(excelDetail);
            }
            _dbTeleBilling_V01Context.UpdateRange(lstExcelDetail);
            await _dbTeleBilling_V01Context.SaveChangesAsync();
            return true;
        }

		//This is only for employee bill
		public async Task<ResponseAC> BillAllocation(BillAllocationAC billAllocationAC, long userId)
		{
            ResponseAC responseAc = new ResponseAC();
            List<ExcelUploadLog> excelUploadLogList = await _dbTeleBilling_V01Context.ExcelUploadLog.Where(x => x.Month == billAllocationAC.Month && x.Year == billAllocationAC.Year && x.ProviderId == billAllocationAC.ProviderId && !x.IsDelete).Include(x => x.Provider).ToListAsync();
            List<ExcelDetail> lstExcelDetailForEmployee = new List<ExcelDetail>();
            List<ExcelUploadLogServiceType> lstExcelUploadLogServiceType = new List<ExcelUploadLogServiceType>();

            foreach (var excelUploadLog in excelUploadLogList)
            {
                foreach (var serviceType in billAllocationAC.ServiceTypes)
                {
                    ExcelUploadLogServiceType excelUploadLogServiceType = await _dbTeleBilling_V01Context.ExcelUploadLogServiceType.FirstOrDefaultAsync(x => x.ServiceTypeId == serviceType.Id && !x.IsAllocated && !x.IsDelete && x.ExceluploadLogId == excelUploadLog.Id);
                    if (excelUploadLogServiceType != null)
                    {
                        excelUploadLogServiceType.IsAllocated = true;
                        excelUploadLogServiceType.UpdatedBy = userId;
                        excelUploadLogServiceType.UpdatedDate = DateTime.Now;

                        lstExcelUploadLogServiceType.Add(excelUploadLogServiceType);
                        lstExcelDetailForEmployee.AddRange(await _dbTeleBilling_V01Context.ExcelDetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == true && x.ServiceTypeId == serviceType.Id).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.ServiceType).ToListAsync());
                    }
                }
            }

            if (lstExcelDetailForEmployee.Any())
            {

                #region Bill Master Entry
                BillMaster billMaster = new BillMaster();
                billMaster.ProviderId = Convert.ToInt64(billAllocationAC.ProviderId);
                billMaster.BillMonth = billAllocationAC.Month;
                billMaster.BillYear = billAllocationAC.Year;
                billMaster.BillStatusId = Convert.ToInt16(EnumList.BillStatus.BillAllocated);
                billMaster.BillNumber = _iLogManagement.GenerateBillNumber();
                billMaster.BillAmount = 0;//remaning;
                billMaster.BillDueDate = billAllocationAC.BillDueDate.AddDays(1);
                billMaster.BillAllocationDate = DateTime.Now;
                billMaster.BillAllocatedBy = userId;
                billMaster.CreatedBy = userId;
                billMaster.CreatedDate = DateTime.Now;
                billMaster.CurrencyId = null;

                await _dbTeleBilling_V01Context.AddAsync(billMaster);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                #endregion

                List<BillMasterServiceType> billMasterServiceTypes = new List<BillMasterServiceType>();
                foreach (var serviceDetail in lstExcelUploadLogServiceType)
                {
					if(!_dbTeleBilling_V01Context.BillMasterServiceType.Any(x=>!x.IsDelete && x.BillMasterId == billMaster.Id && x.ServiceTypeId == serviceDetail.ServiceTypeId))
					{
						BillMasterServiceType billMasterServiceType = new BillMasterServiceType();
						billMasterServiceType.BillMasterId = billMaster.Id;
						billMasterServiceType.ServiceTypeId = serviceDetail.ServiceTypeId;
						billMasterServiceTypes.Add(billMasterServiceType);
					}
                }
                await _dbTeleBilling_V01Context.AddRangeAsync(billMasterServiceTypes);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                decimal totalBillAmount = 0;
                long? currencyId = null;

                var lstofEmployee = lstExcelDetailForEmployee.GroupBy(x => new { x.EmployeeId, x.CallerNumber }).ToList();
                foreach (var item in lstofEmployee.ToList())
                {
                    decimal totalEmployeeBillAmount = 0;
                    List<ExcelDetail> finalItem = item.ToList();
                    if (finalItem.ToList().Any())
                    {

                        #region Employee Bill Master Entry
                        MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == finalItem[0].EmployeeId && !x.IsDelete);
                        TelephoneNumberAllocation telephoneNumberAllocation = await _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstOrDefaultAsync(x => x.TelephoneNumber == finalItem[0].CallerNumber && x.EmployeeId == mstEmployee.UserId && !x.IsDelete);

                        EmployeeBillMaster employeeBillMaster = new EmployeeBillMaster();
                        employeeBillMaster.BillMasterId = billMaster.Id;

                        #region It's Not Proper Way, Need to clear
                        if (mstEmployee != null)
                        {
                            employeeBillMaster.LinemanagerId = mstEmployee.LineManagerId;
                            employeeBillMaster.EmpBusinessUnitId = mstEmployee.BusinessUnitId;
                            employeeBillMaster.EmployeeId = mstEmployee.UserId;
                        }
                        #endregion

                        employeeBillMaster.ProviderId = billAllocationAC.ProviderId;
                        employeeBillMaster.TelephoneNumber = finalItem[0].CallerNumber;
                        employeeBillMaster.BillMonth = billMaster.BillMonth;
                        employeeBillMaster.BillNumber = billMaster.BillNumber;
                        employeeBillMaster.BillYear = billMaster.BillYear;
                        employeeBillMaster.CreatedBy = userId;
                        employeeBillMaster.CreatedDate = DateTime.Now;
                        employeeBillMaster.CurrencyId = finalItem[0].CurrencyId;
                        employeeBillMaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
                        employeeBillMaster.MbileAssignType = telephoneNumberAllocation.AssignTypeId;
                        employeeBillMaster.TotalBillAmount = 0;
						employeeBillMaster.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                        await _dbTeleBilling_V01Context.AddAsync(employeeBillMaster);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();

                        #endregion

                        List<TelePhoneNumberAllocationPackage> telePhoneNumberAllocationPackage = await _dbTeleBilling_V01Context.TelePhoneNumberAllocationPackage.Where(x => x.TelephoneNumberAllocationId == telephoneNumberAllocation.Id && !x.IsDelete).ToListAsync();
                        foreach (var itemService in telePhoneNumberAllocationPackage)
                        {
                            foreach (var selectedService in lstExcelUploadLogServiceType)
                            {
                                if (itemService.ServiceId == selectedService.ServiceTypeId)
                                {
									if(!_dbTeleBilling_V01Context.EmployeeBillServicePackage.Any(x=>x.ServiceTypeId == itemService.ServiceId && x.EmployeeBillId == employeeBillMaster.Id && !x.IsDelete))
									{
										EmployeeBillServicePackage employeeBillServicePackage = new EmployeeBillServicePackage();
										employeeBillServicePackage.PackageId = itemService.PackageId;
										employeeBillServicePackage.ServiceTypeId = itemService.ServiceId;
										employeeBillServicePackage.EmployeeBillId = employeeBillMaster.Id;
										long businessAsignType = Convert.ToInt16(EnumList.AssignType.Business);
									    List<ExcelDetail> lstExcelDetils = finalItem.Where(x=>x.ServiceTypeId == itemService.ServiceId && x.AssignType == businessAsignType).ToList();									
										decimal totalAutoBusinessAmount =0;
										foreach(var exceldetail in lstExcelDetils)
										{
											totalAutoBusinessAmount += (exceldetail.CallAmount != null ? Convert.ToDecimal(exceldetail.CallAmount) : 0);
										}
										employeeBillServicePackage.BusinessIdentificationAmount = totalAutoBusinessAmount;
										employeeBillServicePackage.BusinessTotalAmount = totalAutoBusinessAmount;
										employeeBillServicePackage.PersonalIdentificationAmount = 0;

										await _dbTeleBilling_V01Context.AddAsync(employeeBillServicePackage);
										await _dbTeleBilling_V01Context.SaveChangesAsync();
									}
                                }
                            }
                        }

                        List<BillDetails> billDetailList = new List<BillDetails>();
                        foreach (var numberEmployee in finalItem)
                        {
                            BillDetails billDetails = new BillDetails();
                            billDetails.BillMasterId = billMaster.Id;
                            billDetails.CallAmount = numberEmployee.CallAmount;
                            totalEmployeeBillAmount += numberEmployee.CallAmount != null ? Convert.ToDecimal(billDetails.CallAmount) : 0;
                            billDetails.CallAssignedBy = userId;
                            billDetails.CallAssignedDate = DateTime.Now;
                            billDetails.CallDate = numberEmployee.CallDate;
                            billDetails.CallDuration = numberEmployee.CallDuration;
                            billDetails.CallerName = numberEmployee.CallerName;
                            billDetails.CallerNumber = numberEmployee.CallerNumber;
							billDetails.CallIwithInGroup = numberEmployee.CallWithinGroup;
                            billDetails.CallTime = numberEmployee.CallTime;
                            billDetails.CallTransactionTypeId = numberEmployee.CallTransactionTypeId;
                            billDetails.CallIdentificationType = null;
                            billDetails.CreatedBy = userId;
                            billDetails.CreatedDate = DateTime.Now;
                            billDetails.EmployeeBillId = employeeBillMaster.Id;
                            billDetails.ReceiverName = numberEmployee.ReceiverName;
                            billDetails.ReceiverNumber = numberEmployee.ReceiverNumber;
                            billDetails.ServiceTypeId = numberEmployee.ServiceTypeId;
                            billDetails.SubscriptionType = numberEmployee.SubscriptionType;
                            billDetails.AssignTypeId = numberEmployee.AssignType;
							if(numberEmployee.AssignType == Convert.ToInt16(EnumList.AssignType.Business)) {
								billDetails.BusinessUnitId = numberEmployee.BusinessUnitId;
								billDetails.CallIdentificationType = numberEmployee.AssignType;
								billDetails.IsAutoAssigned = true;
							}
							billDetails.TransType = numberEmployee.TransType;
                            billDetailList.Add(billDetails);
                        }

                        await _dbTeleBilling_V01Context.AddRangeAsync(billDetailList);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();


                        #region Employee Total Amount Update
                        EmployeeBillMaster newEmployeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.FirstAsync(x => x.Id == employeeBillMaster.Id);
                        newEmployeeBillMaster.TotalBillAmount = totalEmployeeBillAmount;
						totalBillAmount += totalEmployeeBillAmount;
                        currencyId = finalItem[0].CurrencyId;
                        _dbTeleBilling_V01Context.Update(newEmployeeBillMaster);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();
                        #endregion
                    }
                }
                #region Bill Master Total Bill AMount Update
                billMaster = await _dbTeleBilling_V01Context.BillMaster.FirstAsync(x => x.Id == billMaster.Id);
                billMaster.BillAmount = totalBillAmount;
                billMaster.CurrencyId = currencyId;
                _dbTeleBilling_V01Context.Update(billMaster);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                #endregion

                #region Update Excel Upload Log Service Table
                if (lstExcelUploadLogServiceType.Any())
                {
                    _dbTeleBilling_V01Context.UpdateRange(lstExcelUploadLogServiceType);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                }
                #endregion
            }
            responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            responseAc.Message = _iStringConstant.BillAllocatedSuccesfully;
            return responseAc;
		}
		#endregion

		#endregion
	}

}
