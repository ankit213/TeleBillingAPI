using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstDepartment
    {
        public MstDepartment()
        {
            MstEmployee = new HashSet<MstEmployee>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long BusinessUnitId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual MstBusinessUnit BusinessUnit { get; set; }
        public virtual ICollection<MstEmployee> MstEmployee { get; set; }
    }
}
