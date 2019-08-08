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

namespace TeleBillingRepository.Repository.Provider
{
	public class ProviderRepository : IProviderRepository
	{
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public ProviderRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
			ILogManagement iLogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion

		#region Public Method(s)
		
		public async Task<List<ProviderListAC>> GetProviders() {
			List<ProviderListAC> listOfProviderAC = new List<ProviderListAC>();
			List<TeleBillingUtility.Models.Provider> providerList = await _dbTeleBilling_V01Context.Provider.Where(x=>!x.IsDelete).Include(x=>x.Country).Include(x=>x.Country.Currency).OrderByDescending(x=>x.CreatedDate).ToListAsync();
			foreach (var item in providerList)
			{
				ProviderListAC providerListAC = new ProviderListAC();
				providerListAC = _mapper.Map<ProviderListAC>(item);
				List<ProviderService> listOfServiceType = await _dbTeleBilling_V01Context.ProviderService.Where(x=>x.ProviderId == item.Id && !x.IsDelete).Include(x=>x.ServiceType).ToListAsync();
				
				string serviceTypes = string.Empty;
				if (listOfServiceType.Any()) { 
					foreach(var providerService in listOfServiceType) {
						serviceTypes += providerService.ServiceType.Name + ",";
					}
					serviceTypes = serviceTypes.Substring(0, serviceTypes.Length-1);
				}
				providerListAC.ServiceTypes = serviceTypes;
				listOfProviderAC.Add(providerListAC);
			}
			return listOfProviderAC;
		}
		
		public async Task<ResponseAC> AddProvider(long userId, ProviderAC providerAC) {
			ResponseAC responeAC= new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Provider.AnyAsync(x=>x.ContractNumber.ToLower() == providerAC.ContractNumber.ToLower() && !x.IsDelete)) {
					TeleBillingUtility.Models.Provider provider = new TeleBillingUtility.Models.Provider();
					provider = _mapper.Map<TeleBillingUtility.Models.Provider>(providerAC);
					provider.IsActive = true;
					provider.CreatedDate = DateTime.Now;
					provider.CreatedBy =  userId;
				    provider.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

					 await  _dbTeleBilling_V01Context.AddAsync(provider);
					 await _dbTeleBilling_V01Context.SaveChangesAsync();
							

					foreach (var serviceType in providerAC.ServiceTypes) {
						ProviderService providerService = new ProviderService();
						providerService.ProviderId = provider.Id;
						providerService.ServiceTypeId = serviceType.Id;
						await _dbTeleBilling_V01Context.AddAsync(providerService);
						await _dbTeleBilling_V01Context.SaveChangesAsync();
					}

					#region Added Provider Contact Detail
						List<ProviderContactDetail> providerContactDetails = new List<ProviderContactDetail>();
							foreach(var item in providerAC.ProviderContactDetailACList) {
								ProviderContactDetail providerContectDatail = new ProviderContactDetail();
								providerContectDatail = _mapper.Map<ProviderContactDetail>(item);
								providerContectDatail.ProviderId = provider.Id;
								providerContactDetails.Add(providerContectDatail);
							}

							await _dbTeleBilling_V01Context.AddRangeAsync(providerContactDetails);
							await _dbTeleBilling_V01Context.SaveChangesAsync();
						#endregion

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
		
		public async Task<ResponseAC> UpdateProvider(long userId, ProviderAC providerAC) {

			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Provider.AnyAsync(x =>x.Id != providerAC.Id && x.ContractNumber.ToLower() == providerAC.ContractNumber.ToLower() && !x.IsDelete)) {

				TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x=>x.Id == providerAC.Id);
				if (provider != null) {
					
					#region Transaction Log Entry
					if (provider.TransactionId == null)
						provider.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

					var jsonSerailzeObj = JsonConvert.SerializeObject(provider);
					await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(provider.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
					#endregion
				
					provider = _mapper.Map(providerAC,provider);
					provider.UpdatedBy = userId;
					provider.UpdatedDate = DateTime.Now;

					_dbTeleBilling_V01Context.Update(provider);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					#region Update Provider Service List
						List<ProviderService> providerServiceList = await _dbTeleBilling_V01Context.ProviderService.Where(x=>x.ProviderId == providerAC.Id && !x.IsDelete).ToListAsync();
					
						if (providerServiceList.Any()) {
							List<ProviderService> dummyProviderServices = new List<ProviderService>();
							foreach (var item in providerServiceList) {
								item.IsDelete = true;
								dummyProviderServices.Add(item);
							}

							_dbTeleBilling_V01Context.UpdateRange(dummyProviderServices);
							_dbTeleBilling_V01Context.SaveChanges();

						}

						providerServiceList = new List<ProviderService>();
						foreach (var serviceType in providerAC.ServiceTypes) {
							ProviderService providerService = new ProviderService();
							providerService.ServiceTypeId = serviceType.Id;
							providerService.ProviderId = providerAC.Id;
							providerServiceList.Add(providerService);
						}
						await _dbTeleBilling_V01Context.AddRangeAsync(providerServiceList);
						await _dbTeleBilling_V01Context.SaveChangesAsync();

					#endregion

					#region Update Provider Contact Details 
						List<ProviderContactDetail> providerContactDetails = await _dbTeleBilling_V01Context.ProviderContactDetail.Where(x => x.ProviderId == provider.Id && !x.IsDeleted).ToListAsync();
						if (providerContactDetails.Any())
						{
							List<ProviderContactDetail> providerContactDetailList = new List<ProviderContactDetail>();
							foreach (var providerContact in providerContactDetails)
							{
								providerContact.IsDeleted = true;
								providerContactDetailList.Add(providerContact);
							}
						}
						

						#region Added Provider Contact Detail
							providerContactDetails = new List<ProviderContactDetail>();
							foreach (var item in providerAC.ProviderContactDetailACList)
							{
								ProviderContactDetail providerContectDatail = new ProviderContactDetail();
								providerContectDatail = _mapper.Map<ProviderContactDetail>(item);
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

        public async Task<List<DrpResponseAC>> ProviderList() {
            List<TeleBillingUtility.Models.Provider> lst = new List<TeleBillingUtility.Models.Provider>();
            lst = await _dbTeleBilling_V01Context.Provider.Where(x => x.IsActive == true && !x.IsDelete).OrderBy(x => x.Id).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lst);
        }

		public async Task<ProviderAC> GetProviderById(long id)
		{
			ProviderAC providerAC = new ProviderAC();
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstAsync(x=>x.Id == id);
		    providerAC= _mapper.Map<ProviderAC>(provider);

			List<ProviderService> listOfService = await _dbTeleBilling_V01Context.ProviderService.Where(x=>x.ProviderId == id && !x.IsDelete).Include(x=>x.ServiceType).ToListAsync();
			providerAC.ServiceTypes = new List<DrpResponseAC>();
			foreach (var item in listOfService) {
				DrpResponseAC serviceType = new DrpResponseAC();
				serviceType.Id = item.ServiceTypeId;
				serviceType.Name = item.ServiceType.Name;

				providerAC.ServiceTypes.Add(serviceType);
			}

			List<ProviderContactDetail> providerContactDetails = await _dbTeleBilling_V01Context.ProviderContactDetail.Where(x=>x.ProviderId == provider.Id && !x.IsDeleted).ToListAsync();
			providerAC.ProviderContactDetailACList = new List<ProviderContactDetailAC>();
			providerAC.ProviderContactDetailACList = _mapper.Map(providerContactDetails,providerAC.ProviderContactDetailACList);
			
			return providerAC;
		}
		
		public async Task<bool> DeleteProvider(long userId, long id) {
			TeleBillingUtility.Models.Provider provider = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == id);
			if (provider != null) {
				provider.IsDelete = true;
				provider.UpdatedBy = userId;
				provider.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(provider);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			return false;
		}

		public async Task<bool> ChangeProviderStatus(long userId, long id) {
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
				return true;
			}
			return false;
		}

		#endregion
	}
}
