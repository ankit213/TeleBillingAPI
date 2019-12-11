using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class MstEmployee
    {
        public MstEmployee()
        {
			Auditactionlog = new HashSet<Auditactionlog>();
			BilldelegateDelegateEmployee = new HashSet<Billdelegate>();
            BilldelegateEmployee = new HashSet<Billdelegate>();
			EmployeebillmasterBillDelegatedEmp = new HashSet<Employeebillmaster>();
			Billmaster = new HashSet<Billmaster>();
            EmployeebillmasterEmployee = new HashSet<Employeebillmaster>();
            EmployeebillmasterLinemanager = new HashSet<Employeebillmaster>();
            Exceldetail = new HashSet<Exceldetail>();
            InverseLineManager = new HashSet<MstEmployee>();
            Operatorcalllog = new HashSet<Operatorcalllog>();
			NotificationlogActionUser = new HashSet<Notificationlog>();
			NotificationlogUser = new HashSet<Notificationlog>();
			Telephonenumberallocation = new HashSet<Telephonenumberallocation>();
		}

        public long UserId { get; set; }
        public long RoleId { get; set; }
        public string EmpPfnumber { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }
        public long DepartmentId { get; set; }
        public long BusinessUnitId { get; set; }
        public long CostCenterId { get; set; }
        public long LineManagerId { get; set; }
        public long CountryId { get; set; }
        public string ExtensionNumber { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
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
        public bool IsPresidentOffice { get; set; }
        public bool IsSystemUser { get; set; }
		public string ImagePath { get; set; }

        public virtual MstCostcenter CostCenter { get; set; }
        public virtual MstDepartment Department { get; set; }
        public virtual MstEmployee LineManager { get; set; }
        public virtual MstRole Role { get; set; }
        public virtual ICollection<Billdelegate> BilldelegateDelegateEmployee { get; set; }
		public virtual ICollection<Employeebillmaster> EmployeebillmasterBillDelegatedEmp { get; set; }
		public virtual ICollection<Billdelegate> BilldelegateEmployee { get; set; }
        public virtual ICollection<Billmaster> Billmaster { get; set; }
        public virtual ICollection<Employeebillmaster> EmployeebillmasterEmployee { get; set; }
        public virtual ICollection<Employeebillmaster> EmployeebillmasterLinemanager { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
        public virtual ICollection<MstEmployee> InverseLineManager { get; set; }
        public virtual ICollection<Operatorcalllog> Operatorcalllog { get; set; }
        public virtual ICollection<Telephonenumberallocation> Telephonenumberallocation { get; set; }
		public virtual ICollection<Notificationlog> NotificationlogActionUser { get; set; }
		public virtual ICollection<Notificationlog> NotificationlogUser { get; set; }

		public virtual ICollection<Auditactionlog> Auditactionlog { get; set; }
	}
}
