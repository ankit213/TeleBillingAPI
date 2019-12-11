using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class BillmasterServicetype
    {
        public long Id { get; set; }
        public long BillMasterId { get; set; }
        public long ServiceTypeId { get; set; }
        public bool IsDelete { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }

        public virtual Billmaster BillMaster { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
    }
}
