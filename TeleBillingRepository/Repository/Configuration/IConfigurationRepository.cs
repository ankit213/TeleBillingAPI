using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Configuration
{
	public interface IConfigurationRepository
	{
		/// <summary>
		/// This method usedfor add/update configuration
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		Task<ResponseAC> AddConfiguration(long userId, TeleBillingUtility.Models.Configuration configuration);
		
		
		/// <summary>
		/// This method used for get configuration 
		/// </summary>
		/// <returns></returns>
		Task<TeleBillingUtility.Models.Configuration> GetConfiguration();

		/// <summary>
		/// This fucntion used for get provider wise transaction list
		/// </summary>
		/// <returns></returns>
		Task<List<ProviderWiseTransactionAC>> GetProviderWiseTransaction();

		/// <summary>
		/// This method sued for add provider wise transaction 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="providerWiseTransactionAC"></param>
		/// <returns></returns>
		Task<ResponseAC> AddProviderWiseTransaction(long userId, ProviderWiseTransactionAC providerWiseTransactionAC);

		/// <summary>
		///  This method sued for update provider wise transaction 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="providerWiseTransactionAC"></param>
		/// <returns></returns>
		Task<ResponseAC> UpdateProviderWiseTransaction(long userId, ProviderWiseTransactionAC providerWiseTransactionAC);

		/// <summary>
		/// This method sued for delete provider wise transaction 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteProviderWiseTransaction(long userId, long id);

		/// <summary>
		/// This method sued for get provider wise transaction 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<ProviderWiseTransactionAC> GetProviderWiseTransactionById(long id);


		/// <summary>
		/// This method used for bulk upload prvider wise transaction type.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="exceluploadDetail"></param>
		/// <param name="providerId"></param>
		/// <returns></returns>
		Task<BulkAssignTelephoneResponseAC> BulkUploadProviderWiseTrans(long userId, ExcelUploadResponseAC exceluploadDetail, long providerId);
		
		/// <summary>
		/// This method used for update transaction setting
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="providerWiseTransactionAC"></param>
		/// <returns></returns>
		Task<ResponseAC> UpdateTransactionTypeSetting(long userId, ProviderWiseTransactionAC providerWiseTransactionAC);
	}
}
