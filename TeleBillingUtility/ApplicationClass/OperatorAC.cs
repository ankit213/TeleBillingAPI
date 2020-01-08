using Newtonsoft.Json;
using System;

namespace TeleBillingUtility.ApplicationClass
{
    public class OperatorCallLogAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("calldate")]
        public DateTime CallDate { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPfnumber { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("dialednumber")]
        public string DialedNumber { get; set; }

        [JsonProperty("providername")]
        public string ProviderName { get; set; }

        [JsonProperty("calltypename")]
        public string CallTypeName { get; set; }

    }
}
