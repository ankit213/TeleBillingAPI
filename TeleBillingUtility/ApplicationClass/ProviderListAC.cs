using Newtonsoft.Json;
using System;

namespace TeleBillingUtility.ApplicationClass
{
	public class ProviderListAC
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("servicetypes")]
		public string ServiceTypes { get; set; }

		[JsonProperty("updateddate")]
		public DateTime UpdatedDate { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("isactive")]
		public bool IsActive { get; set; }
	}
}
