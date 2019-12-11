using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixNotificationtype
    {
        public FixNotificationtype()
        {
            Notificationlog = new HashSet<Notificationlog>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Notificationlog> Notificationlog { get; set; }
    }
}
