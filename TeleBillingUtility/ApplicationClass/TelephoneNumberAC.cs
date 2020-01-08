using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class TelephoneNumberAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber1 { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("providername")]
        public string ProviderName { get; set; }

        [JsonProperty("isdelete")]
        public bool IsDelete { get; set; }

    }
}
