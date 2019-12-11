using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Employeebillmaster
    {
		public Employeebillmaster()
		{
			Billdetails = new HashSet<Billdetails>();
			Billreimburse = new HashSet<Billreimburse>();
			Emailreminderlog = new HashSet<Emailreminderlog>();
			Employeebillservicepackage = new HashSet<Employeebillservicepackage>();
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
		public long? MobileAssignType { get; set; }
		public string Description { get; set; }
		public long? LinemanagerId { get; set; }
		public bool? IsApproved { get; set; }
		public string ApprovalComment { get; set; }
		public DateTime? ApprovalDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? ApprovalDateInt { get; set; }

		public long? ApprovalById { get; set; }
		public long? BillDelegatedEmpId { get; set; }
		public bool IsReImbursementRequest { get; set; }
		public long? PreviousEmployeeBillId { get; set; }
		public bool IsReIdentificationRequest { get; set; }
		public long? IdentificationById { get; set; }
		public bool? IsIdentificationByDelegate { get; set; }
		public bool? IsApprovedByDelegate { get; set; }
		public DateTime? IdentificationDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? IdentificationDateInt { get; set; }
		public DateTime? BillClosedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? BillClosedDateInt { get; set; }
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

		public virtual MstEmployee BillDelegatedEmp { get; set; }
		public virtual Billmaster BillMaster { get; set; }
        public virtual MstCurrency Currency { get; set; }
        public virtual MstBusinessunit EmpBusinessUnit { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual FixBillemployeestatus EmployeeBillStatusNavigation { get; set; }
        public virtual MstEmployee Linemanager { get; set; }
        public virtual FixAssigntype MobileAssignTypeNavigation { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual ICollection<Billdetails> Billdetails { get; set; }
        public virtual ICollection<Billreimburse> Billreimburse { get; set; }
        public virtual ICollection<Emailreminderlog> Emailreminderlog { get; set; }
        public virtual ICollection<Employeebillservicepackage> Employeebillservicepackage { get; set; }
    }
}
