using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Billreimburse
    {
        public long Id { get; set; }
        public long BillMasterId { get; set; }
        public long EmployeeBillId { get; set; }
        public decimal ReImbruseAmount { get; set; }
        public long CurrencyId { get; set; }
        public string Description { get; set; }
        public bool? IsApproved { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovalComment { get; set; }
        public DateTime? ApprovalDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? ApproveDateInt { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual Billmaster BillMaster { get; set; }
        public virtual Employeebillmaster EmployeeBill { get; set; }
    }
}
