using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class MemoAC
	{
		[JsonProperty("id")]
		public long Id { get; set;}

		[JsonProperty("date")]
		public DateTime Date { get; set; }

		[JsonProperty("subject")]
		public string Subject { get; set; }

		[JsonProperty("month")]
		public int Month { get; set; }

		[JsonProperty("year")]
		public int Year { get; set; }

		[JsonProperty("providerid")]
		public long ProviderId { get; set; }

		[JsonProperty("provider")]
		public string ProviderName { get; set; }

		[JsonProperty("totalamount")]
		public decimal TotalAmount { get; set; }

		[JsonProperty("billids")]
		public List<long> BillIds { get; set;}

		[JsonProperty("bank")]
		public string Bank { get; set; }

		[JsonProperty("ibancode")]
		public string Ibancode { get; set; }

		[JsonProperty("swiftcode")]
		public string Swiftcode { get; set; }

		[JsonProperty("isbanktransaction")]
		public bool IsBankTransaction { get; set;}
	}
}
