using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Provider
{
	public class ProviderRepository : IProviderRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		private readonly DALMySql _objDalmysql = new DALMySql();
		#endregion

		#region "Constructor"
		public ProviderRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
			ILogManagement iLogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<ProviderListAC>> GetProviders()
		{
			List<ProviderListAC> listOfProviderAC = new List<ProviderListAC>();
			List<TeleBillingUtility.Models.Provider> providerList = await _dbTeleBilling_V01Context.Provider.Where(x => !x.IsDelete).Include(x => x.Country).Include(x => x.Country.Currency).OrderByDescending(x => x.Id).ToListAsync();
			foreach (var item in providerList)
			{
				ProviderListAC providerListAC = new ProviderListAC();
				providerListAC = _mapper.Map<ProviderListAC>(item);
				List<Providerservice> listOfServiceType = await _dbTeleBilling_V01Context.Providerservice.Where(x => x.ProviderId == item.Id && !x.IsDelete).Include(x => x.ServiceType).ToListAsync();

				string serviceTypes = string.Empty;
				if (listOfServiceType.Any())
				{
					foreach (var providerService in listOfServiceType)
					{
						serviceTypes += providerService.ServiceType.Name + ",";
					}
					serviceTypes = serviceTypes.Substring(0, serviceTypes.Length - 1);
				}
				providerListAC.ServiceTypes = serviceTypes;
				listOfProviderAC.Add(providerListAC);
			}
			return listOfProviderAC;
		}

		public async Task<ResponseAC> AddProvider(long userId, ProviderAC providerAC, string loginUserName)
		{
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Provider.AnyAsync(x => x.ContractNumber.ToLower() == providerAC.ContractNumber.ToLower() && !x.IsDelete))
			{
				TeleBillingUtility.Models.Provider provider = new TeleBillingUtility.Models.Provider();
				provider = _mapper.Map<TeleBillingUtility.Models.Provider>(providerAC);
				provider.IsActive = true;
				provider.CreatedDate = DateTime.Now;
				provider.CreatedBy = userId;
				provider.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				await _dbTeleBilling_V01Context.AddAsync(provider);
				await _dbTeleBilling_V01Context.SaveChangesAsync();


				foreach (var serviceType in providerAC.ServiceTypes)
				{
					Providerservice providerService = new Providerservice();
					providerService.ProviderId = provider.Id;
					providerService.ServiceTypeId = serviceType.Id;
					await _dbTeleBilling_V01Context.AddAsync(providerService);
					await _dbTeleBilling_V01Context.SaveChangesAsync();
				}

				#region Added Provider Contact Detail
				List<Providercontactdetail> providerContactDetails = new List<Providercontactdetail>();
				foreach (var item in providerAC.ProviderContactDetailACList)
				{
					Providercontactdetail providerContectDatail = new Providercontactdetail();
					providerContectDatail = _mapper.Map<Providercontactdetail>(item);
					providerContectDatail.ProviderId = provider.Id;
					providerContactDetails.Add(providerContectDatail);
				}

				await _dbTeleBilling_V01Context.AddRangeAsync(providerContactDetails);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				#endregion

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddProvider, loginUserName, userId, "Provider(" + provider.Name + ")", (int)EnumList.ActionTemplateTypes.Add, provider.Id);
				responeAC.Message = _iStringConstant.ProviderAddedSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				responeAC.Message = _iStringConstant.ContractNumberExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<ResponseAC> UpdateProvider(long userId, ProviderAC providerAC, string loginUserName)
		{

			ResponseAC responeAC = new ResponseAC();
			if (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id != providerAC.Id && x.ContractNumber.ToLower() == providerAC.ContractNumber.ToLower() && !x.IsDelete) == null)
			{

				TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == providerAC.Id);
				if (provider != null)
				{

					#region Transaction Log Entry
					if (provider.TransactionId == null)
						provider.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

					var jsonSerailzeObj = JsonConvert.SerializeObject(provider);
					await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(provider.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
					#endregion

					provider = _mapper.Map(providerAC, provider);
					provider.UpdatedBy = userId;
					provider.UpdatedDate = DateTime.Now;

					_dbTeleBilling_V01Context.Update(provider);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					#region Update Provider Service List
					List<Providerservice> providerServiceList = await _dbTeleBilling_V01Context.Providerservice.Where(x => x.ProviderId == providerAC.Id && !x.IsDelete).ToListAsync();

					if (providerServiceList.Any())
					{
						List<Providerservice> dummyProviderServices = new List<Providerservice>();
						foreach (var item in providerServiceList)
						{
							item.IsDelete = true;
							dummyProviderServices.Add(item);
						}

						_dbTeleBilling_V01Context.UpdateRange(dummyProviderServices);
						_dbTeleBilling_V01Context.SaveChanges();

					}

					providerServiceList = new List<Providerservice>();
					foreach (var serviceType in providerAC.ServiceTypes)
					{
						Providerservice providerService = new Providerservice();
						providerService.ServiceTypeId = serviceType.Id;
						providerService.ProviderId = providerAC.Id;
						providerServiceList.Add(providerService);
					}

					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditProvider, loginUserName, userId, "Provider(" + provider.Name + ")", (int)EnumList.ActionTemplateTypes.Edit, provider.Id);
					await _dbTeleBilling_V01Context.AddRangeAsync(providerServiceList);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					#endregion

					#region Update Provider Contact Details 
					List<Providercontactdetail> providerContactDetails = await _dbTeleBilling_V01Context.Providercontactdetail.Where(x => x.ProviderId == provider.Id && !x.IsDeleted).ToListAsync();
					if (providerContactDetails.Any())
					{
						List<Providercontactdetail> providerContactDetailList = new List<Providercontactdetail>();
						foreach (var providerContact in providerContactDetails)
						{
							providerContact.IsDeleted = true;
							providerContactDetailList.Add(providerContact);
						}
					}


					#region Added Provider Contact Detail
					providerContactDetails = new List<Providercontactdetail>();
					foreach (var item in providerAC.ProviderContactDetailACList)
					{
						Providercontactdetail providerContectDatail = new Providercontactdetail();
						providerContectDatail = _mapper.Map<Providercontactdetail>(item);
						providerContectDatail.Id = 0;
						providerContectDatail.ProviderId = provider.Id;
						providerContactDetails.Add(providerContectDatail);
					}

					await _dbTeleBilling_V01Context.AddRangeAsync(providerContactDetails);
					await _dbTeleBilling_V01Context.SaveChangesAsync();
					#endregion

					#endregion

					responeAC.Message = _iStringConstant.ProviderUpdatedSuccessfully;
					responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				}
			}
			else
			{
				responeAC.Message = _iStringConstant.ContractNumberExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<List<DrpResponseAC>> ProviderList()
		{
			List<TeleBillingUtility.Models.Provider> lst = new List<TeleBillingUtility.Models.Provider>();
			lst = await _dbTeleBilling_V01Context.Provider.Where(x => x.IsActive && !x.IsDelete).OrderByDescending(x => x.Id).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lst);
		}

		public async Task<List<DrpResponseAC>> AllProviderList()
		{
			List<TeleBillingUtility.Models.Provider> lst = new List<TeleBillingUtility.Models.Provider>();
			lst = await _dbTeleBilling_V01Context.Provider.Where(x => !x.IsDelete).OrderByDescending(x => x.Id).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lst);
		}

		public async Task<ProviderAC> GetProviderById(long id)
		{
			ProviderAC providerAC = new ProviderAC();
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstAsync(x => x.Id == id);
			providerAC = _mapper.Map<ProviderAC>(provider);

			List<Providerservice> listOfService = await _dbTeleBilling_V01Context.Providerservice.Where(x => x.ProviderId == id && !x.IsDelete).Include(x => x.ServiceType).ToListAsync();
			providerAC.ServiceTypes = new List<DrpResponseAC>();
			foreach (var item in listOfService)
			{
				DrpResponseAC serviceType = new DrpResponseAC();
				serviceType.Id = item.ServiceTypeId;
				serviceType.Name = item.ServiceType.Name;

				providerAC.ServiceTypes.Add(serviceType);
			}

			List<Providercontactdetail> providerContactDetails = await _dbTeleBilling_V01Context.Providercontactdetail.Where(x => x.ProviderId == provider.Id && !x.IsDeleted).ToListAsync();
			providerAC.ProviderContactDetailACList = new List<ProviderContactDetailAC>();
			providerAC.ProviderContactDetailACList = _mapper.Map(providerContactDetails, providerAC.ProviderContactDetailACList);

			return providerAC;
		}

		public async Task<bool> DeleteProvider(long userId, long id, string loginUserName)
		{
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == id);

			SortedList sl = new SortedList();
			sl.Add("p_ProviderId", id);
			int result = Convert.ToInt16(_objDalmysql.ExecuteScaler("usp_GetProviderExists", sl));
			if (result == 0)
			{
				provider.IsDelete = true;
				provider.UpdatedBy = userId;
				provider.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(provider);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteProvider, loginUserName, userId, "Provider(" + provider.Name + ")", (int)EnumList.ActionTemplateTypes.Delete, provider.Id);
				return true;
			}
			return false;
		}

		public async Task<bool> ChangeProviderStatus(long userId, long id, string loginUserName)
		{
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == id);
			if (provider != null)
			{
				#region Transaction Log Entry
				if (provider.TransactionId == null)
					provider.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(provider);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(provider.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
				#endregion

				provider.IsActive = !provider.IsActive;
				provider.UpdatedBy = userId;
				provider.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(provider);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				if (provider.IsActive)
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ActiveProvider, loginUserName, userId, "Provider(" + provider.Name + ")", (int)EnumList.ActionTemplateTypes.Active, provider.Id);
				else
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeactiveProvider, loginUserName, userId, "Provider(" + provider.Name + ")", (int)EnumList.ActionTemplateTypes.Deactive, provider.Id);

				return true;
			}
			return false;
		}

		#endregion
	}
}
