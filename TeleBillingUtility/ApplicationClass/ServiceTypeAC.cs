using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
	public class ServiceTypeAC
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("isactive")]
		public bool IsActive { get; set; }

		[JsonProperty("isbusinessonly")]
		public bool IsBusinessOnly { get; set; }
	}
}
