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

namespace TeleBillingRepository.Repository.Master.HandsetManagement
{
	public class HandsetRepository : IHandsetRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public HandsetRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, ILogManagement ilogManagement,
			IStringConstant iStringConstant)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
		}
		#endregion

		#region "Public Method(s)"

		public async Task<List<DrpResponseAC>> GetHandsetList()
		{
			List<MstHandsetdetail> lstHandsetDetails = await _dbTeleBilling_V01Context.MstHandsetdetail.Where(x => !x.IsDelete).OrderByDescending(x => x.Id).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lstHandsetDetails);
		}

		public async Task<List<HandsetDetailAC>> GetHandsets()
		{
			List<MstHandsetdetail> lstHandsetDetails = await _dbTeleBilling_V01Context.MstHandsetdetail.Where(x => !x.IsDelete).OrderByDescending(x => x.Id).ToListAsync();
			return _mapper.Map<List<HandsetDetailAC>>(lstHandsetDetails);
		}

		public async Task<HandsetDetailAC> GetHandsetById(long id)
		{
			MstHandsetdetail handsetDetail = await _dbTeleBilling_V01Context.MstHandsetdetail.FirstOrDefaultAsync(x => !x.IsDelete && x.Id == id);
			return _mapper.Map<HandsetDetailAC>(handsetDetail);
		}

		public async Task<ResponseAC> EditHandset(HandsetDetailAC handsetDetailAC, long userId)
		{
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstHandsetdetail.AnyAsync(x => x.Id != handsetDetailAC.Id && x.Name.ToLower().Trim() == handsetDetailAC.Name.Trim().ToLower() && !x.IsDelete))
			{
				MstHandsetdetail mstHandsetDetail = await _dbTeleBilling_V01Context.MstHandsetdetail.FirstOrDefaultAsync(x => x.Id == handsetDetailAC.Id && !x.IsDelete);

				#region Transaction Log Entry
				if (mstHandsetDetail.TransactionId == null)
					mstHandsetDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(mstHandsetDetail);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstHandsetDetail.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				mstHandsetDetail.Name = handsetDetailAC.Name.Trim();
				mstHandsetDetail.UpdatedBy = userId;
				mstHandsetDetail.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstHandsetDetail);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				responeAC.Message = _iStringConstant.HandsetUpdateSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				responeAC.Message = _iStringConstant.HandsetAlreadyExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<ResponseAC> AddHandset(HandsetDetailAC handsetDetailAC, long userId, string loginUserName)
		{
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstHandsetdetail.AnyAsync(x => x.Name.ToLower().Trim() == handsetDetailAC.Name.ToLower().Trim() && !x.IsDelete))
			{
				MstHandsetdetail mstHandsetDetail = new MstHandsetdetail();
				mstHandsetDetail.Name = handsetDetailAC.Name.Trim();
				mstHandsetDetail.CreatedBy = userId;
				mstHandsetDetail.CreatedDate = DateTime.Now;
				mstHandsetDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				await _dbTeleBilling_V01Context.AddAsync(mstHandsetDetail);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				responeAC.Message = _iStringConstant.HandsetAddedSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddHandset, loginUserName, userId, "Handset(" + mstHandsetDetail.Name + ")", (int)EnumList.ActionTemplateTypes.Add, mstHandsetDetail.Id);

			}
			else
			{
				responeAC.Message = _iStringConstant.HandsetAlreadyExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<bool> DeleteHandset(long id, long userId, string loginUserName)
		{
			List<Providerpackage> providerpackages = await _dbTeleBilling_V01Context.Providerpackage.Where(x => x.HandsetDetailIds.Contains(id.ToString()) && x.IsActive && !x.IsDelete).ToListAsync();
			if (!providerpackages.Any())
			{
				MstHandsetdetail mstHandsetDetail = await _dbTeleBilling_V01Context.MstHandsetdetail.FirstOrDefaultAsync(x => x.Id == id);
				mstHandsetDetail.IsDelete = true;
				mstHandsetDetail.UpdatedBy = userId;
				mstHandsetDetail.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstHandsetDetail);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteHandset, loginUserName, userId, "Handset(" + mstHandsetDetail.Name + ")", (int)EnumList.ActionTemplateTypes.Edit, mstHandsetDetail.Id); 
				return true;
			}
			return false;
		}

		#endregion
	}
}
