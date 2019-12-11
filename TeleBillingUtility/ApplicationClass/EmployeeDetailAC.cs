using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
	public class EmployeeDetailAC {

		[JsonProperty("name")]
		public string FullName { get; set; }

		[JsonProperty("email")]
		public string EmailId { get; set; }

		[JsonProperty("rolename")]
		public string RoleName { get; set;}

		[JsonProperty("imagepath")]
		public string ImagePath { get; set;}
		
	}
}
