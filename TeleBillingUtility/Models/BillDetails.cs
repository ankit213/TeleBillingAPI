using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Billdetails
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
        public int? CallIdentificationType { get; set; }
        public long? CallIdentifedBy { get; set; }
        public DateTime? CallIdentifiedDate { get; set; }
        public string EmployeeComment { get; set; }
        public long? BusinessUnitId { get; set; }
        public long? CallAssignedBy { get; set; }
        public DateTime? CallAssignedDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        public bool? IsAutoAssigned { get; set; }
		public string Description { get; set;}
        public virtual Billmaster BillMaster { get; set; }
        public virtual FixCalltype CallIdentificationTypeNavigation { get; set; }
        public virtual Transactiontypesetting CallTransactionType { get; set; }
        public virtual Employeebillmaster EmployeeBill { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
    }
}
