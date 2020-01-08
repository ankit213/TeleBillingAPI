using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class MemoReportListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("refrenceno")]
        public string RefrenceNo { get; set; }

        [JsonProperty("memodate")]
        public string MemoDate { get; set; }

        [JsonProperty("bycheque")]
        public string ByCheque { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("ibancode")]
        public string IBANCode { get; set; }

        [JsonProperty("swiftcode")]
        public string SWIFTCode { get; set; }

        [JsonProperty("totalamount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("status")]
        public string MemoStatus { get; set; }

        [JsonProperty("approvedby")]
        public string ApprovedBy { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("approveddate")]
        public string ApprovedDate { get; set; }
    }
}
