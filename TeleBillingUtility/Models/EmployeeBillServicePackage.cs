using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Employeebillservicepackage
    {
        public long Id { get; set; }
        public long EmployeeBillId { get; set; }
        public long ServiceTypeId { get; set; }
		public long? PackageId { get; set; }
		public decimal? BusinessTotalAmount { get; set; }
        public decimal? PersonalIdentificationAmount { get; set; }
        public decimal? BusinessIdentificationAmount { get; set; }
        public decimal? DeductionAmount { get; set; }
        public long? TransactionId { get; set; }
        public bool IsDelete { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdateDateInt { get; set; }

        public virtual Employeebillmaster EmployeeBill { get; set; }
        public virtual Providerpackage Package { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
    }
}
