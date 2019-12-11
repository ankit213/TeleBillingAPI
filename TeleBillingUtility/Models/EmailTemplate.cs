using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Emailtemplate
    {
        public Emailtemplate()
        {
            Emailreminderlog = new HashSet<Emailreminderlog>();
        }

        public long Id { get; set; }
        public long EmailTemplateTypeId { get; set; }
        public string Subject { get; set; }
        public string EmailText { get; set; }
        public string EmailFrom { get; set; }
        public string EmailBcc { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixEmailtemplatetype EmailTemplateType { get; set; }
        public virtual ICollection<Emailreminderlog> Emailreminderlog { get; set; }
    }
}
