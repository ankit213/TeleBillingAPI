using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class ChangeBillStatusAC
    {
        [JsonProperty("employeebillmasterid")]
        public long EmployeeBillMasterId { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("employeeid")]
        public long EmployeeId { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("employeebillstatus")]
        public int EmployeeBillStatus { get; set; }

        [JsonProperty("billstatus")]
        public string BillStatus { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("employebillchangestatus")]
        public int EmployeeBillChangeStatus { get; set; }

        [JsonProperty("managername")]
        public string ManagerName { get; set; }
    }
}
