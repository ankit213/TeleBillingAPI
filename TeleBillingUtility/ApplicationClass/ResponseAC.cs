using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class ResponseAC
	{
		[JsonProperty("statuscode")]
		public int StatusCode { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}


}
