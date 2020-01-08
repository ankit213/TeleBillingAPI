using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillAllocationAC
    {
        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("monthid")]
        public int Month { get; set; }

        [JsonProperty("yearid")]
        public int Year { get; set; }

        [JsonProperty("toassignetype")]
        public int ToAssigneType { get; set; }

        [JsonProperty("servicetypes")]
        public List<DrpResponseAC> ServiceTypes { get; set; }

        [JsonProperty("setduedate")]
        public DateTime BillDueDate { get; set; }

    }
}
