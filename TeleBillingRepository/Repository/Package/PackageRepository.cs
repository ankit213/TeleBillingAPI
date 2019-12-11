using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Package
{
	public class PackageRepository : IPackageRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context  _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private readonly IMapper _mapper;
		private readonly DALMySql _objDalmysql = new DALMySql();
		#endregion

		#region "Constructor"
		public PackageRepository(telebilling_v01Context  dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
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
			List<Providerpackage> lstProviderPackage = await _dbTeleBilling_V01Context.Providerpackage.Where(x=>!x.IsDelete).Include(x=>x.Provider).Include(x=>x.ServiceType).OrderByDescending(x=>x.Id).ToListAsync();
			return _mapper.Map<List<PackageAC>>(lstProviderPackage);
		}


		public async Task<ResponseAC> AddPackage(long userId, PackageDetailAC packageDetailAC, string loginUserName) {
			ResponseAC responseAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Providerpackage.AnyAsync(x => x.Name.ToLower() == packageDetailAC.Name.ToLower() && !x.IsDelete)) {
			
				Providerpackage providerPackage = new Providerpackage();
				providerPackage = _mapper.Map<Providerpackage>(packageDetailAC);
				providerPackage.CreatedBy = userId;
				providerPackage.IsActive = true;
				providerPackage.CreatedDate = DateTime.Now;
				providerPackage.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
			
				await _dbTeleBilling_V01Context.AddAsync(providerPackage);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddPackage, loginUserName, userId, "Package(" + providerPackage.Name + ")", (int)EnumList.ActionTemplateTypes.Add, providerPackage.Id);

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

		public async Task<bool> DeletePackage(long userId, long id, string loginUserName) {
			Providerpackage providerPackage = await _dbTeleBilling_V01Context.Providerpackage.FirstOrDefaultAsync(x => x.Id == id);
			SortedList sl = new SortedList();
			sl.Add("p_packageid", id);
			int result = Convert.ToInt16(_objDalmysql.ExecuteScaler("usp_GetPackageExists",sl));
			if (result == 0)
			{
				providerPackage.IsDelete = true;
				providerPackage.UpdatedBy = userId;
				providerPackage.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(providerPackage);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeletePackage, loginUserName, userId, "Package(" + providerPackage.Name + ")", (int)EnumList.ActionTemplateTypes.Delete, providerPackage.Id);
				return true;
			}
			return false;
		}

		public async Task<bool> ChangePackageStatus(long userId, long id, string loginUserName) {
			Providerpackage providerPackage = await _dbTeleBilling_V01Context.Providerpackage.FirstOrDefaultAsync(x => x.Id == id);
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

				if(providerPackage.IsActive)
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ActivePackage, loginUserName, userId, "Package(" + providerPackage.Name + ")", (int)EnumList.ActionTemplateTypes.Active, providerPackage.Id);
				else
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeactivePackage, loginUserName, userId, "Package(" + providerPackage.Name + ")", (int)EnumList.ActionTemplateTypes.Deactive, providerPackage.Id);

				return true;
			}
			return false;
		}
		
		public async Task<PackageDetailAC> GetPackageById(long id) {

			Providerpackage providerPackage = await _dbTeleBilling_V01Context.Providerpackage.FirstOrDefaultAsync(x=>x.Id == id);
			PackageDetailAC packageDetailAC = _mapper.Map<PackageDetailAC>(providerPackage);

			if (!string.IsNullOrEmpty(packageDetailAC.HandsetDetailIds))
			{
				packageDetailAC.HandsetList = new List<DrpResponseAC>();
				List<string> lstHandsetIds= packageDetailAC.HandsetDetailIds.Split(',').ToList(); 
				foreach(string handsetId in lstHandsetIds) {
					DrpResponseAC drpResponseAC = new DrpResponseAC();
					long newHandsetId = Convert.ToInt64(handsetId);
					MstHandsetdetail handsetDetail = await _dbTeleBilling_V01Context.MstHandsetdetail.FirstOrDefaultAsync(x=>x.Id == newHandsetId && !x.IsDelete);
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
