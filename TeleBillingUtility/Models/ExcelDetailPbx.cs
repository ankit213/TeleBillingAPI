using System;

namespace TeleBillingUtility.Models
{
    public partial class Exceldetailpbx
    {
        public long Id { get; set; }
        public long ExcelUploadLogId { get; set; }
        public DateTime? CallDate { get; set; }
        public TimeSpan? CallTime { get; set; }
        public long? CallDuration { get; set; }
        public decimal? CallAmount { get; set; }
        public long? CurrencyId { get; set; }
        public string ConnectingParty { get; set; }
        public string Name1 { get; set; }
        public string OtherParty { get; set; }
        public string Name2 { get; set; }
        public string CodeNumber { get; set; }
        public string Name3 { get; set; }
        public string ClassificationCode { get; set; }
        public string Name4 { get; set; }
        public string CallType { get; set; }
        public bool? IsMatched { get; set; }
        public long? SkypeMatchedId { get; set; }
        public string Place { get; set; }
        public string Band { get; set; }
        public string Rate { get; set; }
        public string DestinationType { get; set; }
        public string DistantNumber { get; set; }
        public int? RingingTime { get; set; }
        public byte[] Description { get; set; }

        public virtual Exceluploadlogpbx ExcelUploadLog { get; set; }
    }
}
