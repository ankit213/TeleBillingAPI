using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
    public class DrpCountryAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("currencyId")]
        public long currencyId { get; set; }
        
        [JsonProperty("currency")]
        public string currency { get; set; }
    }
}
