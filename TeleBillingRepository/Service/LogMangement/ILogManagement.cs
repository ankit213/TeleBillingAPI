using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TeleBillingRepository.Service.LogMangement
{
	public interface ILogManagement {

		/// <summary>
		/// This method used for added trase log 
		/// </summary>
		/// <param name="TransactionId"></param>
		/// <param name="Addedby"></param>
		/// <param name="actionId"></param>
		/// <param name="description"></param>
		/// <returns></returns>
		Task<bool> SaveRequestTraseLog(long TransactionId, long Addedby, long actionId = 0, string description = null);

		/// <summary>
		/// This function used for generate trasaction id.
		/// </summary>
		/// <returns></returns>
		long GenerateTeleBillingTransctionID();

		/// <summary>
		/// This function used for generate bill number.
		/// </summary>
		/// <returns></returns>
		string GenerateBillNumber();
		
	}
}
