using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingUtility.ApplicationClass
{
	public class ViewBillDetailAC {

		[JsonProperty("lstunassignedbill")]
		public List<UnAssignedBillAC> lstUnAssignedBill { get; set; }

		[JsonProperty("packageservicelist")]
		public List<PackageServiceAC> PackageServiceList { get; set;}

		[JsonProperty("currency")]
		public string Currency {  get; set;}
		
		[JsonProperty("isdeducateamount")]
		public bool IsDeducateAmount { get; set; }

		[JsonProperty("isdisplayonly")]
		public bool IsDisplayOnly { get; set;}
	}

	public class PackageServiceAC {

		[JsonProperty("packageid")]
		public long PackageId { get; set; }

		[JsonProperty("servicetypeid")]
		public long ServiceTypeId { get; set; }

		[JsonProperty("servicetype")]
		public string ServiceTypeName { get; set; }

		[JsonProperty("packagename")]
		public string PackageName { get; set; }

		[JsonProperty("packagelimitamount")]
		public decimal PackageLimitAmount { get;set;}
	}
}
