using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.BillDelegate
{
    public class BillDelegateRepository : IBillDelegateRepository
    {
        #region "Private Variable(s)"
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;

        #endregion

        #region "Constructor"
        public BillDelegateRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, ILogManagement ilogManagement,
            IStringConstant iStringConstant)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _mapper = mapper;
            _iLogManagement = ilogManagement;
        }
        #endregion

        #region "Public Method(s)"



        public async Task<List<BillDelegatesListAC>> GetDelegates()
        {
            try
            {

                List<TeleBillingUtility.Models.Billdelegate> lstDetails = await _dbTeleBilling_V01Context.Billdelegate
                                                                      .Where(x => !x.IsDelete)
                                                                      .OrderByDescending(x => x.CreatedDate)
                                                                      .Include(x => x.DelegateEmployee)
                                                                      .Include(x => x.Employee)
                                                                      .ToListAsync();
                return _mapper.Map<List<BillDelegatesListAC>>(lstDetails);
            }
            catch (Exception)
            {
                return new List<BillDelegatesListAC>();
            }


        }


        public async Task<BillDelegatesAC> GetDelegateById(long id)
        {
            TeleBillingUtility.Models.Billdelegate delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.Include(x => x.Employee).Include(x => x.DelegateEmployee).FirstOrDefaultAsync(x => !x.IsDelete && x.Id == id);
            BillDelegatesAC billDelegatesAC = new BillDelegatesAC();
            billDelegatesAC.Id = id;
            billDelegatesAC.Employee = new EmployeeAC();
            billDelegatesAC.Employee = _mapper.Map<EmployeeAC>(delegateDetail.Employee);
            billDelegatesAC.DelegateEmployee = new EmployeeAC();
            billDelegatesAC.DelegateEmployee = _mapper.Map<EmployeeAC>(delegateDetail.DelegateEmployee);
            billDelegatesAC.AllowBillApproval = delegateDetail.AllowBillApproval;
            billDelegatesAC.AllowBillIdentification = delegateDetail.AllowBillIdentification;
            return billDelegatesAC;
        }

        public async Task<ResponseAC> EditDelegate(BillDelegatesAC billDelegatesAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();
            if (await _dbTeleBilling_V01Context.Billdelegate.FirstOrDefaultAsync(x => x.Id != billDelegatesAC.Id && x.EmployeeId == billDelegatesAC.Employee.UserId && x.DelegateEmployeeId == billDelegatesAC.DelegateEmployee.UserId && !x.IsDelete) == null)
            {
                TeleBillingUtility.Models.Billdelegate delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.FirstOrDefaultAsync(x => x.Id == billDelegatesAC.Id && !x.IsDelete);

                #region Transaction Log Entry
                if (delegateDetail.TransactionId == null)
                    delegateDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                var jsonSerailzeObj = JsonConvert.SerializeObject(delegateDetail);
                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(delegateDetail.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
                #endregion

                delegateDetail.EmployeeId = billDelegatesAC.Employee.UserId;
                delegateDetail.DelegateEmployeeId = billDelegatesAC.DelegateEmployee.UserId;
                delegateDetail.AllowBillApproval = billDelegatesAC.AllowBillApproval;
                delegateDetail.AllowBillIdentification = billDelegatesAC.AllowBillIdentification;
                delegateDetail.UpdatedBy = userId;
                delegateDetail.UpdatedDate = DateTime.Now;

                _dbTeleBilling_V01Context.Update(delegateDetail);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                responeAC.Message = _iStringConstant.DelegateUpdateSuccessfully;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditDelegate, loginUserName, userId, "Delegate user", (int)EnumList.ActionTemplateTypes.Edit, delegateDetail.Id);
            }
            else
            {
                responeAC.Message = _iStringConstant.DelegateAlreadyExists;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
            }
            return responeAC;
        }

        public async Task<ResponseAC> AddDelegate(BillDelegatesAC billDelegatesAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();

            if (await _dbTeleBilling_V01Context.Billdelegate.FirstOrDefaultAsync(x => x.EmployeeId == billDelegatesAC.Employee.UserId && x.DelegateEmployeeId == billDelegatesAC.DelegateEmployee.UserId && !x.IsDelete) == null)
            {
                TeleBillingUtility.Models.Billdelegate delegateDetail = new TeleBillingUtility.Models.Billdelegate();

                delegateDetail.EmployeeId = billDelegatesAC.Employee.UserId;
                delegateDetail.DelegateEmployeeId = billDelegatesAC.DelegateEmployee.UserId;
                delegateDetail.AllowBillApproval = billDelegatesAC.AllowBillApproval;
                delegateDetail.AllowBillIdentification = billDelegatesAC.AllowBillIdentification;
                delegateDetail.CreatedBy = userId;
                delegateDetail.CreatedDate = DateTime.Now;
                delegateDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                await _dbTeleBilling_V01Context.AddAsync(delegateDetail);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                responeAC.Message = _iStringConstant.DelegateAddedSuccessfully;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddDelegate, loginUserName, userId, "Delegate user", (int)EnumList.ActionTemplateTypes.Add, delegateDetail.Id);
            }
            else
            {
                responeAC.Message = _iStringConstant.DelegateAlreadyExists;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
            }
            return responeAC;
        }

        public async Task<bool> DeleteDelegate(long id, long userId, string loginUserName)
        {
            TeleBillingUtility.Models.Billdelegate delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.FindAsync(id);
            if (delegateDetail != null)
            {
                delegateDetail.IsDelete = true;
                delegateDetail.UpdatedBy = userId;
                delegateDetail.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(delegateDetail);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteDelegate, loginUserName, userId, "Delegate user", (int)EnumList.ActionTemplateTypes.Delete, delegateDetail.Id);
                return true;
            }
            return false;
        }

        public async Task<ResponseAC> checkDelegatePair(EmployeeAC Employee, EmployeeAC DelegateEmployee, long delegateid = 0)
        {
            ResponseAC responeAC = new ResponseAC();


            #region --> Check Pair Wise

            if (Employee != null && Employee.UserId > 0 && DelegateEmployee != null && DelegateEmployee.UserId > 0)
            {
                TeleBillingUtility.Models.Billdelegate delegateDetail = new TeleBillingUtility.Models.Billdelegate();
                // check both not same
                if (Employee.UserId == DelegateEmployee.UserId)
                {
                    responeAC.Message = "Employee can't delegate itself";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }

                // check same pair dose not exists before
                delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.Where(x => x.EmployeeId == Employee.UserId && x.DelegateEmployeeId == DelegateEmployee.UserId && !x.IsDelete).FirstOrDefaultAsync();

                if (delegateDetail != null)
                {
                    if (delegateDetail.Id > 0 && delegateDetail.Id != delegateid)
                    {
                        responeAC.Message = _iStringConstant.DelegateAlreadyExists;
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }
                    else if (delegateDetail.Id == delegateid)
                    {
                        responeAC.Message = "valid";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        return responeAC;
                    }
                }

            }

            #endregion

            #region --> Check it is not Multi level Delegate 

            if (DelegateEmployee != null && DelegateEmployee.UserId > 0)
            {
                TeleBillingUtility.Models.Billdelegate delegateDetail = new TeleBillingUtility.Models.Billdelegate();
                List<TeleBillingUtility.Models.Billdelegate> delegatelist = new List<TeleBillingUtility.Models.Billdelegate>();

                delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.Where(x => x.EmployeeId == DelegateEmployee.UserId && !x.IsDelete).FirstOrDefaultAsync();
                if (delegateDetail != null)
                {
                    if (delegateDetail.Id > 0)
                    {
                        responeAC.Message = "Invalid multilevel delegate! This employee (" + DelegateEmployee.FullName + ") is already delegated to another.";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }
                }
            }
            #endregion

            #region --> Check Employee can delegate to other ?
            if (Employee != null && Employee.UserId > 0)
            {
                TeleBillingUtility.Models.Billdelegate delegateDetail = new TeleBillingUtility.Models.Billdelegate();
                List<TeleBillingUtility.Models.Billdelegate> delegatelist = new List<TeleBillingUtility.Models.Billdelegate>();

                if (delegateid > 0)
                {
                    delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.FindAsync(delegateid);

                    if (delegateDetail != null)
                    {
                        if (delegateDetail.EmployeeId != Employee.UserId)
                        {
                            delegatelist = await _dbTeleBilling_V01Context.Billdelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();
                            if (delegatelist != null)
                            {
                                if (delegatelist.Count() > 0)
                                {
                                    responeAC.Message = "Employee (" + Employee.FullName + ") can't delegate to other because it is already used in delegate.";
                                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                    return responeAC;
                                }
                            }

                        }
                    }
                }
                else
                {
                    delegatelist = await _dbTeleBilling_V01Context.Billdelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();

                    if (delegatelist.Count() > 0)
                    {
                        responeAC.Message = "Employee (" + Employee.FullName + ") can't delegate to other";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }

                }

            }

            #endregion



            responeAC.Message = "valid";
            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            return responeAC;
        }

        public async Task<ResponseAC> checkIsEmployeeCanDelegated(EmployeeAC Employee, long delegateid = 0)
        {
            ResponseAC responeAC = new ResponseAC();
            TeleBillingUtility.Models.Billdelegate delegateDetail = new TeleBillingUtility.Models.Billdelegate();
            List<TeleBillingUtility.Models.Billdelegate> delegatelist = new List<TeleBillingUtility.Models.Billdelegate>();
            if (Employee != null && Employee.UserId > 0)
            {

                if (delegateid > 0)
                {
                    delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.FindAsync(delegateid);
                    if (delegateDetail.EmployeeId != Employee.UserId)
                    {
                        delegatelist = await _dbTeleBilling_V01Context.Billdelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();

                        if (delegatelist.Count() > 0)
                        {
                            responeAC.Message = "Employee can't delegate";
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                            return responeAC;
                        }
                    }
                }

                else
                {
                    delegatelist = await _dbTeleBilling_V01Context.Billdelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();

                    if (delegatelist.Count() > 0)
                    {
                        responeAC.Message = "Employee can't delegate";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }

                }

            }

            responeAC.Message = "valid";
            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            return responeAC;

        }

        public async Task<ResponseAC> checkIsEmployeeNotDelegatedToOther(EmployeeAC DelegateToEmployee)
        {
            ResponseAC responeAC = new ResponseAC();
            TeleBillingUtility.Models.Billdelegate delegateDetail = new TeleBillingUtility.Models.Billdelegate();
            List<TeleBillingUtility.Models.Billdelegate> delegatelist = new List<TeleBillingUtility.Models.Billdelegate>();
            if (DelegateToEmployee != null && DelegateToEmployee.UserId > 0)
            {
                delegateDetail = await _dbTeleBilling_V01Context.Billdelegate.Where(x => x.EmployeeId == DelegateToEmployee.UserId && !x.IsDelete).FirstOrDefaultAsync();

                if (delegateDetail.Id > 0)
                {
                    responeAC.Message = "Invalid multilevel delegate! This employee is already delegated to another.";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }
            }
            responeAC.Message = "valid";
            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            return responeAC;
        }

        #endregion
    }
}
