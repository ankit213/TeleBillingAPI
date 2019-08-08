using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class TelePhoneNumberAllocationPackage
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
        public long ServiceId { get; set; }
        public long TelephoneNumberAllocationId { get; set; }
        public bool IsDelete { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public DateTime StartDate { get; set; }
        public int? StartDateInt { get; set; }
        public DateTime EndDate { get; set; }
        public int? EndDateInt { get; set; }

        public virtual ProviderPackage Package { get; set; }
        public virtual TelephoneNumberAllocation TelephoneNumberAllocation { get; set; }
    }
}
