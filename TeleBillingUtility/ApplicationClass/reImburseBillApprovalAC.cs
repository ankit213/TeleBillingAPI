using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class ReImburseBillApprovalAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("isapproved")]
        public bool IsApproved { get; set; }

        [JsonProperty("approvalcomment")]
        public string ApprovalComment { get; set; }
    }
}
