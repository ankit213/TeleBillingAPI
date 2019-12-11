using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.Models
{
    public partial class SkypeexceldetailError
    {
        public long Id { get; set; }
        public long? ExcelUploadLogId { get; set; }
        public long ServiceTypeId { get; set; }
        public string CallDate { get; set; }
        public string CallTime { get; set; }
        public string CallDuration { get; set; }
        public string CallerNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public string CallAmount { get; set; }
        public string Description { get; set; }
        public string FileGuidNo { get; set; }
        public string ErrorSummary { get; set; }
    }
}
