using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstCostcenter
    {
        public MstCostcenter()
        {
            Exceldetail = new HashSet<Exceldetail>();
            MstEmployee = new HashSet<MstEmployee>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string CostCenterCode { get; set; }
        public bool IsActive { get; set; }
        public long BusinessUnitid { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual MstBusinessunit BusinessUnit { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
        public virtual ICollection<MstEmployee> MstEmployee { get; set; }
    }
}
