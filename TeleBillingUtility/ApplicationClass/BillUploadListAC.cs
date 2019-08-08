using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillUploadListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")] // Month Year
        public string BillDate { get; set; }

        [JsonProperty("uploadeddate")]
        public DateTime UploadedDate { get; set; }
    }
}
