using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
				.SetBasePath(Directory.GetCurrentDirectory())
			   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			IConfigurationRoot configuration = builder.Build();

			//handle Elapsed event
			timerService.Elapsed += timerBillGeneration_Elapsed;

			//This statement is used to set interval to 1 minute (= 60,000 milliseconds)
			timerService.Interval = Convert.ToDouble(configuration.GetConnectionString("TimerService"));;

			//enabling the timer
			timerService.Enabled = true;
		}

		void timerBillGeneration_Elapsed(object sender, ElapsedEventArgs e)
		{
			using (var _context = new TeleBilling_V01Context())
			{
				int billAllocatedStatusId = Convert.ToInt16(EnumList.BillStatus.BillAllocated);
				List<BillMaster> billMasters = _context.BillMaster.Where(x => !x.IsDelete && (x.BillStatusId == billAllocatedStatusId)).ToList();
				if (billMasters.Any()) { 
					foreach (var item in billMasters)
					{
						if(item.BillDueDate != null)
						{
							if (Convert.ToDateTime(item.BillDueDate).Date < DateTime.Now.Date)
							{
								int billCloseStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.CloseBill);
								int billAutoCloseBillStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.AutoCloseBill);
								List<EmployeeBillMaster> employeeBillMasters = _context.EmployeeBillMaster.Where(x=>x.BillMasterId == item.Id && !x.IsDelete && (x.EmployeeBillStatus == billCloseStatusId || x.EmployeeBillStatus == billAutoCloseBillStatusId)).Include(x=>x.Employee).ToList();
								if (employeeBillMasters.Any()) {

									foreach(var subItem in employeeBillMasters)
									{
										if (!subItem.Employee.IsPresidentOffice) 
										{ 
											var telephoneNumberAllocation = _context.TelephoneNumberAllocation.FirstOrDefault(x=>x.TelephoneNumber == subItem.TelephoneNumber && x.EmployeeId == subItem.EmployeeId && !x.IsDelete);
											if(telephoneNumberAllocation.AssignTypeId != Convert.ToInt16(EnumList.AssignType.Business))
											{
												List<EmployeeBillServicePackage> employeeBillServicePackages = _context.EmployeeBillServicePackage.Where(x=>x.EmployeeBillId == subItem.Id && !x.IsDelete).Include(x=>x.Package).ToList();
												foreach(var employeeBillServicePackage in employeeBillServicePackages)
												{
													employeeBillServicePackage.DeductionAmount = 0;
													if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification) || subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.BillReject))
													{
														decimal totalAmount = ((employeeBillServicePackage.PersonalIdentificationAmount != null ? Convert.ToDecimal(employeeBillServicePackage.PersonalIdentificationAmount) : 0) + (employeeBillServicePackage.BusinessIdentificationAmount != null ? Convert.ToDecimal(employeeBillServicePackage.BusinessIdentificationAmount) : 0));
														if (employeeBillServicePackage.Package.PackageAmount < totalAmount) {
															//BusinessIdentificationAmount alway less than or equal Package Amount
															employeeBillServicePackage.DeductionAmount = totalAmount - employeeBillServicePackage.Package.PackageAmount;
														}
													}
													else if (subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval) || subItem.EmployeeBillStatus == Convert.ToInt16(EnumList.EmployeeBillStatus.LineManagerApproved))
													{

													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}


		protected override void OnStop()
		{
			timerService.Enabled = false;
			timerService.Stop();
		}

	}
}
