using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixAuditlogactiontype
    {
        public FixAuditlogactiontype()
        {
            Auditactionlog = new HashSet<Auditactionlog>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Auditactionlog> Auditactionlog { get; set; }
    }
}
