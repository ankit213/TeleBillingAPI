using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillUploadAC
    {
        BillUploadAC()
        {
            MergedWithId = 0;
            IsApproved = false;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("mergedwithid")]
        public long MergedWithId { get; set; }

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

        [JsonProperty("isapproved")]
        public bool IsApproved { get; set; }

        [JsonProperty("servicetypes")]
        public List<DrpResponseAC> ServiceTypes { get; set; }
    }

    public class PbxBillUploadAC
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

        [JsonProperty("deviceid")]
        public int DeviceId { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("excelfilename1")]
        public string ExcelFileName1 { get; set; }

    }

    public class ExcelUploadIdCountAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }
    }

}
