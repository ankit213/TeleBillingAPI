using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Account
{
    public class AccountRepository : IAccountRepository
    {
        #region "Private Variable(s)"
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        #endregion

        #region "Constructor"

        public AccountRepository(telebilling_v01Context dbTeleBilling_V01Context)
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

        public async Task<MstEmployee> checkEmployeeIsActive(string emailOrPfNumber)
        {
            MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => !x.IsDelete && x.EmailId.Trim() == emailOrPfNumber.Trim());
            if (mstEmployee == null)
            {
                mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => !x.IsDelete && x.EmpPfnumber.Trim() == emailOrPfNumber.Trim());
            }
            return mstEmployee;
        }

        public async Task<bool> CheckUserCredentail(string email, string pfnumber, string password)
        {
            string encryptPassword = password;
            if (string.IsNullOrEmpty(email))
            {
                return await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.EmpPfnumber.Trim() == pfnumber.Trim() && x.Password.Trim() == encryptPassword.Trim() && !x.IsDelete) != null;
            }
            else
            {
                return await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.EmailId.Trim() == email.Trim() && x.Password.Trim() == encryptPassword.Trim() && !x.IsDelete) != null;
            }
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
