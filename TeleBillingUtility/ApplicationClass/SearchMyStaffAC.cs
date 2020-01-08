using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class SearchMyStaffAC
    {

        [JsonProperty("monthid")]
        public int Month { get; set; }

        [JsonProperty("yearid")]
        public int Year { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("statusid")]
        public int StatusId { get; set; }

    }
}
