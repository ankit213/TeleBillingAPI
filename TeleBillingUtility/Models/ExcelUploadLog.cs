using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ExcelUploadLog
    {
        public ExcelUploadLog()
        {
            ExcelUploadLogServiceType = new HashSet<ExcelUploadLogServiceType>();
            SkypeExcelDetail = new HashSet<SkypeExcelDetail>();
        }

        public long Id { get; set; }
        public string ExcelFileName { get; set; }
        public string FileNameGuid { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long? ProviderId { get; set; }
        public int? DeviceId { get; set; }
        public bool IsPbxupload { get; set; }
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

        public virtual Provider Provider { get; set; }
        public virtual ICollection<ExcelUploadLogServiceType> ExcelUploadLogServiceType { get; set; }
        public virtual ICollection<SkypeExcelDetail> SkypeExcelDetail { get; set; }
    }
}
