using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class EmployeeBillServicePackage
    {
        public long Id { get; set; }
        public long EmployeeBillId { get; set; }
        public long ServiceTypeId { get; set; }
        public long PackageId { get; set; }
        public decimal? BusinessTotalAmount { get; set; }
        public decimal? PersonalIdentificationAmount { get; set; }
        public decimal? BusinessIdentificationAmount { get; set; }
        public decimal? DeductionAmount { get; set; }
        public bool IsDelete { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }

        public virtual EmployeeBillMaster EmployeeBill { get; set; }
        public virtual ProviderPackage Package { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
    }
}
