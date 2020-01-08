using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class EmailTemplateTagAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("templatetag")]
        public string TemplateTag { get; set; }

        [JsonProperty("templatetext")]
        public string TemplateText { get; set; }
    }
}
