using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class TelephoneAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber1 { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("accountnumber")]
        public string AccountNumber { get; set; }


        [JsonProperty("linetype")]
        public string LineType { get; set; }

        [JsonProperty("isassigned")]
        public bool IsAssigned { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive { get; set; }
    }
}
