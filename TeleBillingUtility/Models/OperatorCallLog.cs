using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Operatorcalllog
    {
        public long Id { get; set; }
        public DateTime CallDate { get; set; }
		
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CallDateInt { get; set; }
        
		public long EmployeeId { get; set; }
        public string EmpPfnumber { get; set; }
        public string ExtensionNumber { get; set; }
        public string DialedNumber { get; set; }
        public long ProviderId { get; set; }
        public int CallTypeId { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
		
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        
		public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }
	
		public long? TransactionId { get; set; }

        public virtual FixCalltype CallType { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
