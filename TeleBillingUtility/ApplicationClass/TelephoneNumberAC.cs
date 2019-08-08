using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class TelephoneNumberAC
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("telephonenumber")]
		public string TelephoneNumber1 { get; set; }

	}
}
