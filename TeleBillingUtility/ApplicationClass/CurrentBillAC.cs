using Newtonsoft.Json;
using System;

namespace TeleBillingUtility.ApplicationClass
{
	public class CurrentBillAC
	{
		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("billdate")]
		public string BillDate { get; set; }

		[JsonProperty("employeename")]
		public string EmployeeName { get; set; }

		[JsonProperty("telephonenumber")]
		public string TelephoneNumber { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }

		[JsonProperty("assignetype")]
		public string AssigneType { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("modifieddate")]
		public DateTime? UpdatedDate { get; set; }

		[JsonProperty("billstatus")]
		public string BillStatus { get; set; }

		[JsonProperty("employeebillstatus")]
		public int EmployeeBillStatus { get; set; }

		[JsonProperty("employeebillmasterid")]
		public long EmployeeBillMasterId { get; set; }

		[JsonProperty("comment")]
		public string ApprovalComment { get; set; }

		[JsonProperty("billnumber")]
		public string BillNumber { get; set; }

		[JsonProperty("managername")]
		public string ManagerName {  get; set;}

		[JsonProperty("isallowreimbrusment")]
		public bool IsAllowToReImbrusment { get; set; }
		
		[JsonProperty("isallowtoreidentification")]
		public bool IsAllowToReIdentification { get; set; }

		[JsonProperty("previousemployeebillid")]
		public long? PreviousEmployeeBillId { get; set; }

		[JsonProperty("isreimbursementrequest")]
		public bool IsReImbursementRequest { get; set;}
	
		[JsonProperty("isallowidentificatication")]
		public bool IsAllowIdentificatication { get; set; }
		
	}
}
