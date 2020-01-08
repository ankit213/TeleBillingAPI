using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class MemoApprovalAC
    {

        [JsonProperty("billmemoacs")]
        public List<BillMemoAC> billMemoACs { get; set; }

        [JsonProperty("isapproved")]
        public bool IsApprvoed { get; set; }

    }
}
