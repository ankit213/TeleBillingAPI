using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MappingExcel
    {
        public MappingExcel()
        {
            MappingExcelColumn = new HashSet<MappingExcelColumn>();
        }

        public long Id { get; set; }
        public long ProviderId { get; set; }
        public long ServiceTypeId { get; set; }
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

        public virtual Provider Provider { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
        public virtual ICollection<MappingExcelColumn> MappingExcelColumn { get; set; }
    }
}
