using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
   public class MappingServiceTypeFieldAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("dbcolumnname")]
        public string DbcolumnName { get; set; }

        [JsonProperty("displayfieldname")]
        public string DisplayFieldName { get; set; }

        [JsonProperty("isrequired")]
        public bool IsRequired { get; set; }

        //[JsonProperty("displayorder")]
        //public int DisplayOrder { get; set; }

        [JsonProperty("columnaddress")]
        public string ColumnAddress { get; set; }

        [JsonProperty("isspecial")]
        public bool IsSpecial { get; set; }

        [JsonProperty("formatfield")]
        public string FormatField { get; set; }


    }
}
