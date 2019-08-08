using TeleBillingUtility.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace TeleBillingRepository.Repository.Account
{
	public class AccountRepository : IAccountRepository
	{
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		#endregion

		#region "Constructor"

		public AccountRepository(TeleBilling_V01Context dbTeleBilling_V01Context)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
		}
		#endregion

		#region "Public Method(s)"

		public async Task<MstEmployee> GetEmployeeBy(string emailOrPfNumber)
		{
             MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.EmailId.Trim() == emailOrPfNumber.Trim());
			if (mstEmployee == null)
			{
				mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.EmpPfnumber.Trim() == emailOrPfNumber.Trim());
			}
			return mstEmployee;
		}

		public async Task<bool> CheckUserCredentail(string email, string password) {
			string encryptPassword = password;
			return await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x=>x.EmailId.Trim() == email.Trim() && x.Password.Trim() == encryptPassword.Trim() && !x.IsDelete) != null;
		}

        public async Task<string> GetLineManagerEmail(string UserId)
        {
            long Userid = Convert.ToInt64(UserId);
            string LineManageEmail = string.Empty;
            MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.UserId == Userid);
            if (mstEmployee != null)
            {
                LineManageEmail = mstEmployee.EmailId;               
            }
            return LineManageEmail;
        }
        #endregion


    }
}
