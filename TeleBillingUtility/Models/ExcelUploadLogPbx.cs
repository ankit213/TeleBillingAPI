using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Exceluploadlogpbx
    {
        public Exceluploadlogpbx()
        {
            Exceldetailpbx = new HashSet<Exceldetailpbx>();
        }

        public long Id { get; set; }
        public string ExcelFileName { get; set; }
        public string FileNameGuid { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long? ProviderId { get; set; }
        public long? DeviceId { get; set; }
        public int TotalRecordImportCount { get; set; }
        public decimal TotalImportedBillAmount { get; set; }
        public long UploadBy { get; set; }
        public DateTime UploadDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UploadDateInt { get; set; }
        public bool IsDelete { get; set; }
        public long? MergedWithId { get; set; }
        public bool? IsMerge { get; set; }
        public bool? IsApproved { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixDevice Device { get; set; }
        public virtual ICollection<Exceldetailpbx> Exceldetailpbx { get; set; }
    }
}
