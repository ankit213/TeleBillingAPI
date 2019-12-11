using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class ReminderIdsAC
	{
		[JsonProperty("id")]
		public long Id { get; set; }
	}
}
