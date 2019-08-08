using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class LogEmail
    {
        public long Id { get; set; }
        public long EmailTypeId { get; set; }
        public long EmailTemplateTypeId { get; set; }
        public long BillId { get; set; }
        public string Subject { get; set; }
        public string EmailText { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailBcc { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SendDate { get; set; }
        public int? SendDateInt { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
    }
}
