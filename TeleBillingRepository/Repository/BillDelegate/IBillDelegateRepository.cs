using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.BillDelegate
{
    public interface IBillDelegateRepository
    {
        /// <summary>
		/// This method used for get all delegate user list
		/// </summary>
		/// <returns></returns>
		Task<List<BillDelegatesListAC>> GetDelegates();

        /// <summary>
		/// This method used for get Delegate detail by id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<BillDelegatesAC> GetDelegateById(long id);

		/// <summary>
		/// This method used for edit Delegate detail
		/// </summary>
		/// <param name="BillDelegatesAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> EditDelegate(BillDelegatesAC BillDelegatesAC, long userId, string loginUserName);

		/// <summary>
		/// Thismethod used for add new Delegate detail
		/// </summary>
		/// <param name="BillDelegatesAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> AddDelegate(BillDelegatesAC BillDelegatesAC, long userId, string loginUserName);

		/// <summary>
		/// This method used fo delete Delegate detail
		/// </summary>
		/// <param name="id"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> DeleteDelegate(long id, long userId, string loginUserName);

        Task<ResponseAC> checkDelegatePair(EmployeeAC Employee, EmployeeAC DelegateEmployee, long delegateid = 0);

        Task<ResponseAC> checkIsEmployeeCanDelegated(EmployeeAC Employee,long delegateid = 0);

        Task<ResponseAC> checkIsEmployeeNotDelegatedToOther(EmployeeAC DelegateToEmployee);
    }
}
