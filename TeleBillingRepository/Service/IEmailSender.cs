using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeleBillingRepository.Service
{
    public interface IEmailSender
    {
        /// <summary>
        /// This method is used to send email.
        /// </summary>
        /// <param name="EmailTemplateId">Passed EmailTemplateId</param> 
        /// <param name="Dictionary<string, string> replacements">Passed email body tag list</param>
        /// <param name="toEmail">Passed toEmail</param>

        // bool SendEmail();
        Task<bool> SendEmail(long EmailTemplateId, Dictionary<string, string> replacements, string toEmail);

        /// <summary>
        /// This method used for added reminder notification log 
        /// </summary>
        /// <param name="emailTemplateTypeId"></param>
        /// <param name="employeeBillId"></param>
        /// <param name="isReminderMail"></param>
        /// <param name="emailTo"></param>
        /// <returns></returns>
        Task AddedReminderNotificationLog(long emailTemplateTypeId, long? employeeBillId, bool isReminderMail, string emailTo);

    }
}
