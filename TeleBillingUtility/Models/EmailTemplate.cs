using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class EmailTemplate
    {
        public long Id { get; set; }
        public long EmailTemplateTypeId { get; set; }
        public string Subject { get; set; }
        public string EmailText { get; set; }
        public string EmailFrom { get; set; }
        public string EmailBcc { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixEmailTemplateType EmailTemplateType { get; set; }
    }
}
