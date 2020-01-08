using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;


namespace TeleBillingRepository.Service.LogMangement
{
    public class LogManagement : ILogManagement
    {
        #region "Private Variable(s)"
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        private readonly IStringConstant _iStringConstant;
        #endregion


        #region "Constructor"
        public LogManagement(telebilling_v01Context dbTeleBilling_V01Context, IStringConstant iStringConstant)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
        }
        #endregion

        #region "Public Method(s)"

        public async Task<bool> SaveRequestTraseLog(long TransactionId, long Addedby, long actionId = 0, string description = null)
        {
            bool result = false;
            Requesttracelog requestlog = new Requesttracelog();
            try
            {
                requestlog.TransactionId = TransactionId;
                requestlog.CreatedById = Addedby;

                //requestlog.IsMobile = HttpContext.Current.Request.Browser.IsMobileDevice;
                //requestlog.Browser = HttpContext.Current.Request.Browser.Browser;

                requestlog.CreatedDate = DateTime.Now;
                requestlog.ActionId = actionId;
                requestlog.Description = description;
                await _dbTeleBilling_V01Context.AddAsync(requestlog);
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

        /// <summary>
        /// Genrate notification object
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actionuserId"></param>
        /// <param name="notificationType"></param>
        /// <param name="employeeBillId"></param>
        /// <returns></returns>
        public Notificationlog GenerateNotificationObject(long userId, long actionuserId, long notificationType, long? employeeBillId)
        {
            Notificationlog notificationlog = new Notificationlog();
            notificationlog.UserId = userId;
            notificationlog.ActionUserId = actionuserId;
            notificationlog.IsReadNotification = false;
            notificationlog.NotificationTypeId = notificationType;
            if (employeeBillId != null)
                notificationlog.EmployeeBillIormemoId = employeeBillId;
            notificationlog.CreatedDate = DateTime.Now;

            switch (notificationType)
            {
                case (long)EnumList.NotificationType.EmployeeBillIdentification:
                    notificationlog.NotificationText = _iStringConstant.EmployeeBillIdentificationNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.DelegateBillIdentification:
                    notificationlog.NotificationText = _iStringConstant.DelegateBillIdentificationNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.LineManagerApproval:
                    notificationlog.NotificationText = _iStringConstant.LineManagerApprovalNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.DelegateBillApproval:
                    notificationlog.NotificationText = _iStringConstant.DelegateBillApprovalNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.LineManagerApprove:
                    notificationlog.NotificationText = _iStringConstant.LineManagerApproveNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.LineManagerReject:
                    notificationlog.NotificationText = _iStringConstant.LineManagerRejectNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.DelegateBillApprove:
                    notificationlog.NotificationText = _iStringConstant.DelegateBillApproveNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.DelegateBillReject:
                    notificationlog.NotificationText = _iStringConstant.DelegateBillRejectNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.BillReImbursementRequest:
                    notificationlog.NotificationText = _iStringConstant.BillReImbursementRequestNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.ReImbursementApprove:
                    notificationlog.NotificationText = _iStringConstant.ReImbursementApproveNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.ReImbursementReject:
                    notificationlog.NotificationText = _iStringConstant.ReImbursementRejectNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.SendMemo:
                    notificationlog.NotificationText = _iStringConstant.SendMemoNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.MemoApprove:
                    notificationlog.NotificationText = _iStringConstant.MemoApproveNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.MemoReject:
                    notificationlog.NotificationText = _iStringConstant.MemoRejectNotificationMessage;
                    break;
                case (long)EnumList.NotificationType.ChangeBillStatus:
                    notificationlog.NotificationText = _iStringConstant.BillChangestatusNotificaiton;
                    break;
            }
            return notificationlog;
        }


        /// <summary>
        /// Insert the notification list
        /// </summary>
        /// <param name="notificationlogList"></param>
        /// <returns></returns>
        public async Task<bool> SaveNotificationList(List<Notificationlog> notificationlogList)
        {
            await _dbTeleBilling_V01Context.AddRangeAsync(notificationlogList);
            await _dbTeleBilling_V01Context.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Insert audit action log
        /// </summary>
        /// <param name="auditlogactiontypeId"></param>
        /// <param name="actionUserName"></param>
        /// <param name="userId"></param>
        /// <param name="objectName"></param>
        /// <param name="actionTemplateType"></param>
        /// <returns></returns>
        public async Task<bool> SaveAuditActionLog(long auditlogactiontypeId, string actionUserName, long userId, string objectName, long actionTemplateType, long? reflectedTableId)
        {
            Auditactionlog auditActionlLog = new Auditactionlog();
            auditActionlLog.AuditLogActionType = auditlogactiontypeId;
            auditActionlLog.CreatedBy = userId;
            auditActionlLog.CreatedDate = DateTime.Now;
            auditActionlLog.ReflectedTableId = reflectedTableId;

            switch (actionTemplateType)
            {
                case (int)EnumList.ActionTemplateTypes.Login:
                    auditActionlLog.Description = string.Format("Logeed in by {0}", actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.LogOut:
                    auditActionlLog.Description = string.Format("Logeed out by {0}", actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.ForgotPassword:
                    auditActionlLog.Description = string.Format("Forgot password by {0}", actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.ResetPassword:
                    auditActionlLog.Description = string.Format("Reset password by {0}", actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Add:
                    auditActionlLog.Description = string.Format("{0} added by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Edit:
                    auditActionlLog.Description = string.Format("{0} updated by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Active:
                    auditActionlLog.Description = string.Format("{0} activated by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Deactive:
                    auditActionlLog.Description = string.Format("{0} deactived by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Delete:
                    auditActionlLog.Description = string.Format("{0} deleted by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Upload:
                    auditActionlLog.Description = string.Format("{0} uploded by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.BillAllcation:
                    auditActionlLog.Description = string.Format("{0} allocated by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.BilIdentiSaveChanges:
                    auditActionlLog.Description = string.Format("{0} identicated by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.BillProcess:
                    auditActionlLog.Description = string.Format("{0} processed by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Approve:
                    auditActionlLog.Description = string.Format("{0} approved by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Reject:
                    auditActionlLog.Description = string.Format("{0} rejected by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.ReIdentification:
                    auditActionlLog.Description = string.Format("{0} reidentification by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.ReimbursementRequest:
                    auditActionlLog.Description = string.Format("{0} reimbursement requested by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.ChangeBillStatus:
                    auditActionlLog.Description = string.Format("{0} status changed by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Print:
                    auditActionlLog.Description = string.Format("{0} printed by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.ReminderNotificaiton:
                    auditActionlLog.Description = string.Format("Reminder notification updated by {0}", actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.SetTransactionType:
                    auditActionlLog.Description = string.Format("{0} set by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.CompareBill:
                    auditActionlLog.Description = string.Format("{0} compared by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.MergeBill:
                    auditActionlLog.Description = string.Format("{0} merged by {1}", objectName, actionUserName);
                    break;
                case (int)EnumList.ActionTemplateTypes.Assign:
                    auditActionlLog.Description = string.Format("{0} assigned by {1}", objectName, actionUserName);
                    break;
            }

            await _dbTeleBilling_V01Context.AddAsync(auditActionlLog);
            await _dbTeleBilling_V01Context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
