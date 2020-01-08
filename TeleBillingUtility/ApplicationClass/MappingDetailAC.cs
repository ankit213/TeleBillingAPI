using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class MappingDetailAC
    {

        public MappingDetailAC()
        {
            DBFiledMappingList = new List<DBFiledMappingAC>();
            ProviderId = 0;
            ServiceTypeId = 0;

        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("serviceTypeid")]
        public long ServiceTypeId { get; set; }



        [JsonProperty("worksheetno")]
        public long WorkSheetNo { get; set; }

        [JsonProperty("haveheader")]
        public bool HaveHeader { get; set; }

        [JsonProperty("havetitle")]
        public bool HaveTitle { get; set; }

        [JsonProperty("titlename")]
        public string TitleName { get; set; }

        [JsonProperty("excelcolumnNamefortitle")]
        public string ExcelColumnNameForTitle { get; set; }

        [JsonProperty("excelreadingcolumn")]
        public string ExcelReadingColumn { get; set; }

        // Added ON 2019/10/08
        [JsonProperty("iscommonmapped")]
        public bool? IsCommonMapped { get; set; }

        [JsonProperty("mappedmappingid")]
        public long? MappedMappingId { get; set; }

        [JsonProperty("mappedservicetypeid")]
        public long? MappedServiceTypeId { get; set; }



        public List<DBFiledMappingAC> DBFiledMappingList { get; set; }
    }

    public class DBFiledMappingAC
    {
        [JsonProperty("mappingdetailid")]
        public long MappingDetailId { get; set; }

        [JsonProperty("dbcolumnname")]
        public string DBColumnName { get; set; }

        [JsonProperty("excelcolumnname")]
        public string ExcelcolumnName { get; set; }

        [JsonProperty("formatfield")]
        public string FormatField { get; set; }
    }

    public class MappingDetailPbxAC
    {

        public MappingDetailPbxAC()
        {
            DBFiledMappingList = new List<DBFiledMappingAC>();

            DeviceId = 0;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

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

        [JsonProperty("excelcolumnNamefortitle")]
        public string ExcelColumnNameForTitle { get; set; }

        [JsonProperty("excelreadingcolumn")]
        public string ExcelReadingColumn { get; set; }

        public List<DBFiledMappingAC> DBFiledMappingList { get; set; }
    }
}
