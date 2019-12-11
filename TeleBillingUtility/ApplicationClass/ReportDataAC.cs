using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

    public  class AccountBillReportAC
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
        public long CallTransactionTypeId { get; set; }

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
}
