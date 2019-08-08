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
using TeleBillingUtility.Helpers.CommonFunction;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.BillProcess
{
	public class BillProcessRepository : IBillProcessRepository
	{
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public BillProcessRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
			, ILogManagement ilogManagement) {
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<CurrentBillAC>> GetCurrentBills(long userId) {
			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();
			int billWaitingForIdentificationStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
			int billWaitingForLineMangerApprovalStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
			int lineMangerRejectStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject);
			int lineMangerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);

			List<EmployeeBillMaster> employeeBillMasters = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x => !x.IsDelete && x.EmployeeId == userId && (x.EmployeeBillStatus == billWaitingForIdentificationStatusId || x.EmployeeBillStatus == billWaitingForLineMangerApprovalStatusId || x.EmployeeBillStatus == lineMangerRejectStatusId || x.EmployeeBillStatus == lineMangerApprovedStatusId)).Include(x=>x.MbileAssignTypeNavigation).Include(x=>x.Currency).Include(x => x.Employee).Include(x => x.Provider).ToListAsync();
			
			List<TeleBillingUtility.Models.BillDelegate> billDelegates = await _dbTeleBilling_V01Context.BillDelegate.Where(x=>x.DelegateEmployeeId == userId && !x.IsDelete).ToListAsync();
			if (billDelegates.Any()) {
				foreach (var item in billDelegates) {
					List<EmployeeBillMaster> employeeBillMasterDelegates = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x => !x.IsDelete && x.EmployeeId == item.EmployeeId && (x.EmployeeBillStatus == billWaitingForIdentificationStatusId || x.EmployeeBillStatus == billWaitingForLineMangerApprovalStatusId || x.EmployeeBillStatus == lineMangerRejectStatusId || x.EmployeeBillStatus == lineMangerApprovedStatusId)).Include(x => x.MbileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).ToListAsync();
					if(employeeBillMasterDelegates.Any())
						currentBillACs =  GetCurrentBillList(currentBillACs, employeeBillMasterDelegates, item.AllowBillIdentification,true);
				}
			}

			if (employeeBillMasters.Any()) {
				currentBillACs = GetCurrentBillList(currentBillACs, employeeBillMasters, false, false);
			}

			return currentBillACs;
		}
		

		public async Task<ViewBillDetailAC> GetViewBillDetails(long employeebillmasterid) {
			ViewBillDetailAC viewBillDetailAC = new ViewBillDetailAC();
			EmployeeBillMaster employeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.Include(x=>x.Currency).Include(x=>x.Employee).FirstOrDefaultAsync(x=>x.Id == employeebillmasterid && !x.IsDelete);
			List<BillDetails> billDetails =  await _dbTeleBilling_V01Context.BillDetails.Where(x=>x.EmployeeBillId == employeeBillMaster.Id).Include(x=>x.ServiceType).ToListAsync();
			viewBillDetailAC.lstUnAssignedBill = _mapper.Map<List<UnAssignedBillAC>>(billDetails);
			
			List<EmployeeBillServicePackage> employeebillServicePackage = await _dbTeleBilling_V01Context.EmployeeBillServicePackage.Where(x=>x.EmployeeBillId == employeebillmasterid && !x.IsDelete).Include(x=>x.Package).Include(x=>x.ServiceType).ToListAsync();
			viewBillDetailAC.PackageServiceList = _mapper.Map<List<PackageServiceAC>>(employeebillServicePackage);
			viewBillDetailAC.Currency =  employeeBillMaster.Currency.Code;
			
			if(employeeBillMaster.MbileAssignType == Convert.ToInt16(EnumList.AssignType.Business)
				|| employeeBillMaster.Employee.IsPresidentOffice) {
				  viewBillDetailAC.IsDeducateAmount = true;
			}
			else
				viewBillDetailAC.IsDeducateAmount = false;
			
			if(employeeBillMaster.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification) ||
			   employeeBillMaster.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject))
				viewBillDetailAC.IsDisplayOnly = false;
			else
				viewBillDetailAC.IsDisplayOnly = true;

			return viewBillDetailAC;
		}

		public async Task<ResponseAC> BillIdentificationSave(BillIdentificationAC billIdentificationAC,long userId) {
			ResponseAC responseAC = new ResponseAC();
			List<BillDetails> billDetails = new List<BillDetails>();
			foreach(var item in billIdentificationAC.lstUnAssignedBill) {
				BillDetails billDetail = new BillDetails();
				long? callIdentificationType = null;
				if (billIdentificationAC.CallTypeId == 1) {
					 long callTypeId = Convert.ToInt16(EnumList.CallType.UnIdentified);
					billDetail = await _dbTeleBilling_V01Context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId && (x.CallIdentificationType == callTypeId || x.CallIdentificationType == null));
					callIdentificationType = billIdentificationAC.CallId;
				}
				else if(billIdentificationAC.CallTypeId == 2)
				{
					billDetail = await _dbTeleBilling_V01Context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId);
					callIdentificationType = billIdentificationAC.CallId;
				}
				else
				{
					billDetail = await _dbTeleBilling_V01Context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId);
					callIdentificationType = item.CallIdentificationType;
				}
				if (billDetail != null)
				{
					billDetail.CallIdentificationType = callIdentificationType;
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
			return responseAC;
		}

		public async Task<ResponseAC> BillProcess(BillIdentificationAC billIdentificationAC, long userId) {
			ResponseAC responseAC = new ResponseAC();
			List<BillDetails> billDetails = new List<BillDetails>();
			foreach (var item in billIdentificationAC.lstUnAssignedBill) {
				BillDetails billDetail = new BillDetails();
				long? callIdentificationType = null;
				if (billDetail.CallIdentificationType == 1)
				{
					long callTypeId = Convert.ToInt16(EnumList.CallType.UnIdentified);
					billDetail = await _dbTeleBilling_V01Context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId && x.CallIdentificationType == callTypeId);
					callIdentificationType = billIdentificationAC.CallTypeId;
				}
				else if (billDetail.CallIdentificationType == 2)
				{
					billDetail = await _dbTeleBilling_V01Context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId);
					callIdentificationType = billIdentificationAC.CallTypeId;
				}
				else
				{
					billDetail = await _dbTeleBilling_V01Context.BillDetails.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId);
					callIdentificationType = item.CallIdentificationType;
				}
				if (billDetail != null)
				{
					billDetail.CallIdentificationType = callIdentificationType;
					billDetail.CallIdentifiedDate = DateTime.Now;
					billDetail.CallIdentifedBy = userId;
					billDetail.EmployeeComment = item.Comment;
					billDetails.Add(billDetail);
				}
			}

			if (billIdentificationAC.lstUnAssignedBill.Any()) {

				_dbTeleBilling_V01Context.UpdateRange(billDetails);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				EmployeeBillMaster employeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.FirstOrDefaultAsync(x => x.Id == billDetails[0].EmployeeBillId && !x.IsDelete);
				employeeBillMaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
				employeeBillMaster.UpdatedBy = userId;
				employeeBillMaster.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(employeeBillMaster);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				
				foreach(var item in billIdentificationAC.ServicePackageAmountDetail) {
					//PersonalIdentificationAmount+= item.PersonalDeduction;
					//BusinessIdentificationAmount+= item.BusinessCharge;
					var  employeeBillServicePackage = await _dbTeleBilling_V01Context.EmployeeBillServicePackage.FirstOrDefaultAsync(x=>x.ServiceTypeId == item.ServiceTypeId && !x.IsDelete  && x.EmployeeBillId == employeeBillMaster.Id);
					employeeBillServicePackage.PersonalIdentificationAmount = item.PersonalDeduction;
					employeeBillServicePackage.BusinessIdentificationAmount = item.BusinessCharge;
					employeeBillServicePackage.UpdatedBy = userId;
					employeeBillServicePackage.UpdatedDate = DateTime.Now;

					_dbTeleBilling_V01Context.Update(employeeBillServicePackage);
					await _dbTeleBilling_V01Context.SaveChangesAsync();
				}
				



			}
		
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.BillProcessSuccessfully;
			return responseAC;
		}
		
		public async Task<List<CurrentBillAC>> GetLineManagerApprovalList(long loginedUserId) {
			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();
			List<MstEmployee> mstEmployees = await _dbTeleBilling_V01Context.MstEmployee.Where(x=> !x.IsDelete && x.IsActive && x.LineManagerId == loginedUserId).ToListAsync();
			if (mstEmployees.Any()) {
				foreach(var employee in mstEmployees)
				{
					int employeeBillstatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
					List<EmployeeBillMaster> employeeBillMasters = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x => !x.IsDelete && x.EmployeeId == employee.UserId && x.EmployeeBillStatus == employeeBillstatusId).Include(x => x.MbileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).ToListAsync();
					if (employeeBillMasters.Any()) {
						currentBillACs = GetApprvoalList(currentBillACs, employeeBillMasters,false,false);
					}
				}
			}
			return currentBillACs;
		}
		
		public async Task<ResponseAC> LineManagerApproval(LineManagerApprovalAC lineManagerApprovalAC, long userId) {
			ResponseAC response = new ResponseAC();
			if (lineManagerApprovalAC.LineManagerApprovalBills.Any()) {
				List<EmployeeBillMaster> employeeBillMasterList = new List<EmployeeBillMaster>();
				foreach (var item in lineManagerApprovalAC.LineManagerApprovalBills) {
					EmployeeBillMaster employeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.FirstOrDefaultAsync(x=>x.Id == item.EmployeeBillMasterId && !x.IsDelete);
					employeeBillMaster.IsApproved = lineManagerApprovalAC.IsApprove;
					employeeBillMaster.ApprovalComment = item.ApprovalComment;
					employeeBillMaster.EmployeeBillStatus = lineManagerApprovalAC.IsApprove ? Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved) : Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject);
					employeeBillMaster.ApprovalDate = DateTime.Now;
					employeeBillMaster.UpdatedBy = userId;
					employeeBillMaster.UpdatedDate = DateTime.Now;
					employeeBillMasterList.Add(employeeBillMaster);
				}
				_dbTeleBilling_V01Context.UpdateRange(employeeBillMasterList);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				response.Message = _iStringConstant.LineManagerApprovalSuccessfully;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				response.Message = _iStringConstant.AtLeastSelectOneRecord;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return response;
		}

		public async Task<List<CurrentBillAC>> GetMyStaffBills(long userId) {
			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();
			List<EmployeeBillMaster> employeeBillMasters = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x=> !x.IsDelete && x.LinemanagerId == userId).Include(x=>x.EmployeeBillStatusNavigation).Include(x=>x.Employee).Include(x=>x.Provider).Include(x=>x.MbileAssignTypeNavigation).Include(x=>x.Currency).ToListAsync();
			if (employeeBillMasters.Any()) {
				foreach(var item in employeeBillMasters)
				{
				   CurrentBillAC currentBillAC = _mapper.Map<CurrentBillAC>(item);
				   EnumList.Month month = (EnumList.Month)item.BillMonth;
				   currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
                   currentBillACs.Add(currentBillAC);
				}
			}
			return currentBillACs;
		}
		
		public async Task<List<CurrentBillAC>> GetPreviousPeriodBills(long userId) {
			List<CurrentBillAC> currentBillACs = new List<CurrentBillAC>();
			int billLineManagerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);
			int billCloseStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.CloseBill);
			int billAutoStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
			List<EmployeeBillMaster> employeeBillMasters = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x => !x.IsDelete && x.EmployeeId == userId && ((x.EmployeeBillStatus == billAutoStatusId || x.EmployeeBillStatus == billCloseStatusId) || (x.IsReIdentificationRequest && x.EmployeeBillStatus == billLineManagerApprovedStatusId))).Include(x => x.EmployeeBillStatusNavigation).Include(x => x.Employee).Include(x => x.Provider).Include(x => x.MbileAssignTypeNavigation).Include(x => x.Currency).ToListAsync();
			
			if (employeeBillMasters.Any()) { 
				foreach(var employeeBillMaster in employeeBillMasters) {
					if(!currentBillACs.Any(x=>x.EmployeeBillMasterId == employeeBillMaster.Id))
					{
						CurrentBillAC currentBillAC = _mapper.Map<CurrentBillAC>(employeeBillMaster);
						EnumList.Month month = (EnumList.Month)employeeBillMaster.BillMonth;
						currentBillAC.BillDate = month.ToString() + " " + employeeBillMaster.BillYear;
						currentBillAC.ManagerName = _dbTeleBilling_V01Context.MstEmployee.FirstOrDefault(x=>x.UserId == employeeBillMaster.LinemanagerId && !x.IsDelete && x.IsActive)?.FullName;
						if(employeeBillMaster.EmployeeBillStatus == billAutoStatusId || employeeBillMaster.IsReIdentificationRequest)
							currentBillAC.IsAllowToReIdentification = false;
						else
							currentBillAC.IsAllowToReIdentification = true;

						if(employeeBillMaster.IsReIdentificationRequest && employeeBillMaster.EmployeeBillStatus == billLineManagerApprovedStatusId)
						{
							if(employeeBillMaster.IsReImbursementRequest)
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
			return currentBillACs;
		}

		public async Task<long> ReIdentificationRequest(long userId, long employeebillmasterid) {
			
			EmployeeBillMaster employeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.FirstAsync(x=>x.Id == employeebillmasterid && !x.IsDelete);
			EmployeeBillMaster newEmployeeBillMaster = new EmployeeBillMaster();

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
				//newEmployeeBillMaster.BusinessIdentificationAmount = employeeBillMaster.BusinessIdentificationAmount;
				//newEmployeeBillMaster.BusinessTotalAmount = employeeBillMaster.BusinessTotalAmount;
				newEmployeeBillMaster.CurrencyId = employeeBillMaster.CurrencyId;
				newEmployeeBillMaster.Description = employeeBillMaster.Description;
				newEmployeeBillMaster.EmpBusinessUnitId = employeeBillMaster.EmpBusinessUnitId;
				newEmployeeBillMaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
				newEmployeeBillMaster.EmployeeId = employeeBillMaster.EmployeeId;
				newEmployeeBillMaster.IsReIdentificationRequest = true;
				newEmployeeBillMaster.LinemanagerId = employeeBillMaster.LinemanagerId;
				newEmployeeBillMaster.MbileAssignType = employeeBillMaster.MbileAssignType;
				//newEmployeeBillMaster.PersonalIdentificationAmount = employeeBillMaster.PersonalIdentificationAmount;
				newEmployeeBillMaster.PreviousEmployeeBillId = employeeBillMaster.Id;
				newEmployeeBillMaster.ProviderId = employeeBillMaster.ProviderId;
				newEmployeeBillMaster.TelephoneNumber = employeeBillMaster.TelephoneNumber;
				newEmployeeBillMaster.TotalBillAmount = employeeBillMaster.TotalBillAmount;
				newEmployeeBillMaster.CreatedBy = userId;
				newEmployeeBillMaster.CreatedDate = DateTime.Now;

				await _dbTeleBilling_V01Context.AddAsync(newEmployeeBillMaster);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			
			
			#endregion

			List<EmployeeBillServicePackage> employeeBillServicePackageList = await _dbTeleBilling_V01Context.EmployeeBillServicePackage.Where(x=>x.EmployeeBillId == employeeBillMaster.Id && !x.IsDelete).ToListAsync();
			
			#region Added New EmployeeBillServicePackage
			
			List<EmployeeBillServicePackage> newEmployeeBillServicePackageList = new List<EmployeeBillServicePackage>();
			foreach (var item in employeeBillServicePackageList) {
				EmployeeBillServicePackage employeeBillServicePackage = new EmployeeBillServicePackage();
				employeeBillServicePackage.EmployeeBillId = newEmployeeBillMaster.Id;
				employeeBillServicePackage.PackageId = item.PackageId;
				employeeBillServicePackage.ServiceTypeId = item.ServiceTypeId;
				employeeBillServicePackage.PersonalIdentificationAmount = item.PersonalIdentificationAmount;
				employeeBillServicePackage.BusinessTotalAmount = item.BusinessTotalAmount;
				employeeBillServicePackage.BusinessIdentificationAmount = item.BusinessIdentificationAmount;
				newEmployeeBillServicePackageList.Add(employeeBillServicePackage);
			}
			await _dbTeleBilling_V01Context.AddRangeAsync(newEmployeeBillServicePackageList);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added Bill Details 
			List<BillDetails> billDetails = await _dbTeleBilling_V01Context.BillDetails.Where(x=>x.EmployeeBillId == employeeBillMaster.Id).ToListAsync();
			List<BillDetails> newBillDetails = new List<BillDetails>();
			foreach(var item in billDetails)
			{
				BillDetails billDetail = new BillDetails();
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
				billDetail.GroupId =item.GroupId;
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

			return newEmployeeBillMaster.Id;
		}

		public async Task<ResponseAC> ReImbursementRequest(long userId, ReImbursementRequestAC reImbursementRequestAC) {
			ResponseAC responseAC = new ResponseAC();
			EmployeeBillMaster employeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.FirstAsync(x=>x.Id == reImbursementRequestAC.EmployeeBillMasterId && !x.IsDelete);
			BillReImburse billReImburse = new BillReImburse();
			billReImburse.EmployeeBillId = employeeBillMaster.Id;
			billReImburse.BillMasterId = employeeBillMaster.BillMasterId;
			billReImburse.ReImbruseAmount = reImbursementRequestAC.Amount;
			billReImburse.Description = reImbursementRequestAC.Description;
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

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.ReImbursementRequestAddedSuccessfully;
			
			return responseAC;
		}

		public async Task<List<ReImburseBillsAC>> GetReImburseBills() {
			List<BillReImburse> billReImburse = await _dbTeleBilling_V01Context.BillReImburse.Where(x=> !x.IsDelete && x.IsApproved == null).Include(x=>x.EmployeeBill).Include(x=>x.EmployeeBill.Employee).Include(x => x.EmployeeBill.Provider).ToListAsync();
			List<ReImburseBillsAC> reImburseBillsACs = new List<ReImburseBillsAC>();
			foreach(var item in billReImburse) {

				ReImburseBillsAC reImburseBillsAC = new ReImburseBillsAC();
				reImburseBillsAC = _mapper.Map<ReImburseBillsAC>(item);
				EnumList.Month month = (EnumList.Month)item.EmployeeBill.BillMonth;
				reImburseBillsAC.BillDate = month.ToString() + " " + item.EmployeeBill.BillYear;
				reImburseBillsAC.ManagerName = _dbTeleBilling_V01Context.MstEmployee.FirstOrDefault(x => x.UserId == item.EmployeeBill.LinemanagerId && !x.IsDelete && x.IsActive)?.FullName;
				reImburseBillsACs.Add(reImburseBillsAC);
			}
			return reImburseBillsACs;
		}
		
		public async Task<ResponseAC> ReImburseBillApproval(long userId, ReImburseBillApprovalAC reImburseBillApprovalAC) {
			ResponseAC responseAC = new ResponseAC();

			BillReImburse billReImburse = await _dbTeleBilling_V01Context.BillReImburse.FirstAsync(x=> !x.IsDelete && x.Id == reImburseBillApprovalAC.Id);
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

			responseAC.Message = reImburseBillApprovalAC.IsApproved ? _iStringConstant.BillReImburseApprovesuccessfully : _iStringConstant.BillReImburseRejectsuccessfully;
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			return responseAC;
		}
		
		public async Task<List<ChangeBillStatusAC>> GetChangeBillStatusList() {
			int billWaitingForLineMangerApprovalStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
			int lineMangerRejectStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject);
			int lineMangerApprovedStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved);
			List<ChangeBillStatusAC> changeBillStatusACs = new List<ChangeBillStatusAC>();
			List<EmployeeBillMaster> employeeBillMasters = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x => !x.IsDelete && (x.EmployeeBillStatus == billWaitingForLineMangerApprovalStatusId || x.EmployeeBillStatus == lineMangerRejectStatusId || x.EmployeeBillStatus == lineMangerApprovedStatusId)).Include(x => x.MbileAssignTypeNavigation).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.Provider).Include(x=>x.Linemanager).ToListAsync();
			if (employeeBillMasters.Any()) {
				foreach (var item in employeeBillMasters) {

					ChangeBillStatusAC changeBillStatusAC = new ChangeBillStatusAC();
					changeBillStatusAC.Amount = item.TotalBillAmount;
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					changeBillStatusAC.BillDate = month.ToString() + " " + item.BillYear;
					changeBillStatusAC.BillNumber = item.BillNumber;
					changeBillStatusAC.BillStatus = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
					changeBillStatusAC.Currency = item.Currency.Code;
					changeBillStatusAC.EmployeeBillMasterId = item.Id;
					changeBillStatusAC.EmployeeBillStatus = item.EmployeeBillStatus;
					if(item.EmployeeId != null)
					{
						changeBillStatusAC.EmployeeId = Convert.ToInt64(item.EmployeeId);
						changeBillStatusAC.EmployeeName = item.Employee.FullName;
					}
					changeBillStatusAC.Month = item.BillMonth;
					changeBillStatusAC.Year	= item.BillYear;
					changeBillStatusAC.Provider = item.Provider.Name;
					changeBillStatusAC.ProviderId = item.ProviderId;
					if(item.LinemanagerId != null)
						changeBillStatusAC.ManagerName =  item.Linemanager.FullName;

					changeBillStatusACs.Add(changeBillStatusAC);
				}
			}
			return changeBillStatusACs;
		}

		public async Task<ResponseAC> ChangeBillStatus(List<ChangeBillStatusAC> changeBillStatusACs, long userId) {
			ResponseAC responseAC = new ResponseAC();
			if (changeBillStatusACs.Any()) {

				List<EmployeeBillMaster> employeeBillMasterList = new List<EmployeeBillMaster>();
				foreach(var item in changeBillStatusACs)
				{
					EmployeeBillMaster employeeBillMaster = await _dbTeleBilling_V01Context.EmployeeBillMaster.FirstOrDefaultAsync(x => x.Id == item.EmployeeBillMasterId && !x.IsDelete);
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
						employeeBillMasterList.Add(employeeBillMaster);
					}
				}

				_dbTeleBilling_V01Context.UpdateRange(employeeBillMasterList);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				responseAC.Message = _iStringConstant.BillChangeStatusSuccesfully;
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			return responseAC;
		}

		#endregion

		#region Private Method(s)

		/// <summary>
		/// This method used for move to bill list model class(EmployeeBillMaster) to application class(CurrentBillAC)
		/// </summary>
		/// <param name="currentBillList"></param>
		/// <param name="employeeBillMasters"></param>
		/// <param name="allowBillIdentification"></param>
		/// <param name="IsDelegate"></param>
		/// <returns></returns>
		private List<CurrentBillAC> GetCurrentBillList(List<CurrentBillAC>  currentBillList, List<EmployeeBillMaster> employeeBillMasters, bool allowBillIdentification , bool IsDelegate) {
			foreach (var item in employeeBillMasters)
			{
				if(!currentBillList.Any(x=>x.EmployeeBillMasterId == item.Id)) { 

					CurrentBillAC currentBillAC = new CurrentBillAC();
					currentBillAC.Amount = item.TotalBillAmount;
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					currentBillAC.AssigneType = item.MbileAssignTypeNavigation.Name;
					currentBillAC.BillStatus = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
					currentBillAC.Currency = item.Currency.Code;
					currentBillAC.Description = item.Description;
					currentBillAC.EmployeeBillMasterId = item.Id;
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
		/// This method used for move to line manager approval model class(EmployeeBillMaster) to application class(CurrentBillAC)
		/// </summary>
		/// <param name="currentBillACs"></param>
		/// <param name="employeeBillMasters"></param>
		/// <param name="allowBillAppoval"></param>
		/// <param name="IsDelegate"></param>
		/// <returns></returns>
		private List<CurrentBillAC> GetApprvoalList(List<CurrentBillAC> currentBillACs, List<EmployeeBillMaster> employeeBillMasters, bool allowBillAppoval, bool IsDelegate) {
			foreach (var item in employeeBillMasters)
			{
				if (!currentBillACs.Any(x => x.EmployeeBillMasterId == item.BillMasterId))
				{
					CurrentBillAC currentBillAC = new CurrentBillAC();
					currentBillAC.Amount = item.TotalBillAmount;
					EnumList.Month month = (EnumList.Month)item.BillMonth;
					currentBillAC.BillDate = month.ToString() + " " + item.BillYear;
					currentBillAC.AssigneType = item.MbileAssignTypeNavigation.Name;
					currentBillAC.BillStatus = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
					currentBillAC.Currency = item.Currency.Code;
					currentBillAC.Description = item.Description;
					currentBillAC.EmployeeBillMasterId = item.Id;
					currentBillAC.EmployeeName = item.Employee.FullName;
					currentBillAC.TelephoneNumber = item.TelephoneNumber;
					currentBillAC.UpdatedDate = item.UpdatedDate;
					currentBillAC.Provider = item.Provider.Name;
					currentBillACs.Add(currentBillAC);
				}
			}
			return currentBillACs;
		}


		#endregion
	}
}
