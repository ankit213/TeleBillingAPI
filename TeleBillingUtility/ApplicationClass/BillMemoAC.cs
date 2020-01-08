using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillMemoAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("refrence")]
        public string RefrenceNo { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("isapproved")]
        public bool? IsApproved { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("ibancode")]
        public string Ibancode { get; set; }

        [JsonProperty("swiftcode")]
        public string Swiftcode { get; set; }
    }
}
