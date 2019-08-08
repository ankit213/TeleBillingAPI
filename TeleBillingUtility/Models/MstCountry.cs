using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstCountry
    {
        public MstCountry()
        {
            Provider = new HashSet<Provider>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long CurrencyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual MstCurrency Currency { get; set; }
        public virtual ICollection<Provider> Provider { get; set; }
    }
}
