using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeleBillingUtility.Models;

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

		/// <summary>
		/// This method used for get notificaiton object
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="actionuserId"></param>
		/// <param name="notificationType"></param>
		/// <param name="employeeBillId"></param>
		/// <returns></returns>
		Notificationlog GenerateNotificationObject(long userId, long actionuserId, long notificationType, long? employeeBillId);
		
		/// <summary>
		/// This method used for save notification list
		/// </summary>
		/// <param name="notificationLogList"></param>
		/// <returns></returns>
		Task<bool> SaveNotificationList(List<Notificationlog> notificationLogList);

		/// <summary>
		/// This function used for added audit log
		/// </summary>
		/// <param name="auditlogactiontypeId"></param>
		/// <param name="actionUserName"></param>
		/// <param name="userId"></param>
		/// <param name="objectName"></param>
		/// <param name="actionTemplateType"></param>
		/// <param name="reflectedTableId"></param>
		/// <returns></returns>
		Task<bool> SaveAuditActionLog(long auditlogactiontypeId, string actionUserName,long userId,string objectName,long actionTemplateType,long? reflectedTableId);



		

	}
}
