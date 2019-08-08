using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ExcelUploadLogServiceType
    {
        public long Id { get; set; }
        public long ExceluploadLogId { get; set; }
        public long ServiceTypeId { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAllocated { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }

        public virtual ExcelUploadLog ExceluploadLog { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
    }
}
