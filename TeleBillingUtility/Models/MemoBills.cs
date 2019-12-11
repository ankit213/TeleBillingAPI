using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Memobills
    {
        public long Id { get; set; }
        public long MemoId { get; set; }
        public long BillId { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual Billmaster Bill { get; set; }
        public virtual Memo Memo { get; set; }
    }
}
