using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstHandsetDetail
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
    }
}
