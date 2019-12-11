using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixEmailtemplatetype
    {
        public FixEmailtemplatetype()
        {
            Emailtemplate = new HashSet<Emailtemplate>();
        }

        public long Id { get; set; }
        public string TemplateType { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Emailtemplate> Emailtemplate { get; set; }
    }
}
