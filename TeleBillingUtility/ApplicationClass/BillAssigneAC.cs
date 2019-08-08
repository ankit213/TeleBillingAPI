using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class BillAssigneAC {

		[JsonProperty("unassignedbills")]
		public List<UnAssignedBillAC> UnAssignedBillList { get; set; }

		[JsonProperty("assignetype")]
		public long AssigneType {  get; set;}
		
		[JsonProperty("businessunit")]
		public long? BusinessUnitId { get; set; }

		[JsonProperty("employee")]
		public EmployeeAC EmployeeData { get; set; }
	}
}
