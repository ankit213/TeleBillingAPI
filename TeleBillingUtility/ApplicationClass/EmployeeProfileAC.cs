using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class EmployeeProfileAC
    {
        [JsonProperty("statuscode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("userprofiledata")]
        public EmployeeProfileDetailAC UserProfileData { get; set; }

    }


    public class EmployeeProfileDetailSP
    {
        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("newpassword")]
        public string NewPassword { get; set; }

        [JsonProperty("confirmpassword")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("ispresidentoffice")]
        public long IsPresidentOffice { get; set; }

        [JsonProperty("issystemuser")]
        public long IsSystemUser { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }

        [JsonProperty("linemanagerid")]
        public long LineManagerId { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mobileallocated")]
        public string MobileAllocated { get; set; }

        [JsonProperty("delegateuser")]
        public string DelegateUser { get; set; }

        [JsonProperty("isactive")]
        public long IsActive { get; set; }

        [JsonProperty("isdelete")]
        public long IsDelete { get; set; }

        public List<EmployeeTelephoneDetailsAC> employeeTelephoneDetails { get; set; }
    }

    public class EmployeeProfileDetailAC
    {
        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("newpassword")]
        public string NewPassword { get; set; }

        [JsonProperty("confirmpassword")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("ispresidentoffice")]
        public bool IsPresidentOffice { get; set; }

        [JsonProperty("issystemuser")]
        public bool IsSystemUser { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }

        [JsonProperty("linemanagerid")]
        public long LineManagerId { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mobileallocated")]
        public string MobileAllocated { get; set; }

        [JsonProperty("delegateuser")]
        public string DelegateUser { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive { get; set; }

        [JsonProperty("isdelete")]
        public bool IsDelete { get; set; }

        public List<EmployeeTelephoneDetailsAC> employeeTelephoneDetails { get; set; }
    }

    public class EmployeeTelephoneDetailsAC
    {
        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("connectionstatus")]
        public string ConnectionStatus { get; set; }
    }

    public class MstEmployeeAC
    {
        public MstEmployeeAC()
        {
            ManagerEmployee = new EmployeeAC();
        }

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("newpassword")]
        public string NewPassword { get; set; }

        [JsonProperty("confirmpassword")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("ispresidentoffice")]
        public bool IsPresidentOffice { get; set; }

        [JsonProperty("issystemuser")]
        public bool IsSystemUser { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("roleid")]
        public long RoleId { get; set; }

        [JsonProperty("departmentid")]
        public long DepartmentId { get; set; }

        [JsonProperty("businessunitid")]
        public long BusinessUnitId { get; set; }

        [JsonProperty("costcenterid")]
        public long CostCenterId { get; set; }

        [JsonProperty("linemanagerid")]
        public long LineManagerId { get; set; }

        [JsonProperty("countryid")]
        public long CountryId { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive { get; set; }

        [JsonProperty("isdelete")]
        public bool IsDelete { get; set; }

        [JsonProperty("transactionid")]
        public long? TransactionId { get; set; }


        [JsonProperty("employeename")]
        public EmployeeAC ManagerEmployee { get; set; }
    }

    public class MstEmployeeSP
    {
        public MstEmployeeSP()
        {
            ManagerEmployee = new EmployeeAC();
        }

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("newpassword")]
        public string NewPassword { get; set; }

        [JsonProperty("confirmpassword")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("ispresidentoffice")]
        public long IsPresidentOffice { get; set; }

        [JsonProperty("issystemuser")]
        public long IsSystemUser { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("roleid")]
        public long RoleId { get; set; }

        [JsonProperty("departmentid")]
        public long DepartmentId { get; set; }

        [JsonProperty("businessunitid")]
        public long BusinessUnitId { get; set; }

        [JsonProperty("costcenterid")]
        public long CostCenterId { get; set; }

        [JsonProperty("linemanagerid")]
        public long LineManagerId { get; set; }

        [JsonProperty("countryid")]
        public long CountryId { get; set; }

        [JsonProperty("isactive")]
        public long IsActive { get; set; }

        [JsonProperty("isdelete")]
        public long IsDelete { get; set; }

        [JsonProperty("transactionid")]
        public long? TransactionId { get; set; }


        [JsonProperty("employeename")]
        public EmployeeAC ManagerEmployee { get; set; }
    }


}


