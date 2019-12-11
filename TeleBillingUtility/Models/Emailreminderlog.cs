using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Emailreminderlog
    { 
        public long Id { get; set; }
        public long TemplateId { get; set; }
		public string EmailTo { get; set;}
        public long? EmployeeBillId { get; set; }
        public bool IsReminderMail { get; set; }
		public bool IsReadNotification { get; set;}
        public DateTime CreatedDate { get; set; }
		
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }

        public virtual Employeebillmaster EmployeeBill { get; set; }
        public virtual Emailtemplate Template { get; set; }
    }
}
