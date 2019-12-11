using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Requesttracelog
    {
        public long RequestTraceLogId { get; set; }
        public long TransactionId { get; set; }
        public string Ipaddress { get; set; }
        public string Browser { get; set; }
        public string Version { get; set; }
        public bool IsMobile { get; set; }
        public string DeviceId { get; set; }
        public string Gcmid { get; set; }
        public string PhoneModel { get; set; }
        public string Os { get; set; }
        public long? ActionId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        public long CreatedById { get; set; }
    }
}
