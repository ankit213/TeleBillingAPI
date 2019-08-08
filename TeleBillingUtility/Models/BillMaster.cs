using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class BillMaster
    {
        public BillMaster()
        {
            BillDetails = new HashSet<BillDetails>();
            BillMasterServiceType = new HashSet<BillMasterServiceType>();
            BillReImburse = new HashSet<BillReImburse>();
            EmployeeBillMaster = new HashSet<EmployeeBillMaster>();
            MemoBills = new HashSet<MemoBills>();
        }

        public long Id { get; set; }
        public string BillNumber { get; set; }
        public int BillMonth { get; set; }
        public int BillYear { get; set; }
        public long ProviderId { get; set; }
        public long BillStatusId { get; set; }
        public decimal BillAmount { get; set; }
        public long? CurrencyId { get; set; }
        public string Description { get; set; }
        public long? BillAllocatedBy { get; set; }
        public DateTime? BillAllocationDate { get; set; }
        public int? BillAllocationDateInt { get; set; }
        public DateTime? BillDueDate { get; set; }
        public int? BillDueDateInt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public bool IsDelete { get; set; }
        public bool IsBusinessOnly { get; set; }

        public virtual MstEmployee BillAllocatedByNavigation { get; set; }
        public virtual FixBillStatus BillStatus { get; set; }
        public virtual MstCurrency Currency { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual ICollection<BillDetails> BillDetails { get; set; }
        public virtual ICollection<BillMasterServiceType> BillMasterServiceType { get; set; }
        public virtual ICollection<BillReImburse> BillReImburse { get; set; }
        public virtual ICollection<EmployeeBillMaster> EmployeeBillMaster { get; set; }
        public virtual ICollection<MemoBills> MemoBills { get; set; }
    }
}
