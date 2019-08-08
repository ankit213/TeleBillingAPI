using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class EmployeeBillMaster
    {
        public EmployeeBillMaster()
        {
            BillDetails = new HashSet<BillDetails>();
            BillReImburse = new HashSet<BillReImburse>();
            EmployeeBillServicePackage = new HashSet<EmployeeBillServicePackage>();
        }

        public long Id { get; set; }
        public long BillMasterId { get; set; }
        public string BillNumber { get; set; }
        public int BillMonth { get; set; }
        public int BillYear { get; set; }
        public long ProviderId { get; set; }
        public int EmployeeBillStatus { get; set; }
        public decimal TotalBillAmount { get; set; }
        public long? CurrencyId { get; set; }
        public string TelephoneNumber { get; set; }
        public long? EmployeeId { get; set; }
        public long? EmpBusinessUnitId { get; set; }
        public long? MbileAssignType { get; set; }
        public string Description { get; set; }
        public long? LinemanagerId { get; set; }
        public bool? IsApproved { get; set; }
        public string ApprovalComment { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? ApprovalDateInt { get; set; }
        public bool IsBillDelegated { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public bool IsDelete { get; set; }
        public bool IsReImbursementRequest { get; set; }
        public long? PreviousEmployeeBillId { get; set; }
        public bool IsReIdentificationRequest { get; set; }
        public long? TransactionId { get; set; }

        public virtual BillMaster BillMaster { get; set; }
        public virtual MstCurrency Currency { get; set; }
        public virtual MstBusinessUnit EmpBusinessUnit { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual FixBillEmployeeStatus EmployeeBillStatusNavigation { get; set; }
        public virtual MstEmployee Linemanager { get; set; }
        public virtual FixAssignType MbileAssignTypeNavigation { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual ICollection<BillDetails> BillDetails { get; set; }
        public virtual ICollection<BillReImburse> BillReImburse { get; set; }
        public virtual ICollection<EmployeeBillServicePackage> EmployeeBillServicePackage { get; set; }
    }
}
