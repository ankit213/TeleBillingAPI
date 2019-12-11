using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using TeleBillingUtility.Models;
using System.Timers;
using MailKit.Net.Smtp;
using TeleBillingUtility.Helpers.Enums;
using Microsoft.EntityFrameworkCore;

namespace TeleBillingRemiderService
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var service = new TestSevice())
			{
				ServiceBase.Run(service);
			}
		}
	}

	internal class TestSevice : ServiceBase
	{
		Timer timerService = new Timer();

		public TestSevice()
		{
			ServiceName = "Tell Billing Reminder Service";
		}

		protected override void OnStart(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true, true);

			IConfigurationRoot configuration = builder.Build();

			//handle Elapsed event
			timerService.Elapsed += timerReminder_Elapsed;

			//This statement is used to set interval to 1 minute (= 60,000 milliseconds)
			timerService.Interval = !string.IsNullOrEmpty(configuration["TimerService"]) ? Convert.ToDouble(configuration["TimerService"]) : 60000;

			////enabling the timer
			timerService.Enabled = true;
		}

		#region Main Service Method
		void timerReminder_Elapsed(object sender, ElapsedEventArgs e)
		{
			using (var _dbTeleBillingContext = new telebilling_v01Context())
			{
				int waitingForIdentificationStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
				int waitingForLineManagerApprovalStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
				List<Employeebillmaster> employeeBillMasters = _dbTeleBillingContext.Employeebillmaster.Where(x => !x.IsDelete && (x.EmployeeBillStatus == waitingForIdentificationStatus || x.EmployeeBillStatus == waitingForLineManagerApprovalStatus)).Include(x => x.Provider).Include(x => x.Employee).Include(x => x.Linemanager).Include(x => x.BillDelegatedEmp).ToList();
				Configuration configuration = _dbTeleBillingContext.Configuration.First();
				DateTime todaydate = DateTime.Now.Date;

				//mail sending only one time for bill identification 
				if (configuration.NBillAllocationToEmployee)
				{
					List<Employeebillmaster> employeeBillMastersForNotiIdentification = employeeBillMasters.Where(x => !x.IsDelete && x.EmployeeBillStatus == waitingForIdentificationStatus && x.CreatedDate.Date == todaydate).ToList();
					if (employeeBillMastersForNotiIdentification.Any())
					{
						int emailTemplateTypeId = Convert.ToInt16(EnumList.EmailTemplateType.EmployeeCallIdentificationNotification);
						Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeId);
						if (emailTemplate != null)
						{
							foreach (var item in employeeBillMastersForNotiIdentification)
							{
								Emailreminderlog emailReminderLog = _dbTeleBillingContext.Emailreminderlog.FirstOrDefault(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplate.Id && !x.IsReminderMail);
								if (emailReminderLog == null && !string.IsNullOrEmpty(item.Employee.EmailId))
								{
									Dictionary<string, string> replacements = new Dictionary<string, string>();
									EnumList.Month month = (EnumList.Month)item.BillMonth;
									replacements.Add("{newEmpName}", item.Employee.FullName);
									replacements.Add("{BillNumber}", item.BillNumber);
									replacements.Add("{BillMonth}", month.ToString());
									replacements.Add("{Provider}", item.Provider.Name);
									replacements.Add("{BillYear}", item.BillYear.ToString());
									replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

									string body = emailTemplate.EmailText;
									replacements.ToList().ForEach(x =>
									{
										body = body.Replace(x.Key, Convert.ToString(x.Value));
									});

									if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, item.Employee.EmailId))
										AddedReminderNotificationLog(emailTemplate.Id, item.Id, false, item.Employee.EmailId);
								}
							}
						}
					}
				}

				//mail sending only one time for bill delegated identification 
				if (configuration.NBillDelegatesForIdentification)
				{
					List<Employeebillmaster> employeeBillMastersForDelgatedIdentification = employeeBillMasters.Where(x => !x.IsDelete && x.EmployeeBillStatus == waitingForIdentificationStatus && x.BillDelegatedEmpId != null).ToList();
					if (employeeBillMastersForDelgatedIdentification.Any())
					{
						int emailTemplateForDelegatedTypeId = Convert.ToInt16(EnumList.EmailTemplateType.BillDelegatesForIdentification);
						Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateForDelegatedTypeId);
						if (emailTemplate != null)
						{
							foreach (var item in employeeBillMastersForDelgatedIdentification)
							{
								Emailreminderlog emailReminderLog = _dbTeleBillingContext.Emailreminderlog.FirstOrDefault(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplate.Id && !x.IsReminderMail);
								if (emailReminderLog == null && !string.IsNullOrEmpty(item.BillDelegatedEmp.EmailId))
								{
									Dictionary<string, string> replacements = new Dictionary<string, string>();
									EnumList.Month month = (EnumList.Month)item.BillMonth;
									replacements.Add("{newEmpName}", item.BillDelegatedEmp.FullName);
									replacements.Add("{BillNumber}", item.BillNumber);
									replacements.Add("{BillMonth}", month.ToString());
									replacements.Add("{Provider}", item.Provider.Name);
									replacements.Add("{BillYear}", item.BillYear.ToString());

									string body = emailTemplate.EmailText;
									replacements.ToList().ForEach(x =>
									{
										body = body.Replace(x.Key, Convert.ToString(x.Value));
									});

									if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, item.BillDelegatedEmp.EmailId))
										AddedReminderNotificationLog(emailTemplate.Id, item.Id, false, item.BillDelegatedEmp.EmailId);
								}
							}
						}
					}
				}

				//mail sending only one time for bill identification request approval to line manager
				if (configuration.NNewBillReceiveForApproval)
				{
					List<Employeebillmaster> employeeBillMastersForbillApproval = employeeBillMasters.Where(x => !x.IsDelete && x.EmployeeBillStatus == waitingForLineManagerApprovalStatus && x.CreatedDate.Date == todaydate).ToList();
					if (employeeBillMastersForbillApproval.Any())
					{
						int emailTemplateTypeId = Convert.ToInt16(EnumList.EmailTemplateType.LineManagerApprovalNotification);
						Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeId);
						if (emailTemplate != null)
						{
							foreach (var item in employeeBillMastersForbillApproval)
							{
								Emailreminderlog emailReminderLog = _dbTeleBillingContext.Emailreminderlog.FirstOrDefault(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplate.Id && !x.IsReminderMail);
								if (emailReminderLog == null && !string.IsNullOrEmpty(item.Linemanager.EmailId))
								{
									Dictionary<string, string> replacements = new Dictionary<string, string>();
									replacements.Add("{newEmpName}", item.Employee.FullName);
									EnumList.Month month = (EnumList.Month)item.BillMonth;
									replacements.Add("{BillNumber}", item.BillNumber);
									replacements.Add("{BillMonth}", month.ToString());
									replacements.Add("{BillYear}", item.BillYear.ToString());
									replacements.Add("{Provider}", item.Provider.Name);
									replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

									string body = emailTemplate.EmailText;
									replacements.ToList().ForEach(x =>
									{
										body = body.Replace(x.Key, Convert.ToString(x.Value));
									});

									if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, item.Linemanager.EmailId))
										AddedReminderNotificationLog(emailTemplate.Id, item.Id, false, item.Linemanager.EmailId);
								}
							}
						}
					}
				}

				//mail sending only one time for delegates bill identification request approval to line manager
				if (configuration.NDelegatesBillForApproval)
				{
					List<Employeebillmaster> employeeBillMastersForDelegatesBillApproval = employeeBillMasters.Where(x => !x.IsDelete && x.EmployeeBillStatus == waitingForLineManagerApprovalStatus && x.CreatedDate.Date == todaydate && x.BillDelegatedEmpId != null).ToList();
					if (employeeBillMastersForDelegatesBillApproval.Any())
					{
						int emailTemplateTypeId = Convert.ToInt16(EnumList.EmailTemplateType.DelegateBillApproval);
						Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeId);
						if (emailTemplate != null)
						{
							foreach (var item in employeeBillMastersForDelegatesBillApproval)
							{
								Emailreminderlog emailReminderLog = _dbTeleBillingContext.Emailreminderlog.FirstOrDefault(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplate.Id && !x.IsReminderMail);
								if (emailReminderLog == null && !string.IsNullOrEmpty(item.BillDelegatedEmp.EmailId))
								{
									Dictionary<string, string> replacements = new Dictionary<string, string>();
									replacements.Add("{newEmpName}", item.Employee.FullName);
									EnumList.Month month = (EnumList.Month)item.BillMonth;
									replacements.Add("{BillNumber}", item.BillNumber);
									replacements.Add("{BillMonth}", month.ToString());
									replacements.Add("{BillYear}", item.BillYear.ToString());
									replacements.Add("{Provider}", item.Provider.Name);
									replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

									string body = emailTemplate.EmailText;
									replacements.ToList().ForEach(x =>
									{
										body = body.Replace(x.Key, Convert.ToString(x.Value));
									});

									if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, item.BillDelegatedEmp.EmailId))
										AddedReminderNotificationLog(emailTemplate.Id, item.Id, false, item.BillDelegatedEmp.EmailId);
								}
							}
						}
					}
				}

				//mail sending reminder in perticular interval for bill identification and also delegated employee
				if (configuration.REmployeeCallIdentificationIsActive)//Included delgated employee
				{
					List<Employeebillmaster> employeeBillMastersForREmpCllIdetification = employeeBillMasters.Where(x => !x.IsDelete && x.EmployeeBillStatus == waitingForIdentificationStatus).ToList();
					if (employeeBillMastersForREmpCllIdetification.Any())
					{
						int emailTemplateTypeId = Convert.ToInt16(EnumList.EmailTemplateType.EmployeeCallIdentificationRemider);
						int emailTemplateTypeForDelegateUserId = Convert.ToInt16(EnumList.EmailTemplateType.BillDelegatesForIdentification);

						foreach (var item in employeeBillMastersForREmpCllIdetification)
						{
							DateTime reminderDate = new DateTime();
							Emailreminderlog emailReminderLog = new Emailreminderlog();
							//check bill delgated or not 
							if (item.BillDelegatedEmpId != null)
							{
								Emailtemplate emailTemplateForDelegateUser = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeForDelegateUserId);
								if (emailTemplateForDelegateUser != null && !string.IsNullOrEmpty(item.BillDelegatedEmp.EmailId)) {
									emailReminderLog = _dbTeleBillingContext.Emailreminderlog.Where(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplateForDelegateUser.Id && x.IsReminderMail).OrderByDescending(x => x.CreatedDateInt).FirstOrDefault();

									int intervalDay = configuration.REmployeeCallIdentificationInterval != null ? Convert.ToInt32(configuration.REmployeeCallIdentificationInterval) : 0;
									if (emailReminderLog != null)
										reminderDate = emailReminderLog.CreatedDate.AddDays(intervalDay);
									else
										reminderDate = item.CreatedDate.AddDays(intervalDay);

									if (reminderDate.Date == DateTime.Now.Date)
									{
										Dictionary<string, string> replacements = new Dictionary<string, string>();
										replacements.Add("{newEmpName}", item.Employee.FullName);
										EnumList.Month month = (EnumList.Month)item.BillMonth;
										replacements.Add("{BillNumber}", item.BillNumber);
										replacements.Add("{Provider}", item.Provider.Name);
										replacements.Add("{BillMonth}", month.ToString());
										replacements.Add("{BillYear}", item.BillYear.ToString());
										replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

										string body = emailTemplateForDelegateUser.EmailText;
										replacements.ToList().ForEach(x =>
										{
											body = body.Replace(x.Key, Convert.ToString(x.Value));
										});

										if (SendEmail(emailTemplateForDelegateUser.EmailFrom, emailTemplateForDelegateUser.EmailBcc, emailTemplateForDelegateUser.Subject, body, item.BillDelegatedEmp.EmailId))
											AddedReminderNotificationLog(emailTemplateForDelegateUser.Id, item.Id, true, item.BillDelegatedEmp.EmailId);
									}
								}
							}
							else
							{
								Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeId);
								if (emailTemplate != null && !string.IsNullOrEmpty(item.Employee.EmailId)) {
									emailReminderLog = _dbTeleBillingContext.Emailreminderlog.Where(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplate.Id && x.IsReminderMail).OrderByDescending(x => x.CreatedDateInt).FirstOrDefault();

									int intervalDay = configuration.REmployeeCallIdentificationInterval != null ? Convert.ToInt32(configuration.REmployeeCallIdentificationInterval) : 0;
									if (emailReminderLog != null)
										reminderDate = emailReminderLog.CreatedDate.AddDays(intervalDay);
									else
										reminderDate = item.CreatedDate.AddDays(intervalDay);

									if (reminderDate.Date == DateTime.Now.Date)
									{
										Dictionary<string, string> replacements = new Dictionary<string, string>();
										replacements.Add("{newEmpName}", item.Employee.FullName);
										EnumList.Month month = (EnumList.Month)item.BillMonth;
										replacements.Add("{BillNumber}", item.BillNumber);
										replacements.Add("{BillMonth}", month.ToString());
										replacements.Add("{BillYear}", item.BillYear.ToString());
										replacements.Add("{Provider}", item.Provider.Name);
										replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

										string body = emailTemplate.EmailText;
										replacements.ToList().ForEach(x =>
										{
											body = body.Replace(x.Key, Convert.ToString(x.Value));
										});

										if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, item.Employee.EmailId))
											AddedReminderNotificationLog(emailTemplate.Id, item.Id, true, item.Employee.EmailId);
									}
								}
							}
						}
					}
				}

				//mail sending reminder perticular interval for bill identification request approval to line manager
				if (configuration.RLinemanagerApprovalIsActive)
				{
					List<Employeebillmaster> employeeBillMastersForRLineManager = employeeBillMasters.Where(x => !x.IsDelete && x.EmployeeBillStatus == waitingForLineManagerApprovalStatus).ToList();
					if (employeeBillMastersForRLineManager.Any())
					{
						foreach (var item in employeeBillMastersForRLineManager)
						{
							DateTime reminderDate = new DateTime();
							Emailreminderlog emailReminderLog = new Emailreminderlog();
							//check bill delgated or not 
							if (item.BillDelegatedEmpId != null) {
								int emailTemplateDelegateTypeId = Convert.ToInt16(EnumList.EmailTemplateType.DelegateBillApproval);
								Emailtemplate emailTemplateForDelegateUser = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateDelegateTypeId);
								if (emailTemplateForDelegateUser != null && !string.IsNullOrEmpty(item.BillDelegatedEmp.EmailId))
								{
									emailReminderLog = _dbTeleBillingContext.Emailreminderlog.Where(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplateForDelegateUser.Id && x.IsReminderMail).OrderByDescending(x => x.CreatedDateInt).FirstOrDefault();

									int intervalDay = configuration.RLinemanagerApprovalInterval != null ? Convert.ToInt32(configuration.RLinemanagerApprovalInterval) : 0;
									if (emailReminderLog != null)
										reminderDate = emailReminderLog.CreatedDate.AddDays(intervalDay);
									else
										reminderDate = item.CreatedDate.AddDays(intervalDay);

									if (reminderDate.Date == DateTime.Now.Date)
									{
										Dictionary<string, string> replacements = new Dictionary<string, string>();
										replacements.Add("{newEmpName}", item.Employee.FullName);
										EnumList.Month month = (EnumList.Month)item.BillMonth;
										replacements.Add("{BillNumber}", item.BillNumber);
										replacements.Add("{BillMonth}", month.ToString());
										replacements.Add("{BillYear}", item.BillYear.ToString());
										replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

										string body = emailTemplateForDelegateUser.EmailText;
										replacements.ToList().ForEach(x =>
										{
											body = body.Replace(x.Key, Convert.ToString(x.Value));
										});

										if (SendEmail(emailTemplateForDelegateUser.EmailFrom, emailTemplateForDelegateUser.EmailBcc, emailTemplateForDelegateUser.Subject, body, item.BillDelegatedEmp.EmailId))
											AddedReminderNotificationLog(emailTemplateForDelegateUser.Id, item.Id, true, item.BillDelegatedEmp.EmailId);
									}
								}
							}
							else
							{
								int emailTemplateTypeId = Convert.ToInt16(EnumList.EmailTemplateType.LineManagerApprovalRemider);
								Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeId);

								if (emailTemplate != null && !string.IsNullOrEmpty(item.Linemanager.EmailId)) {
									emailReminderLog = _dbTeleBillingContext.Emailreminderlog.Where(x => x.EmployeeBillId == item.Id && x.TemplateId == emailTemplate.Id && x.IsReminderMail).OrderByDescending(x => x.CreatedDateInt).FirstOrDefault();

									int intervalDay = configuration.RLinemanagerApprovalInterval != null ? Convert.ToInt32(configuration.RLinemanagerApprovalInterval) : 0;
									if (emailReminderLog != null)
										reminderDate = emailReminderLog.CreatedDate.AddDays(intervalDay);
									else
										reminderDate = item.CreatedDate.AddDays(intervalDay);

									if (reminderDate.Date == DateTime.Now.Date)
									{
										Dictionary<string, string> replacements = new Dictionary<string, string>();
										replacements.Add("{newEmpName}", item.Employee.FullName);
										EnumList.Month month = (EnumList.Month)item.BillMonth;
										replacements.Add("{BillNumber}", item.BillNumber);
										replacements.Add("{BillMonth}", month.ToString());
										replacements.Add("{BillYear}", item.BillYear.ToString());
										replacements.Add("{TelePhoneNumber}", item.TelephoneNumber.Trim());

										string body = emailTemplate.EmailText;
										replacements.ToList().ForEach(x =>
										{
											body = body.Replace(x.Key, Convert.ToString(x.Value));
										});

										if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, item.Linemanager.EmailId))
											AddedReminderNotificationLog(emailTemplate.Id, item.Id, true, item.Linemanager.EmailId);
									}
								}
							}
						}

					}
				}

			}
		}
		#endregion

		protected override void OnStop()
		{
			timerService.Enabled = false;
			timerService.Stop();
		}

		#region Private Method(s)
		/// <summary>
		/// This method used for sending mail
		/// </summary>
		/// <param name="emailFrom"></param>
		/// <param name="emailBcc"></param>
		/// <param name="subject"></param>
		/// <param name="body"></param>
		/// <param name="toEmail"></param>
		/// <returns></returns>
		private bool SendEmail(string emailFrom, string emailBcc, string subject, string body, string toEmail)
		{
			try
			{
				var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true, true);

				IConfigurationRoot configuration = builder.Build();

				var msg = new MimeMessage();
				msg.From.Add(new MailboxAddress(emailFrom, emailFrom));
				msg.To.Add(new MailboxAddress(toEmail));

				if (!string.IsNullOrEmpty(emailBcc))
					msg.Bcc.Add(new MailboxAddress(emailBcc));

				msg.Subject = subject;
				var bodyBuilder = new BodyBuilder();
				bodyBuilder.HtmlBody = body;
				msg.Body = bodyBuilder.ToMessageBody();

				using (var smtp = new SmtpClient())
				{
					smtp.Connect(configuration.GetSection("EmailCrednetials").GetSection("Host").Value, Convert.ToInt16(configuration.GetSection("EmailCrednetials").GetSection("Port").Value), SecureSocketOptions.SslOnConnect);
					smtp.Authenticate(credentials: new NetworkCredential(configuration.GetSection("EmailCrednetials").GetSection("From").Value, configuration.GetSection("EmailCrednetials").GetSection("Password").Value));
					smtp.Send(msg, System.Threading.CancellationToken.None);
					smtp.Disconnect(true, System.Threading.CancellationToken.None);
				}
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// This method used for added reminder notification log 
		/// </summary>
		/// <param name="emailTemplateId"></param>
		/// <param name="employeeBillId"></param>
		/// <param name="isReminderMail"></param>
		private void AddedReminderNotificationLog(long emailTemplateId, long employeeBillId, bool isReminderMail, string emailTo)
		{
			using (var _dbTeleBillingContext = new telebilling_v01Context())
			{
				Emailreminderlog newEmailReminderLog = new Emailreminderlog();
				newEmailReminderLog.CreatedDate = DateTime.Now;
				newEmailReminderLog.IsReminderMail = isReminderMail;
				newEmailReminderLog.TemplateId = emailTemplateId;
				newEmailReminderLog.EmailTo = emailTo;
				newEmailReminderLog.EmployeeBillId = employeeBillId;

				_dbTeleBillingContext.Add(newEmailReminderLog);
				_dbTeleBillingContext.SaveChanges();
			}
		}
		#endregion
	}

}