using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Billmaster
    {
        public Billmaster()
        {
            Billdetails = new HashSet<Billdetails>();
            BillmasterServicetype = new HashSet<BillmasterServicetype>();
            Billreimburse = new HashSet<Billreimburse>();
            Employeebillmaster = new HashSet<Employeebillmaster>();
            Memobills = new HashSet<Memobills>();
        }

        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string BillNumber { get; set; }
        public int BillMonth { get; set; }
        public int BillYear { get; set; }
        public int BillStatusId { get; set; }
        public decimal BillAmount { get; set; }
        public long? CurrencyId { get; set; }
        public string Description { get; set; }
        public long? BillAllocatedBy { get; set; }
        public DateTime? BillAllocationDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? BillAllocationDateInt { get; set; }
        public DateTime? BillDueDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? BillDueDateInt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
        public bool IsBusinessOnly { get; set; }
        public bool IsDelete { get; set; }

        public virtual MstEmployee BillAllocatedByNavigation { get; set; }
        public virtual FixBillstatus BillStatus { get; set; }
        public virtual MstCurrency Currency { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual ICollection<Billdetails> Billdetails { get; set; }
        public virtual ICollection<BillmasterServicetype> BillmasterServicetype { get; set; }
        public virtual ICollection<Billreimburse> Billreimburse { get; set; }
        public virtual ICollection<Employeebillmaster> Employeebillmaster { get; set; }
        public virtual ICollection<Memobills> Memobills { get; set; }
    }
}
