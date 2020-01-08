using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Timers;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;


namespace TeleBillingBillCloseService
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
			ServiceName = "Tell Billing Bill Cose";
		}

		protected override void OnStart(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true, true);

			IConfigurationRoot configuration = builder.Build();

			//handle Elapsed event
			timerService.Elapsed += timerBillClosed_Elapsed;

			//This statement is used to set interval to 1 minute (= 60,000 milliseconds)
			timerService.Interval = !string.IsNullOrEmpty(configuration["TimerService"]) ? Convert.ToDouble(configuration["TimerService"]) : 60000;

			////enabling the timer
			timerService.Enabled = true;
		}

		#region Main Service Method
		void timerBillClosed_Elapsed(object sender, ElapsedEventArgs e)
		{
			using (var _dbTeleBillingContext = new telebilling_v01Context())
			{
				try
				{
					int billAllocatedStatusId = Convert.ToInt16(EnumList.BillStatus.BillAllocated);
					List<Billmaster> billMasters = _dbTeleBillingContext.Billmaster.Where(x => !x.IsDelete && (x.BillStatusId == billAllocatedStatusId)).ToList();
					if (billMasters.Any())
					{
						foreach (var item in billMasters)
						{
							if (item.BillDueDate != null)
							{
								if (Convert.ToDateTime(item.BillDueDate).Date < DateTime.Now.Date)
								{
									int billCloseStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.CloseBill);
									int billAutoCloseBillStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
									List<Employeebillmaster> employeeBillMasters = _dbTeleBillingContext.Employeebillmaster.Where(x => x.BillMasterId == item.Id && !x.IsDelete && (x.EmployeeBillStatus != billCloseStatusId && x.EmployeeBillStatus != billAutoCloseBillStatusId)).Include(x => x.Employee).Include(x => x.Provider).ToList();
									List<Notificationlog> notificationlogs = new List<Notificationlog>();
									if (employeeBillMasters.Any())
									{
										foreach (var subItem in employeeBillMasters)
										{
											#region Transaction Log Entry

											if (subItem.TransactionId == null)
												subItem.TransactionId = GenerateTeleBillingTransctionID();

											//var jsonSerailzeObj = JsonConvert.SerializeObject(subItem);

											//SaveRequestTraseLog(Convert.ToInt64(subItem.TransactionId), 0, Convert.ToInt64(EnumList.TransactionTraseLog.BillClosed), jsonSerailzeObj);
											#endregion

											List<Employeebillservicepackage> employeeBillServicePackages = _dbTeleBillingContext.Employeebillservicepackage.Where(x => x.EmployeeBillId == subItem.Id && !x.IsDelete).Include(x => x.Package).ToList();
											foreach (var employeeBillServicePackage in employeeBillServicePackages)
											{
												if (!subItem.Employee.IsPresidentOffice)
												{
													if (string.IsNullOrEmpty(subItem.TelephoneNumber))//For Skyp
													{
														//for service landline or voip 
														if (employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.VOIP) || employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.MOC))
														{
															if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification) || subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject))
															{
																List<Billdetails> billdetails = _dbTeleBillingContext.Billdetails.Where(x => x.EmployeeBillId == subItem.Id && x.ServiceTypeId == employeeBillServicePackage.ServiceTypeId).ToList();
																decimal personalDeduction = 0;
																decimal businessCharge = 0;
																foreach (var subBillDetail in billdetails)
																{
																	if (subBillDetail.CallIdentificationType == Convert.ToInt16(EnumList.AssignType.Business))
																	{
																		businessCharge += subBillDetail.CallAmount != null ? Convert.ToDecimal(subBillDetail.CallAmount) : 0;
																	}
																	else if (subBillDetail.CallIdentificationType == Convert.ToInt16(EnumList.AssignType.Employee))
																	{
																		personalDeduction += subBillDetail.CallAmount != null ? Convert.ToDecimal(subBillDetail.CallAmount) : 0;
																	}
																}
																employeeBillServicePackage.PersonalIdentificationAmount = personalDeduction;
																employeeBillServicePackage.BusinessIdentificationAmount = businessCharge;
																employeeBillServicePackage.DeductionAmount = personalDeduction;
															}
															else if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval) || subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved))
															{
																employeeBillServicePackage.DeductionAmount = employeeBillServicePackage.PersonalIdentificationAmount;
															}
															SendEmailNotificationToEmployee(employeeBillServicePackage, subItem);
														}
													}
													else
													{
														var telephoneNumberAllocation = _dbTeleBillingContext.Telephonenumberallocation.FirstOrDefault(x => x.TelephoneNumber == subItem.TelephoneNumber && x.EmployeeId == subItem.EmployeeId && !x.IsDelete);
														if (telephoneNumberAllocation.AssignTypeId != Convert.ToInt16(EnumList.AssignType.Business))
														{
															employeeBillServicePackage.DeductionAmount = 0;
															List<Billdetails> billDetails = _dbTeleBillingContext.Billdetails.Where(x => x.EmployeeBillId == subItem.Id && x.ServiceTypeId == employeeBillServicePackage.ServiceTypeId).ToList();
															decimal totalAmount = billDetails.Sum(x => x.CallAmount).Value;

															//for service landline
															if (employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.LandLine) || employeeBillServicePackage.ServiceTypeId == Convert.ToInt64(EnumList.ServiceType.StaticIP))
															{
																//remaning
																employeeBillServicePackage.DeductionAmount = employeeBillServicePackage.PersonalIdentificationAmount;
																SendEmailNotificationToEmployee(employeeBillServicePackage, subItem);
															}
															else
															{
																if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification) || subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject))
																{
																	if (employeeBillServicePackage.Package.PackageAmount < totalAmount)
																	{
																		decimal amount = 0;
																		decimal businessIdentificationAmount = employeeBillServicePackage.BusinessIdentificationAmount != null ? Convert.ToDecimal(employeeBillServicePackage.BusinessIdentificationAmount) : 0;
																		if (employeeBillServicePackage.Package.PackageAmount < businessIdentificationAmount)
																		{
																			amount = businessIdentificationAmount;
																		}
																		else
																		{
																			amount = employeeBillServicePackage.Package.PackageAmount != null ? Convert.ToDecimal(employeeBillServicePackage.Package.PackageAmount) : 0;
																		}
																		//BusinessIdentificationAmount alway less than or equal Package Amount
																		employeeBillServicePackage.DeductionAmount = totalAmount - amount;
																		SendEmailNotificationToEmployee(employeeBillServicePackage, subItem);
																	}
																	else
																		employeeBillServicePackage.DeductionAmount = 0;
																}
																else if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval) || subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved))
																{

																	if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval))
																	{
																		List<Billdetails> billdetails = _dbTeleBillingContext.Billdetails.Where(x => x.EmployeeBillId == subItem.Id).ToList();
																		var serviceBillDetials = billdetails.GroupBy(x => x.ServiceTypeId);
																		var finalServiceDetails = serviceBillDetials.Where(x => x.Key == employeeBillServicePackage.ServiceTypeId);

																		foreach (var billDetailsData in finalServiceDetails)
																		{
																			decimal personalDeduction = 0;
																			decimal businessCharge = 0;
																			foreach (var subBillDetail in billDetailsData)
																			{
																				if (subBillDetail.CallIdentificationType == Convert.ToInt16(EnumList.AssignType.Business))
																				{
																					businessCharge += subBillDetail.CallAmount != null ? Convert.ToDecimal(subBillDetail.CallAmount) : 0;
																				}
																				else if (subBillDetail.CallIdentificationType == Convert.ToInt16(EnumList.AssignType.Employee))
																				{
																					personalDeduction += subBillDetail.CallAmount != null ? Convert.ToDecimal(subBillDetail.CallAmount) : 0;
																				}
																			}
																			employeeBillServicePackage.PersonalIdentificationAmount = personalDeduction;
																			employeeBillServicePackage.BusinessIdentificationAmount = businessCharge;
																		}
																	}

																	if (employeeBillServicePackage.Package.PackageAmount < totalAmount)
																	{
																		decimal businessIdentificationAmount = employeeBillServicePackage.BusinessIdentificationAmount != null ? Convert.ToDecimal(employeeBillServicePackage.BusinessIdentificationAmount) : 0;
																		decimal amount = 0;
																		if (employeeBillServicePackage.Package.PackageAmount < businessIdentificationAmount)
																		{
																			amount = businessIdentificationAmount;
																		}
																		else
																		{
																			amount = employeeBillServicePackage.Package.PackageAmount != null ? Convert.ToDecimal(employeeBillServicePackage.Package.PackageAmount) : 0;
																		}
																		employeeBillServicePackage.DeductionAmount = totalAmount - amount;
																		SendEmailNotificationToEmployee(employeeBillServicePackage, subItem);
																	}
																}
															}
														}
														else
															employeeBillServicePackage.DeductionAmount = 0;
													}
												}
												else
													employeeBillServicePackage.DeductionAmount = 0;
												employeeBillServicePackage.UpdateDate = DateTime.Now;
											}

											_dbTeleBillingContext.UpdateRange(employeeBillServicePackages);
											_dbTeleBillingContext.SaveChanges();

											subItem.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
											subItem.UpdatedDate = DateTime.Now;
											subItem.BillClosedDate = DateTime.Now;


											#region Notification FOR Bill Close
											notificationlogs.Add(GenerateNotificationObject(Convert.ToInt16(subItem.EmployeeId), 1, Convert.ToInt16(EnumList.NotificationType.BillClosed), subItem.Id));
											#endregion

										}

										if (notificationlogs.Any())
										{
											_dbTeleBillingContext.AddRange(notificationlogs);
											_dbTeleBillingContext.SaveChanges();
										}

										_dbTeleBillingContext.UpdateRange(employeeBillMasters);
										_dbTeleBillingContext.SaveChanges();

										Billmaster billMaster = _dbTeleBillingContext.Billmaster.FirstOrDefault(x => x.Id == employeeBillMasters[0].BillMasterId && !x.IsDelete);
										if (billMaster != null)
										{
											billMaster.BillStatusId = Convert.ToInt16(EnumList.BillStatus.BillClosed);
											billMaster.UpdatedDate = DateTime.Now;

											_dbTeleBillingContext.Update(billMaster);
											_dbTeleBillingContext.SaveChanges();
										}
									}
								}
							}
						}
					}
				}
				catch (global::System.Exception ex)
				{
					throw ex;
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

		private bool SaveRequestTraseLog(long TransactionId, long Addedby, long actionId = 0, string description = null)
		{
			bool result = false;
			Requesttracelog requestlog = new Requesttracelog();
			try
			{
				using (var _dbTeleBillingContext = new telebilling_v01Context())
				{
					requestlog.TransactionId = TransactionId;
					requestlog.CreatedById = Addedby;

					requestlog.CreatedDate = DateTime.Now;
					requestlog.ActionId = actionId;
					requestlog.Description = description;

					_dbTeleBillingContext.AddAsync(requestlog);
					_dbTeleBillingContext.SaveChangesAsync();
				}
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return (result);
		}

		#region --> Generate 10 - DIGIT Tele Billing Transaction ID
		private long GenerateTeleBillingTransctionID()
		{
			Random random = new Random();
			long random10 = (long)random.Next(0, 1000000) * (long)random.Next(0, 10000);
			long result = random10;
			return result;
		}
		#endregion

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
				if (!string.IsNullOrEmpty(toEmail))
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

					using (var smtp = new MailKit.Net.Smtp.SmtpClient())
					{
						smtp.Connect(configuration.GetSection("EmailCrednetials").GetSection("Host").Value, Convert.ToInt16(configuration.GetSection("EmailCrednetials").GetSection("Port").Value), SecureSocketOptions.SslOnConnect);
						smtp.Authenticate(credentials: new NetworkCredential(configuration.GetSection("EmailCrednetials").GetSection("From").Value, configuration.GetSection("EmailCrednetials").GetSection("Password").Value));
						smtp.Send(msg, System.Threading.CancellationToken.None);
						smtp.Disconnect(true, System.Threading.CancellationToken.None);
					}
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
				newEmailReminderLog.EmailTo = string.IsNullOrEmpty(emailTo) ? string.Empty : emailTo;
				newEmailReminderLog.EmployeeBillId = employeeBillId;

				_dbTeleBillingContext.Add(newEmailReminderLog);
				_dbTeleBillingContext.SaveChanges();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="employeeBillServicePackage"></param>
		/// <param name="employeeBillMasters"></param>
		private void SendEmailNotificationToEmployee(Employeebillservicepackage employeeBillServicePackage, Employeebillmaster employeeBillMaster)
		{
			if (!string.IsNullOrEmpty(employeeBillMaster.Employee.EmailId))
			{

				Dictionary<string, string> replacements = new Dictionary<string, string>();
				EnumList.Month month = (EnumList.Month)employeeBillMaster.BillMonth;
				replacements.Add("{BillNumber}", employeeBillMaster.BillNumber);
				replacements.Add("{BillMonth}", month.ToString());
				replacements.Add("{BillYear}", employeeBillMaster.BillYear.ToString());
				replacements.Add("{newEmpName}", employeeBillMaster.Employee.FullName);
				replacements.Add("{Provider}", employeeBillMaster.Provider.Name);
				string telphoneNumber = !string.IsNullOrEmpty(employeeBillMaster.TelephoneNumber) ? employeeBillMaster.TelephoneNumber.Trim() : string.Empty;
				replacements.Add("{TelePhoneNumber}", telphoneNumber);

				string deductionAmount = string.Empty;
				if (employeeBillServicePackage.DeductionAmount != null)
					deductionAmount = Convert.ToString(employeeBillServicePackage.DeductionAmount);
				replacements.Add("{DeductionAmount}", deductionAmount);

				using (var _dbTeleBillingContext = new telebilling_v01Context())
				{
					long emailTemplateTypeId = Convert.ToInt64(EnumList.EmailTemplateType.SendEmailNotificationToEmployeForAmountToBeDeducted);
					Emailtemplate emailTemplate = _dbTeleBillingContext.Emailtemplate.FirstOrDefault(x => x.EmailTemplateTypeId == emailTemplateTypeId);
					if (emailTemplate != null && !string.IsNullOrEmpty(employeeBillMaster.Employee.EmailId))
					{
						string body = emailTemplate.EmailText;
						replacements.ToList().ForEach(x =>
						{
							body = body.Replace(x.Key, Convert.ToString(x.Value));
						});

						if (SendEmail(emailTemplate.EmailFrom, emailTemplate.EmailBcc, emailTemplate.Subject, body, employeeBillMaster.Employee.EmailId))
							AddedReminderNotificationLog(emailTemplate.Id, employeeBillMaster.Id, false, employeeBillMaster.Employee.EmailId);
					}
				}
			}
		}

		/// <summary>
		/// This method used for added notifictions
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="actionuserId"></param>
		/// <param name="notificationType"></param>
		/// <param name="employeeBillId"></param>
		/// <returns></returns>
		private Notificationlog GenerateNotificationObject(long userId, long actionuserId, long notificationType, long? employeeBillId)
		{
			Notificationlog notificationlog = new Notificationlog();
			notificationlog.UserId = userId;
			notificationlog.ActionUserId = actionuserId;
			notificationlog.NotificationTypeId = notificationType;
			notificationlog.IsReadNotification = false;
			if (employeeBillId != null)
				notificationlog.EmployeeBillIormemoId = employeeBillId;
			notificationlog.CreatedDate = DateTime.Now;
			notificationlog.NotificationText = "Bill closed";

			return notificationlog;
		}

		#endregion
	}
}
