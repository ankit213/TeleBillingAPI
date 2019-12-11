using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MappingservicetypefieldPbx
    {
        public MappingservicetypefieldPbx()
        {
            MappingexcelcolumnPbx = new HashSet<MappingexcelcolumnPbx>();
        }

        public long Id { get; set; }
        public long DeviceId { get; set; }
        public string DbtableName { get; set; }
        public string DbcolumnName { get; set; }
        public string DisplayFieldName { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSpecial { get; set; }

        public virtual FixDevice Device { get; set; }
        public virtual ICollection<MappingexcelcolumnPbx> MappingexcelcolumnPbx { get; set; }
    }
}
