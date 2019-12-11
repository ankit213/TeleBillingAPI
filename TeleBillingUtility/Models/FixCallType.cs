using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixCalltype
    {
        public FixCalltype()
        {
            Billdetails = new HashSet<Billdetails>();
            Operatorcalllog = new HashSet<Operatorcalllog>();
            Transactiontypesetting = new HashSet<Transactiontypesetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Billdetails> Billdetails { get; set; }
        public virtual ICollection<Operatorcalllog> Operatorcalllog { get; set; }
        public virtual ICollection<Transactiontypesetting> Transactiontypesetting { get; set; }
    }
}
