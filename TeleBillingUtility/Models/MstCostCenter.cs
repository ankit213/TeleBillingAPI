using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstCostCenter
    {
        public MstCostCenter()
        {
            ExcelDetail = new HashSet<ExcelDetail>();
            MstEmployee = new HashSet<MstEmployee>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string CostCenterCode { get; set; }
        public long BusinessUnitid { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual MstBusinessUnit BusinessUnit { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
        public virtual ICollection<MstEmployee> MstEmployee { get; set; }
    }
}
