using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
  public class BillUploadAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("monthid")]
        public int MonthId { get; set; }

        [JsonProperty("month")]
        public string Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("yearid")]
        public int YearId { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("deviceid")]
        public int DeviceId { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("excelfilename1")]
        public string ExcelFileName1 { get; set; }

        [JsonProperty("excelfilename2")]
        public string ExcelFileName2 { get; set; }

        [JsonProperty("excelfilename3")]
        public string ExcelFileName3 { get; set; }

        [JsonProperty("servicetypes")]
        public List<DrpResponseAC> ServiceTypes { get; set; }
    }
}
