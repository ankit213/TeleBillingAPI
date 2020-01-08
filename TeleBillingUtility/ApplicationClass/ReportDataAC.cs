using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class ReportDataAC
    {
    }


    public class OperatorLogReportAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("calldate")]
        public string CallDate { get; set; }

        [JsonProperty("callmonth")]
        public string CallMonth { get; set; }

        [JsonProperty("callyear")]
        public string CallYear { get; set; }

        [JsonProperty("dialednumber")]
        public string DialedNumber { get; set; }
        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("calltype")]
        public string CallType { get; set; }

        [JsonProperty("operatorname")]
        public string OperatorName { get; set; }

        [JsonProperty("entrytime")]
        public string EntryTime { get; set; }

    }


    public class ProviderBillReportAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("billmonth")]
        public int BillMonth { get; set; }

        [JsonProperty("billyear")]
        public int BillYear { get; set; }

        [JsonProperty("billstatusid")]
        public int BillStatusId { get; set; }

        [JsonProperty("billstatus")]
        public string BillStatus { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

        [JsonProperty("isbusinessonly")]
        public string IsBusinessOnly { get; set; }

        [JsonProperty("allocatedby")]
        public string AllocatedBy { get; set; }

        [JsonProperty("allocationdate")]
        public string AllocationDate { get; set; }

        [JsonProperty("duedate")]
        public string DueDate { get; set; }

    }

    public class AuditLogReportAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("activity")]
        public string Activity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("doneby")]
        public string DoneBy { get; set; }

        [JsonProperty("activitytime")]
        public string ActivityTime { get; set; }

    }

    public class ReimbursementBillReportAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("reimbursementamount")]
        public string ReimbursementAmount { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }


        [JsonProperty("requestreason")]
        public string RequestReason { get; set; }

        [JsonProperty("approvedstatus")]
        public string ApprovedStatus { get; set; }


        [JsonProperty("approvedby")]
        public string ApprovedBy { get; set; }

        [JsonProperty("approvaldate")]
        public string ApprovalDate { get; set; }

    }

    public class AccountBillReportAC
    {
        [JsonProperty("employeebillid")]
        public long EmployeeBillId { get; set; }

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("mobileassigntype")]
        public string MobileAssignType { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("billstatus")]
        public string BillStatus { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("approvedstatus")]
        public string ApprovedStatus { get; set; }

        [JsonProperty("approvedby")]
        public string ApprovedBy { get; set; }

        [JsonProperty("approvaldate")]
        public string ApprovalDate { get; set; }
    }

    public class AccountBillDetailsAC
    {

        [JsonProperty("employeebillid")]
        public long EmployeeBillId { get; set; }

        [JsonProperty("transtype")]
        public string TransType { get; set; }

        [JsonProperty("transtypeid")]
        public string CallTransactionTypeId { get; set; }

        [JsonProperty("calldate")]
        public string CallDate { get; set; }

        [JsonProperty("calltime")]
        public string CallTime { get; set; }

        [JsonProperty("callduration")]
        public string CallDuration { get; set; }

        [JsonProperty("callamount")]
        public string CallAmount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("calltype")]
        public string CallType { get; set; }

        [JsonProperty("mobileassigntype")]
        public string MobileAssignType { get; set; }

    }

    public class HighestUserConsumptionReportAC
    {
        [JsonProperty("employeeid")]
        public long EmployeeId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("transtype")]
        public string TransTypes { get; set; }

        [JsonProperty("currencyweighted")]
        public string CurrencyWeighted { get; set; }

        [JsonProperty("callamount")]
        public decimal CallAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class UserConsumptionReportAC
    {

        [JsonProperty("empbillid")]
        public long EmpBillId { get; set; }

        [JsonProperty("employeeid")]
        public long EmployeeId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }



        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }



        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("mobileassigntype")]
        public string MobileAssignType { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

    }

    public class UserConsumptionDetailAC
    {

        public UserConsumptionDetailAC()
        {
            userCallDetailList = new List<UserCallDetailAC>();
            userPacakgeDetailList = new List<UserPacakgeDetailListAC>();
        }

        [JsonProperty("employeeid")]
        public long EmployeeId { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }



        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("linemanager")]
        public string LineManager { get; set; }


        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("mobileassigntype")]
        public string MobileAssignType { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

        [JsonProperty("deductionamount")]
        public string DeductionAmount { get; set; }

        [JsonProperty("empbillstatus")]
        public string EmpBillStatus { get; set; }


        [JsonProperty("usercalldetailslist")]
        public List<UserCallDetailAC> userCallDetailList { get; set; }

        [JsonProperty("userpacakgedetaillist")]
        public List<UserPacakgeDetailListAC> userPacakgeDetailList { get; set; }

    }

    public class UserCallDetailAC
    {
        public UserCallDetailAC()
        {
            CallDetailsList = new List<UserConsumptionCallDetailListAC>();

        }

        [JsonProperty("empbillid")]
        public long EmpBillId { get; set; }

        [JsonProperty("transactiontypeId")]
        public long TransactionTypeId { get; set; }

        [JsonProperty("transtype")]
        public string TransType { get; set; }

        [JsonProperty("subtotal")]
        public decimal subtotal { get; set; }

        [JsonProperty("calldetailslist")]
        public List<UserConsumptionCallDetailListAC> CallDetailsList { get; set; }


    }

    public class UserConsumptionCallDetailListAC
    {
        [JsonProperty("empbillid")]
        public long EmpBillId { get; set; }

        [JsonProperty("transactiontypeId")]
        public long TransactionTypeId { get; set; }

        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }

        [JsonProperty("package")]
        public string Package { get; set; }

        [JsonProperty("transtype")]
        public string TransType { get; set; }

        [JsonProperty("calldate")]
        public string CallDate { get; set; }

        [JsonProperty("callduration")]
        public long CallDuration { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("callidentificationtype")]
        public long CallIdentificationType { get; set; }

        [JsonProperty("calltype")]
        public string CallType { get; set; }

        [JsonProperty("callamount")]
        public decimal CallAmount { get; set; }

    }


    public class UserPacakgeDetailListAC
    {
        [JsonProperty("package")]
        public string Package { get; set; }

        [JsonProperty("packageamount")]
        public string PackageAmount { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }

        [JsonProperty("userconsumptionamount")]
        public string UserConsumptionAmount { get; set; }
    }


    public class MultipleLinesUserListReportAC
    {
        [JsonProperty("employeeid")]
        public long EmployeeId { get; set; }

        [JsonProperty("totallines")]
        public long TotalLines { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("extensionno")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("additionalinfo")]
        public string AdditionalInfo { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }
    }

    public class UserMobilePackageDetailReportAC
    {
        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("extensionno")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("usermobiledetailList")]
        public List<UserMobileDetailListAC> userMobileDetailList { get; set; }
    }

    public class UserMobileDetailListAC
    {
        public UserMobileDetailListAC()
        {
            userMobilePackageDetailLists = new List<UserMobilePackageDetailListAC>();
        }

        [JsonProperty("employeeId")]
        public long EmployeeId { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("assigntype")]
        public string AssignType { get; set; }

        [JsonProperty("linestatus")]
        public string LineStatus { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("mobilepackagelist")]
        public List<UserMobilePackageDetailListAC> userMobilePackageDetailLists { get; set; }

    }

    public class UserMobilePackageDetailListAC
    {
        [JsonProperty("employeeId")]
        public long EmployeeId { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("assigntype")]
        public string AssignType { get; set; }

        [JsonProperty("linestatus")]
        public string LineStatus { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("packagename")]
        public string PackageName { get; set; }

        [JsonProperty("packageamount")]
        public string PackageAmount { get; set; }

        [JsonProperty("startdate")]
        public string StartDate { get; set; }

        [JsonProperty("enddate")]
        public string EndDate { get; set; }

        [JsonProperty("packagecurrentstatus")]
        public string PackageCurrentStatus { get; set; }


    }


}
