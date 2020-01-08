using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class VendorWisePackageDetailsAC
    {
        [JsonProperty("providername")]
        public string ProviderName { get; set; }

        [JsonProperty("packagedetaillistac")]
        public List<PackageDetailListAC> PackageDetailListAC { get; set; }

    }

    public class PackageDetailListAC
    {

        [JsonProperty("providername")]
        public string ProviderName { get; set; }

        [JsonProperty("packagename")]
        public string PackageName { get; set; }

        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("packageamount")]
        public string PackageAmount { get; set; }

        [JsonProperty("packagestartdate")]
        public string PackageStartDate { get; set; }

        [JsonProperty("totalpackageallocationcount")]
        public long TotalPackageAllocationCount { get; set; }
    }

}
