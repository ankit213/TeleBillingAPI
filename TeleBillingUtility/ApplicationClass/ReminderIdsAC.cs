using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class ReminderIdsAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
