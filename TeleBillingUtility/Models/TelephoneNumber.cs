using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class TelephoneNumber
    {
        public TelephoneNumber()
        {
            TelephoneNumberAllocation = new HashSet<TelephoneNumberAllocation>();
        }

        public long Id { get; set; }
        public string TelephoneNumber1 { get; set; }
        public long ProviderId { get; set; }
        public string AccountNumber { get; set; }
        public long LineTypeId { get; set; }
        public string Description { get; set; }
        public bool IsDataRoaming { get; set; }
        public bool IsInternationalCalls { get; set; }
        public bool IsVoiceRoaming { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixLineType LineType { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual ICollection<TelephoneNumberAllocation> TelephoneNumberAllocation { get; set; }
    }
}
