using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixBillstatus
    {
        public FixBillstatus()
        {
            Billmaster = new HashSet<Billmaster>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Billmaster> Billmaster { get; set; }
    }
}
