using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class TelephoneDetailAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber1 { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("accountnumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("linetypeid")]
        public long LineTypeId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isdataroaming")]
        public bool IsDataRoaming { get; set; }

        [JsonProperty("isinternationalcalls")]
        public bool IsInternationalCalls { get; set; }

        [JsonProperty("isvoiceroaming")]
        public bool IsVoiceRoaming { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
