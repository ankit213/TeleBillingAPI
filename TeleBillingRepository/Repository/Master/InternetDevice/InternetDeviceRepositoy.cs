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

namespace TeleBillingRepository.Repository.Master.InternetDevice
{
    public class InternetDeviceRepositoy : IinternetDeviceRepositoy
    {

        #region "Private Variable(s)"
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
        #endregion

        #region "Constructor"
        public InternetDeviceRepositoy(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, ILogManagement ilogManagement,
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
            List<MstInternetdevicedetail> mstInternetDeviceDetails = await _dbTeleBilling_V01Context.MstInternetdevicedetail.Where(x => !x.IsDelete).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<InternetDeviceAC>>(mstInternetDeviceDetails);
        }

        public async Task<InternetDeviceAC> GetInternetDeviceById(long id)
        {
            MstInternetdevicedetail mstInternetDeviceDetail = await _dbTeleBilling_V01Context.MstInternetdevicedetail.FirstOrDefaultAsync(x => !x.IsDelete && x.Id == id);
            return _mapper.Map<InternetDeviceAC>(mstInternetDeviceDetail);
        }

        public async Task<ResponseAC> EditInternetDevice(InternetDeviceAC internetDeviceAC, long userId)
        {
            ResponseAC responeAC = new ResponseAC();
            if (!await _dbTeleBilling_V01Context.MstInternetdevicedetail.AnyAsync(x => x.Id != internetDeviceAC.Id && x.Name.ToLower().Trim() == internetDeviceAC.Name.Trim().ToLower() && !x.IsDelete))
            {
                MstInternetdevicedetail mstInternetDeviceDetail = await _dbTeleBilling_V01Context.MstInternetdevicedetail.FirstOrDefaultAsync(x => x.Id == internetDeviceAC.Id && !x.IsDelete);

                #region Transaction Log Entry
                if (mstInternetDeviceDetail.TransactionId == null)
                    mstInternetDeviceDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                var jsonSerailzeObj = JsonConvert.SerializeObject(mstInternetDeviceDetail);
                await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstInternetDeviceDetail.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
                #endregion

                mstInternetDeviceDetail.Name = internetDeviceAC.Name.Trim();
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

        public async Task<ResponseAC> AddInternetDevice(InternetDeviceAC internetDeviceAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();
            if (!await _dbTeleBilling_V01Context.MstInternetdevicedetail.AnyAsync(x => x.Name.ToLower().Trim() == internetDeviceAC.Name.ToLower().Trim() && !x.IsDelete))
            {
                MstInternetdevicedetail mstInternetDeviceDetail = new MstInternetdevicedetail();
                mstInternetDeviceDetail.Name = internetDeviceAC.Name.Trim();
                mstInternetDeviceDetail.CreatedBy = userId;
                mstInternetDeviceDetail.CreatedDate = DateTime.Now;
                mstInternetDeviceDetail.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                await _dbTeleBilling_V01Context.AddAsync(mstInternetDeviceDetail);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                responeAC.Message = _iStringConstant.InternetDeviceAddedSuccessfully;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddInternetDevice, loginUserName, userId, "Internet device(" + mstInternetDeviceDetail.Name + ")", (int)EnumList.ActionTemplateTypes.Add, mstInternetDeviceDetail.Id);
            }
            else
            {
                responeAC.Message = _iStringConstant.InternetDeviceAlreadyExists;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
            }
            return responeAC;
        }

        public async Task<bool> DeleteInternetDevice(long id, long userId, string loginUserName)
        {
            List<Providerpackage> providerpackages = await _dbTeleBilling_V01Context.Providerpackage.Where(x => x.InternetDeviceId == id && x.IsActive && !x.IsDelete).ToListAsync();
            if (!providerpackages.Any())
            {
                MstInternetdevicedetail mstInternetDeviceDetail = await _dbTeleBilling_V01Context.MstInternetdevicedetail.FirstOrDefaultAsync(x => x.Id == id);
                mstInternetDeviceDetail.IsDelete = true;
                mstInternetDeviceDetail.UpdatedBy = userId;
                mstInternetDeviceDetail.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(mstInternetDeviceDetail);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteInternetDevice, loginUserName, userId, "Internet device(" + mstInternetDeviceDetail.Name + ")", (int)EnumList.ActionTemplateTypes.Delete, mstInternetDeviceDetail.Id);
                return true;
            }
            return false;
        }

        #endregion
    }
}
