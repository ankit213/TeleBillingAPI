using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class LogAuditTrial
    {
        public long AuditTrialLogId { get; set; }
        public long UserId { get; set; }
        public long LogTypeId { get; set; }
        public string Description { get; set; }
        public string Ipaddress { get; set; }
        public string Browser { get; set; }
        public string Version { get; set; }
        public bool IsMobile { get; set; }
        public DateTime AuditDate { get; set; }
        public int? AuditDateInt { get; set; }

        public virtual FixLogType LogType { get; set; }
    }
}
