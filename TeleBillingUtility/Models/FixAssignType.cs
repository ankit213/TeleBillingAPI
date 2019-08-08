using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixAssignType
    {
        public FixAssignType()
        {
            EmployeeBillMaster = new HashSet<EmployeeBillMaster>();
            ExcelDetail = new HashSet<ExcelDetail>();
            TelephoneNumberAllocation = new HashSet<TelephoneNumberAllocation>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<EmployeeBillMaster> EmployeeBillMaster { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
        public virtual ICollection<TelephoneNumberAllocation> TelephoneNumberAllocation { get; set; }
    }
}
