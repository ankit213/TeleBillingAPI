using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class EmployeeAC
    {
        [JsonProperty("id")]
        public long UserId { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPfnumber { get; set; }

        [JsonProperty("name")]
        public string FullName { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }
    }
}
