using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class BillDelegate
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public long DelegateEmployeeId { get; set; }
        public bool AllowBillIdentification { get; set; }
        public bool AllowBillApproval { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public bool IsDelete { get; set; }
        public long? TransactionId { get; set; }

        public virtual MstEmployee DelegateEmployee { get; set; }
        public virtual MstEmployee Employee { get; set; }
    }
}
