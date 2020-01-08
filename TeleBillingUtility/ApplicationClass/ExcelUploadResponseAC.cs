using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class ExcelUploadResponseAC
    {

        [JsonProperty("statuscode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("filepath")]
        public string FilePath { get; set; }

        [JsonProperty("filenameguid")]
        public string FileNameGuid { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

    }
}
