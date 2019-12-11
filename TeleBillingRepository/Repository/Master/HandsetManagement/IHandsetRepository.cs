using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Master.HandsetManagement
{
	public interface IHandsetRepository {

		/// <summary>
		/// This method used for get handset details for drop down
		/// </summary>
		/// <returns></returns>
		Task<List<DrpResponseAC>> GetHandsetList();

		/// <summary>
		/// This method used for get handset detail list
		/// </summary>
		/// <returns></returns>
		Task<List<HandsetDetailAC>> GetHandsets();


		/// <summary>
		/// This method used for get handset detail by id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<HandsetDetailAC> GetHandsetById(long id);

		/// <summary>
		/// This method used for edit handset detail
		/// </summary>
		/// <param name="handsetDetailAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> EditHandset(HandsetDetailAC handsetDetailAC, long userId);

		/// <summary>
		/// Thismethod used for add new handset detail
		/// </summary>
		/// <param name="handsetDetailAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> AddHandset(HandsetDetailAC handsetDetailAC, long userId, string loginUserName);

		/// <summary>
		/// This method used fo delete handset detail
		/// </summary>
		/// <param name="id"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> DeleteHandset(long id, long userId, string loginUserName);
	}
}
