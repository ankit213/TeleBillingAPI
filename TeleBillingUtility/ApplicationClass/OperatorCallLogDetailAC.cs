using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class OperatorCallLogDetailAC {

		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("calldate")]
		public DateTime CallDate { get; set; }
		
		[JsonProperty("employee")]
		public EmployeeAC EmployeeAC { get; set; }
		
		[JsonProperty("extensionnumber")]
		public string ExtensionNumber { get; set; }

		[JsonProperty("dialednumber")]
		public string DialedNumber { get; set; }

		[JsonProperty("providerid")]
		public long ProviderId { get; set; }

		[JsonProperty("calltypeid")]
		public long CallTypeId { get; set; }
	}
}
