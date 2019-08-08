using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstCurrency
    {
        public MstCurrency()
        {
            BillMaster = new HashSet<BillMaster>();
            EmployeeBillMaster = new HashSet<EmployeeBillMaster>();
            ExcelDetail = new HashSet<ExcelDetail>();
            MstCountry = new HashSet<MstCountry>();
            SkypeExcelDetail = new HashSet<SkypeExcelDetail>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public long IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<BillMaster> BillMaster { get; set; }
        public virtual ICollection<EmployeeBillMaster> EmployeeBillMaster { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
        public virtual ICollection<MstCountry> MstCountry { get; set; }
        public virtual ICollection<SkypeExcelDetail> SkypeExcelDetail { get; set; }
    }
}
