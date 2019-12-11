using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Master.InternetDevice
{

	public interface IinternetDeviceRepositoy  {

		/// <summary>
		/// This method used for get internet device list
		/// </summary>
		/// <returns></returns>
		Task<List<InternetDeviceAC>> GetInternetDevices();


		/// <summary>
		/// This method used for get internet device detail by id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<InternetDeviceAC> GetInternetDeviceById(long id);

		/// <summary>
		/// This method used for edit internet device detail
		/// </summary>
		/// <param name="internetDeviceAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> EditInternetDevice(InternetDeviceAC internetDeviceAC, long userId);

		/// <summary>
		/// Thismethod used for add new internet device detail
		/// </summary>
		/// <param name="internetDeviceAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> AddInternetDevice(InternetDeviceAC internetDeviceAC, long userId,string loginUserName);

		/// <summary>
		/// This method used fo delete internet device detail
		/// </summary>
		/// <param name="id"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> DeleteInternetDevice(long id, long userId, string loginUserName);
	}
}
