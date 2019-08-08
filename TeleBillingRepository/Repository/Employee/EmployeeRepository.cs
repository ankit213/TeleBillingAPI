using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Service;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Employee
{
    public class EmployeeRepository : IEmployeeRepository
    {

        #region "Private Variable(s)"
        private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private readonly IEmailSender _iEmailSender;
        private IMapper _mapper;
        private readonly DAL _objDal = new DAL();
        #endregion

        #region "Constructor"
        public EmployeeRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
            , ILogManagement ilogManagement
            , IEmailSender iemailSender)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _mapper = mapper;
            _iLogManagement = ilogManagement;
            _iEmailSender = iemailSender;
        }
        #endregion

        #region Public Method(s)

        public async Task<EmployeeProfileAC> GetUserProfile(long userId)
        {
            EmployeeProfileAC responseAc = new EmployeeProfileAC();

            try
            {
                MstEmployee emp = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(userId);

                EmployeeProfileDetailAC userProfile = new EmployeeProfileDetailAC();
                SortedList sl = new SortedList();
                sl.Add("@UserId", userId);
                DataSet ds = _objDal.GetDataSet("uspGetUserProfile", sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        userProfile = _objDal.ConvertDataTableToGenericList<EmployeeProfileDetailAC>(ds.Tables[0]).FirstOrDefault();
                    }
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
                    {
                        userProfile.employeeTelephoneDetails = _objDal.ConvertDataTableToGenericList<EmployeeTelephoneDetailsAC>(ds.Tables[1]);

                    }
                }

                if (userProfile.UserId > 0)
                {
                    responseAc.Message = _iStringConstant.DataFound;
                    responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                    responseAc.UserProfileData = userProfile;
                    return responseAc;
                }
                else
                {
                    responseAc.Message = _iStringConstant.DataNotFound;
                    responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.NotFound);
                    return responseAc;
                }
            }
            catch (Exception e)
            {
                responseAc.Message = "Error :" + e.Message;
                responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responseAc;
            }
        }

        public async Task<ResponseAC> resetPassword(EmployeeProfileDetailAC employeeProfileDetailAC, long userId)
        {
            ResponseAC responeAC = new ResponseAC();
            MstEmployee mstEmployee = new MstEmployee();

            mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(employeeProfileDetailAC.UserId);
            if (mstEmployee != null)
            {
                #region Transaction Log Entry
                if (mstEmployee.TransactionId == null)
                    mstEmployee.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                var jsonSerailzeObj = JsonConvert.SerializeObject(mstEmployee);
                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstEmployee.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
                #endregion

                mstEmployee.Password = employeeProfileDetailAC.NewPassword;
                mstEmployee.UpdatedBy = userId;
                mstEmployee.UpdatedDate = DateTime.Now;

                _dbTeleBilling_V01Context.Update(mstEmployee);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                responeAC.Message = "Password Reset Successfully";
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

            }
            else
            {
                responeAC.Message = _iStringConstant.DelegateAlreadyExists;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
            }
            return responeAC;
        }

        public List<EmployeeProfileDetailAC> GetEmployeeList()
        {
            List<EmployeeProfileDetailAC> employeelist = new List<EmployeeProfileDetailAC>();

            try
            {
                DataSet ds = _objDal.GetDataSet("uspGetEmployeeList");
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        employeelist = _objDal.ConvertDataTableToGenericList<EmployeeProfileDetailAC>(ds.Tables[0]).ToList();
                    }
                }

                if (employeelist != null)
                {
                    if (employeelist.Count() > 0)
                    {
                        return employeelist;
                    }
                }
                return new List<EmployeeProfileDetailAC>();
            }
            catch (Exception e)
            {
                return new List<EmployeeProfileDetailAC>();
            }
        }

        public async Task<bool> DeleteEmployee(long id, long userId)
        {
            TeleBillingUtility.Models.MstEmployee employee = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(id);
            if (employee != null)
            {
                if (employee.IsSystemUser)
                {
                    employee.IsDelete = true;
                    employee.UpdatedBy = userId;
                    employee.UpdatedDate = DateTime.Now;
                    _dbTeleBilling_V01Context.Update(employee);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ChangeEmployeeStatus(long Id, long userId)
        {
            MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == Id);
            if (mstEmployee != null)
            {
                #region Transaction Log Entry
                if (mstEmployee.TransactionId == null)
                    mstEmployee.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                var jsonSerailzeObj = JsonConvert.SerializeObject(mstEmployee);
                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstEmployee.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
                #endregion

                mstEmployee.IsActive = !mstEmployee.IsActive;
                mstEmployee.UpdatedBy = userId;
                mstEmployee.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(mstEmployee);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> checkPFNumberUnique(string EmpPFNumber, long empId = 0)
        {

            try
            {
                TeleBillingUtility.Models.MstEmployee employee = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.EmpPfnumber == EmpPFNumber && !x.IsDelete).FirstOrDefaultAsync();
                if (employee != null)
                {
                    if (employee.UserId == empId)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<ResponseAC> AddEmployee(MstEmployeeAC employee, long userId)
        {
            ResponseAC responeAC = new ResponseAC();

            try
            {
                if (employee != null)
                {
                    string pfNumber = employee.EmpPFNumber;
                    if (await checkPFNumberUnique(pfNumber, 0))
                    {
                        MstEmployee mstEmployee = new MstEmployee();
                        mstEmployee.FullName = employee.FullName;
                        mstEmployee.ExtensionNumber = employee.ExtensionNumber;
                        mstEmployee.EmpPfnumber = employee.EmpPFNumber;
                        mstEmployee.EmailId = employee.EmailId;

                        mstEmployee.Password = "User@123";
                        mstEmployee.RoleId = employee.RoleId;
                        mstEmployee.IsActive = true;
                        mstEmployee.IsSystemUser = true;

                        mstEmployee.DepartmentId = employee.DepartmentId;
                        mstEmployee.Designation = employee.Designation;
                        mstEmployee.Description = employee.Description;
                        mstEmployee.BusinessUnitId = employee.BusinessUnitId;
                        mstEmployee.CostCenterId = employee.CostCenterId;
                        mstEmployee.CountryId = employee.CountryId;
                        mstEmployee.IsPresidentOffice = employee.IsPresidentOffice;
                        if (employee.ManagerEmployee != null)
                        {
                            if (employee.ManagerEmployee.UserId > 0)
                            {
                                mstEmployee.LineManagerId = employee.ManagerEmployee.UserId;
                            }
                        }

                        if (mstEmployee.LineManagerId == 0)
                        {
                            responeAC.Message = "Line Manager is not valid !";
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                            return responeAC;
                        }



                        mstEmployee.CreatedBy = userId;
                        mstEmployee.CreatedDate = DateTime.Now;
                        mstEmployee.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                        await _dbTeleBilling_V01Context.AddAsync(mstEmployee);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();
                        responeAC.Message = "Employee Added Successfully !";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

                        #region Send Email for Registration Confirmation
                        EmployeeProfileAC empDetail = new EmployeeProfileAC();
                        try
                        {
                            empDetail = await GetUserProfile(mstEmployee.UserId);
                            if (empDetail != null)
                            {
                                if (empDetail.UserProfileData.UserId > 0)
                                {
                                    Dictionary<string, string> replacement = new Dictionary<string, string>();
                                    replacement.Add("{newEmpName}", empDetail.UserProfileData.FullName);
                                    replacement.Add("{PFNumber}", empDetail.UserProfileData.EmpPFNumber);
                                    replacement.Add("{Email}", empDetail.UserProfileData.EmailId);
                                    replacement.Add("{Password}", empDetail.UserProfileData.Password);

                                    replacement.Add("{EmpDesignation}", empDetail.UserProfileData.Designation);
                                    replacement.Add("{Emplocation}", empDetail.UserProfileData.Country);
                                    replacement.Add("{EmplineManger}", empDetail.UserProfileData.LineManager);
                                    replacement.Add("{EmpDepartment}", empDetail.UserProfileData.Department);
                                    replacement.Add("{EmpCostCenter}", empDetail.UserProfileData.CostCenter);
                                    replacement.Add("{EmpBusinessUnit}", empDetail.UserProfileData.BusinessUnit);
                                    bool issent = false;
                                    string EmailId = empDetail.UserProfileData.EmailId;
                                    if (empDetail.UserProfileData.EmailId != null && empDetail.UserProfileData.EmailId.Length > 0)
                                    {
                                        issent = await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.NewRegistrationConfirmation), replacement, EmailId);
                                    }
                                    else
                                    {// get line manager email
                                        string linemanagerEmail = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.UserId == mstEmployee.LineManagerId).Select(x => x.EmailId).FirstOrDefaultAsync();

                                        issent = await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.NewRegistrationInYourTeam), replacement, linemanagerEmail);

                                    }

                                    if (!issent)
                                    {
                                        responeAC.StatusCode = Convert.ToInt32(EnumList.ResponseType.Success);
                                        responeAC.Message = "Employee Added Successfully! We Could Not Sent Mail Confirmation.";
                                    }

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            responeAC.StatusCode = Convert.ToInt32(EnumList.ResponseType.Success);
                            responeAC.Message = "Employee Added Successfully! Error :" + e.Message + " We Could Not Sent Mail Confirmation.";
                        }

                        #endregion

                        return responeAC;
                    }
                    else
                    {
                        responeAC.Message = "PFNumber is Invalid";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }
                }
                else
                {
                    responeAC.Message = _iStringConstant.DataNotFound;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }

            }
            catch (Exception e)
            {
                responeAC.Message = "Error : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }

        }

        public async Task<MstEmployeeAC> GetEmployeeById(long userId)
        {
            MstEmployeeAC responseAc = new MstEmployeeAC();
            try
            {
                MstEmployee employee = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(userId);


                MstEmployeeAC mstEmployee = new MstEmployeeAC();

               
                SortedList sl = new SortedList();
                sl.Add("@UserId", userId);
                DataSet ds = _objDal.GetDataSet("uspGetEmployeeById", sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        mstEmployee = _objDal.ConvertDataTableToGenericList<MstEmployeeAC>(ds.Tables[0]).FirstOrDefault();
                    }
                }

                if (employee != null)
                {
                    mstEmployee.FullName = employee.FullName;
                    mstEmployee.ExtensionNumber = employee.ExtensionNumber;
                    mstEmployee.EmpPFNumber = employee.EmpPfnumber;
                    mstEmployee.EmailId = employee.EmailId;
                    mstEmployee.Password = employee.Password;
                    mstEmployee.RoleId = employee.RoleId;

                    mstEmployee.IsSystemUser = employee.IsSystemUser;
                    mstEmployee.Designation = employee.Designation;

                    mstEmployee.DepartmentId = employee.DepartmentId;
                    mstEmployee.BusinessUnitId = employee.BusinessUnitId;
                    mstEmployee.CostCenterId = employee.CostCenterId;
                    mstEmployee.CountryId = employee.CountryId;
                    mstEmployee.IsPresidentOffice = employee.IsPresidentOffice;
                    mstEmployee.Description = employee.Description;
                    mstEmployee.TransactionId = employee.TransactionId;
                    mstEmployee.IsActive = employee.IsActive;
                    mstEmployee.IsDelete = employee.IsDelete;

                    if (employee.LineManagerId > 0)
                    {
                        MstEmployee lineManagerEmpDetail = new MstEmployee();
                        lineManagerEmpDetail = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.UserId == employee.LineManagerId).FirstOrDefaultAsync();
                        mstEmployee.LineManagerId = employee.LineManagerId;
                        mstEmployee.ManagerEmployee = _mapper.Map<EmployeeAC>(lineManagerEmpDetail);
                    }

                }
                if (mstEmployee != null)
                {
                    if (mstEmployee.UserId > 0)
                    {
                        responseAc = mstEmployee;
                        return responseAc;
                    }
                }

                return new MstEmployeeAC();

            }
            catch (Exception e)
            {
                return new MstEmployeeAC();
            }
        }

        public async Task<ResponseAC> EditEmployee(MstEmployeeAC employee, long userId)
        {
            ResponseAC responeAC = new ResponseAC();
            try
            {
                if (employee != null)
                {
                    if (employee.UserId > 0)
                    {
                        MstEmployee mstEmployee = new MstEmployee();
                        mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(employee.UserId);

                        if (mstEmployee != null)
                        {
                            if (mstEmployee.IsSystemUser)
                            {
                                string pfNumber = employee.EmpPFNumber;
                                if (await checkPFNumberUnique(pfNumber, employee.UserId))
                                {
                                    mstEmployee.FullName = employee.FullName;
                                    mstEmployee.ExtensionNumber = employee.ExtensionNumber;
                                    mstEmployee.EmpPfnumber = employee.EmpPFNumber;
                                    mstEmployee.EmailId = employee.EmailId;

                                    mstEmployee.DepartmentId = employee.DepartmentId;
                                    mstEmployee.Designation = employee.Designation;
                                    mstEmployee.Description = employee.Description;
                                    mstEmployee.BusinessUnitId = employee.BusinessUnitId;
                                    mstEmployee.CostCenterId = employee.CostCenterId;
                                    mstEmployee.CountryId = employee.CountryId;
                                    mstEmployee.IsPresidentOffice = employee.IsPresidentOffice;
                                    mstEmployee.RoleId = employee.RoleId;
                                    mstEmployee.LineManagerId = employee.LineManagerId;

                                    mstEmployee.UpdatedBy = userId;
                                    mstEmployee.UpdatedDate = DateTime.Now;
                                    mstEmployee.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
                                    _dbTeleBilling_V01Context.Update(mstEmployee);
                                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                                    responeAC.Message = "Employee Updated Successfully !";
                                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                                    return responeAC;
                                }
                                else
                                {
                                    responeAC.Message = "PFNumber is Invalid";
                                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                    return responeAC;
                                }
                            }
                            else
                            {
                                responeAC.Message = "Employee is not system user.";
                                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                return responeAC;
                            }
                        }
                    }
                }
                responeAC.Message = _iStringConstant.DataNotFound;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }
            catch (Exception e)
            {
                responeAC.Message = "Error : " + e.Message;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }
        }

        public async Task<bool> checkEmailUnique(string email, long? id = 0)
        {
            var emailid = email.Replace(" ", String.Empty).ToLower();
            var Id = id;
            bool IsValid = false;
            if (id > 0)
            {
                var oldEmail = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.UserId == id).Select(x => x.EmailId).FirstOrDefaultAsync();
                oldEmail = oldEmail.Replace(" ", String.Empty).ToLower();
                if (emailid == oldEmail)
                {
                    IsValid = true;
                    return IsValid;
                }
                else
                {
                    var chkEmail = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.EmailId.Replace(" ", String.Empty).ToUpper() == emailid.Replace(" ", String.Empty).ToUpper() && !x.IsDelete).Select(x => x.UserId).ToListAsync();
                    if (chkEmail.Count > 0)
                    {
                        var ids = await _dbTeleBilling_V01Context.MstEmployee.Where(x => chkEmail.Contains(x.UserId)).Select(x => x.UserId).FirstOrDefaultAsync();
                        if (ids > 0)
                        {
                            IsValid = false;
                            return IsValid;
                        }
                        else
                        {
                            IsValid = true;
                            return IsValid;
                        }

                    }
                }
            }
            else
            {
                if (emailid != null)
                {
                    var chkid = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.EmailId.Replace(" ", String.Empty).ToUpper() == emailid.Replace(" ", String.Empty).ToUpper() && !x.IsDelete).Select(x => x.UserId).ToListAsync();
                    if (chkid.Count > 0)
                    {
                        IsValid = false;
                        return IsValid;
                    }
                    else
                    {
                        IsValid = true;
                        return IsValid;
                    }
                }
            }
            IsValid = true;
            return IsValid;
        }

        #endregion
    }

}
