using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class TemplateAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("templatetype")]
        public string TemplateType { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("from")]
        public string EmailFrom { get; set; }

        [JsonProperty("bcc")]
        public string EmailBcc { get; set; }
    }
}
