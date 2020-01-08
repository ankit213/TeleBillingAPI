using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class PbxExcelMappingAC
    {
        public PbxExcelMappingAC()
        {
            List<MappingServiceTypeFieldAC> dbfieldList = new List<MappingServiceTypeFieldAC>();
        }
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
        [JsonProperty("deviceid")]
        public long DeviceId { get; set; }
        [JsonProperty("worksheetno")]
        public long WorkSheetNo { get; set; }
        [JsonProperty("haveheader")]
        public bool HaveHeader { get; set; }
        [JsonProperty("havetitle")]
        public bool HaveTitle { get; set; }
        [JsonProperty("titlename")]
        public string TitleName { get; set; }
        [JsonProperty("excelcolumnnamefortitle")]
        public string ExcelColumnNameForTitle { get; set; }
        [JsonProperty("excelreadingcolumn")]
        public string ExcelReadingColumn { get; set; }

        public List<MappingServiceTypeFieldAC> dbfieldList { get; set; }
    }
}
