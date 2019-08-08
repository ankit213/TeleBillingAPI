using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TeleBillingUtility.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Linq;

namespace TeleBillingRepository.Service
{
    public class EmailSender : IEmailSender
    {

        #region "Private Variable(s)"
        private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;

        #endregion

        #region "Constructor"

        public EmailSender(TeleBilling_V01Context dbTeleBilling_V01Context)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
        }

        #endregion

        public async Task<bool> SendEmail(long EmailTemplateId, Dictionary<string, string> replacements, string toEmail)
        {
            try
            {
				EmailTemplate template =  await _dbTeleBilling_V01Context.EmailTemplate.FirstOrDefaultAsync(x => x.EmailTemplateTypeId == EmailTemplateId);
				if (template != null)
                {
                    string body = template.EmailText;
                    replacements.ToList().ForEach(x =>
                    {
                        body = body.Replace(x.Key, Convert.ToString(x.Value));
                    });
                    // Plug in your email service here to send an email.
                    var msg = new MimeMessage();
                    msg.From.Add(new MailboxAddress(template.EmailFrom,template.EmailFrom));
                    msg.To.Add(new MailboxAddress(toEmail));
                    msg.Bcc.Add(new MailboxAddress(template.EmailBcc));
                    msg.Subject = template.Subject;
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = body;
                    msg.Body = bodyBuilder.ToMessageBody();

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                        smtp.Authenticate(credentials: new NetworkCredential("bhanvadiyaankit007@gmail.com", "ankit2635"));
                        smtp.Send(msg, CancellationToken.None);
                        smtp.Disconnect(true, CancellationToken.None);
                    }

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
          
        }
    }
}
