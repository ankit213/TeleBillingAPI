using AutoMapper;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
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
        private readonly telebilling_v01Context  _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private readonly IEmailSender _iEmailSender;
        private IMapper _mapper;
        private readonly DAL _objDal = new DAL();
        private readonly DALMySql _objDalmysql = new DALMySql();
        #endregion

        #region "Constructor"
        public EmployeeRepository(telebilling_v01Context  dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
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
                EmployeeProfileDetailSP userProfileData = new EmployeeProfileDetailSP();

                SortedList sl = new SortedList();
                sl.Add("userId", userId);
               
                DataSet ds = _objDalmysql.GetDataSet("uspGetUserProfile", sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        userProfileData = _objDal.ConvertDataTableToGenericList<EmployeeProfileDetailSP>(ds.Tables[0]).FirstOrDefault();
                    }
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
                    {
                        userProfileData.employeeTelephoneDetails = _objDal.ConvertDataTableToGenericList<EmployeeTelephoneDetailsAC>(ds.Tables[1]);
                    }
                }

                if (userProfileData.UserId > 0)
                {
                    userProfile = _mapper.Map<EmployeeProfileDetailAC>(userProfileData);

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
            try
            {
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
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ResetPassword, mstEmployee.FullName, userId, string.Empty, (int)EnumList.ActionTemplateTypes.ResetPassword, mstEmployee.UserId);
				}
                else
                {
                    responeAC.Message = "Employee does not found";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
       
            return responeAC;
        }

        public JqueryDataTablesPagedResults<EmployeeProfileDetailAC> GetEmployeeList(JqueryDataTablesParameters param)
		{
            List<EmployeeProfileDetailSP> employeelistData = new List<EmployeeProfileDetailSP>();
            List<EmployeeProfileDetailAC> employeelist = new List<EmployeeProfileDetailAC>();
            try
            {
				long skipRecord = param.Start;
				int length = param.Length;
				int? sortColumnNumber = null;
				string sortType = string.Empty;
				string searchValue = param.Search.Value;
				int totalSize = 0;

				if (param.Order.Length > 0)
				{
					sortColumnNumber = param.Order[0].Column;
					sortType = param.Order[0].Dir.ToString();
				}

				SortedList sl = new SortedList();
				sl.Add("SkipRecord", skipRecord);
				sl.Add("Length", length);
				sl.Add("SearchValue", searchValue);

				DataSet ds = _objDalmysql.GetDataSet("usp_GetEmployeesWithPagging",sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        employeelistData = _objDal.ConvertDataTableToGenericList<EmployeeProfileDetailSP>(ds.Tables[0]).ToList();
                    }
					if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
					{
						totalSize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
					}
				}

				if (employeelistData.Any())
                {
                    employeelist = _mapper.Map<List<EmployeeProfileDetailAC>>(employeelistData);
                }

				return new JqueryDataTablesPagedResults<EmployeeProfileDetailAC>
				{
					Items = employeelist,
					TotalSize = totalSize
				};

			}
            catch (Exception e)
            {
                return null;
            }
        }

		public List<ExportEmployeeDetailAC> GetExportEmployeeList()
		{
			List<ExportEmployeeDetailAC> exportEmployeeDetailACs = new List<ExportEmployeeDetailAC>();
			DataSet ds = _objDalmysql.GetDataSet("usp_GetEmployeeListForExport");
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					exportEmployeeDetailACs = _objDal.ConvertDataTableToGenericList<ExportEmployeeDetailAC>(ds.Tables[0]).ToList();
				}
			}
			return exportEmployeeDetailACs;
		}

		public async Task<long> DeleteEmployee(long id, long userId, string loginUserName)
        {
			if (!await _dbTeleBilling_V01Context.MstEmployee.AnyAsync(x => !x.IsDelete && x.LineManagerId == id))
			{
				SortedList sl = new SortedList();
				sl.Add("Employee_Id", id);
				int result = Convert.ToInt16(_objDalmysql.ExecuteScaler("usp_GetEmployeeExists", sl));
				if (result == 0)
				{
					TeleBillingUtility.Models.MstEmployee employee = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(id);
					if (employee.IsSystemUser)
					{
						employee.IsDelete = true;
						employee.UpdatedBy = userId;
						employee.UpdatedDate = DateTime.Now;
						_dbTeleBilling_V01Context.Update(employee);
						await _dbTeleBilling_V01Context.SaveChangesAsync();
						await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteEmployee, loginUserName, userId, "Employee(" + employee.FullName + ")", (int)EnumList.ActionTemplateTypes.Delete, employee.UserId);
						return Convert.ToInt16(EnumList.ResponseType.Success);

					}
				}
				return Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return Convert.ToInt16(EnumList.ResponseType.UserAsLineManager);
		}

        public async Task<bool> ChangeEmployeeStatus(long Id, long userId, string loginUserName)
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

				if(mstEmployee.IsActive)
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ActiveEmployee, loginUserName, userId, "Employee(" + mstEmployee.FullName + ")", (int)EnumList.ActionTemplateTypes.Active, mstEmployee.UserId);
				else
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeactiveEmployee, loginUserName, userId, "Employee(" + mstEmployee.FullName + ")", (int)EnumList.ActionTemplateTypes.Deactive, mstEmployee.UserId);

				return true;
            }
            return false;
        }

        public async Task<bool> checkPFNumberUnique(string EmpPFNumber, long empId = 0) { 
				return await _dbTeleBilling_V01Context.MstEmployee.AnyAsync(x=>x.EmpPfnumber.Trim() == EmpPFNumber.Trim() && !x.IsDelete && x.UserId != empId);
		}

        public async Task<ResponseAC> AddEmployee(MstEmployeeAC employee, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();

            try
            {
                if (employee != null)
                {
                    string pfNumber = employee.EmpPFNumber;
                    if (!await checkPFNumberUnique(pfNumber, 0))
                    {
                        MstEmployee mstEmployee = new MstEmployee();
                        mstEmployee.FullName = employee.FullName;
                        mstEmployee.ExtensionNumber = employee.ExtensionNumber;
                        mstEmployee.EmpPfnumber = employee.EmpPFNumber;
                        mstEmployee.EmailId = employee.EmailId;

                        string randomPassword = "User@";
                        string randomPadding = CreatePassword(4);
                        mstEmployee.Password = randomPassword + randomPadding;
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
						await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddEmployee, loginUserName, userId, "Employee(" + mstEmployee.FullName + ")", (int)EnumList.ActionTemplateTypes.Add, mstEmployee.UserId);

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
                                    replacement.Add("{lineManagerDepartment}", empDetail.UserProfileData.LineManager);
                                    replacement.Add("{EmpDepartment}", empDetail.UserProfileData.Department);
                                    replacement.Add("{EmpCostCenter}", empDetail.UserProfileData.CostCenter);
                                    replacement.Add("{EmpBusinessUnit}", empDetail.UserProfileData.BusinessUnit);
                                    bool issent = false;
                                    string EmailId = empDetail.UserProfileData.EmailId;
                                    
                                    if (!(string.IsNullOrEmpty(empDetail.UserProfileData.EmailId)  || empDetail.UserProfileData.EmailId == "n/a"))
                                    {
                                        issent = await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.NewRegistrationConfirmation), replacement, employee.EmailId);
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
                        responeAC.Message = "PFNumber is already exists!";
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

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public async Task<MstEmployeeAC> GetEmployeeById(long userId)
        {
            MstEmployeeAC responseAc = new MstEmployeeAC();
          
            try
            {
                MstEmployee employee = await _dbTeleBilling_V01Context.MstEmployee.FindAsync(userId);


                MstEmployeeAC mstEmployee = new MstEmployeeAC();
                MstEmployeeSP mstEmployeeData = new MstEmployeeSP();

                SortedList sl = new SortedList();
                sl.Add("userId", userId);
                DataSet ds = _objDalmysql.GetDataSet("uspGetEmployeeById", sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        mstEmployeeData = _objDal.ConvertDataTableToGenericList<MstEmployeeSP>(ds.Tables[0]).FirstOrDefault();
                    }
                }

                if (mstEmployeeData != null)
                {
                    if (mstEmployeeData.UserId > 0)
                    {
                        mstEmployee = _mapper.Map<MstEmployeeAC>(mstEmployeeData);
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

                    //mstEmployee.BusinessUnit = mstEmployeeData.BusinessUnit;
                    //mstEmployee.CostCenter = mstEmployeeData.CostCenter;
                    //mstEmployee.Department = mstEmployeeData.Department;
                    //mstEmployee.Country = mstEmployeeData.Country;



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

        public async Task<ResponseAC> EditEmployee(MstEmployeeAC employee, long userId, string loginUserName)
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
                                if (!await checkPFNumberUnique(pfNumber, employee.UserId))
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

                                    if (employee.ManagerEmployee != null)
                                    {
                                        if (employee.ManagerEmployee.UserId > 0)
                                        {
                                            mstEmployee.LineManagerId = employee.ManagerEmployee.UserId;
                                        }
                                    }
                                    else
                                    {
                                        mstEmployee.LineManagerId = employee.LineManagerId;
                                    }

                                    if (mstEmployee.LineManagerId == 0)
                                    {
                                        responeAC.Message = "Line Manager is not valid !";
                                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                        return responeAC;
                                    }

                                   
                                    mstEmployee.UpdatedBy = userId;
                                    mstEmployee.UpdatedDate = DateTime.Now;
                                    mstEmployee.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
                                    _dbTeleBilling_V01Context.Update(mstEmployee);
                                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                                    responeAC.Message = "Employee Updated Successfully !";
                                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
									await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditEmployee, loginUserName, userId, "Employee(" + mstEmployee.FullName + ")", (int)EnumList.ActionTemplateTypes.Edit, mstEmployee.UserId);
									return responeAC;
                                }
                                else
                                {
                                    responeAC.Message = "PFNumber is already exists!";
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
                if (oldEmail != null)
                {
                    oldEmail = oldEmail.Replace(" ", String.Empty).ToLower();
                }

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

		public async Task<bool> LogOutUser(long userId, string loginUserName) {
			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.LogOut, loginUserName, userId,loginUserName, (int)EnumList.ActionTemplateTypes.LogOut, userId);
			return true;
		}

		#endregion
	}

}
