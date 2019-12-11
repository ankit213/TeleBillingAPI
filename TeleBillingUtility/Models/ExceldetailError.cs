using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.Models
{
    public partial class ExceldetailError
    {
        public long Id { get; set; }
        public string FileGuidNo { get; set; }
        public long? ExcelUploadLogId { get; set; }
        public long ServiceTypeId { get; set; }
        public string CallDate { get; set; }
        public string CallTime { get; set; }
        public string CallDuration { get; set; }
        public string CallerNumber { get; set; }
        public string CallerName { get; set; }
        public string ReceiverNumber { get; set; }
        public string ReceiverName { get; set; }
        public string CallAmount { get; set; }
        public string CallDataKb { get; set; }
        public string MessageCount { get; set; }
        public string TransType { get; set; }
        public string Destination { get; set; }
        public string GroupId { get; set; }
        public string SubscriptionType { get; set; }
        public string CallWithinGroup { get; set; }
        public string SiteName { get; set; }
        public string GroupDetail { get; set; }
        public string Bandwidth { get; set; }
        public string MonthlyPrice { get; set; }
        public string CommentOnPrice { get; set; }
        public string CommentOnBandwidth { get; set; }
        public string Description { get; set; }
        public string BusinessUnit { get; set; }
        public string CostCentre { get; set; }
        public string FinalAnnualChargesKd { get; set; }
        public string ServiceDetail { get; set; }
        public string InitialDiscountedMonthlyPriceKd { get; set; }
        public string InitialDiscountedAnnualPriceKd { get; set; }
        public string InitialDiscountedSavingMonthlyKd { get; set; }
        public string InitialDiscountedSavingYearlyKd { get; set; }
        public string ErrorSummary { get; set; }
    }
}
