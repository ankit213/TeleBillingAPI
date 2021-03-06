﻿using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class PbxExcelMappingListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("worksheetno")]
        public string WorkSheetNo { get; set; }

        [JsonProperty("haveheader")]
        public bool HaveHeader { get; set; }

        [JsonProperty("havetitle")]
        public bool HaveTitle { get; set; }

        [JsonProperty("titlename")]
        public string TitleName { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive { get; set; }
    }
}
