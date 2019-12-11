using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstBusinessunit
    {
        public MstBusinessunit()
        {
            Employeebillmaster = new HashSet<Employeebillmaster>();
            Exceldetail = new HashSet<Exceldetail>();
            MstCostcenter = new HashSet<MstCostcenter>();
            MstDepartment = new HashSet<MstDepartment>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Employeebillmaster> Employeebillmaster { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
        public virtual ICollection<MstCostcenter> MstCostcenter { get; set; }
        public virtual ICollection<MstDepartment> MstDepartment { get; set; }
    }
}
