using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstEmployee
    {
        public MstEmployee()
        {
            BillDelegateDelegateEmployee = new HashSet<BillDelegate>();
            BillDelegateEmployee = new HashSet<BillDelegate>();
            BillMaster = new HashSet<BillMaster>();
            EmployeeBillMasterEmployee = new HashSet<EmployeeBillMaster>();
            EmployeeBillMasterLinemanager = new HashSet<EmployeeBillMaster>();
            ExcelDetail = new HashSet<ExcelDetail>();
            InverseLineManager = new HashSet<MstEmployee>();
            OperatorCallLog = new HashSet<OperatorCallLog>();
            TelephoneNumberAllocation = new HashSet<TelephoneNumberAllocation>();
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
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
        public bool IsPresidentOffice { get; set; }
        public bool IsSystemUser { get; set; }

        public virtual MstCostCenter CostCenter { get; set; }
        public virtual MstDepartment Department { get; set; }
        public virtual MstEmployee LineManager { get; set; }
        public virtual ICollection<BillDelegate> BillDelegateDelegateEmployee { get; set; }
        public virtual ICollection<BillDelegate> BillDelegateEmployee { get; set; }
        public virtual ICollection<BillMaster> BillMaster { get; set; }
        public virtual ICollection<EmployeeBillMaster> EmployeeBillMasterEmployee { get; set; }
        public virtual ICollection<EmployeeBillMaster> EmployeeBillMasterLinemanager { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
        public virtual ICollection<MstEmployee> InverseLineManager { get; set; }
        public virtual ICollection<OperatorCallLog> OperatorCallLog { get; set; }
        public virtual ICollection<TelephoneNumberAllocation> TelephoneNumberAllocation { get; set; }
    }
}
