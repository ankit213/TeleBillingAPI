using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class Skypeexceldetail
    {
        public long Id { get; set; }
        public long ExcelUploadLogId { get; set; }
        public long ServiceTypeId { get; set; }
        public DateTime? CallDate { get; set; }
        public TimeSpan? CallTime { get; set; }
        public long? CallDuration { get; set; }
        public string CallerNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public decimal? CallAmount { get; set; }
        public long? CurrencyId { get; set; }
        public bool? IsMatched { get; set; }
        public long? AssignType { get; set; }
        public string Description { get; set; }

        public virtual MstCurrency Currency { get; set; }
        public virtual Exceluploadlog ExcelUploadLog { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
    }
}
