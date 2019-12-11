using System;


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
				if (iMonthNo > 0 && iMonthNo <= 12)
				{

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

        #region --> Service Name 
        public static string GetServiceName(long serviceid)
        {
            if (serviceid > 0)
            {
                switch (serviceid)
                {
                    case 1:
                        return "Mobility";
                    case 2:
                        return "Voice Only";
                    case 3:
                        return "Internet Service";
                    case 4:
                        return "Data Center Facility";
                    case 5:
                        return "Managed Hosting Service";
                    case 6:
                        return "Static IP";
                    case 7:
                        return "VOIP";
                    case 8:
                        return "MOC";
                    case 9:
                        return "General Service Mada";
                    case 10:
                        return "General Service Kems";
                    case 11:
                        return "LandLine";
                    case 12:
                        return "Internet Plan Device Offer";             
                    default:
                        return "";
                }
               
            }
            return string.Empty;
        }
        #endregion

    }
}
