using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixDevice
    {
        public FixDevice()
        {
            MappingExcelPbx = new HashSet<MappingExcelPbx>();
            MappingServiceTypeFieldPbx = new HashSet<MappingServiceTypeFieldPbx>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<MappingExcelPbx> MappingExcelPbx { get; set; }
        public virtual ICollection<MappingServiceTypeFieldPbx> MappingServiceTypeFieldPbx { get; set; }
    }
}
