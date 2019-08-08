using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
	public class BillAllocationListAC
	{
		[JsonProperty("assignedbills")]
		public List<AssignedBillAC>  AssignedBillList { get;set;}

		[JsonProperty("unassignedbills")]
		public List<UnAssignedBillAC> UnAssignedBillList { get; set; }

		[JsonProperty("businessassignedbills")]
		public List<BusinessAssignedBillAC> BusinessAssignedBillList { get; set; }

		[JsonProperty("providerid")]
		public long ProviderId { get; set; }

		[JsonProperty("monthid")]
		public int Month { get; set; }

		[JsonProperty("yearid")]
		public int Year { get; set; }

		[JsonProperty("toassignetype")]
		public int ToAssigneType { get; set; }

		[JsonProperty("servicetypes")]
		public List<DrpResponseAC> ServiceTypes { get; set; }
		
	}
}
