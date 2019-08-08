using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
	public class InternetDeviceAC {

		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
