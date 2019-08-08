using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ExcelDetail
    {
        public long Id { get; set; }
        public long ExcelUploadLogId { get; set; }
        public long ServiceTypeId { get; set; }
        public DateTime? CallDate { get; set; }
        public TimeSpan? CallTime { get; set; }
        public long? CallDuration { get; set; }
        public string CallerNumber { get; set; }
        public string CallerName { get; set; }
        public string ReceiverNumber { get; set; }
        public string ReceiverName { get; set; }
        public decimal? CallAmount { get; set; }
        public long? CurrencyId { get; set; }
        public long? CallTransactionTypeId { get; set; }
        public string TransType { get; set; }
        public string Destiantion { get; set; }
        public long? GroupId { get; set; }
        public string SubscriptionType { get; set; }
        public bool? CallWithinGroup { get; set; }
        public bool? IsAssigned { get; set; }
        public long? EmployeeId { get; set; }
        public long? AssignType { get; set; }
        public long? BusinessUnitId { get; set; }
        public long? CostCenterId { get; set; }
        public string SiteName { get; set; }
        public string GroupDetail { get; set; }
        public string Bandwidth { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public string CommentOnPrice { get; set; }
        public string CommentOnBandwidth { get; set; }
        public string Description { get; set; }
        public string BusinessUnit { get; set; }
        public string CostCentre { get; set; }
        public decimal? FinalAnnualChargesKd { get; set; }
        public string ServiceDetail { get; set; }
        public decimal? InitialDiscountedMonthlyPriceKd { get; set; }
        public decimal? InitialDiscountedAnnualPriceKd { get; set; }
        public decimal? InitialDiscountedSavingMonthlyKd { get; set; }
        public decimal? InitialDiscountedSavingYearlyKd { get; set; }

        public virtual FixAssignType AssignTypeNavigation { get; set; }
        public virtual MstBusinessUnit BusinessUnitNavigation { get; set; }
        public virtual TransactionTypeSetting CallTransactionType { get; set; }
        public virtual MstCostCenter CostCenter { get; set; }
        public virtual MstCurrency Currency { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
    }
}
