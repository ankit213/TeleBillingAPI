using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixLogtype
    {
        public FixLogtype()
        {
            LogAudittrial = new HashSet<LogAudittrial>();
        }

        public long LogTypeId { get; set; }
        public string TypeName { get; set; }
        public string LogText { get; set; }

        public virtual ICollection<LogAudittrial> LogAudittrial { get; set; }
    }
}
