namespace TeleBillingUtility.Models
{
    public partial class ExceldetailpbxError
    {
        public long Id { get; set; }
        public long? ExcelUploadLogId { get; set; }
        public string CallDate { get; set; }
        public string CallTime { get; set; }
        public string CallDuration { get; set; }
        public string CallAmount { get; set; }
        public string CurrencyId { get; set; }
        public string ConnectingParty { get; set; }
        public string Name1 { get; set; }
        public string OtherParty { get; set; }
        public string Name2 { get; set; }
        public string CodeNumber { get; set; }
        public string Name3 { get; set; }
        public string ClassificationCode { get; set; }
        public string Name4 { get; set; }
        public string CallType { get; set; }
        public string Place { get; set; }
        public string Band { get; set; }
        public string Rate { get; set; }
        public string DestinationType { get; set; }
        public string DistantNumber { get; set; }
        public string RingingTime { get; set; }
        public string Description { get; set; }
        public string FileGuidNo { get; set; }
        public string ErrorSummary { get; set; }
    }
}
