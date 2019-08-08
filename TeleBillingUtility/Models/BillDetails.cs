using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class BillDetails
    {
        public long Id { get; set; }
        public long BillMasterId { get; set; }
        public long ServiceTypeId { get; set; }
        public long EmployeeBillId { get; set; }
        public long? AssignTypeId { get; set; }
        public DateTime? CallDate { get; set; }
        public TimeSpan? CallTime { get; set; }
        public long? CallDuration { get; set; }
        public decimal? CallAmount { get; set; }
        public string CallerNumber { get; set; }
        public string CallerName { get; set; }
        public string ReceiverNumber { get; set; }
        public string ReceiverName { get; set; }
        public string TransType { get; set; }
        public string Destination { get; set; }
        public long? GroupId { get; set; }
        public string SubscriptionType { get; set; }
        public long? CallTransactionTypeId { get; set; }
        public bool? CallIwithInGroup { get; set; }
        public long? CallIdentificationType { get; set; }
        public long? CallIdentifedBy { get; set; }
        public DateTime? CallIdentifiedDate { get; set; }
        public string EmployeeComment { get; set; }
        public long? BusinessUnitId { get; set; }
        public long? CallAssignedBy { get; set; }
        public DateTime? CallAssignedDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public bool IsAutoAssigned { get; set; }

        public virtual BillMaster BillMaster { get; set; }
        public virtual FixCallType CallIdentificationTypeNavigation { get; set; }
        public virtual TransactionTypeSetting CallTransactionType { get; set; }
        public virtual EmployeeBillMaster EmployeeBill { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
    }
}
