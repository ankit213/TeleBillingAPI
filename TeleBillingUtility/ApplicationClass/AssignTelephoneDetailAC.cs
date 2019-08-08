using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class AssignTelephoneDetailAC {

		[JsonProperty("id")]
		public long Id { get; set; }
		
		[JsonProperty("telephonenumber")]
		public TelephoneNumberAC TelephoneNumberData { get; set; }
		
		[JsonProperty("assigntypeid")]
		public long AssignTypeId { get; set; }

		[JsonProperty("employee")]
		public EmployeeAC EmployeeData { get; set; }
		

		[JsonProperty("linestatusid")]
		public long LineStatusId { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }

		[JsonProperty("packageDetails")]
		public List<TelePhonePackageDetails> TelePhonePackageDetails { get; set;}
	}

	public class TelePhonePackageDetails
	{
		[JsonProperty("packageid")]
		public long PackageId { get; set; }

		[JsonProperty("servicetypeid")]
		public long ServiceId { get; set; }

		[JsonProperty("startdate")]
		public DateTime StartDate { get; set; }

		[JsonProperty("enddate")]
		public DateTime EndDate { get; set; }
	}
}
