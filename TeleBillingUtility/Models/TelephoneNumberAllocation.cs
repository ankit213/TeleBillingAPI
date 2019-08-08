using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class TelephoneNumberAllocation
    {
        public TelephoneNumberAllocation()
        {
            TelePhoneNumberAllocationPackage = new HashSet<TelePhoneNumberAllocationPackage>();
        }

        public long Id { get; set; }
        public string TelephoneNumber { get; set; }
        public long TelephoneNumberId { get; set; }
        public long AssignTypeId { get; set; }
        public long EmployeeId { get; set; }
        public string EmpPfnumber { get; set; }
        public string Reason { get; set; }
        public long? BusinessUnitId { get; set; }
        public long LineStatusId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixAssignType AssignType { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual FixLineStatus LineStatus { get; set; }
        public virtual TelephoneNumber TelephoneNumberNavigation { get; set; }
        public virtual ICollection<TelePhoneNumberAllocationPackage> TelePhoneNumberAllocationPackage { get; set; }
    }
}
