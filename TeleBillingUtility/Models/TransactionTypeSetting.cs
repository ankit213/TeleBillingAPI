using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Transactiontypesetting
    {
        public Transactiontypesetting()
        {
            Billdetails = new HashSet<Billdetails>();
            Exceldetail = new HashSet<Exceldetail>();
        }

        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string TransactionType { get; set; }
        public int? SetTypeAs { get; set; }
        public bool IsDelete { get; set; }

		public bool IsActive { get; set; }

		public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }

        public long? TransactionId { get; set; }

        public virtual Provider Provider { get; set; }
        public virtual FixCalltype SetTypeAsNavigation { get; set; }
        public virtual ICollection<Billdetails> Billdetails { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
    }
}
