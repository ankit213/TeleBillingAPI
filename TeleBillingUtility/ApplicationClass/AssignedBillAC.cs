using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class AssignedBillAC
    {
        [JsonProperty("exceluploadlogid")]
        public long ExcelUploadLogId { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("assigntype")]
        public string AssignType { get; set; }

        [JsonProperty("billamount")]
        public decimal BillAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("employeeId")]
        public long EmployeeId { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }

    }

}
