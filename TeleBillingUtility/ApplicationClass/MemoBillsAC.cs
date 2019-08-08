using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class MemoBillResponse
	{
		[JsonProperty("memobills")]
		public List<MemoBillsAC> MemoBills {get; set;}
		
		[JsonProperty("bank")]
		public string Bank { get; set; }

		[JsonProperty("ibancode")]
		public string Ibancode { get; set; }

		[JsonProperty("swiftcode")]
		public string Swiftcode { get; set; }

	}


	public class MemoBillsAC
	{
		[JsonProperty("billmasterid")]
		public long BillMasterId { get; set;}

		[JsonProperty("billnumber")]
		public string BillNumber { get; set; }

		[JsonProperty("billmonth")]
		public int BillMonth { get; set; }
		
		[JsonProperty("billyear")]
		public int BillYear { get; set; }

		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }
	}
}
