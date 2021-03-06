﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Operator
{
    public class OperatorRepository : IOperatorRepository
    {

        #region "Private Variable(s)"
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
        #endregion

        #region "Constructor"
        public OperatorRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
            ILogManagement iLogManagement)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _iLogManagement = iLogManagement;
            _mapper = mapper;
        }
        #endregion

        #region Public Method(s)

        public async Task<List<OperatorCallLogAC>> OperatorCallLogList()
        {
            List<Operatorcalllog> operatorCallLogs = await _dbTeleBilling_V01Context.Operatorcalllog.Where(x => !x.IsDelete).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.CallType).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<OperatorCallLogAC>>(operatorCallLogs);
        }

        public async Task<ResponseAC> AddOperatorCallLog(long userId, OperatorCallLogDetailAC operatorCallLogDetailAC, string loginUserName)
        {
            ResponseAC response = new ResponseAC();
            Operatorcalllog operatorCall = _mapper.Map<Operatorcalllog>(operatorCallLogDetailAC);
            operatorCall.CreatedBy = userId;
            operatorCall.CreatedDate = DateTime.Now;
            operatorCall.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
            await _dbTeleBilling_V01Context.AddAsync(operatorCall);
            await _dbTeleBilling_V01Context.SaveChangesAsync();

            await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddOperatorCallLog, loginUserName, userId, "Operator call log(Dialed Number:" + operatorCall.DialedNumber + "Extensino Number:" + operatorCall.ExtensionNumber + ")", (int)EnumList.ActionTemplateTypes.Add, operatorCall.Id);

            response.Message = _iStringConstant.OperatorCallLogAddedSuccessfully;
            response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            return response;
        }

        public async Task<bool> DeleteOperatorCallLog(long userId, long id, string loginUserName)
        {
            Operatorcalllog operatorCallLog = await _dbTeleBilling_V01Context.Operatorcalllog.FirstOrDefaultAsync(x => x.Id == id);
            if (operatorCallLog != null)
            {
                operatorCallLog.IsDelete = true;
                operatorCallLog.UpdatedBy = userId;
                operatorCallLog.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(operatorCallLog);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteOperatorCallLog, loginUserName, userId, "Operator call log(Dialed Number:" + operatorCallLog.DialedNumber + "Extensino Number:" + operatorCallLog.ExtensionNumber + ")", (int)EnumList.ActionTemplateTypes.Delete, operatorCallLog.Id);
                return true;
            }
            return false;
        }

        public async Task<ResponseAC> EditOperatorCallLog(long userId, OperatorCallLogDetailAC operatorCallLogDetailAC, string loginUserName)
        {
            ResponseAC response = new ResponseAC();
            Operatorcalllog operatorCall = await _dbTeleBilling_V01Context.Operatorcalllog.FirstAsync(x => x.Id == operatorCallLogDetailAC.Id);

            #region Transaction Log Entry
            if (operatorCall.TransactionId == null)
                operatorCall.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

            var jsonSerailzeObj = JsonConvert.SerializeObject(operatorCall);
            await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(operatorCall.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
            #endregion

            operatorCall = _mapper.Map(operatorCallLogDetailAC, operatorCall);
            operatorCall.UpdatedBy = userId;
            operatorCall.UpdatedDate = DateTime.Now;

            _dbTeleBilling_V01Context.Update(operatorCall);
            await _dbTeleBilling_V01Context.SaveChangesAsync();
            await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditOperatorCallLog, loginUserName, userId, "Operator call log(Dialed Number:" + operatorCall.DialedNumber + "Extensino Number:" + operatorCall.ExtensionNumber + ")", (int)EnumList.ActionTemplateTypes.Edit, operatorCall.Id);
            response.Message = _iStringConstant.OperatorCallLogUpdateSuccessfully;
            response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            return response;

        }

        public async Task<OperatorCallLogDetailAC> GetOperatorCallLog(long id)
        {
            Operatorcalllog operatorCallLog = await _dbTeleBilling_V01Context.Operatorcalllog.Include(x => x.Employee).Include(x => x.Employee.Department).FirstAsync(x => x.Id == id);
            OperatorCallLogDetailAC operatorCallLogDetailAC = _mapper.Map<OperatorCallLogDetailAC>(operatorCallLog);
            operatorCallLogDetailAC.EmployeeAC = new EmployeeAC();
            operatorCallLogDetailAC.EmployeeAC.FullName = operatorCallLog.Employee.FullName;
            operatorCallLogDetailAC.EmployeeAC.Department = operatorCallLog.Employee.Department.Name;
            operatorCallLogDetailAC.EmployeeAC.EmpPfnumber = operatorCallLog.Employee.EmpPfnumber;
            operatorCallLogDetailAC.EmployeeAC.UserId = operatorCallLog.Employee.UserId;
            return operatorCallLogDetailAC;
        }

        public async Task<BulkAssignTelephoneResponseAC> BulkUploadOperatorCallLog(long userId, ExcelUploadResponseAC exceluploadDetail, string loginUserName)
        {

            BulkAssignTelephoneResponseAC bulkAssignTelephoneResponseAC = new BulkAssignTelephoneResponseAC();
            List<ExcelUploadResult> excelUploadResultList = new List<ExcelUploadResult>();
            List<Operatorcalllog> operatorCallLogList = new List<Operatorcalllog>();
            FileInfo fileinfo = new FileInfo(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));
            try
            {
                using (ExcelPackage package = new ExcelPackage(fileinfo))
                {
                    for (int i = 1; i <= package.Workbook.Worksheets.Count(); i++)
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets[i];
                        if (workSheet != null)
                        {
                            string sheetName = workSheet.Name;
                            int totalRows = workSheet.Dimension.Rows;
                            int totalColums = workSheet.Dimension.Columns;
                            for (int j = 1; j <= totalRows - 1; j++)
                            {
                                Operatorcalllog operatorCallLog = new Operatorcalllog();
                                bulkAssignTelephoneResponseAC.TotalRecords += 1;
                                string callDate = workSheet.Cells[j + 1, 1].Value != null ? workSheet.Cells[j + 1, 1].Value.ToString() : string.Empty;
                                if (!string.IsNullOrEmpty(callDate))
                                {
                                    DateTime newCallDate;
                                    if (DateTime.TryParse(callDate, out newCallDate))
                                    {
                                        String.Format("{0:d/MM/yyyy}", newCallDate);
                                        string pfNumber = workSheet.Cells[j + 1, 2].Value != null ? workSheet.Cells[j + 1, 2].Value.ToString() : string.Empty;
                                        pfNumber = !string.IsNullOrEmpty(pfNumber) ? pfNumber.ToLower().Trim() : string.Empty;
                                        MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete && x.EmpPfnumber.ToLower().Trim() == pfNumber);
                                        if (mstEmployee != null)
                                        {
                                            string telephoneNumber = workSheet.Cells[j + 1, 3].Value != null ? workSheet.Cells[j + 1, 3].Value.ToString() : string.Empty;
                                            if (!string.IsNullOrEmpty(telephoneNumber))
                                            {
                                                if (telephoneNumber.Length < 50)
                                                {
                                                    string provider = workSheet.Cells[j + 1, 4].Value != null ? workSheet.Cells[j + 1, 4].Value.ToString() : string.Empty;
                                                    provider = !string.IsNullOrEmpty(provider) ? provider.ToLower().Trim() : string.Empty;
                                                    TeleBillingUtility.Models.Provider providerObj = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete && x.Name.ToLower().Trim() == provider);
                                                    if (providerObj != null)
                                                    {
                                                        string callType = workSheet.Cells[j + 1, 5].Value != null ? workSheet.Cells[j + 1, 5].Value.ToString() : string.Empty;
                                                        callType = !string.IsNullOrEmpty(callType) ? callType.ToLower().Trim() : string.Empty;
                                                        FixCalltype callTypeObj = await _dbTeleBilling_V01Context.FixCalltype.FirstOrDefaultAsync(x => x.IsActive && x.Name.ToLower().Trim() == callType);
                                                        if (callTypeObj != null)
                                                        {
                                                            operatorCallLog.CallDate = newCallDate;
                                                            operatorCallLog.CallTypeId = callTypeObj.Id;
                                                            operatorCallLog.ProviderId = providerObj.Id;
                                                            operatorCallLog.EmployeeId = mstEmployee.UserId;
                                                            operatorCallLog.EmpPfnumber = mstEmployee.EmpPfnumber;
                                                            operatorCallLog.ExtensionNumber = mstEmployee.ExtensionNumber;
                                                            operatorCallLog.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
                                                            operatorCallLog.DialedNumber = telephoneNumber;
                                                            operatorCallLog.CreatedBy = userId;
                                                            operatorCallLog.CreatedDate = DateTime.Now;
                                                            operatorCallLogList.Add(operatorCallLog);
                                                            bulkAssignTelephoneResponseAC.SuccessRecords += 1;
                                                        }
                                                        else
                                                        {
                                                            //Call Type Not Exists
                                                            bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                                            excelUploadResultList = AddedFileDataResponse(4, j, callType, _iStringConstant.CallTypeNotExists, sheetName, excelUploadResultList);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //Provider Not Exists
                                                        bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                                        excelUploadResultList = AddedFileDataResponse(4, j, provider, _iStringConstant.ProviderNotExists, sheetName, excelUploadResultList);
                                                    }
                                                }
                                                else
                                                {
                                                    //Telephone Number length
                                                    bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                                    excelUploadResultList = AddedFileDataResponse(3, j, telephoneNumber, _iStringConstant.TelePhoneNumberMaxLength, sheetName, excelUploadResultList);
                                                }
                                            }
                                            else
                                            {
                                                //Telephone number is empty
                                                bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                                excelUploadResultList = AddedFileDataResponse(3, j, telephoneNumber, _iStringConstant.TelePhoneNumberIsEmpty, sheetName, excelUploadResultList);
                                            }
                                        }
                                        else
                                        {
                                            //Employee Not Exists
                                            bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                            excelUploadResultList = AddedFileDataResponse(2, j, pfNumber, _iStringConstant.EmployeeNotExists, sheetName, excelUploadResultList);
                                        }
                                    }
                                    else
                                    {
                                        //Start Date Not Valid
                                        bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                        excelUploadResultList = AddedFileDataResponse(1, j + 1, callDate, _iStringConstant.CallDateNotValid, sheetName, excelUploadResultList);
                                    }
                                }
                                else
                                {       //Call Date Is Empty
                                    bulkAssignTelephoneResponseAC.SkipRecords += 1;
                                    excelUploadResultList = AddedFileDataResponse(1, j, callDate, _iStringConstant.CallDateIsEmpty, sheetName, excelUploadResultList);
                                }
                            }
                        }
                    }
                }
                if (File.Exists(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid)))
                    File.Delete(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));

                if (operatorCallLogList.Any())
                {
                    await _dbTeleBilling_V01Context.AddRangeAsync(operatorCallLogList);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                    await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.BulkUploadOperatorCallLog, loginUserName, userId, "Bulk Operator call log", (int)EnumList.ActionTemplateTypes.Upload, null);
                }
                bulkAssignTelephoneResponseAC.excelUploadResultList = excelUploadResultList;
                return bulkAssignTelephoneResponseAC;
            }
            catch (Exception ex)
            {
                if (File.Exists(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid)))
                    File.Delete(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));
                throw ex;
            }
        }
        #endregion

        #region Private Method(s)

        /// <summary>
        /// This method used for create response list for bulk assign telephone.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="recordDetail"></param>
        /// <param name="errorMessage"></param>
        /// <param name="sheetName"></param>
        /// <param name="excelUploadResultList"></param>
        /// <returns></returns>
        private List<ExcelUploadResult> AddedFileDataResponse(int column, int row, string recordDetail, string errorMessage, string sheetName, List<ExcelUploadResult> excelUploadResultList)
        {
            ExcelUploadResult excelUploadResult = new ExcelUploadResult();
            excelUploadResult.CellAddress = "Column: " + column + ",Row: " + row;
            excelUploadResult.ErrorMessage = errorMessage;
            excelUploadResult.RecordDetail = recordDetail;
            excelUploadResult.SheetName = sheetName;
            excelUploadResultList.Add(excelUploadResult);
            return excelUploadResultList;
        }

        #endregion
    }
}
