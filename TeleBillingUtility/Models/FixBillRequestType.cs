using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixBillRequestType
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
    }
}
