using AutoMapper;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TeleBillingRepository.Service;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.CommonFunction;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.BillProcess
{
	public class BillProcessRepository : IBillProcessRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IEmailSender _iEmailSender;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public BillProcessRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
			, ILogManagement ilogManagement, IEmailSender iEmailSender)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iEmailSender = iEmailSender;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<CurrentBillAC>> GetCurrentBills(long userId)
		{
			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();
			int billWaitingForIdentificationStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
			int billWaitingForLineMangerApprovalStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
			int lineMangerRejectStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject);
			int lineMangerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);

			List<Employeebillmaster> employeeBillMasters = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete && x.EmployeeId == userId && (x.EmployeeBillStatus == billWaitingForIdentificationStatusId || x.EmployeeBillStatus == billWaitingForLineMangerApprovalStatusId || x.EmployeeBillStatus == lineMangerRejectStatusId || x.EmployeeBillStatus == lineMangerApprovedStatusId)).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).OrderByDescending(x => x.Id).ToListAsync();

			List<TeleBillingUtility.Models.Billdelegate> billDelegates = await _dbTeleBilling_V01Context.Billdelegate.Where(x => x.DelegateEmployeeId == userId && !x.IsDelete && x.AllowBillIdentification).ToListAsync();
			if (billDelegates.Any())
			{
				foreach (var item in billDelegates)
				{
					List<Employeebillmaster> employeeBillMasterDelegates = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete && x.EmployeeId == item.EmployeeId && (x.EmployeeBillStatus == billWaitingForIdentificationStatusId || x.EmployeeBillStatus == billWaitingForLineMangerApprovalStatusId || x.EmployeeBillStatus == lineMangerRejectStatusId || x.EmployeeBillStatus == lineMangerApprovedStatusId)).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).OrderByDescending(x => x.Id).ToListAsync();
					if (employeeBillMasterDelegates.Any())
						currentBillACs = GetCurrentBillList(currentBillACs, employeeBillMasterDelegates, item.AllowBillIdentification, true);
				}
			}

			if (employeeBillMasters.Any())
			{
				currentBillACs = GetCurrentBillList(currentBillACs, employeeBillMasters, false, false);
			}

			return currentBillACs;
		}

		public async Task<ViewBillDetailAC> GetViewBillDetails(long employeebillmasterid)
		{
			ViewBillDetailAC viewBillDetailAC = new ViewBillDetailAC();
			Employeebillmaster employeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => x.Id == employeebillmasterid && !x.IsDelete).Include(x => x.Currency).Include(x => x.Employee).FirstOrDefaultAsync();
			List<Billdetails> billDetails = await _dbTeleBilling_V01Context.Billdetails.Where(x => x.EmployeeBillId == employeeBillMaster.Id).Include(x => x.ServiceType).ToListAsync();
			viewBillDetailAC.lstUnAssignedBill = _mapper.Map<List<UnAssignedBillAC>>(billDetails);

			List<Employeebillservicepackage> employeebillServicePackage = await _dbTeleBilling_V01Context.Employeebillservicepackage.Where(x => x.EmployeeBillId == employeebillmasterid && !x.IsDelete).Include(x => x.Package).Include(x => x.ServiceType).ToListAsync();
			viewBillDetailAC.PackageServiceList = _mapper.Map<List<PackageServiceAC>>(employeebillServicePackage);
			viewBillDetailAC.Currency = employeeBillMaster.Currency.Code;
			viewBillDetailAC.EmployeeBillStatus = employeeBillMaster.EmployeeBillStatus;
			viewBillDetailAC.TelephoneNumber = employeeBillMaster.TelephoneNumber;
			viewBillDetailAC.TotalBillAmount = employeeBillMaster.TotalBillAmount;
			viewBillDetailAC.IsReIdentificationRequest = employeeBillMaster.IsReIdentificationRequest;

			if (employeeBillMaster.MobileAssignType == Convert.ToInt16(EnumList.AssignType.Business)
				|| employeeBillMaster.Employee.IsPresidentOffice)
			{
				viewBillDetailAC.IsDeducateAmount = true;
			}
			else
				viewBillDetailAC.IsDeducateAmount = false;

			if (employeeBillMaster.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification) ||
			   employeeBillMaster.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject))
				viewBillDetailAC.IsDisplayOnly = false;
			else
				viewBillDetailAC.IsDisplayOnly = true;

			return viewBillDetailAC;
		}

		public async Task<ResponseAC> BillIdentificationSave(BillIdentificationAC billIdentificationAC, long userId, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			List<Billdetails> billDetails = new List<Billdetails>();
			foreach (var item in billIdentificationAC.lstUnAssignedBill)
			{
				Billdetails billDetail = new Billdetails();
				long? callIdentificationType = null;
				billDetail = await _dbTeleBilling_V01Context.Billdetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId);
				if (item.CallIdentificationType != 1 && item.CallIdentificationType != 2)//Not Assigne
				{
					if (billIdentificationAC.CallTypeId == 1)
						callIdentificationType = billIdentificationAC.CallId;
					else
						callIdentificationType = item.CallIdentificationType;
				}
				else //Assigend
					callIdentificationType = item.CallIdentificationType;

				if (billDetail != null)
				{
					if (callIdentificationType != null)
						billDetail.CallIdentificationType = Convert.ToInt16(callIdentificationType);
					billDetail.CallIdentifiedDate = DateTime.Now;
					billDetail.EmployeeComment = item.Comment;
					billDetail.CallIdentifedBy = userId;
					billDetails.Add(billDetail);
				}
			}

			_dbTeleBilling_V01Context.UpdateRange(billDetails);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.IdentificationSaveChangeSuccessfully;

			#region AuditTrailLogs
			if (billDetails.Any()) { 
				Employeebillmaster employeebillmaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstOrDefaultAsync(x=>x.BillMasterId == billDetails[0].BillMasterId && x.TelephoneNumber == billDetails[0].CallerNumber && !x.IsDelete);
				if(employeebillmaster != null)
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.BillIdentificationSaveChanges, loginUserName, userId, "Bill Identification(bill number: "+ employeebillmaster.BillNumber+ " and telePhone number: "+ employeebillmaster.TelephoneNumber+")", (int)EnumList.ActionTemplateTypes.BilIdentiSaveChanges, employeebillmaster.Id);
			}
			#endregion
			return responseAC;
		}

		public async Task<ResponseAC> BillProcess(BillIdentificationAC billIdentificationAC, long userId, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			List<Billdetails> billDetails = new List<Billdetails>();

			foreach (var item in billIdentificationAC.lstUnAssignedBill)
			{
				Billdetails billDetail = new Billdetails();
				long? callIdentificationType = null;

				billDetail = await _dbTeleBilling_V01Context.Billdetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId);

				if (item.CallIdentificationType != 1 && item.CallIdentificationType != 2)//Not Assigne
				{
					if (billIdentificationAC.CallTypeId == 1)
						callIdentificationType = billIdentificationAC.CallId;
					else
						callIdentificationType = item.CallIdentificationType;
				}
				else //Assigend
					callIdentificationType = item.CallIdentificationType;

				if (billDetail != null)
				{
					if (callIdentificationType != null)
						billDetail.CallIdentificationType = Convert.ToInt16(callIdentificationType);
					billDetail.CallIdentifiedDate = DateTime.Now;
					billDetail.EmployeeComment = item.Comment;
					billDetail.CallIdentifedBy = userId;
					billDetails.Add(billDetail);
				}
			}

			if (billDetails.Any())
			{
				Employeebillmaster employeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstOrDefaultAsync(x => x.Id == billDetails[0].EmployeeBillId && !x.IsDelete);
				if (employeeBillMaster.IdentificationById == null)
				{
					_dbTeleBilling_V01Context.UpdateRange(billDetails);
					await _dbTeleBilling_V01Context.SaveChangesAsync();


					employeeBillMaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
					employeeBillMaster.UpdatedBy = userId;
					employeeBillMaster.UpdatedDate = DateTime.Now;
					employeeBillMaster.IsApproved = null;
					employeeBillMaster.IsApprovedByDelegate = null;
					employeeBillMaster.IsIdentificationByDelegate = employeeBillMaster.EmployeeId == userId ? false : true;
					employeeBillMaster.IdentificationById = userId;
					employeeBillMaster.IdentificationDate = DateTime.Now;
					_dbTeleBilling_V01Context.Update(employeeBillMaster);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					#region Notification Logs 
					List<Notificationlog> notificationlogs = new List<Notificationlog>();
					if (employeeBillMaster.BillDelegatedEmpId != null)
					{
						Billdelegate billdelegate = await _dbTeleBilling_V01Context.Billdelegate.FirstOrDefaultAsync(x => !x.IsDelete && x.EmployeeId == employeeBillMaster.EmployeeId && x.DelegateEmployeeId == employeeBillMaster.BillDelegatedEmpId);
						if (billdelegate != null && billdelegate.AllowBillApproval)
						{
							notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.BillDelegatedEmpId), userId, Convert.ToInt16(EnumList.NotificationType.DelegateBillApproval), employeeBillMaster.Id));
							await _iLogManagement.SaveNotificationList(notificationlogs);
							notificationlogs = new List<Notificationlog>();
						}
					}

					notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.LinemanagerId), userId, Convert.ToInt16(EnumList.NotificationType.LineManagerApproval), employeeBillMaster.Id));
					await _iLogManagement.SaveNotificationList(notificationlogs);
					#endregion

					#region AuditTrailLogs
					  await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.BillProceess, loginUserName, userId, "Bill Process(bill number: " + employeeBillMaster.BillNumber + " and telePhone number: " + employeeBillMaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.BillProcess, employeeBillMaster.Id);
					#endregion
				}
			}

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.BillProcessSuccessfully;
			return responseAC;
		}

		public async Task<List<CurrentBillAC>> GetLineManagerApprovalList(long loginedUserId)
		{
			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();
			List<MstEmployee> mstEmployees = await _dbTeleBilling_V01Context.MstEmployee.Where(x => !x.IsDelete && x.IsActive && x.LineManagerId == loginedUserId).ToListAsync();
			List<TeleBillingUtility.Models.Billdelegate> billDelegates = await _dbTeleBilling_V01Context.Billdelegate.Where(x => x.DelegateEmployeeId == loginedUserId && !x.IsDelete && x.AllowBillApproval).ToListAsync();
			if (billDelegates.Any())
			{
				foreach (var subItem in billDelegates)
				{
					int employeeBillstatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
					List<Employeebillmaster> employeeBillMasters = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete && x.EmployeeId == subItem.EmployeeId && x.EmployeeBillStatus == employeeBillstatusId).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).OrderByDescending(x => x.Id).ToListAsync();
					if (employeeBillMasters.Any())
					{
						currentBillACs = GetApprvoalList(currentBillACs, employeeBillMasters, false, true);
					}
				}
			}
			if (mstEmployees.Any())
			{
				foreach (var employee in mstEmployees)
				{
					int employeeBillstatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
					List<Employeebillmaster> employeeBillMasters = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete && x.EmployeeId == employee.UserId && x.EmployeeBillStatus == employeeBillstatusId).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).OrderByDescending(x => x.Id).ToListAsync();
					if (employeeBillMasters.Any())
					{
						currentBillACs = GetApprvoalList(currentBillACs, employeeBillMasters, false, false);
					}
				}
			}

			return currentBillACs;
		}

		public async Task<ResponseAC> LineManagerApproval(LineManagerApprovalAC lineManagerApprovalAC, long userId, string loginUserName)
		{
			ResponseAC response = new ResponseAC();

			if (lineManagerApprovalAC.LineManagerApprovalBills.Any())
			{
				List<Employeebillmaster> employeeBillMasterList = new List<Employeebillmaster>();
				List<string> stringNameArray = new List<string>();
				List<Notificationlog> notificationlogs = new List<Notificationlog>();
				foreach (var item in lineManagerApprovalAC.LineManagerApprovalBills)
				{
					Employeebillmaster employeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => x.Id == item.EmployeeBillMasterId && !x.IsDelete).Include(x => x.Employee).Include(x => x.BillDelegatedEmp).Include(x => x.Provider).FirstOrDefaultAsync();
					if (employeeBillMaster.IsApproved == null)
					{
						employeeBillMaster.IsApproved = lineManagerApprovalAC.IsApprove;
						employeeBillMaster.ApprovalComment = item.ApprovalComment;
						employeeBillMaster.EmployeeBillStatus = lineManagerApprovalAC.IsApprove ? Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved) : Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject);
						employeeBillMaster.ApprovalDate = DateTime.Now;
						employeeBillMaster.UpdatedBy = userId;
						employeeBillMaster.UpdatedDate = DateTime.Now;
						employeeBillMaster.IsApprovedByDelegate = item.IsDelegatedUser;
						employeeBillMaster.ApprovalById = userId;

						if(!lineManagerApprovalAC.IsApprove)
							employeeBillMaster.IdentificationById = null;

						employeeBillMasterList.Add(employeeBillMaster);

						if (lineManagerApprovalAC.IsApprove)
						{
							List<Billdetails> billdetails = await _dbTeleBilling_V01Context.Billdetails.Where(x => x.EmployeeBillId == item.EmployeeBillMasterId).ToListAsync();
							var serviceBillDetials = billdetails.GroupBy(x => x.ServiceTypeId);
							foreach (var billDetailsData in serviceBillDetials)
							{
								decimal personalDeduction = 0;
								decimal businessCharge = 0;
								foreach (var subItem in billDetailsData)
								{
									if (subItem.CallIdentificationType == Convert.ToInt16(EnumList.AssignType.Business))
									{
										businessCharge += subItem.CallAmount != null ? Convert.ToDecimal(subItem.CallAmount) : 0;
									}
									else if (subItem.CallIdentificationType == Convert.ToInt16(EnumList.AssignType.Employee))
									{
										personalDeduction += subItem.CallAmount != null ? Convert.ToDecimal(subItem.CallAmount) : 0;
									}
								}

								var employeeBillServicePackage = await _dbTeleBilling_V01Context.Employeebillservicepackage.Where(x => x.ServiceTypeId == billDetailsData.Key && !x.IsDelete && x.EmployeeBillId == employeeBillMaster.Id).Include(x => x.Package).FirstOrDefaultAsync();
								employeeBillServicePackage.PersonalIdentificationAmount = personalDeduction;
								employeeBillServicePackage.BusinessIdentificationAmount = businessCharge;

								#region Bill Close Code On ReIdentification
								//If Bill ReIdentifcation is done
								if (employeeBillMaster.IsReIdentificationRequest)
								{
									if (!employeeBillMaster.Employee.IsPresidentOffice)
									{
										var telephoneNumberAllocation = _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefault(x => x.TelephoneNumber == employeeBillMaster.TelephoneNumber && x.EmployeeId == employeeBillMaster.EmployeeId && !x.IsDelete);
										if (telephoneNumberAllocation.AssignTypeId != Convert.ToInt16(EnumList.AssignType.Business))
										{
											if (employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.LandLine) || employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.VOIP) || employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.StaticIP))
											{
												employeeBillServicePackage.DeductionAmount = employeeBillServicePackage.PersonalIdentificationAmount;
											}
											else
											{
												List<Billdetails> billDetails = _dbTeleBilling_V01Context.Billdetails.Where(x => x.EmployeeBillId == employeeBillMaster.Id && x.ServiceTypeId == employeeBillServicePackage.ServiceTypeId).ToList();
												decimal totalAmount = billDetails.Sum(x => x.CallAmount).Value;
												if (employeeBillServicePackage.Package.PackageAmount < totalAmount)
												{
													decimal businessIdentificationAmount = Convert.ToDecimal(employeeBillServicePackage.BusinessIdentificationAmount);
													decimal amount = 0;
													if (employeeBillServicePackage.Package.PackageAmount < businessIdentificationAmount)
													{
														amount = businessIdentificationAmount;
													}
													else
													{
														amount = employeeBillServicePackage.Package.PackageAmount != null ? Convert.ToDecimal(employeeBillServicePackage.Package.PackageAmount) : 0;
													}
													employeeBillServicePackage.DeductionAmount = totalAmount - amount;
												}
											}
										}
										else
											employeeBillServicePackage.DeductionAmount = 0;
									}
									else
										employeeBillServicePackage.DeductionAmount = 0;
								}
								#endregion

								employeeBillServicePackage.UpdatedBy = userId;
								employeeBillServicePackage.UpdateDate = DateTime.Now;

								_dbTeleBilling_V01Context.Update(employeeBillServicePackage);
								await _dbTeleBilling_V01Context.SaveChangesAsync();
							}

							if (employeeBillMaster.BillDelegatedEmpId == userId)
								notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.DelegateBillApprove), employeeBillMaster.Id));
							else
								notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.LineManagerApprove), employeeBillMaster.Id));

							#region AuditLog 
							 await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.LineManagerApprove, loginUserName, userId, "Bill(bill number: "+ employeeBillMaster.BillNumber + " and telephone number: "+ employeeBillMaster .TelephoneNumber+ ")", (int)EnumList.ActionTemplateTypes.Approve, employeeBillMaster.Id);
							#endregion

						}
						else
						{
							if (employeeBillMaster.BillDelegatedEmpId == userId)
								notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.DelegateBillReject), employeeBillMaster.Id));
							else
								notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.LineManagerReject), employeeBillMaster.Id));

							#region AuditLog 
								await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.LineManagerReject, loginUserName, userId, "Bill(bill number: " + employeeBillMaster.BillNumber + " and telephone number: " + employeeBillMaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Reject, employeeBillMaster.Id);
							#endregion
						}

						//check employee delgate user allow to identification or not 
						if (employeeBillMaster.BillDelegatedEmpId != null)
						{
							Billdelegate billdelegate = await _dbTeleBilling_V01Context.Billdelegate.FirstOrDefaultAsync(x => x.EmployeeId == employeeBillMaster.EmployeeId && x.DelegateEmployeeId == employeeBillMaster.BillDelegatedEmpId && !x.IsDelete);
							if (billdelegate != null && billdelegate.AllowBillIdentification)
							{
								if (lineManagerApprovalAC.IsApprove)
								{
									if (employeeBillMaster.BillDelegatedEmpId == userId)
										notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.BillDelegatedEmpId), userId, Convert.ToInt16(EnumList.NotificationType.DelegateBillApprove), employeeBillMaster.Id));
									else
										notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.BillDelegatedEmpId), userId, Convert.ToInt16(EnumList.NotificationType.LineManagerApprove), employeeBillMaster.Id));
								}
								else
								{
									if (employeeBillMaster.BillDelegatedEmpId == userId)
										notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.BillDelegatedEmpId), userId, Convert.ToInt16(EnumList.NotificationType.DelegateBillReject), employeeBillMaster.Id));
									else
										notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt64(employeeBillMaster.BillDelegatedEmpId), userId, Convert.ToInt16(EnumList.NotificationType.LineManagerReject), employeeBillMaster.Id));
								}
							}
						}

						//await SendMailForApproval(employeeBillMaster, lineManagerApprovalAC.IsApprove);


					}
					else
					{
						var employee = stringNameArray.FirstOrDefault(x => x.Contains(employeeBillMaster.Employee.FullName));
						if (employee == null)
						{
							stringNameArray.Add(employeeBillMaster.Employee.FullName);
						}
					}
				}

				if (employeeBillMasterList.Any())
				{
					_dbTeleBilling_V01Context.UpdateRange(employeeBillMasterList);
					await _dbTeleBilling_V01Context.SaveChangesAsync();
				}

				if (notificationlogs.Any())
					await _iLogManagement.SaveNotificationList(notificationlogs);

				if (employeeBillMasterList.Count() == lineManagerApprovalAC.LineManagerApprovalBills.Count())
				{
					response.Message = lineManagerApprovalAC.IsApprove ? _iStringConstant.LineManagerApproveSuccessfully : _iStringConstant.LineManagerRejectSuccessfully;
					response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				}
				else
				{
					string message = String.Join(',', stringNameArray);
					response.Message = lineManagerApprovalAC.IsApprove ? _iStringConstant.BillApprovalMessageSuccessfully.Replace("{{@currentapproval}}", "approved").Replace("{{@employee}}", message) : _iStringConstant.BillApprovalMessageSuccessfully.Replace("{{@currentapproval}}", "rejected").Replace("{{@employee}}", message);
					response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Warning);
				}
			}
			else
			{
				response.Message = _iStringConstant.AtLeastSelectOneRecord;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return response;
		}

		public async Task<JqueryDataTablesPagedResults<CurrentBillAC>> GetMyStaffBills(JqueryDataWithExtraParameterAC param, long userId)
		{
			int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
			int take = param.DataTablesParameters.Length;

			int billAutoClosedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
			int billClosedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);

			Expression<Func<Employeebillmaster, bool>> predicate = x => x.LinemanagerId == userId && !x.IsDelete
																								   && (param.Month != 0 ? x.BillMonth == param.Month : x.BillMonth == x.BillMonth)
																								   && (param.Year != 0 ? x.BillYear == param.Year : x.BillYear == x.BillYear)
																								   && (param.ProviderId != 0 ? x.ProviderId == param.ProviderId : x.ProviderId == x.ProviderId)
																								   && (param.StatusId == 1 ? (x.EmployeeBillStatus != billAutoClosedStatusId && x.EmployeeBillStatus != billClosedStatusId) : (param.StatusId == 2 ? (x.EmployeeBillStatus == billAutoClosedStatusId && x.EmployeeBillStatus == billClosedStatusId) : x.EmployeeBillStatus == x.EmployeeBillStatus));


			IQueryable<Employeebillmaster> query = _dbTeleBilling_V01Context.Employeebillmaster.Where(predicate).Include(x => x.EmployeeBillStatusNavigation).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).OrderByDescending(x => x.Id).AsNoTracking();

			query = query.Where(x => x.BillNumber.Contains(param.DataTablesParameters.Search.Value) || x.Employee.FullName.Contains(param.DataTablesParameters.Search.Value) || x.TelephoneNumber.Contains(param.DataTablesParameters.Search.Value));

			var size = await query.CountAsync();

			var currentEmployeeBills = await query.Skip((param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length).Take(param.DataTablesParameters.Length).ToArrayAsync();

			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();

			if (currentEmployeeBills.Any())
			{
				foreach (var item in currentEmployeeBills)
				{
					CurrentBillAC currentBillAC = _mapper.Map<CurrentBillAC>(item);
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					currentBillACs.Add(currentBillAC);

				}
			}

			return new JqueryDataTablesPagedResults<CurrentBillAC>
			{
				Items = currentBillACs,
				TotalSize = size
			};
		}

		public List<ExportMyStaffBillsAC> GetMyStaffExportList(SearchMyStaffAC searchMyStaffAC, long userId)
		{

			int billAutoClosedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
			int billClosedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);

			Expression<Func<Employeebillmaster, bool>> predicate = x => x.LinemanagerId == userId && !x.IsDelete
																								   && (searchMyStaffAC.Month != 0 ? x.BillMonth == searchMyStaffAC.Month : x.BillMonth == x.BillMonth)
																								   && (searchMyStaffAC.Year != 0 ? x.BillYear == searchMyStaffAC.Year : x.BillYear == x.BillYear)
																								   && (searchMyStaffAC.ProviderId != 0 ? x.ProviderId == searchMyStaffAC.ProviderId : x.ProviderId == x.ProviderId)
																								   && (searchMyStaffAC.StatusId == 1 ? (x.EmployeeBillStatus != billAutoClosedStatusId && x.EmployeeBillStatus != billClosedStatusId) : (searchMyStaffAC.StatusId == 2 ? (x.EmployeeBillStatus == billAutoClosedStatusId && x.EmployeeBillStatus == billClosedStatusId) : x.EmployeeBillStatus == x.EmployeeBillStatus));

			IQueryable<Employeebillmaster> query = _dbTeleBilling_V01Context.Employeebillmaster.Where(predicate).Include(x => x.EmployeeBillStatusNavigation).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).OrderByDescending(x => x.Id).AsNoTracking();

			List<ExportMyStaffBillsAC> currentBillACs = new List<ExportMyStaffBillsAC>();

			if (query.Any())
			{
				foreach (var item in query)
				{
					ExportMyStaffBillsAC currentBillAC = _mapper.Map<ExportMyStaffBillsAC>(item);
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					currentBillACs.Add(currentBillAC);
				}
			}
			return currentBillACs;
		}

		public List<ExportPreviousPeriodBillsAC> GetExportPreviousPeriodBills(SearchMyStaffAC searchMyStaffAC, long userId) {

			int billLineManagerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);
			int billCloseStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.CloseBill);
			int billAutoStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);

			Expression<Func<Employeebillmaster, bool>> predicate = x => x.EmployeeId == userId && !x.IsDelete && ((x.EmployeeBillStatus == billAutoStatusId || x.EmployeeBillStatus == billCloseStatusId) || (x.IsReIdentificationRequest && x.EmployeeBillStatus == billLineManagerApprovedStatusId))
																								   && (searchMyStaffAC.Month != 0 ? x.BillMonth == searchMyStaffAC.Month : x.BillMonth == x.BillMonth)
																								   && (searchMyStaffAC.Year != 0 ? x.BillYear == searchMyStaffAC.Year : x.BillYear == x.BillYear)
																								   && (searchMyStaffAC.ProviderId != 0 ? x.ProviderId == searchMyStaffAC.ProviderId : x.ProviderId == x.ProviderId);
			
			IQueryable<Employeebillmaster> query = _dbTeleBilling_V01Context.Employeebillmaster.Where(predicate).Include(x => x.EmployeeBillStatusNavigation).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Linemanager).Include(x => x.Currency).OrderByDescending(x => x.Id).AsNoTracking();

			List<ExportPreviousPeriodBillsAC> exportPreviousPeriodBillsACs = new List<ExportPreviousPeriodBillsAC>();

			if (query.Any())
			{
				foreach (var item in query)
				{
					ExportPreviousPeriodBillsAC currentBillAC = _mapper.Map<ExportPreviousPeriodBillsAC>(item);
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					exportPreviousPeriodBillsACs.Add(currentBillAC);
				}
			}
			return exportPreviousPeriodBillsACs;
		}

		public async Task<JqueryDataTablesPagedResults<CurrentBillAC>> GetPreviousPeriodBills(JqueryDataWithExtraParameterAC param, long userId) {

			int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
			int take = param.DataTablesParameters.Length;

			int billLineManagerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);
			int billCloseStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.CloseBill);
			int billAutoStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);

			Expression<Func<Employeebillmaster, bool>> predicate = x => x.EmployeeId == userId && !x.IsDelete && ((x.EmployeeBillStatus == billAutoStatusId || x.EmployeeBillStatus == billCloseStatusId) || (x.IsReIdentificationRequest && x.EmployeeBillStatus == billLineManagerApprovedStatusId))
																								   && (param.Month != 0 ? x.BillMonth == param.Month : x.BillMonth == x.BillMonth)
																								   && (param.Year != 0 ? x.BillYear == param.Year : x.BillYear == x.BillYear)
																								   && (param.ProviderId != 0 ? x.ProviderId == param.ProviderId : x.ProviderId == x.ProviderId);
			
			IQueryable<Employeebillmaster> query = _dbTeleBilling_V01Context.Employeebillmaster.Where(predicate).Include(x => x.EmployeeBillStatusNavigation).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.MobileAssignTypeNavigation).Include(x=>x.Linemanager).Include(x => x.Currency).OrderByDescending(x => x.Id).AsNoTracking();

			query = query.Where(x => x.BillNumber.Contains(param.DataTablesParameters.Search.Value) || x.Employee.FullName.Contains(param.DataTablesParameters.Search.Value) || x.TelephoneNumber.Contains(param.DataTablesParameters.Search.Value) || x.Linemanager.FullName.Contains(param.DataTablesParameters.Search.Value));

			var size = await query.CountAsync();

			var currentEmployeeBills = await query.Skip((param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length).Take(param.DataTablesParameters.Length).ToArrayAsync();


			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();

			if (currentEmployeeBills.Any())
			{
				foreach (var employeeBillMaster in currentEmployeeBills)
				{
					if (!currentBillACs.Any(x => x.EmployeeBillMasterId == employeeBillMaster.Id))
					{
						decimal? deductionAmount = _dbTeleBilling_V01Context.Employeebillservicepackage.Where(x => x.EmployeeBillId == employeeBillMaster.Id && !x.IsDelete).Sum(x => x.DeductionAmount);
						CurrentBillAC currentBillAC = _mapper.Map<CurrentBillAC>(employeeBillMaster);
						currentBillAC.DeductionAmount = deductionAmount == null ? 0 : deductionAmount;
						if (employeeBillMaster.IsReIdentificationRequest)
						{
							decimal? oldDeductionAmount = _dbTeleBilling_V01Context.Employeebillservicepackage.Where(x => x.EmployeeBillId == employeeBillMaster.PreviousEmployeeBillId && !x.IsDelete).Sum(x => x.DeductionAmount);
							currentBillAC.OldDeductionAmount = oldDeductionAmount;
						}
						EnumList.Month month = (EnumList.Month)employeeBillMaster.BillMonth;
						currentBillAC.BillDate = month.ToString() + " " + employeeBillMaster.BillYear;
						currentBillAC.ManagerName = employeeBillMaster.Linemanager?.FullName;
						if (employeeBillMaster.IsReIdentificationRequest)
							currentBillAC.IsAllowToReIdentification = false;
						else
							currentBillAC.IsAllowToReIdentification = true;

						if (employeeBillMaster.IsReIdentificationRequest && employeeBillMaster.EmployeeBillStatus == billLineManagerApprovedStatusId)
						{
							if (employeeBillMaster.IsReImbursementRequest)
								currentBillAC.IsAllowToReImbrusment = false;
							else
								currentBillAC.IsAllowToReImbrusment = true;
						}
						else
							currentBillAC.IsAllowToReImbrusment = false;

						currentBillACs.Add(currentBillAC);
					}
				}
			}

			return new JqueryDataTablesPagedResults<CurrentBillAC>
			{
				Items = currentBillACs,
				TotalSize = size
			};
		}

		public async Task<long> ReIdentificationRequest(long userId, long employeebillmasterid,string loginUserName)
		{

			Employeebillmaster employeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstAsync(x => x.Id == employeebillmasterid && !x.IsDelete);
			Employeebillmaster newEmployeeBillMaster = new Employeebillmaster();

			#region Update Old Employee Bill Master
			employeeBillMaster.IsDelete = true;
			employeeBillMaster.UpdatedBy = userId;
			employeeBillMaster.UpdatedDate = DateTime.Now;

			_dbTeleBilling_V01Context.Update(employeeBillMaster);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added New Employee Bill Master
			newEmployeeBillMaster.BillMasterId = employeeBillMaster.BillMasterId;
			newEmployeeBillMaster.BillMonth = employeeBillMaster.BillMonth;
			newEmployeeBillMaster.BillNumber = employeeBillMaster.BillNumber;
			newEmployeeBillMaster.BillYear = employeeBillMaster.BillYear;
			newEmployeeBillMaster.CurrencyId = employeeBillMaster.CurrencyId;
			newEmployeeBillMaster.Description = employeeBillMaster.Description;
			newEmployeeBillMaster.EmpBusinessUnitId = employeeBillMaster.EmpBusinessUnitId;
			newEmployeeBillMaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
			newEmployeeBillMaster.EmployeeId = employeeBillMaster.EmployeeId;
			newEmployeeBillMaster.IsReIdentificationRequest = true;
			newEmployeeBillMaster.LinemanagerId = employeeBillMaster.LinemanagerId;
			newEmployeeBillMaster.MobileAssignType = employeeBillMaster.MobileAssignType;
			newEmployeeBillMaster.PreviousEmployeeBillId = employeeBillMaster.Id;
			newEmployeeBillMaster.ProviderId = employeeBillMaster.ProviderId;
			newEmployeeBillMaster.TelephoneNumber = employeeBillMaster.TelephoneNumber;
			newEmployeeBillMaster.TotalBillAmount = employeeBillMaster.TotalBillAmount;
			newEmployeeBillMaster.CreatedBy = userId;
			newEmployeeBillMaster.CreatedDate = DateTime.Now;

			await _dbTeleBilling_V01Context.AddAsync(newEmployeeBillMaster);
			await _dbTeleBilling_V01Context.SaveChangesAsync();


			#endregion

			List<Employeebillservicepackage> employeeBillServicePackageList = await _dbTeleBilling_V01Context.Employeebillservicepackage.Where(x => x.EmployeeBillId == employeeBillMaster.Id && !x.IsDelete).ToListAsync();

			#region Added New Employeebillservicepackage

			List<Employeebillservicepackage> newEmployeeBillServicePackageList = new List<Employeebillservicepackage>();
			foreach (var item in employeeBillServicePackageList)
			{
				Employeebillservicepackage employeeBillServicePackage = new Employeebillservicepackage();
				employeeBillServicePackage.EmployeeBillId = newEmployeeBillMaster.Id;
				employeeBillServicePackage.PackageId = item.PackageId;
				employeeBillServicePackage.ServiceTypeId = item.ServiceTypeId;
				employeeBillServicePackage.PersonalIdentificationAmount = item.PersonalIdentificationAmount;
				employeeBillServicePackage.BusinessTotalAmount = item.BusinessTotalAmount;
				employeeBillServicePackage.BusinessIdentificationAmount = item.BusinessIdentificationAmount;
				employeeBillServicePackage.DeductionAmount = item.DeductionAmount;
				newEmployeeBillServicePackageList.Add(employeeBillServicePackage);
			}
			await _dbTeleBilling_V01Context.AddRangeAsync(newEmployeeBillServicePackageList);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added Bill Details 
			List<Billdetails> billDetails = await _dbTeleBilling_V01Context.Billdetails.Where(x => x.EmployeeBillId == employeeBillMaster.Id).ToListAsync();
			List<Billdetails> newBillDetails = new List<Billdetails>();
			foreach (var item in billDetails)
			{
				Billdetails billDetail = new Billdetails();
				billDetail.EmployeeBillId = newEmployeeBillMaster.Id;
				billDetail.AssignTypeId = item.AssignTypeId;
				billDetail.BillMasterId = item.BillMasterId;
				billDetail.BusinessUnitId = item.BusinessUnitId;
				billDetail.CallAmount = item.CallAmount;
				billDetail.CallAssignedBy = item.CallAssignedBy;
				billDetail.CallAssignedDate = item.CallAssignedDate;
				billDetail.CallDate = item.CallDate;
				billDetail.CallDuration = item.CallDuration;
				billDetail.CallerName = item.CallerName;
				billDetail.CallerNumber = item.CallerNumber;
				billDetail.CallIdentifedBy = item.CallIdentifedBy;
				billDetail.CallIdentificationType = item.CallIdentificationType;
				billDetail.CallIdentifiedDate = item.CallIdentifiedDate;
				billDetail.CallIwithInGroup = item.CallIwithInGroup;
				billDetail.CallTime = item.CallTime;
				billDetail.CallTransactionTypeId = item.CallTransactionTypeId;
				billDetail.CreatedBy = userId;
				billDetail.CreatedDate = DateTime.Now;
				billDetail.Destination = item.Destination;
				billDetail.EmployeeComment = item.EmployeeComment;
				billDetail.GroupId = item.GroupId;
				billDetail.ReceiverName = item.ReceiverName;
				billDetail.ReceiverNumber = item.ReceiverNumber;
				billDetail.ServiceTypeId = item.ServiceTypeId;
				billDetail.SubscriptionType = item.SubscriptionType;
				billDetail.TransType = item.TransType;
				newBillDetails.Add(billDetail);
			}

			await _dbTeleBilling_V01Context.AddRangeAsync(newBillDetails);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			#endregion

			#region AuditLog
			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ReIdentificaiton, loginUserName, userId, "Bill(bill number: " + employeeBillMaster.BillNumber + " and telephone number: " + employeeBillMaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.ReIdentification, employeeBillMaster.Id);
			#endregion

			return newEmployeeBillMaster.Id;
		}

		public async Task<ResponseAC> ReImbursementRequest(long userId, ReImbursementRequestAC reImbursementRequestAC,string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			Employeebillmaster employeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstAsync(x => x.Id == reImbursementRequestAC.EmployeeBillMasterId && !x.IsDelete);
			Billreimburse billReImburse = new Billreimburse();
			billReImburse.EmployeeBillId = employeeBillMaster.Id;
			billReImburse.BillMasterId = employeeBillMaster.BillMasterId;
			billReImburse.ReImbruseAmount = reImbursementRequestAC.Amount;
			billReImburse.Description = reImbursementRequestAC.Description != null ? reImbursementRequestAC.Description : string.Empty;
			billReImburse.CreatedBy = userId;
			billReImburse.CreatedDate = DateTime.Now;
			billReImburse.CurrencyId = Convert.ToInt64(employeeBillMaster.CurrencyId);
			billReImburse.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

			await _dbTeleBilling_V01Context.AddAsync(billReImburse);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			employeeBillMaster.IsReImbursementRequest = true;
			employeeBillMaster.UpdatedBy = userId;
			employeeBillMaster.UpdatedDate = DateTime.Now;


			_dbTeleBilling_V01Context.Update(employeeBillMaster);
			await _dbTeleBilling_V01Context.SaveChangesAsync();


			#region Notification For Reimbursement
			int finaceRoleId = Convert.ToInt16(EnumList.RoleType.Finance);
			List<MstEmployee> mstEmployees = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.RoleId == finaceRoleId && !x.IsDelete && x.IsActive).ToListAsync();
			if (mstEmployees.Any()) { 
				List<Notificationlog> notificationlogs = new List<Notificationlog>();
				foreach (var item in mstEmployees)
				{
					notificationlogs.Add(_iLogManagement.GenerateNotificationObject(item.UserId, userId, Convert.ToInt16(EnumList.NotificationType.BillReImbursementRequest), employeeBillMaster.Id));
				}
				await _iLogManagement.SaveNotificationList(notificationlogs);
			}
			#endregion

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.ReImbursementRequestAddedSuccessfully;

			#region AuditLog
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ReimbursementRequest, loginUserName, userId, "Bill(bill number: " + employeeBillMaster.BillNumber + " and telephone number: " + employeeBillMaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.ReimbursementRequest, employeeBillMaster.Id);
			#endregion

			return responseAC;
		}

		public async Task<List<ReImburseBillsAC>> GetReImburseBills()
		{
			List<Billreimburse> billReImburse = await _dbTeleBilling_V01Context.Billreimburse.Where(x => !x.IsDelete && x.IsApproved == null).Include(x => x.EmployeeBill).Include(x => x.EmployeeBill.Employee).Include(x => x.EmployeeBill.Provider).OrderByDescending(x => x.Id).ToListAsync();
			List<ReImburseBillsAC> reImburseBillsACs = new List<ReImburseBillsAC>();
			foreach (var item in billReImburse)
			{

				ReImburseBillsAC reImburseBillsAC = new ReImburseBillsAC();
				reImburseBillsAC = _mapper.Map<ReImburseBillsAC>(item);
				EnumList.Month month = (EnumList.Month)item.EmployeeBill.BillMonth;
				reImburseBillsAC.BillDate = month.ToString() + " " + item.EmployeeBill.BillYear;
				reImburseBillsAC.ManagerName = _dbTeleBilling_V01Context.MstEmployee.FirstOrDefault(x => x.UserId == item.EmployeeBill.LinemanagerId && !x.IsDelete && x.IsActive)?.FullName;
				reImburseBillsACs.Add(reImburseBillsAC);
			}
			return reImburseBillsACs;
		}

		public async Task<ResponseAC> ReImburseBillApproval(long userId, ReImburseBillApprovalAC reImburseBillApprovalAC,string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();

			Billreimburse billReImburse = await _dbTeleBilling_V01Context.Billreimburse.FirstAsync(x => !x.IsDelete && x.Id == reImburseBillApprovalAC.Id);
			#region Transaction Log Entry
			if (billReImburse.TransactionId == null)
				billReImburse.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

			var jsonSerailzeObj = JsonConvert.SerializeObject(billReImburse);
			await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(billReImburse.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
			#endregion

			billReImburse.IsApproved = reImburseBillApprovalAC.IsApproved;
			billReImburse.ApprovalComment = reImburseBillApprovalAC.ApprovalComment;
			billReImburse.ApprovalDate = DateTime.Now;
			billReImburse.ApprovedBy = userId;

			_dbTeleBilling_V01Context.Update(billReImburse);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			#region Update Employee Bill Master
			Employeebillmaster employeebillmaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstOrDefaultAsync(x => x.Id == billReImburse.EmployeeBillId && !x.IsDelete);
			employeebillmaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
			employeebillmaster.UpdatedBy = userId;
			employeebillmaster.UpdatedDate = DateTime.Now;
			_dbTeleBilling_V01Context.Update(employeebillmaster);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion


			#region Notification and AuditLog For ReImbursement Approve/Reject and 
			List<Notificationlog> notificationlogs = new List<Notificationlog>();
			if (reImburseBillApprovalAC.IsApproved)
			{
				notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt16(employeebillmaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.ReImbursementApprove), employeebillmaster.Id));
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ReimbursementBillApprove, loginUserName, userId, "Bill(bill number: " + employeebillmaster.BillNumber + " and telephone number: " + employeebillmaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Approve, employeebillmaster.Id);			
			}
			else
			{
				notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt16(employeebillmaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.ReImbursementReject), employeebillmaster.Id));
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ReimbursementBillReject, loginUserName, userId, "Bill(bill number: " + employeebillmaster.BillNumber + " and telephone number: " + employeebillmaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Reject, employeebillmaster.Id);
			}

			if (notificationlogs.Any())
				await _iLogManagement.SaveNotificationList(notificationlogs);

			#endregion

			responseAC.Message = reImburseBillApprovalAC.IsApproved ? _iStringConstant.BillReImburseApprovesuccessfully : _iStringConstant.BillReImburseRejectsuccessfully;
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			return responseAC;
		}

		public async Task<List<ChangeBillStatusAC>> GetChangeBillStatusList()
		{
			int billWaitingForLineMangerApprovalStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
			int lineMangerRejectStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject);
			int lineMangerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);
			List<ChangeBillStatusAC> changeBillStatusACs = new List<ChangeBillStatusAC>();
			List<Employeebillmaster> employeeBillMasters = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete && (x.EmployeeBillStatus == billWaitingForLineMangerApprovalStatusId || x.EmployeeBillStatus == lineMangerRejectStatusId || x.EmployeeBillStatus == lineMangerApprovedStatusId) && !x.IsReImbursementRequest).Include(x => x.MobileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.Linemanager).OrderByDescending(x => x.Id).ToListAsync();
			if (employeeBillMasters.Any())
			{
				foreach (var item in employeeBillMasters)
				{

					ChangeBillStatusAC changeBillStatusAC = new ChangeBillStatusAC();
					changeBillStatusAC.Amount = item.TotalBillAmount;
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					changeBillStatusAC.BillDate = month.ToString() + " " + item.BillYear;
					changeBillStatusAC.BillNumber = item.BillNumber;
					changeBillStatusAC.BillStatus = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
					changeBillStatusAC.Currency = item.Currency.Code;
					changeBillStatusAC.EmployeeBillMasterId = item.Id;
					changeBillStatusAC.EmployeeBillStatus = item.EmployeeBillStatus;
					if (item.EmployeeId != null)
					{
						changeBillStatusAC.EmployeeId = Convert.ToInt64(item.EmployeeId);
						changeBillStatusAC.EmployeeName = item.Employee.FullName;
					}
					changeBillStatusAC.Month = item.BillMonth;
					changeBillStatusAC.Year = item.BillYear;
					changeBillStatusAC.Provider = item.Provider.Name;
					changeBillStatusAC.ProviderId = item.ProviderId;
					if (item.LinemanagerId != null)
						changeBillStatusAC.ManagerName = item.Linemanager.FullName;

					changeBillStatusACs.Add(changeBillStatusAC);
				}
			}
			return changeBillStatusACs;
		}

		public async Task<ResponseAC> ChangeBillStatus(List<ChangeBillStatusAC> changeBillStatusACs, long userId,string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			if (changeBillStatusACs.Any())
			{
				List<Notificationlog> notificationlogs = new List<Notificationlog>();
				List<Employeebillmaster> employeeBillMasterList = new List<Employeebillmaster>();
				foreach (var item in changeBillStatusACs)
				{
					Employeebillmaster employeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstOrDefaultAsync(x => x.Id == item.EmployeeBillMasterId && !x.IsDelete && !x.IsReImbursementRequest);
					if (employeeBillMaster != null)
					{
						#region Transaction Log Entry
						if (employeeBillMaster.TransactionId == null)
							employeeBillMaster.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

						var jsonSerailzeObj = JsonConvert.SerializeObject(employeeBillMaster);
						await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(employeeBillMaster.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.BillStatusChange), jsonSerailzeObj);
						#endregion

						employeeBillMaster.IsApproved = null;
						employeeBillMaster.ApprovalComment = "";
						employeeBillMaster.ApprovalDate = null;
						employeeBillMaster.UpdatedBy = userId;
						employeeBillMaster.UpdatedDate = DateTime.Now;
						employeeBillMaster.EmployeeBillStatus = item.EmployeeBillChangeStatus;

						if(employeeBillMaster.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification))
								employeeBillMaster.IdentificationById = null;

						employeeBillMasterList.Add(employeeBillMaster);

						#region AuditLog And Notification Log
							await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ChangeBillStatus, loginUserName, userId, "Bill(bill number: " + employeeBillMaster.BillNumber + " and telephone number: " + employeeBillMaster.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.ChangeBillStatus, employeeBillMaster.Id);
							notificationlogs.Add(_iLogManagement.GenerateNotificationObject(Convert.ToInt16(employeeBillMaster.EmployeeId), userId, Convert.ToInt16(EnumList.NotificationType.ChangeBillStatus), employeeBillMaster.Id));
						#endregion
					}
				}

				if (employeeBillMasterList.Any())
				{
					_dbTeleBilling_V01Context.UpdateRange(employeeBillMasterList);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					await _iLogManagement.SaveNotificationList(notificationlogs);
				}
				responseAC.Message = _iStringConstant.BillChangeStatusSuccesfully;
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			return responseAC;
		}
		#endregion

		#region Private Method(s)

		/// <summary>
		/// This method used for move to bill list model class(Employeebillmaster) to application class(CurrentBillAC)
		/// </summary>
		/// <param name="currentBillList"></param>
		/// <param name="employeeBillMasters"></param>
		/// <param name="allowBillIdentification"></param>
		/// <param name="IsDelegate"></param>
		/// <returns></returns>
		private List<CurrentBillAC> GetCurrentBillList(List<CurrentBillAC> currentBillList, List<Employeebillmaster> employeeBillMasters, bool allowBillIdentification, bool IsDelegate)
		{
			foreach (var item in employeeBillMasters)
			{
				if (!currentBillList.Any(x => x.EmployeeBillMasterId == item.Id))
				{

					CurrentBillAC currentBillAC = new CurrentBillAC();
					currentBillAC.Amount = item.TotalBillAmount;
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					currentBillAC.AssigneType = item.MobileAssignTypeNavigation.Name;
					currentBillAC.BillStatus = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
					currentBillAC.Currency = item.Currency.Code;
					currentBillAC.Description = item.Description;
					currentBillAC.EmployeeBillMasterId = item.Id;
					currentBillAC.BillNumber = item.BillNumber;
					currentBillAC.EmployeeName = item.Employee.FullName;
					currentBillAC.TelephoneNumber = item.TelephoneNumber;
					currentBillAC.UpdatedDate = item.UpdatedDate;
					currentBillAC.Provider = item.Provider.Name;
					currentBillAC.EmployeeBillStatus = item.EmployeeBillStatus;
					if (item.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval))
						currentBillAC.IsAllowIdentificatication = false;
					else
					{
						if (IsDelegate)
							currentBillAC.IsAllowIdentificatication = allowBillIdentification;
						else
							currentBillAC.IsAllowIdentificatication = true;
					}
					currentBillList.Add(currentBillAC);
				}
			}
			return currentBillList;
		}

		/// <summary>
		/// This method used for move to line manager approval model class(Employeebillmaster) to application class(CurrentBillAC)
		/// </summary>
		/// <param name="currentBillACs"></param>
		/// <param name="employeeBillMasters"></param>
		/// <param name="allowBillAppoval"></param>
		/// <param name="IsDelegate"></param>
		/// <returns></returns>
		private List<CurrentBillAC> GetApprvoalList(List<CurrentBillAC> currentBillACs, List<Employeebillmaster> employeeBillMasters, bool allowBillAppoval, bool IsDelegate)
		{
			foreach (var item in employeeBillMasters)
			{
				CurrentBillAC currentBillAC = new CurrentBillAC();
				if (!currentBillACs.Any(x => x.EmployeeBillMasterId == item.Id))
				{
					currentBillAC.EmployeeBillMasterId = item.Id;
					currentBillAC.Amount = item.TotalBillAmount;
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					currentBillAC.AssigneType = item.MobileAssignTypeNavigation.Name;
					currentBillAC.BillStatus = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
					currentBillAC.Currency = item.Currency.Code;
					currentBillAC.Description = item.Description;
					currentBillAC.BillNumber = item.BillNumber;
					currentBillAC.EmployeeBillMasterId = item.Id;
					currentBillAC.EmployeeName = item.Employee.FullName;
					currentBillAC.TelephoneNumber = item.TelephoneNumber;
					currentBillAC.IsReImbursementRequest = item.IsReImbursementRequest;
					currentBillAC.UpdatedDate = item.UpdatedDate;
					currentBillAC.IsDelegatedUser = IsDelegate;
					currentBillAC.Provider = item.Provider.Name;
					currentBillACs.Add(currentBillAC);
				}
			}
			return currentBillACs;
		}

		/// <summary>
		/// This method sued for send mail for approval
		/// </summary>
		/// <param name="employeeBillMaster"></param>
		/// <param name="isApprove"></param>
		private async Task SendMailForApproval(Employeebillmaster employeeBillMaster, bool isApprove)
		{
			TeleBillingUtility.Models.Configuration configuration = await _dbTeleBilling_V01Context.Configuration.FirstOrDefaultAsync();
			if (configuration != null)
			{
				Emailtemplate emailTemplate = new Emailtemplate();
				Dictionary<string, string> replacements = new Dictionary<string, string>();
				EnumList.Month month = (EnumList.Month)employeeBillMaster.BillMonth;
				replacements.Add("{BillNumber}", employeeBillMaster.BillNumber);
				replacements.Add("{BillMonth}", month.ToString());
				replacements.Add("{BillYear}", employeeBillMaster.BillYear.ToString());
				replacements.Add("{ApprovalComment}", employeeBillMaster.ApprovalComment);
				replacements.Add("{newEmpName}", employeeBillMaster.Employee.FullName);
				replacements.Add("{Provider}", employeeBillMaster.Provider.Name);
				replacements.Add("{TelePhoneNumber}", employeeBillMaster.TelephoneNumber.Trim());
				if (isApprove)
				{
					if (configuration.NApprovedByLineManager && !string.IsNullOrEmpty(employeeBillMaster.Employee.EmailId))
					{
						if (await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.ApprovedByLineManager), replacements, employeeBillMaster.Employee.EmailId))
							await _iEmailSender.AddedReminderNotificationLog(Convert.ToInt64(EnumList.EmailTemplateType.ApprovedByLineManager), employeeBillMaster.Id, false, employeeBillMaster.Employee.EmailId);
					}
				}
				else
				{
					if (configuration.NRejectedByLineManager && !string.IsNullOrEmpty(employeeBillMaster.Employee.EmailId))
					{
						if (await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.RejectedByLineManager), replacements, employeeBillMaster.Employee.EmailId))
							await _iEmailSender.AddedReminderNotificationLog(Convert.ToInt64(EnumList.EmailTemplateType.RejectedByLineManager), employeeBillMaster.Id, false, employeeBillMaster.Employee.EmailId);
					}
				}
			}
		}

		#endregion
	}
}
