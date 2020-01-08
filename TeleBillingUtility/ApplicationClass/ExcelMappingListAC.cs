using Newtonsoft.Json;

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

        [JsonProperty("mergeservices")]
        public string MergeServices { get; set; }


        [JsonProperty("worksheetno")]
        public string WorkSheetNo { get; set; }

        [JsonProperty("titlename")]
        public string TitleName { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive
        {

            get { return (IsActiveInt == 1 ? true : false); }
            set
            {
                IsActive = value;
            }
        }


        [JsonProperty("haveheader")]
        public bool HaveHeader
        {

            get { return (HaveHeaderInt == 1 ? true : false); }
            set
            {
                HaveHeader = value;
            }
        }

        [JsonProperty("havetitle")]
        public bool HaveTitle
        {

            get { return (HaveTitleInt == 1 ? true : false); }
            set
            {
                HaveTitle = value;
            }
        }

        public long IsActiveInt { get; set; }
        public long HaveHeaderInt { get; set; }
        public long HaveTitleInt { get; set; }


    }
}
