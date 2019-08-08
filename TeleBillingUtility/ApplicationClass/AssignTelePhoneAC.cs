using Newtonsoft.Json;
using System;
namespace TeleBillingUtility.ApplicationClass
{
	public class AssignTelePhoneAC
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("telephonenumber")]
		public string TelephoneNumber { get; set; }

		[JsonProperty("employeename")]
		public string EmployeeName { get; set; }

		[JsonProperty("emppfnumber")]
		public string EmpPFNumber { get; set; }

		[JsonProperty("description")]
		public string Descrption { get; set; }

		[JsonProperty("department")]
		public string Department { get; set; }

		[JsonProperty("assigntype")]
		public string AssignType { get; set; }

		[JsonProperty("costcenter")]
		public string CostCenter { get; set; }

		[JsonProperty("linestatus")]
		public string LineStatus { get; set; }

		[JsonProperty("startdate")]
		public DateTime? StartDate { get; set; }

		[JsonProperty("enddate")]
		public DateTime? EndDate { get; set; }

		[JsonProperty("isactive")]
		public bool IsActive { get; set; }
	}
}
