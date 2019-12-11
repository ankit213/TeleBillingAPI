using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixDevice
    {
        public FixDevice()
        {
            Exceluploadlog = new HashSet<Exceluploadlog>();
            Exceluploadlogpbx = new HashSet<Exceluploadlogpbx>();
            MappingexcelPbx = new HashSet<MappingexcelPbx>();
            MappingservicetypefieldPbx = new HashSet<MappingservicetypefieldPbx>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Exceluploadlog> Exceluploadlog { get; set; }
        public virtual ICollection<Exceluploadlogpbx> Exceluploadlogpbx { get; set; }
        public virtual ICollection<MappingexcelPbx> MappingexcelPbx { get; set; }
        public virtual ICollection<MappingservicetypefieldPbx> MappingservicetypefieldPbx { get; set; }
    }
}
