using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class ReImbursementRequestAC
    {
        [JsonProperty("employeebillmasterid")]
        public long EmployeeBillMasterId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

    }
}
