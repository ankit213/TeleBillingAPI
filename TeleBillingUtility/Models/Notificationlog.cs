using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class Notificationlog
	{
		public long Id { get; set; }
		public long UserId { get; set; }
		public long NotificationTypeId { get; set; }
		public long? EmployeeBillIormemoId { get; set; }
		public long? ActionUserId { get; set; }
		public string NotificationText { get; set; }
		public bool IsReadNotification { get; set; }
		public DateTime CreatedDate { get; set; }
		public long? CreatedDateInt { get; set; }
		public bool IsDeleted { get; set; }

		public virtual MstEmployee ActionUser { get; set; }
		public virtual FixNotificationtype NotificationType { get; set; }
		public virtual MstEmployee User { get; set; }
	}
}
