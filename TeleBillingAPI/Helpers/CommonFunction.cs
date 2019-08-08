using System;
using System.Web;
using System.Data;
using System.IO;


namespace TeleBillingAPI.Helpers
{
	public static class CommonFunction
	{
		#region --> Password Encryption
		public static string Encrypt(string StringToEncode)
		{
			try
			{
				StringToEncode = StringToEncode.ToUpper();
				byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(StringToEncode);
				string str = Convert.ToBase64String(data);
				str = str + "@";
				return str;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region --> Password Decryption 
		public static string Decrypt(string stringToDecode)
		{
			string str = string.Empty;
			try
			{
				stringToDecode = stringToDecode.Replace("@", "");
				byte[] data = Convert.FromBase64String(stringToDecode);
				str = System.Text.ASCIIEncoding.ASCII.GetString(data);
				return str;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
        #endregion

        #region --> Month Number Decryption 
        public static string GetMonth(int iMonthNo)
        {
            string sMonthName = string.Empty;
            try
            {
                if (iMonthNo > 0 && iMonthNo<=12)                {
                   
                    DateTime dtDate = new DateTime(2000, iMonthNo, 1);
                    sMonthName = dtDate.ToString("MMM");
                }
                return sMonthName;
            }
            catch (Exception)
            {
                return sMonthName;
            }
        }
        #endregion

    }
}
