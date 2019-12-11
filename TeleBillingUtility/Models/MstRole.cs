using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class MstRole
    {
        public MstRole()
        {
            MstEmployee = new HashSet<MstEmployee>();
            MstRolerights = new HashSet<MstRolerights>();
        }

        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public long? TransactionId { get; set; }

        public virtual ICollection<MstEmployee> MstEmployee { get; set; }
        public virtual ICollection<MstRolerights> MstRolerights { get; set; }
    }
}
