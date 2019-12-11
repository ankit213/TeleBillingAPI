using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class DrpResponseAC
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set;} 
	}

    public class KeyValueResponseAC
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public class CallerDetailResponseAC
    {
        [JsonProperty("caller")]
        public string CallerNumber { get; set; }

        [JsonProperty("receiver")]
        public string ReceiverNumber { get; set; }
    }
}
