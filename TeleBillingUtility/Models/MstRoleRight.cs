using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstRoleRight
    {
        public long RoleRightId { get; set; }
        public long RoleId { get; set; }
        public long LinkId { get; set; }
        public bool IsView { get; set; }
        public bool IsViewOnly { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsChangeStatus { get; set; }
        public bool HaveFullAccess { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? TransactionId { get; set; }
    }
}
