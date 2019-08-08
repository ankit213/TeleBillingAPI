using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixEmailTemplateType
    {
        public FixEmailTemplateType()
        {
            EmailTemplate = new HashSet<EmailTemplate>();
        }

        public long Id { get; set; }
        public string TemplateType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<EmailTemplate> EmailTemplate { get; set; }
    }
}
