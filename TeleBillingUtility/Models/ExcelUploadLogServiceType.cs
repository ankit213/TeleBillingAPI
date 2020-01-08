using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class ExceluploadlogServicetype
    {
        public long Id { get; set; }
        public long ExcelUploadLogId { get; set; }
        public bool IsAllocated { get; set; }
        public long ServiceTypeId { get; set; }
        public bool IsDelete { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual Exceluploadlog ExcelUploadLog { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
    }
}
