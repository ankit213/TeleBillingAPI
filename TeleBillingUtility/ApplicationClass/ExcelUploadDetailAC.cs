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

        [JsonProperty("calldatakb")]
        public string CallDataKB { get; set; }

        [JsonProperty("messagecount")]
        public string MessageCount { get; set; }


        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class StaticIPExcelUploadDetailStringAC
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

        [JsonProperty("calldatakb")]
        public string CallDataKB { get; set; }

        [JsonProperty("messagecount")]
        public string MessageCount { get; set; }


        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class VoiceOnlyExcelUploadDetailStringAC
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

        [JsonProperty("calldatakb")]
        public string CallDataKB { get; set; }

        [JsonProperty("messagecount")]
        public string MessageCount { get; set; }


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

        [JsonProperty("initialdiscountedmonthlypricekd")]
        public string InitialDiscountedMonthlyPriceKd { get; set; }

        [JsonProperty("initialdiscountedannualpricekd")]
        public string InitialDiscountedAnnualPriceKd { get; set; }

        [JsonProperty("initialdiscountedsavingmonthlykd")]
        public string InitialDiscountedSavingMonthlyKd { get; set; }

        [JsonProperty("initialdiscountedsavingyearlykd")]
        public string InitialDiscountedSavingYearlyKd { get; set; }

        [JsonProperty("monthlyprice")]
        public string MonthlyPrice { get; set; }

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

        [JsonProperty("finalannualchargeskd")]
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

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

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

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

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

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

        [JsonProperty("commentonprice")]
        public string CommentOnPrice { get; set; }

        [JsonProperty("commentonbandwidth")]
        public string CommentOnBandwidth { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

    public class VoipExcelUploadDetailStringAC
    {

        [JsonProperty("calldate")]
        public string CallDate { get; set; }

        [JsonProperty("calltime")]
        public string CallTime { get; set; }

        [JsonProperty("callduration")]
        public string CallDuration { get; set; }

        [JsonProperty("callernumber")]
        public string CallerNumber { get; set; }    

        [JsonProperty("receivernumber")]
        public string ReceiverNumber { get; set; }
        
        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }

     public class PbxExcelUploadDetailStringAC
    {

        [JsonProperty("calldate")]
        public string CallDate { get; set; }

        [JsonProperty("calltime")]
        public string CallTime { get; set; }

        [JsonProperty("callduration")]
        public string CallDuration { get; set; }

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

        [JsonProperty("connectingparty")]
        public string ConnectingParty { get; set; }

        [JsonProperty("name1")]
        public string Name1 { get; set; }

        [JsonProperty("otherparty")]
        public string OtherParty { get; set; }

        [JsonProperty("name2")]
        public string Name2 { get; set; }

        [JsonProperty("codenumber")]
        public string CodeNumber { get; set; }

        [JsonProperty("name3")]
        public string Name3 { get; set; }

        [JsonProperty("classificationcode")]
        public string ClassificationCode { get; set; }

        [JsonProperty("name4")]
        public string Name4 { get; set; }

        [JsonProperty("calltype")]
        public string CallType { get; set; }

        [JsonProperty("place")]
        public string Place { get; set; }

        [JsonProperty("band")]
        public string Band { get; set; }

        [JsonProperty("rate")]
        public string Rate { get; set; }

        [JsonProperty("destinationyype")]
        public string DestinationType { get; set; }

        [JsonProperty("distantnumber")]
        public string DistantNumber { get; set; }

        [JsonProperty("ringingtime")]
        public string RingingTime { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }    


        [JsonProperty("erromessage")]
        public string ErrorMessage { get; set; }

    }
}
