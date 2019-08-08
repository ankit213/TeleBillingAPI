using System;
using System.Threading.Tasks;
using TeleBillingUtility.Models;


namespace TeleBillingRepository.Service.LogMangement
{
	public class LogManagement : ILogManagement
	{
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		#endregion


		#region "Constructor"
		public LogManagement(TeleBilling_V01Context dbTeleBilling_V01Context)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			
		}
		#endregion
		
		#region "Public Method(s)"
	
		public async Task<bool> SaveRequestTraseLog(long TransactionId, long Addedby, long actionId = 0, string description = null)
		{
			bool result = false;
			RequestTraceLog requestlog = new RequestTraceLog();
			try
			{
				requestlog.TransactionId = TransactionId;
				requestlog.CreatedById = Addedby;

				//requestlog.IsMobile = HttpContext.Current.Request.Browser.IsMobileDevice;
				//requestlog.Browser = HttpContext.Current.Request.Browser.Browser;

				requestlog.CreatedDate = DateTime.Now;
				requestlog.ActionId = actionId;
				requestlog.Description = description;
				await  _dbTeleBilling_V01Context.AddAsync(requestlog);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				
				result = true;

			}
			catch (Exception)
			{
				result = false;
			}
			return (result);
		}


		#region --> Generate 10 - DIGIT Tele Billing Transaction ID
		public long GenerateTeleBillingTransctionID()
		{
			Random random = new Random();
			long random10 = (long)random.Next(0, 1000000) * (long)random.Next(0, 10000);
			long result = random10;
			return result;
		}
		#endregion

		public string GenerateBillNumber()
		{
			Random random = new Random();
			DateTime today = DateTime.Now;
			string result = today.Day.ToString() + today.Month.ToString() + today.Year.ToString() + today.Hour.ToString() + today.Minute.ToString() + today.Second.ToString();
			return result;
		}
		#endregion
	}
}
