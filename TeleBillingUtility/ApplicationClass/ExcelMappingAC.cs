using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
   public class ExcelMappingAC
    {
       public  ExcelMappingAC()
        {
            List<MappingServiceTypeFieldAC> dbfieldList = new List<MappingServiceTypeFieldAC>();
        }
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }



        [JsonProperty("servicetypeidinline")]
        public List<DrpResponseAC> ServiceTypeIdInline { get; set; }


        [JsonProperty("servicetypesinline")]
        public string ServiceTypesInline { get; set; }


        [JsonProperty("providerid")]
        public long ProviderId { get; set; }
        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }
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
