using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MemoBills
    {
        public long Id { get; set; }
        public long MemoId { get; set; }
        public long BillId { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual BillMaster Bill { get; set; }
        public virtual Memo Memo { get; set; }
    }
}
