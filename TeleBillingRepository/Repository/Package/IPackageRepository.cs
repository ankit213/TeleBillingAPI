using System;
using System.Collections.Generic;
using System.Text;
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
		/// <returns></returns>
		Task<ResponseAC> AddPackage(long userId, PackageDetailAC packageDetailAC);

		/// <summary>
		/// This method used for delete package
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeletePackage(long userId, long id);

		/// <summary>
		/// This method used for change package status
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> ChangePackageStatus(long userId, long id);
		
		
		/// <summary>
		/// This method used for get package detail by id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<PackageDetailAC> GetPackageById(long id);
	}
}
