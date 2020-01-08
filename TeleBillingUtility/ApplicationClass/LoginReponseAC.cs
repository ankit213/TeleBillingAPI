using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class LoginReponseAC
    {
        [JsonProperty("accesstoken")]
        public string AccessToken { get; set; }

		[JsonProperty("roleid")]
		public long RoleId { get; set;}

        [JsonProperty("statuscode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
