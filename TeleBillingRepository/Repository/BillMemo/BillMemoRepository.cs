using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

namespace TeleBillingRepository.Repository.BillMemo
{
	public class BillMemoRepository : IBillMemoRepository {


		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public BillMemoRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
			, ILogManagement ilogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<BillMemoAC>> GetMemoList() {
			List<BillMemoAC> billMemoACs = new List<BillMemoAC>();
			List<Memo> memos = await _dbTeleBilling_V01Context.Memo.Where(x=> !x.IsDelete).Include(x=>x.Provider).ToListAsync(); 
			foreach(var item in memos) {
				var memo = _mapper.Map<BillMemoAC>(item);
				memo.Status = item.IsApproved == null ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)1)) : (item.IsApproved == true ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)2)) : CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)3)));
				billMemoACs.Add(memo);
			}
			return billMemoACs;
		}

		public async Task<MemoBillResponse> GetMemoBills(int month, int year, int providerid) {
			long billStatusId = Convert.ToInt16(EnumList.BillStatus.BillClosed);
			MemoBillResponse memoBillRespons = new MemoBillResponse();
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x=>x.Id == providerid);
			if(provider != null) {
				memoBillRespons.Ibancode = provider.Ibancode;
				memoBillRespons.Bank = provider.Bank;
				memoBillRespons.Swiftcode = provider.Swiftcode;
			}
			
			List<BillMaster> billMasters = await _dbTeleBilling_V01Context.BillMaster.Where(x=> !x.IsDelete && x.BillStatusId == billStatusId && x.BillMonth == month && x.BillYear == year && x.ProviderId == providerid).ToListAsync();
			memoBillRespons.MemoBills = _mapper.Map<List<MemoBillsAC>>(billMasters);
			return memoBillRespons;
		}

		public async Task<ResponseAC> AddMemo(MemoAC memoAC, long userId) {
			ResponseAC responseAC = new ResponseAC();
			Memo memo = _mapper.Map<Memo>(memoAC);
			memo.CreatedBy = userId;
			memo.CreatedDate = DateTime.Now;
			memo.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
			memo.RefrenceNo = memoAC.ProviderName + "/INV/"+memoAC.Month+"/"+memo.Year;
			_dbTeleBilling_V01Context.Add(memo);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			
			List<MemoBills> billMemos = new List<MemoBills>();
			List<BillMaster> billMasters = new List<BillMaster>();
			foreach(long item in memoAC.BillIds) {
				MemoBills memoBill = new MemoBills();
				memoBill.BillId = item;
				memoBill.MemoId = memo.Id;
				memoBill.TransactionId = memo.TransactionId;
				memoBill.CreatedBy = userId;
				memoBill.CreatedDate = DateTime.Now;
				billMemos.Add(memoBill);
				
				BillMaster billMaster = await _dbTeleBilling_V01Context.BillMaster.FirstOrDefaultAsync(x=>x.Id == item);
				billMaster.UpdatedBy = userId;
				billMaster.BillStatusId = Convert.ToInt16(EnumList.BillStatus.MemoCreated);
				billMaster.UpdatedDate = DateTime.Now;
				billMasters.Add(billMaster);
			}
			
			if(billMemos.Any()) {
				await _dbTeleBilling_V01Context.AddRangeAsync(billMemos);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			}

			if (billMasters.Any()) {
				_dbTeleBilling_V01Context.UpdateRange(billMasters);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			}

			responseAC.Message = _iStringConstant.MemoAddedsuccessfully;
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

			return responseAC;
		}

		public async Task<bool> DeleteMemoBills(long id, long userId) {
			Memo memo = await _dbTeleBilling_V01Context.Memo.FirstAsync(x=>x.Id == id);
			memo.IsDelete = true;
			memo.UpdatedBy = userId;
			memo.UpdatedDate = DateTime.Now;

			_dbTeleBilling_V01Context.Update(memo);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			return true;
		}


		public async Task<List<BillMemoAC>> GetApprvoalMemoList() {
			List<BillMemoAC> billMemoACs = new List<BillMemoAC>();
			List<Memo> memos = await _dbTeleBilling_V01Context.Memo.Where(x => !x.IsDelete && x.IsApproved == null).Include(x => x.Provider).ToListAsync();
			foreach (var item in memos)
			{
				var memo = _mapper.Map<BillMemoAC>(item);
				memo.Status = item.IsApproved == null ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)1)) : (item.IsApproved == true ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)2)) : CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)3)));
				billMemoACs.Add(memo);
			}
			return billMemoACs;
		}

		public async Task<ResponseAC> MemoApproval(MemoApprovalAC memoApprovalAC, long userId) {
			ResponseAC responseAC = new ResponseAC();
			List<Memo> lstMemo = new List<Memo>();
			List<BillMaster> billMaster = new List<BillMaster>();
			foreach (var item in memoApprovalAC.billMemoACs) {
				Memo memoObj = await _dbTeleBilling_V01Context.Memo.FirstOrDefaultAsync(x=>x.Id == item.Id);
				memoObj.IsApproved = memoApprovalAC.IsApprvoed;
				memoObj.ApprovedDate = DateTime.Now;
				memoObj.ApprovedBy = userId;
				memoObj.Comment = item.Comment;
				memoObj.UpdatedBy = userId;
				memoObj.UpdatedDate = DateTime.Now;
				lstMemo.Add(memoObj);

				if (!memoApprovalAC.IsApprvoed)
					billMaster.AddRange(await _dbTeleBilling_V01Context.MemoBills.Where(x => x.MemoId == item.Id && !x.IsDelete).Include(x=>x.Bill).Select(x=>x.Bill).ToListAsync());
			}

			#region Update Bill Status
			if (billMaster.Any()) {
				foreach(var item in billMaster) {
					item.BillStatusId = Convert.ToInt16(EnumList.BillStatus.BillAllocated);
					item.UpdatedBy = userId;
					item.UpdatedDate = DateTime.Now;
				}

				_dbTeleBilling_V01Context.UpdateRange(billMaster);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			}
			#endregion

			_dbTeleBilling_V01Context.UpdateRange(lstMemo);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			
			responseAC.Message = memoApprovalAC.IsApprvoed  ? _iStringConstant.MemoApprovedsuccessfully : _iStringConstant.MemoRejectedsuccessfully;
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			return responseAC;
		}

		public async Task<List<BillMemoAC>> GetAccountMemoList() {
			List<BillMemoAC> billMemoACs = new List<BillMemoAC>();
			List<Memo> memos = await _dbTeleBilling_V01Context.Memo.Where(x => !x.IsDelete).Include(x => x.Provider).ToListAsync();
			foreach (var item in memos)
			{
				var memo = _mapper.Map<BillMemoAC>(item);
				if (!item.IsBankTransaction) {
					memo.Bank = string.Empty;
					memo.Swiftcode = string.Empty;
					memo.Ibancode = string.Empty;
				}
				memo.Status = item.IsApproved == null ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)1)) : (item.IsApproved == true ? CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)2)) : CommonFunction.GetDescriptionFromEnumValue(((EnumList.MemoStatusType)3)));
				billMemoACs.Add(memo);
			}
			return billMemoACs;
		}

		#endregion
	}
}
