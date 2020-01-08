using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class HandsetDetailAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
