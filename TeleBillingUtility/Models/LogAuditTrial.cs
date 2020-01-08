using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class LogAudittrial
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
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? AuditDateInt { get; set; }

        public virtual FixLogtype LogType { get; set; }
    }
}
