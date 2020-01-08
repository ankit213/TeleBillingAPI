using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class BusinessAssignedBillAC
    {
        [JsonProperty("exceluploadlogid")]
        public long ExcelUploadLogId { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("assigntype")]
        public string AssignType { get; set; }

        [JsonProperty("billamount")]
        public decimal BillAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelePhoneNumber { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("businessunitid")]
        public long BusinessUnitId { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }
    }

}
