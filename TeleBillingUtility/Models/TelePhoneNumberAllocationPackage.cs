using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Telephonenumberallocationpackage
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
        public long ServiceId { get; set; }
        public long TelephoneNumberAllocationId { get; set; }
        public DateTime StartDate { get; set; }
		
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? StartDateInt { get; set; }

        public DateTime EndDate { get; set; }
		
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? EndDateInt { get; set; }

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

        public virtual Providerpackage Package { get; set; }
        public virtual Telephonenumberallocation TelephoneNumberAllocation { get; set; }
    }
}
