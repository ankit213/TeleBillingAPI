using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class LineManagerApprovalAC {

		[JsonProperty("linemanagerapprovalbills")]
		public List<CurrentBillAC> LineManagerApprovalBills { get; set; }

		[JsonProperty("isapprove")]
		public bool IsApprove { get; set; }

	}
}
