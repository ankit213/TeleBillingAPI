using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
	public class EmployeeDetailAC {

		[JsonProperty("name")]
		public string FullName { get; set; }

		[JsonProperty("email")]
		public string EmailId { get; set; }
		
	}
}
