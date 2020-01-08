using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class ProviderWiseClosedBillAC
	{
		[JsonProperty("providerid")]
		public long ProviderId { get;set;}

		[JsonProperty("providername")]
		public string ProviderName { get;set;}

		[JsonProperty("totalbillamount")]
		public decimal TotalBillAmount { get;set;}

		[JsonProperty("companypayable")]
		public decimal CompanyPayable { get;set;}

		[JsonProperty("employeedeductable")]
		public decimal EmployeeDeducatable { get;set;}

		[JsonProperty("billnumber")]
		public string BillNumber { get; set;}

		[JsonProperty("monthyear")]
		public string MonthYear { get;set;}

		[JsonProperty("currency")]
		public string Currency { get;set;}
	}
}
