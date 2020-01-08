using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Package
{
    public interface IPackageRepository
    {
        /// <summary>
        /// This method used for get package list
        /// </summary>
        /// <returns></returns>
        Task<List<PackageAC>> GetPackageList();

        /// <summary>
        /// This method used for add package 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="packageDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> AddPackage(long userId, PackageDetailAC packageDetailAC, string loginUserName);

        /// <summary>
        /// This method used for delete package
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> DeletePackage(long userId, long id, string loginUserName);

        /// <summary>
        /// This method used for change package status
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> ChangePackageStatus(long userId, long id, string loginUserName);


        /// <summary>
        /// This method used for get package detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<PackageDetailAC> GetPackageById(long id);
    }
}
