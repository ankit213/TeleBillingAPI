using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MappingExcelPbx
    {
        public MappingExcelPbx()
        {
            MappingExcelColumnPbx = new HashSet<MappingExcelColumnPbx>();
        }

        public long Id { get; set; }
        public long DeviceId { get; set; }
        public long WorkSheetNo { get; set; }
        public bool HaveHeader { get; set; }
        public bool HaveTitle { get; set; }
        public string TitleName { get; set; }
        public string ExcelColumnNameForTitle { get; set; }
        public string ExcelReadingColumn { get; set; }
        public long CurrencyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixDevice Device { get; set; }
        public virtual ICollection<MappingExcelColumnPbx> MappingExcelColumnPbx { get; set; }
    }
}
