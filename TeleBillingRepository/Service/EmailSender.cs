using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net;
using TeleBillingUtility.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace TeleBillingRepository.Service
{
	public class EmailSender : IEmailSender
    {

        #region "Private Variable(s)"
        private readonly telebilling_v01Context  _dbTeleBilling_V01Context;

        #endregion

        #region "Constructor"

        public EmailSender(telebilling_v01Context  dbTeleBilling_V01Context)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
        }

        #endregion

        public async Task<bool> SendEmail(long EmailTemplateId, Dictionary<string, string> replacements, string toEmail)
        {
            try
            {
				//var builder = new ConfigurationBuilder()
				//.AddJsonFile("appsettings.json", true, true);

				//IConfigurationRoot configuration = builder.Build();

				//Emailtemplate template = await _dbTeleBilling_V01Context.Emailtemplate.FirstOrDefaultAsync(x => x.EmailTemplateTypeId == EmailTemplateId);
				//if (template != null)
				//{
				//	string body = template.EmailText;
				//	replacements.ToList().ForEach(x =>
				//	{
				//		body = body.Replace(x.Key, Convert.ToString(x.Value));
				//	});
				//	// Plug in your email service here to send an email.
				//	var msg = new MimeMessage();
				//	msg.From.Add(new MailboxAddress(template.EmailFrom, template.EmailFrom));
				//	msg.To.Add(new MailboxAddress(toEmail));


				//	if (!string.IsNullOrEmpty(template.EmailBcc))
				//		msg.Bcc.Add(new MailboxAddress(template.EmailBcc));

				//	msg.Subject = template.Subject;
				//	var bodyBuilder = new BodyBuilder();
				//	bodyBuilder.HtmlBody = body;
				//	msg.Body = bodyBuilder.ToMessageBody();

				//	using (var smtp = new SmtpClient())
				//	{
				//		smtp.Connect(configuration.GetSection("EmailCrednetials").GetSection("Host").Value, Convert.ToInt16(configuration.GetSection("EmailCrednetials").GetSection("Port").Value), SecureSocketOptions.SslOnConnect);
				//		smtp.Authenticate(credentials: new NetworkCredential(configuration.GetSection("EmailCrednetials").GetSection("From").Value,configuration.GetSection("EmailCrednetials").GetSection("Password").Value));
				//		smtp.Send(msg, CancellationToken.None);
				//		smtp.Disconnect(true, CancellationToken.None);
				//	}
					//return true;
				//}
				return true;
			}
            catch (Exception ex)
            {
				throw ex;
            }
        }


		public async Task AddedReminderNotificationLog(long emailTemplateTypeId, long? employeeBillId, bool isReminderMail,string emailTo)
		{
			Emailtemplate emailTemplate = await _dbTeleBilling_V01Context.Emailtemplate.FirstOrDefaultAsync(x=>x.EmailTemplateTypeId == emailTemplateTypeId); 
			if(emailTemplate != null)
			{
				Emailreminderlog newEmailReminderLog = new Emailreminderlog();
				newEmailReminderLog.CreatedDate = DateTime.Now;
				newEmailReminderLog.IsReminderMail = isReminderMail;
				newEmailReminderLog.TemplateId = emailTemplate.Id;
				newEmailReminderLog.EmployeeBillId = employeeBillId;
				newEmailReminderLog.EmailTo	 = emailTo != null ? emailTo : string.Empty;

				await _dbTeleBilling_V01Context.AddAsync(newEmailReminderLog);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
			}	
		}
	}
}
