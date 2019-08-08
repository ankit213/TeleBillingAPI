using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixCallType
    {
        public FixCallType()
        {
            BillDetails = new HashSet<BillDetails>();
            OperatorCallLog = new HashSet<OperatorCallLog>();
            TransactionTypeSetting = new HashSet<TransactionTypeSetting>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<BillDetails> BillDetails { get; set; }
        public virtual ICollection<OperatorCallLog> OperatorCallLog { get; set; }
        public virtual ICollection<TransactionTypeSetting> TransactionTypeSetting { get; set; }
    }
}
