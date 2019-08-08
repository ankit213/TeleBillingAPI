using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class BillMasterServiceType
    {
        public long Id { get; set; }
        public long BillMasterId { get; set; }
        public long ServiceTypeId { get; set; }
        public bool IsDelete { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }

        public virtual BillMaster BillMaster { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
    }
}
