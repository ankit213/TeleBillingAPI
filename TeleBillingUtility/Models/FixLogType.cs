using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixLogType
    {
        public FixLogType()
        {
            LogAuditTrial = new HashSet<LogAuditTrial>();
        }

        public long LogTypeId { get; set; }
        public string TypeName { get; set; }
        public string LogText { get; set; }

        public virtual ICollection<LogAuditTrial> LogAuditTrial { get; set; }
    }
}
