using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class Providerservice
    {
        public long Id { get; set; }
        public long ProviderId { get; set; }
        public long ServiceTypeId { get; set; }
        public bool IsDelete { get; set; }

        public virtual Provider Provider { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
    }
}
