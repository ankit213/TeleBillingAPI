using System.Threading.Tasks;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Account
{
	public interface IAccountRepository
	{

		/// <summary>
		/// This method used for get employee by pf number or email.
		/// </summary>
		/// <param name="emailOrPfNumber"></param>
		/// <returns></returns>
	   	Task<MstEmployee> GetEmployeeBy(string emailOrPfNumber);

        /// <summary>
		/// This method used for get employee by pf number or email even if it is inactive.
		/// </summary>
		/// <param name="emailOrPfNumber"></param>
		/// <returns></returns>
        Task<MstEmployee> checkEmployeeIsActive(string emailOrPfNumber);


        /// <summary>
        /// This method used for check user credentail is valid or not.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> CheckUserCredentail(string email, string pfnumber, string password);

        Task<string> GetLineManagerEmail(string UserId);



    }
}
