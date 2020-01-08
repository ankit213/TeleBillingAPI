
using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class MemoBillDetailAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("memorefrenceno")]
        public string MemoRefrenceNo { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }


        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

        [JsonProperty("createddate")]
        public string CreatedDate { get; set; }

    }
}
