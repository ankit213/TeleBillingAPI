using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixAssigntype
    {
        public FixAssigntype()
        {
            Employeebillmaster = new HashSet<Employeebillmaster>();
            Exceldetail = new HashSet<Exceldetail>();
            Telephonenumberallocation = new HashSet<Telephonenumberallocation>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<Employeebillmaster> Employeebillmaster { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
        public virtual ICollection<Telephonenumberallocation> Telephonenumberallocation { get; set; }
    }
}
