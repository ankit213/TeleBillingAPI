using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixEmailtemplatetag
    {
        public int Id { get; set; }
        public string TemplateTag { get; set; }
        public string TemplateText { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
