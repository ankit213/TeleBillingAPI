using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

namespace TeleBillingRepository.Repository.BillDelegate
{
    public class BillDelegateRepository : IBillDelegateRepository
    {
        #region "Private Variable(s)"
        private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
     
        #endregion

        #region "Constructor"
        public BillDelegateRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, ILogManagement ilogManagement,
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
                List<TeleBillingUtility.Models.BillDelegate> lstDetails = await _dbTeleBilling_V01Context.BillDelegate
                                                                      .Where(x => !x.IsDelete)
                                                                      .OrderByDescending(x => x.CreatedDate)
                                                                      .Include(x => x.DelegateEmployee)
                                                                      .Include(x => x.Employee)
                                                                      .ToListAsync();      
                return _mapper.Map<List<BillDelegatesListAC>>(lstDetails);
            }
            catch(Exception)
            {
                return new List<BillDelegatesListAC>();
            }
           
           
        }


        public async Task<BillDelegatesAC> GetDelegateById(long id)
        {
            TeleBillingUtility.Models.BillDelegate delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.Include(x=>x.Employee).Include(x=>x.DelegateEmployee).FirstOrDefaultAsync(x => !x.IsDelete && x.Id == id);
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

        public async Task<ResponseAC> EditDelegate(BillDelegatesAC billDelegatesAC, long userId)
        {
            ResponseAC responeAC = new ResponseAC();
            if (!await _dbTeleBilling_V01Context.BillDelegate.AnyAsync(x => x.Id != billDelegatesAC.Id && x.EmployeeId == billDelegatesAC.Employee.UserId && x.DelegateEmployeeId == billDelegatesAC.DelegateEmployee.UserId && !x.IsDelete))
            {
                TeleBillingUtility.Models.BillDelegate delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.FirstOrDefaultAsync(x => x.Id == billDelegatesAC.Id && !x.IsDelete);

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
            }
            else
            {
                responeAC.Message = _iStringConstant.DelegateAlreadyExists;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
            }
            return responeAC;
        }

        public async Task<ResponseAC> AddDelegate(BillDelegatesAC billDelegatesAC, long userId)
        {
            ResponseAC responeAC = new ResponseAC();
            
            if (!await _dbTeleBilling_V01Context.BillDelegate.AnyAsync(x => x.EmployeeId == billDelegatesAC.Employee.UserId && x.DelegateEmployeeId == billDelegatesAC.DelegateEmployee.UserId && !x.IsDelete))
            {
                TeleBillingUtility.Models.BillDelegate delegateDetail = new TeleBillingUtility.Models.BillDelegate();

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
            }
            else
            {
                responeAC.Message = _iStringConstant.DelegateAlreadyExists;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
            }
            return responeAC;
        }

        public async Task<bool> DeleteDelegate(long id, long userId)
        {
            TeleBillingUtility.Models.BillDelegate delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.FindAsync(id);
            if (delegateDetail != null)
            {
                delegateDetail.IsDelete = true;
                delegateDetail.UpdatedBy = userId;
                delegateDetail.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(delegateDetail);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<ResponseAC> checkDelegatePair(EmployeeAC Employee, EmployeeAC DelegateEmployee,long delegateid=0)
        {
            ResponseAC responeAC = new ResponseAC();


            #region --> Check Employee can delegate to other ?
            if (Employee != null && Employee.UserId > 0)
            {
                TeleBillingUtility.Models.BillDelegate delegateDetail = new TeleBillingUtility.Models.BillDelegate();
                List<TeleBillingUtility.Models.BillDelegate> delegatelist = new List<TeleBillingUtility.Models.BillDelegate>();

                if (delegateid > 0)
                {
                    delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.FindAsync(delegateid);

                    if (delegateDetail != null) { 
                    if (delegateDetail.EmployeeId != Employee.UserId)
                    {
                        delegatelist = await _dbTeleBilling_V01Context.BillDelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();
                        if (delegatelist != null)
                        {
                            if (delegatelist.Count() > 0)
                            {
                                responeAC.Message = "Employee (" + Employee.FullName + ") can't Delegate to Other";
                                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                return responeAC;
                            }
                        }

                    }
                }
                }

                else
                {
                    delegatelist = await _dbTeleBilling_V01Context.BillDelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();

                    if (delegatelist.Count() > 0)
                    {
                        responeAC.Message = "Employee (" + Employee.FullName + ") can't Delegate to Other";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }

                }

            }

            #endregion

            #region --> Check it is not Multi level Delegate 

            if (DelegateEmployee != null && DelegateEmployee.UserId > 0)
            {
                TeleBillingUtility.Models.BillDelegate delegateDetail = new TeleBillingUtility.Models.BillDelegate();
                List<TeleBillingUtility.Models.BillDelegate> delegatelist = new List<TeleBillingUtility.Models.BillDelegate>();

                delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.Where(x => x.EmployeeId == DelegateEmployee.UserId && !x.IsDelete).FirstOrDefaultAsync();
                if (delegateDetail != null)
                {
                    if (delegateDetail.Id > 0)
                    {
                        responeAC.Message = "Invalid Multilevel Delegate! This Employee (" + DelegateEmployee.FullName + ") is already delegated to another.";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }
                }
            }
            #endregion

            #region --> Check Pair Wise

            if (Employee!=null && Employee.UserId > 0 && DelegateEmployee != null && DelegateEmployee.UserId>0)
            {
                TeleBillingUtility.Models.BillDelegate delegateDetail = new TeleBillingUtility.Models.BillDelegate();
                // check both not same
                if (Employee.UserId == DelegateEmployee.UserId)
                {
                    responeAC.Message = "Employee can't Delegated to himself.";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }

                // check same pair dose not exists before
                delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.Where(x => x.EmployeeId == Employee.UserId && x.DelegateEmployeeId == DelegateEmployee.UserId && !x.IsDelete).FirstOrDefaultAsync();

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

            responeAC.Message = "valid";
            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
            return responeAC;
        }

        public async Task<ResponseAC> checkIsEmployeeCanDelegated(EmployeeAC Employee, long delegateid = 0)
        {
            ResponseAC responeAC = new ResponseAC();
            TeleBillingUtility.Models.BillDelegate delegateDetail = new TeleBillingUtility.Models.BillDelegate();
            List<TeleBillingUtility.Models.BillDelegate> delegatelist = new List<TeleBillingUtility.Models.BillDelegate>();
            if (Employee != null && Employee.UserId > 0)
            {

                if (delegateid > 0)
                {
                    delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.FindAsync(delegateid);
                    if (delegateDetail.EmployeeId != Employee.UserId)
                    {
                        delegatelist = await _dbTeleBilling_V01Context.BillDelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();

                        if (delegatelist.Count() > 0)
                        {
                            responeAC.Message = "Employee can't Delegate";
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                            return responeAC;
                        }
                    }
                }

                else
                {
                    delegatelist = await _dbTeleBilling_V01Context.BillDelegate.Where(x => (x.EmployeeId == Employee.UserId || x.DelegateEmployeeId == Employee.UserId) && !x.IsDelete).ToListAsync();

                    if (delegatelist.Count() > 0)
                    {
                        responeAC.Message = "Employee can't Delegate";
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
            TeleBillingUtility.Models.BillDelegate delegateDetail = new TeleBillingUtility.Models.BillDelegate();
            List<TeleBillingUtility.Models.BillDelegate> delegatelist = new List<TeleBillingUtility.Models.BillDelegate>();
            if (DelegateToEmployee != null && DelegateToEmployee.UserId > 0)
            {
                delegateDetail = await _dbTeleBilling_V01Context.BillDelegate.Where(x =>x.EmployeeId == DelegateToEmployee.UserId && !x.IsDelete).FirstOrDefaultAsync();

                if (delegateDetail.Id > 0)
                {
                    responeAC.Message = "Invalid Multilevel Delegate! This Employee is already delegated to another.";
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
