using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ExcelCommonTable
    {
        public long Id { get; set; }
        public long ExcelUploadLogId { get; set; }
        public long ServiceTypeId { get; set; }
        public DateTime? CallDate { get; set; }
        public string CallTime { get; set; }
        public decimal? CallDuration { get; set; }
        public string CallerNumber { get; set; }
        public string CallerName { get; set; }
        public string ReceiverNumber { get; set; }
        public string ReceiverName { get; set; }
        public decimal CallAmount { get; set; }
        public long? CurrencyId { get; set; }
        public long? CallType { get; set; }
        public string TransType { get; set; }
        public string Description { get; set; }
        public string Destination { get; set; }
        public string SubscriptionType { get; set; }
        public bool? CallWithinGroup { get; set; }
        public long? EmployeeId { get; set; }
        public string SiteName { get; set; }
        public string GroupDetail { get; set; }
        public string Bandwidth { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public string CommentOnPrice { get; set; }
        public string CommentOnBandwidth { get; set; }
        public long? BusinessUnitId { get; set; }
        public long? TransactionId { get; set; }
    }
}
