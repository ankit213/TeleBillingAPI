using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixBillStatus
    {
        public FixBillStatus()
        {
            BillMaster = new HashSet<BillMaster>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<BillMaster> BillMaster { get; set; }
    }
}
