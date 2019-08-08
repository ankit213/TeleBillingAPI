using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixBillEmployeeStatus
    {
        public FixBillEmployeeStatus()
        {
            EmployeeBillMaster = new HashSet<EmployeeBillMaster>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }

        public virtual ICollection<EmployeeBillMaster> EmployeeBillMaster { get; set; }
    }
}
