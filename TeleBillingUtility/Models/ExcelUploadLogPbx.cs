using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ExcelUploadLogPbx
    {
        public ExcelUploadLogPbx()
        {
            ExcelDetailPbx = new HashSet<ExcelDetailPbx>();
        }

        public long Id { get; set; }
        public string ExcelFileName { get; set; }
        public string FileNameGuid { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long DeviceId { get; set; }
        public long UploadBy { get; set; }
        public DateTime UploadDate { get; set; }
        public int? UploadDateInt { get; set; }
        public int TotalRecordImportCount { get; set; }
        public decimal TotalImportedBillAmount { get; set; }
        public bool IsDelete { get; set; }
        public long? TransactionId { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }

        public virtual FixDevice Device { get; set; }
        public virtual ICollection<ExcelDetailPbx> ExcelDetailPbx { get; set; }
    }
}
