using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class RoleAC
    {
        [JsonProperty("roleid")]
        public long RoleId { get; set; }

        [JsonProperty("rolename")]
        public string RoleName { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive { get; set; }
    }
}
