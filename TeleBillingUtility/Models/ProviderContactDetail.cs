using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ProviderContactDetail
    {
        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string ContactNumbers { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Provider Provider { get; set; }
    }
}
