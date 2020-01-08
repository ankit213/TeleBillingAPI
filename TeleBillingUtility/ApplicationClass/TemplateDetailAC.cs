using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class TemplateDetailAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("templatetypeid")]
        public long EmailTemplateTypeId { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("from")]
        public string EmailFrom { get; set; }

        [JsonProperty("bcc")]
        public string EmailBcc { get; set; }

        [JsonProperty("text")]
        public string EmailText { get; set; }

    }
}
