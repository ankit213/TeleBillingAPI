using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
   public class ExcelMappingListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }

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
