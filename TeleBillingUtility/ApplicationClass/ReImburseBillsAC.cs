using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
	public class ReImburseBillsAC {

		[JsonProperty("id")]
		public long Id { get; set;}

		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("billmonth")]
		public int BillMonth { get; set; }

		[JsonProperty("billyear")]
		public int BillYear { get; set; }

		[JsonProperty("providerid")]
		public long Providerid { get; set; }

		[JsonProperty("billdate")]
		public string BillDate { get; set; }

		[JsonProperty("employeeid")]
		public long EmployeeId { get; set; }

		[JsonProperty("employeename")]
		public string EmployeeName { get; set; }

		[JsonProperty("telephonenumber")]
		public string TelephoneNumber { get; set; }

		[JsonProperty("reimbruseamount")]
		public decimal ReImbruseAmount { get; set;}

		[JsonProperty("amount")]
		public decimal Amount { get; set; }

		[JsonProperty("managername")]
		public string ManagerName { get; set; }

		[JsonProperty("employeebillid")]
		public long EmployeeBillId { get; set; }

		[JsonProperty("billnumber")]
		public string BillNumber { get; set; }
	}
}
