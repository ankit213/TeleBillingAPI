using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Dashboard
{
    public interface IDashboardRepository
    {

        /// <summary>
        /// get User Bill Data For Pie Chart based on different mobile bills
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        UserDashoboarAC getUserBillDataForPieChart(long employeeid);
		
		/// <summary>
		/// This method used for get prvider wise last closed bill details
		/// </summary>
		/// <returns></returns>
		Task<List<ProviderWiseClosedBillAC>> GetProviderWiseLastClosedBillDetails();
		

		/// <summary>
		/// This method used for get chart detail by provider 
		/// </summary>
		/// <param name="providerid"></param>
		/// <returns></returns>
		Task<List<ProviderBillChartDetailAC>> GetChartDetailByProvider(long providerid);
		
		
		/// <summary>
		/// This method used for get provider wise open bill details.
		/// </summary>
		/// <returns></returns>
		//Task<List<ProviderWiseOpenBillAC>> GetProviderWiseLastOpenBillDetails();
	}
}
