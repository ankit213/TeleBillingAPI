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

namespace TeleBillingRepository.Repository.Package
{
	public class PackageRepository : IPackageRepository
	{
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private readonly IMapper _mapper;
		#endregion

		#region "Constructor"
		public PackageRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
			ILogManagement iLogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<PackageAC>> GetPackageList() {
			List<ProviderPackage> lstProviderPackage = await _dbTeleBilling_V01Context.ProviderPackage.Where(x=>!x.IsDelete).Include(x=>x.Provider).Include(x=>x.ServiceType).OrderByDescending(x=>x.CreatedDate).ToListAsync();
			return _mapper.Map<List<PackageAC>>(lstProviderPackage);
		}


		public async Task<ResponseAC> AddPackage(long userId, PackageDetailAC packageDetailAC) {
			ResponseAC responseAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.ProviderPackage.AnyAsync(x => x.Name.ToLower() == packageDetailAC.Name.ToLower() && !x.IsDelete)) {
			
				ProviderPackage providerPackage = new ProviderPackage();
				providerPackage = _mapper.Map<ProviderPackage>(packageDetailAC);
				providerPackage.CreatedBy = userId;
				providerPackage.IsActive = true;
				providerPackage.CreatedDate = DateTime.Now;
				providerPackage.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
			
				await _dbTeleBilling_V01Context.AddAsync(providerPackage);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.PackageAddedSuccessfully;
			}
			else
			{
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
				responseAC.Message = _iStringConstant.PackageAlreadyExists;
			}
			return responseAC;
		}

		public async Task<bool> DeletePackage(long userId, long id) {
			ProviderPackage providerPackage = await _dbTeleBilling_V01Context.ProviderPackage.FirstOrDefaultAsync(x => x.Id == id);
			if (providerPackage != null)
			{
				providerPackage.IsDelete = true;
				providerPackage.UpdatedBy = userId;
				providerPackage.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(providerPackage);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			return false;

		}

		public async Task<bool> ChangePackageStatus(long userId, long id) {
			ProviderPackage providerPackage = await _dbTeleBilling_V01Context.ProviderPackage.FirstOrDefaultAsync(x => x.Id == id);
			if (providerPackage != null)
			{
				#region Transaction Log Entry
				if (providerPackage.TransactionId == null)
					providerPackage.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(providerPackage);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(providerPackage.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
				#endregion

				providerPackage.IsActive = !providerPackage.IsActive;
				providerPackage.UpdatedBy = userId;
				providerPackage.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(providerPackage);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			return false;
		}
		
		public async Task<PackageDetailAC> GetPackageById(long id) {

			ProviderPackage providerPackage = await _dbTeleBilling_V01Context.ProviderPackage.FirstOrDefaultAsync(x=>x.Id == id);
			PackageDetailAC packageDetailAC = _mapper.Map<PackageDetailAC>(providerPackage);

			if (!string.IsNullOrEmpty(packageDetailAC.HandsetDetailIds))
			{
				packageDetailAC.HandsetList = new List<DrpResponseAC>();
				List<string> lstHandsetIds= packageDetailAC.HandsetDetailIds.Split(',').ToList(); 
				foreach(string handsetId in lstHandsetIds) {
					DrpResponseAC drpResponseAC = new DrpResponseAC();
					long newHandsetId = Convert.ToInt64(handsetId);
					MstHandsetDetail handsetDetail = await _dbTeleBilling_V01Context.MstHandsetDetail.FirstOrDefaultAsync(x=>x.Id == newHandsetId && !x.IsDelete);
					drpResponseAC.Id = handsetDetail.Id;
					drpResponseAC.Name = handsetDetail.Name;
					packageDetailAC.HandsetList.Add(drpResponseAC);
				}
			}
			return packageDetailAC;
		}
		#endregion
	}
}
