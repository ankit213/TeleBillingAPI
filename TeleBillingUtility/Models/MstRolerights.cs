using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class MstRolerights
    {
        public long RoleRightId { get; set; }
        public long RoleId { get; set; }
        public long LinkId { get; set; }
        public bool IsView { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsEditable { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsChangeStatus { get; set; }
        public bool HaveFullAccess { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual MstRole Role { get; set; }
    }
}
