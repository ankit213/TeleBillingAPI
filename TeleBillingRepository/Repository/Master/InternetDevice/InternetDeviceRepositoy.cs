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

namespace TeleBillingRepository.Repository.Master.InternetDevice
{
	public class InternetDeviceRepositoy : IinternetDeviceRepositoy {
		
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public InternetDeviceRepositoy(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, ILogManagement ilogManagement,
			IStringConstant iStringConstant)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
		}
		#endregion

		#region "Public Method(s)"
		
		public async Task<List<InternetDeviceAC>> GetInternetDevices()
		{
			List<MstInternetDeviceDetail> mstInternetDeviceDetails = await _dbTeleBilling_V01Context.MstInternetDeviceDetail.Where(x => !x.IsDelete).OrderByDescending(x => x.CreatedDate).ToListAsync();
			return _mapper.Map<List<InternetDeviceAC>>(mstInternetDeviceDetails);
		}

		public async Task<InternetDeviceAC> GetInternetDeviceById(long id)
		{
			MstInternetDeviceDetail mstInternetDeviceDetail = await _dbTeleBilling_V01Context.MstInternetDeviceDetail.FirstOrDefaultAsync(x => !x.IsDelete && x.Id == id);
			return _mapper.Map<InternetDeviceAC>(mstInternetDeviceDetail);
		}

		public async Task<ResponseAC> EditInternetDevice(InternetDeviceAC internetDeviceAC, long userId)
		{
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstInternetDeviceDetail.AnyAsync(x => x.Id != internetDeviceAC.Id && x.Name.ToLower().Trim() == internetDeviceAC.Name.Trim().ToLower() && !x.IsDelete))
			{
				MstInternetDeviceDetail mstInternetDeviceDetail = await _dbTeleBilling_V01Context.MstInternetDeviceDetail.FirstOrDefaultAsync(x => x.Id == internetDeviceAC.Id && !x.IsDelete);

				#region Transaction Log Entry
				if (mstInternetDeviceDetail.TransactionId == null)
					mstInternetDeviceDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(mstInternetDeviceDetail);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstInternetDeviceDetail.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				mstInternetDeviceDetail.Name = internetDeviceAC.Name;
				mstInternetDeviceDetail.UpdatedBy = userId;
				mstInternetDeviceDetail.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstInternetDeviceDetail);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				responeAC.Message = _iStringConstant.InternetDeviceUpdateSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				responeAC.Message = _iStringConstant.InternetDeviceAlreadyExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<ResponseAC> AddInternetDevice(InternetDeviceAC internetDeviceAC, long userId)
		{
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstInternetDeviceDetail.AnyAsync(x => x.Name.ToLower().Trim() == internetDeviceAC.Name.ToLower().Trim() && !x.IsDelete))
			{
				MstInternetDeviceDetail mstInternetDeviceDetail = new MstInternetDeviceDetail();
				mstInternetDeviceDetail.Name = internetDeviceAC.Name;
				mstInternetDeviceDetail.CreatedBy = userId;
				mstInternetDeviceDetail.CreatedDate = DateTime.Now;
				mstInternetDeviceDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				await _dbTeleBilling_V01Context.AddAsync(mstInternetDeviceDetail);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				responeAC.Message = _iStringConstant.InternetDeviceAddedSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				responeAC.Message = _iStringConstant.InternetDeviceAlreadyExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<bool> DeleteInternetDevice(long id, long userId)
		{
			MstInternetDeviceDetail mstInternetDeviceDetail = await _dbTeleBilling_V01Context.MstInternetDeviceDetail.FirstOrDefaultAsync(x => x.Id == id);
			if (mstInternetDeviceDetail != null)
			{
				mstInternetDeviceDetail.IsDelete = true;
				mstInternetDeviceDetail.UpdatedBy = userId;
				mstInternetDeviceDetail.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstInternetDeviceDetail);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			return false;
		}

		#endregion
	}
}
