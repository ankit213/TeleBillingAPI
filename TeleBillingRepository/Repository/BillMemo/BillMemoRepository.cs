using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.CommonFunction;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.BillMemo
{
	public class BillMemoRepository : IBillMemoRepository
	{


		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private readonly IEmailSender _iEmailSender;
		private IMapper _mapper;
		private readonly DALMySql _objDalmysql = new DALMySql();
		private readonly DAL _objDal = new DAL();
		#endregion

		#region "Constructor"
		public BillMemoRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
			, ILogManagement ilogManagement, IEmailSender iEmailSender)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
			_iEmailSender = iEmailSender;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<BillMemoAC>> GetMemoList()
		{
			List<BillMemoAC> billMemoACs = new List<BillMemoAC>();
			List<Memo> memos = await _dbTeleBilling_V01Context.Memo.Where(x => !x.IsDelete).Include(x => x.Provider).OrderByDescending(x => x.Id).ToListAsync();
			foreach (var item in memos)
			{
				var memo = _mapper.Map<BillMemoAC>(item);
				memo.Status = item.IsApproved == null ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)1)) : (item.IsApproved == true ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)2)) : CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)3)));
				billMemoACs.Add(memo);
			}
			return billMemoACs;
		}

		public async Task<MemoBillResponse> GetMemoBills(int month, int year, int providerid)
		{
			long billStatusId = Convert.ToInt16(EnumList.BillStatus.BillClosed);
			MemoBillResponse memoBillRespons = new MemoBillResponse();
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == providerid);
			if (provider != null)
			{
				memoBillRespons.Ibancode = provider.Ibancode;
				memoBillRespons.Bank = provider.Bank;
				memoBillRespons.Swiftcode = provider.Swiftcode;
			}

			List<Billmaster> billMasters = await _dbTeleBilling_V01Context.Billmaster.Where(x => !x.IsDelete && x.BillStatusId == billStatusId && x.BillMonth == month && x.BillYear == year && x.ProviderId == providerid).ToListAsync();
			memoBillRespons.MemoBills = _mapper.Map<List<MemoBillsAC>>(billMasters);
			return memoBillRespons;
		}

		public async Task<ResponseAC> AddMemo(MemoAC memoAC, long userId, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			Memo memo = _mapper.Map<Memo>(memoAC);
			memo.Id = 0;
			memo.CreatedBy = userId;
			memo.CreatedDate = DateTime.Now;
			memo.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
			memo.RefrenceNo = memoAC.ProviderName + "/INV/" + memoAC.Month + "/" + memo.Year;
			_dbTeleBilling_V01Context.Add(memo);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			List<Memobills> billMemos = new List<Memobills>();
			List<Billmaster> billMasters = new List<Billmaster>();
			foreach (long item in memoAC.BillIds)
			{
				Memobills memoBill = new Memobills();
				memoBill.BillId = item;
				memoBill.MemoId = memo.Id;
				memoBill.TransactionId = memo.TransactionId;
				memoBill.CreatedBy = userId;
				memoBill.CreatedDate = DateTime.Now;
				billMemos.Add(memoBill);

				Billmaster billMaster = await _dbTeleBilling_V01Context.Billmaster.FirstOrDefaultAsync(x => x.Id == item);
				billMaster.UpdatedBy = userId;
				billMaster.BillStatusId = Convert.ToInt16(EnumList.BillStatus.MemoCreated);
				billMaster.UpdatedDate = DateTime.Now;
				billMasters.Add(billMaster);
			}

			if (billMemos.Any())
			{
				await _dbTeleBilling_V01Context.AddRangeAsync(billMemos);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			}


			if (billMasters.Any())
			{
				_dbTeleBilling_V01Context.UpdateRange(billMasters);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				memo = await _dbTeleBilling_V01Context.Memo.Where(x => x.Id == memo.Id && !x.IsDelete).Include(x => x.Provider).FirstOrDefaultAsync();

				#region Send Mail For Create Memo
				TeleBillingUtility.Models.Configuration configuration = await _dbTeleBilling_V01Context.Configuration.FirstOrDefaultAsync();
				List<MstEmployee> mstEmployees = await _dbTeleBilling_V01Context.MstEmployee.Where(x => !x.IsDelete && x.IsActive && x.IsPresidentOffice).ToListAsync();
				if (configuration != null && configuration.NSendMemo)
				{
					foreach (var item in billMasters)
					{
						if (mstEmployees.Any())
						{
							foreach (var employee in mstEmployees)
							{
								if (!string.IsNullOrEmpty(employee.EmailId))
								{
									Emailtemplate emailTemplate = new Emailtemplate();
									Dictionary<string, string> replacements = new Dictionary<string, string>();
									EnumList.Month month = (EnumList.Month)item.BillMonth;

									replacements.Add("{BillMonth}", month.ToString());
									replacements.Add("{BillYear}", item.BillYear.ToString());
									replacements.Add("{RefrenceNo}", memo.RefrenceNo);
									replacements.Add("{newEmpName}", employee.FullName);
									replacements.Add("{MemoSubject}", memo.Subject);
									replacements.Add("{BillNumber}", item.BillNumber);
									replacements.Add("{BillAmount}", memo.TotalAmount.ToString());
									replacements.Add("{Provider}", memo.Provider.Name);

									if (await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.SendMemo), replacements, employee.EmailId))
										await _iEmailSender.AddedReminderNotificationLog(Convert.ToInt64(EnumList.EmailTemplateType.SendMemo), null, false, employee.EmailId);
								}


							}
						}
					}
				}
				#endregion


				#region Notification For Memo
				List<Notificationlog> notificationlogs = new List<Notificationlog>();
				if (mstEmployees.Any())
				{
					foreach (var item in mstEmployees)
					{
						notificationlogs.Add(_iLogManagement.GenerateNotificationObject(item.UserId, userId, Convert.ToInt16(EnumList.NotificationType.SendMemo), memo.Id));
					}
					await _iLogManagement.SaveNotificationList(notificationlogs);
				}
				#endregion

			}

			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddMemo, loginUserName, userId, "Memo(" + memo.RefrenceNo + ")", (int)EnumList.ActionTemplateTypes.Add, memo.Id);

			responseAC.Message = _iStringConstant.MemoAddedsuccessfully;
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

			return responseAC;
		}

		public async Task<bool> DeleteMemoBills(long id, long userId, string loginUserName)
		{
			Memo memo = await _dbTeleBilling_V01Context.Memo.FirstAsync(x => x.Id == id);
			memo.IsDelete = true;
			memo.UpdatedBy = userId;
			memo.UpdatedDate = DateTime.Now;

			List<Billmaster> billMaster = await _dbTeleBilling_V01Context.Memobills.Where(x => x.MemoId == id && !x.IsDelete).Include(x => x.Bill).Select(x => x.Bill).ToListAsync();
			billMaster.ForEach(x => { x.BillStatusId = Convert.ToInt16(EnumList.BillStatus.BillClosed); x.UpdatedBy = userId; x.UpdatedDate = DateTime.Now; });

			_dbTeleBilling_V01Context.UpdateRange(billMaster);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			_dbTeleBilling_V01Context.Update(memo);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteMemo, loginUserName, userId, "Memo(" + memo.RefrenceNo + ")", (int)EnumList.ActionTemplateTypes.Delete, memo.Id);
			return true;
		}


		public async Task<List<BillMemoAC>> GetApprvoalMemoList()
		{
			List<BillMemoAC> billMemoACs = new List<BillMemoAC>();
			List<Memo> memos = await _dbTeleBilling_V01Context.Memo.Where(x => !x.IsDelete && x.IsApproved == null).Include(x => x.Provider).OrderByDescending(x => x.Id).ToListAsync();
			foreach (var item in memos)
			{
				var memo = _mapper.Map<BillMemoAC>(item);
				memo.Status = item.IsApproved == null ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)1)) : (item.IsApproved == true ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)2)) : CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)3)));
				billMemoACs.Add(memo);
			}
			return billMemoACs;
		}

		public async Task<ResponseAC> MemoApproval(MemoApprovalAC memoApprovalAC, long userId, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			List<Memo> lstMemo = new List<Memo>();
			List<Billmaster> billMasters = new List<Billmaster>();
			List<string> stringMemoArray = new List<string>();
			foreach (var item in memoApprovalAC.billMemoACs)
			{
				Memo memoObj = await _dbTeleBilling_V01Context.Memo.Where(x => x.Id == item.Id).Include(x => x.Provider).FirstOrDefaultAsync();
				if (memoObj.IsApproved == null)
				{
					memoObj.IsApproved = memoApprovalAC.IsApprvoed;
					memoObj.ApprovedDate = DateTime.Now;
					memoObj.ApprovedBy = userId;
					memoObj.Comment = item.Comment;
					memoObj.UpdatedBy = userId;
					memoObj.UpdatedDate = DateTime.Now;
					lstMemo.Add(memoObj);

					string memoApproval = string.Empty;
					if (!memoApprovalAC.IsApprvoed)
					{
						billMasters.AddRange(await _dbTeleBilling_V01Context.Memobills.Where(x => x.MemoId == item.Id && !x.IsDelete).Include(x => x.Bill).Select(x => x.Bill).ToListAsync());
						memoApproval = "Rejected";
					}
					else
						memoApproval = "Approved";

					#region Send Mail For Memo Approval
					TeleBillingUtility.Models.Configuration configuration = await _dbTeleBilling_V01Context.Configuration.FirstOrDefaultAsync();
					MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => !x.IsDelete && x.UserId == memoObj.CreatedBy);
					if (configuration != null && configuration.NMemoApprovalRejection)
					{
						if (mstEmployee != null)
						{
							if (!string.IsNullOrEmpty(mstEmployee.EmailId))
							{
								Dictionary<string, string> replacements = new Dictionary<string, string>();
								EnumList.Month month = (EnumList.Month)memoObj.Month;
								replacements.Add("{MemoApproval}", memoApproval);
								replacements.Add("{BillMonth}", month.ToString());
								replacements.Add("{BillYear}", memoObj.Year.ToString());
								replacements.Add("{RefrenceNo}", memoObj.RefrenceNo);
								replacements.Add("{newEmpName}", mstEmployee.FullName);
								replacements.Add("{ApprovalComment}", memoObj.Comment);
								replacements.Add("{BillAmount}", memoObj.TotalAmount.ToString());
								replacements.Add("{MemoSubject}", memoObj.Subject);
								replacements.Add("{Provider}", memoObj.Provider.Name);

								if (await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.MemoApproval), replacements, mstEmployee.EmailId))
									await _iEmailSender.AddedReminderNotificationLog(Convert.ToInt64(EnumList.EmailTemplateType.MemoApproval), null, false, mstEmployee.EmailId);
							}
						}
					}
					#endregion

					#region Notification For Memo
					List<Notificationlog> notificationlogs = new List<Notificationlog>();
					if (mstEmployee != null)
					{
						if (memoApprovalAC.IsApprvoed)
						{
							notificationlogs.Add(_iLogManagement.GenerateNotificationObject(mstEmployee.UserId, userId, Convert.ToInt16(EnumList.NotificationType.MemoApprove), memoObj.Id));
							await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.MemoApprove, loginUserName, userId, "Memo(" + memoObj.RefrenceNo + ")", (int)EnumList.ActionTemplateTypes.Approve, memoObj.Id);
						}
						else
						{
							notificationlogs.Add(_iLogManagement.GenerateNotificationObject(mstEmployee.UserId, userId, Convert.ToInt16(EnumList.NotificationType.MemoReject), memoObj.Id));
							await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.MemoReject, loginUserName, userId, "Memo(" + memoObj.RefrenceNo + ")", (int)EnumList.ActionTemplateTypes.Reject, memoObj.Id);
						}
						await _iLogManagement.SaveNotificationList(notificationlogs);
					}
					#endregion
				}
				else
					stringMemoArray.Add(memoObj.RefrenceNo);

			}
			#region Update Bill Status
			if (billMasters.Any())
			{
				foreach (var item in billMasters)
				{
					item.BillStatusId = Convert.ToInt16(EnumList.BillStatus.BillAllocated);
					item.UpdatedBy = userId;
					item.UpdatedDate = DateTime.Now;
				}

				_dbTeleBilling_V01Context.UpdateRange(billMasters);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

			}
			#endregion

			if (lstMemo.Any())
			{
				_dbTeleBilling_V01Context.UpdateRange(lstMemo);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			}

			if (lstMemo.Count() == memoApprovalAC.billMemoACs.Count())
			{
				responseAC.Message = memoApprovalAC.IsApprvoed ? _iStringConstant.MemoApprovedsuccessfully : _iStringConstant.MemoRejectedsuccessfully;
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				string message = String.Join(',', stringMemoArray);
				responseAC.Message = memoApprovalAC.IsApprvoed ? _iStringConstant.MemoApprovalMessagesuccessfully.Replace("{{@currentapproval}}", "approved").Replace("{{@memo}}", message) : _iStringConstant.MemoApprovalMessagesuccessfully.Replace("{{@currentapproval}}", "rejected").Replace("{{@memo}}", message);
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Warning);
			}
			return responseAC;
		}

		public async Task<List<BillMemoAC>> GetAccountMemoList()
		{
			List<BillMemoAC> billMemoACs = new List<BillMemoAC>();
			List<Memo> memos = await _dbTeleBilling_V01Context.Memo.Where(x => !x.IsDelete).Include(x => x.Provider).ToListAsync();
			foreach (var item in memos)
			{
				var memo = _mapper.Map<BillMemoAC>(item);
				if (!item.IsBankTransaction)
				{
					memo.Bank = string.Empty;
					memo.Swiftcode = string.Empty;
					memo.Ibancode = string.Empty;
				}
				memo.Status = item.IsApproved == null ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)1)) : (item.IsApproved == true ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)2)) : CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)3)));
				billMemoACs.Add(memo);
			}
			return billMemoACs;
		}

		public ExportMemoAC GetMemoExportDetail(long id)
		{
			ExportMemoAC exportMemoAC = new ExportMemoAC();
			SortedList sl = new SortedList();
			sl.Add("memoid", id);
			DataSet ds = _objDalmysql.GetDataSet("usp_GetExportMemoDetails", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					exportMemoAC = _objDal.ConvertDataTableToGenericList<ExportMemoAC>(ds.Tables[0]).ToList().FirstOrDefault();
				}
				if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
				{
					exportMemoAC.exportMemoBillsACs = _objDal.ConvertDataTableToGenericList<ExportMemoBillsAC>(ds.Tables[1]).ToList();
				}
			}

			return exportMemoAC;
		}
		#endregion
	}
}
