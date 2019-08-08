using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
	public class BillAC
	{
		 [JsonProperty("provider")]
		 public string Provider { get; set;}

		 [JsonProperty("providerid")]
		 public long ProviderId { get; set;}

		 [JsonProperty("month")]
		 public int Month {get; set;}

		 [JsonProperty("year")]
		 public int Year { get; set;}

		 [JsonProperty("billdate")]
		 public string BillDate { get; set;}

		 [JsonProperty("employeeid")]
		 public long EmployeeId { get; set;}

		 [JsonProperty("employeename")]
		 public string EmployeeName { get; set; }

		 [JsonProperty("mobilenumber")]
		 public string MobileNumber { get; set; }

		 [JsonProperty("assignedtype")]
		 public string AssignedType { get; set; }

		 [JsonProperty("amount")]
		 public decimal Amount { get; set; }

		 [JsonProperty("currency")]
		 public string Currency { get; set; }

		 [JsonProperty("businessunitid")]
		 public long BusinessUnitId { get; set;}

		 [JsonProperty("costcenter")]
		 public string CostCenter { get; set; }

		 [JsonProperty("costcenterid")]
		 public long CostCenterId { get; set;}

		 [JsonProperty("businessunit")]
		 public string BusinessUnit { get; set; }

		 [JsonProperty("description")]
		 public string Description { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }
	}
}
