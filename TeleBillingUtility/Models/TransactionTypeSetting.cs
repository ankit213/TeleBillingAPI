using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class TransactionTypeSetting
    {
        public TransactionTypeSetting()
        {
            BillDetails = new HashSet<BillDetails>();
            ExcelDetail = new HashSet<ExcelDetail>();
        }

        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string TransactionType { get; set; }
        public long? SetTypeAs { get; set; }
        public bool IsDelete { get; set; }
        public long? TransactionId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }

        public virtual Provider Provider { get; set; }
        public virtual FixCallType SetTypeAsNavigation { get; set; }
        public virtual ICollection<BillDetails> BillDetails { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
    }
}
