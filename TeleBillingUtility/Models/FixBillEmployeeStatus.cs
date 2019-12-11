using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixBillemployeestatus
    {
        public FixBillemployeestatus()
        {
            Employeebillmaster = new HashSet<Employeebillmaster>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Employeebillmaster> Employeebillmaster { get; set; }
    }
}
