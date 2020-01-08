using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class ProviderBillChartDetailAC
	{
		public ProviderBillChartDetailAC()
		{
			transTypeDetailsACs = new List<TransTypeDetailsAC>();
		}


		[JsonProperty("monthyears")]
		public string MonthYears { get; set;}

		[JsonProperty("totalamount")]
		public decimal TotalAmount { get; set;}

		[JsonProperty("currency")]
		public string Currency { get; set;}

		[JsonProperty("transtypedetails")]
		public List<TransTypeDetailsAC> transTypeDetailsACs { get;set; }


	}

	public class TransTypeDetailsAC
	{
		[JsonProperty("transtype")]
		public string TransType { get; set; }

		[JsonProperty("billamount")]
		public Decimal? BillAmount { get;set;}

		[JsonProperty("backgroundcolor")]
		public string BackGroundColor { get; set;}
	}
}
