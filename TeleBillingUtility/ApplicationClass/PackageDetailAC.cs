using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class PackageDetailAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("startdate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("servicetypeid")]
        public long? ServiceTypeId { get; set; }

        [JsonProperty("packagemonth")]
        public int PackageMonth { get; set; }

        [JsonProperty("packageminute")]
        public decimal? PackageMinute { get; set; }

        [JsonProperty("packagedata")]
        public int? PackageData { get; set; }

        [JsonProperty("packageamount")]
        public decimal PackageAmount { get; set; }

        [JsonProperty("localminute")]
        public decimal? LocalMinute { get; set; }

        [JsonProperty("roamingminute")]
        public decimal? RoamingMinute { get; set; }

        [JsonProperty("internationalcallminute")]
        public decimal? InternationalCallMinute { get; set; }

        [JsonProperty("ingroupminute")]
        public decimal? InGroupMinute { get; set; }

        [JsonProperty("localinternetdata")]
        public int? LocalInternetData { get; set; }

        [JsonProperty("internationalsharingdata")]
        public int? InternationalSharingData { get; set; }

        [JsonProperty("internationalroamingdata")]
        public int? InternationalRoaminData { get; set; }

        [JsonProperty("additionalmonth")]
        public int? AdditionalMonth { get; set; }

        [JsonProperty("additionalminute")]
        public decimal? AdditionalMinute { get; set; }

        [JsonProperty("additionaldata")]
        public int? AdditionalData { get; set; }

        [JsonProperty("additionalchargedurationamount")]
        public decimal? AdditionalChargeDurationAmount { get; set; }

        [JsonProperty("additionalchargeminuteamount")]
        public decimal? AdditionalChargeMinuteAmount { get; set; }

        [JsonProperty("additionalchargedataamount")]
        public decimal? AdditionalChargeDataAmount { get; set; }

        [JsonProperty("terminationfees")]
        public decimal? TerminationFees { get; set; }

        [JsonProperty("handsetdetailids")]
        public string HandsetDetailIds { get; set; }

        [JsonProperty("deviceamount")]
        public decimal? DeviceAmount { get; set; }

        [JsonProperty("internetdeviceid")]
        public long? InternetDeviceId { get; set; }

        [JsonProperty("devicepenaltyamount")]
        public decimal? DevicePenaltyAmount { get; set; }

        [JsonProperty("handsetlist")]
        public List<DrpResponseAC> HandsetList { get; set; }
    }
}
