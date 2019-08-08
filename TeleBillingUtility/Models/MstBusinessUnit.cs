using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstBusinessUnit
    {
        public MstBusinessUnit()
        {
            EmployeeBillMaster = new HashSet<EmployeeBillMaster>();
            ExcelDetail = new HashSet<ExcelDetail>();
            MstCostCenter = new HashSet<MstCostCenter>();
            MstDepartment = new HashSet<MstDepartment>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<EmployeeBillMaster> EmployeeBillMaster { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
        public virtual ICollection<MstCostCenter> MstCostCenter { get; set; }
        public virtual ICollection<MstDepartment> MstDepartment { get; set; }
    }
}
