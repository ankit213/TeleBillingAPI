using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class LoginReponseAC
	{
		[JsonProperty("accesstoken")]
		public string AccessToken { get;set;}

		[JsonProperty("statuscode")]
		public int StatusCode { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
