using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Auditactionlog
    {
		public long Id { get; set; }
		public long AuditLogActionType { get; set; }
		public string Description { get; set; }
		public DateTime CreatedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
		public long CreatedBy { get; set; }
		public long? ReflectedTableId { get; set; }

		public virtual FixAuditlogactiontype AuditLogActionTypeNavigation { get; set; }
		public virtual MstEmployee CreatedByNavigation { get; set; }
	}
}
