using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Provider
{
    public interface IProviderRepository
    {

        /// <summary>
        /// This method used for get provider list 
        /// </summary>
        /// <returns></returns>
        Task<List<ProviderListAC>> GetProviders();

        /// <summary>
        /// This method used for add new provider 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="providerAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> AddProvider(long userId, ProviderAC providerAC, string loginUserName);


        /// <summary>
        /// This method used for get provider list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> ProviderList();

        /// <summary>
        /// This method used for get provider by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProviderAC> GetProviderById(long id);

        /// <summary>
        /// This method used for delete exists provider
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> DeleteProvider(long userId, long id, string loginUserName);


        /// <summary>
        /// This method used for get all(active/inactive) provider list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> AllProviderList();


        /// <summary>
        /// This method used for change exists provider status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> ChangeProviderStatus(long userId, long id, string loginUserName);

        /// <summary>
        /// This method used for add new provider 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="providerAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> UpdateProvider(long userId, ProviderAC providerAC, string loginUserName);

    }
}