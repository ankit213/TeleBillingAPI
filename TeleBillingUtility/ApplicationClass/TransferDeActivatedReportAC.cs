using Newtonsoft.Json;
using System;

namespace TeleBillingUtility.ApplicationClass
{
	public class TransferDeActivatedReportAC
	{
	
		[JsonProperty("mobilenumber")]
		public string MobileNumber { get; set; }

		[JsonProperty("assignetype")]
		public string AssgineType { get; set; }

		[JsonProperty("employeename")]
		public string EmployeeName { get; set; }

		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("linestatus")]
		public string LineStatus { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }

		[JsonProperty("istransferred")]
		public string IsTransferred { get; set;}

		[JsonProperty("affecteddate")]
		public string AffectedDate { get; set; }

	}
}
