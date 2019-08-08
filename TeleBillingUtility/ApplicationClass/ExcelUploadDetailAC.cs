using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{

    public class MobilityExcelUploadDetailStringAC
    {
  
        [JsonProperty("calldate")]
        public string CallDate { get; set; }

        [JsonProperty("calltime")]
        public string CallTime { get; set; }

        [JsonProperty("callduration")]
        public string CallDuration { get; set; }

        [JsonProperty("callernumber")]
        public string CallerNumber { get; set; }

        [JsonProperty("callername")]
        public string CallerName { get; set; }

        [JsonProperty("receivernumber")]
        public string ReceiverNumber { get; set; }

        [JsonProperty("receivername")]
        public string ReceiverName { get; set; }

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

        [JsonProperty("calltype")]
        public string CallType { get; set; }

        [JsonProperty("transtype")]
        public string TransType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("subscriptiontype")]
        public string SubscriptionType { get; set; }

        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class MadaExcelUploadDetailStringAC
    {
        [JsonProperty("sitename")]
        public string SiteName { get; set; }

        [JsonProperty("servicedetail")]
        public string ServiceDetail { get; set; }

        [JsonProperty("bandwidth")]
        public string Bandwidth { get; set; }

        [JsonProperty("costcentre")]
        public string CostCentre { get; set; }

        [JsonProperty("initialdiscountedmonthlyprice")]
        public string InitialDiscountedMonthlyPriceKd { get; set; }

        [JsonProperty("initialdiscountedannualPrice")]
        public string InitialDiscountedAnnualPriceKd { get; set; }

        [JsonProperty("initialdiscountedsavingmonthly")]
        public string InitialDiscountedSavingMonthlyKd { get; set; }

        [JsonProperty("initialdiscountedsavingyearly")]
        public string InitialDiscountedSavingYearlyKd { get; set; }

        [JsonProperty("monthlyprice")]
        public string MonthlyPrice { get; set; }

        [JsonProperty("finalannualcharges")]
        public string FinalAnnualChargesKd { get; set; }
       
        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class InternetServiceExcelUploadDetailStringAC
    {

        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("sitename")]
        public string SiteName { get; set; }

        [JsonProperty("groupdetail")]
        public string GroupDetail { get; set; }

        [JsonProperty("bandwidth")]
        public string Bandwidth { get; set; }

        [JsonProperty("monthlyprice")]
        public string MonthlyPrice { get; set; }

        [JsonProperty("commentonprice")]
        public string CommentOnPrice { get; set; }

        [JsonProperty("commentonbandwidth")]
        public string CommentOnBandwidth { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class DataCenterFacilityExcelUploadDetailStringAC
    {
       
        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("sitename")]
        public string SiteName { get; set; }

        [JsonProperty("groupdetail")]
        public string GroupDetail { get; set; }

        [JsonProperty("bandwidth")]
        public string Bandwidth { get; set; }

        [JsonProperty("monthlyprice")]
        public string MonthlyPrice { get; set; }

        [JsonProperty("commentonprice")]
        public string CommentOnPrice { get; set; }

        [JsonProperty("commentonbandwidth")]
        public string CommentOnBandwidth { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class ManagedHostingServiceExcelUploadDetailStringAC
    {
       

        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("sitename")]
        public string SiteName { get; set; }

        [JsonProperty("groupdetail")]
        public string GroupDetail { get; set; }

        [JsonProperty("bandwidth")]
        public string Bandwidth { get; set; }

        [JsonProperty("monthlyprice")]
        public string MonthlyPrice { get; set; }

        [JsonProperty("commentonprice")]
        public string CommentOnPrice { get; set; }

        [JsonProperty("commentonbandwidth")]
        public string CommentOnBandwidth { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }
}
