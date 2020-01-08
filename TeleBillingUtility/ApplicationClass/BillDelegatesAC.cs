using Newtonsoft.Json;
using System;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillDelegatesListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("employeeid")]
        public long EmployeeId { get; set; }

        [JsonProperty("delegateemployeeid")]
        public long DelegateEmployeeId { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("delegateemployeename")]
        public string DelegateEmployeeName { get; set; }

        [JsonProperty("allowbillidentification")]
        public bool AllowBillIdentification { get; set; }

        [JsonProperty("allowbillapproval")]
        public bool AllowBillApproval { get; set; }

        [JsonProperty("delegatedate")]
        public DateTime CreatedDate { get; set; }

        //   [JsonProperty("createdby")]
        //   public long CreatedBy { get; set; }
        //   [JsonProperty("isdelete")]
        //   public bool IsDelete { get; set; }
    }


    public class BillDelegatesAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("employeename")]
        public EmployeeAC Employee { get; set; }

        [JsonProperty("delegateemployeename")]
        public EmployeeAC DelegateEmployee { get; set; }

        [JsonProperty("allowbillidentification")]
        public bool AllowBillIdentification { get; set; }

        [JsonProperty("allowbillapproval")]
        public bool AllowBillApproval { get; set; }

        //   [JsonProperty("createdby")]
        //   public long CreatedBy { get; set; }

        [JsonProperty("delegatedate")]
        public DateTime CreatedDate { get; set; }

        // [JsonProperty("isdelete")]
        //  public bool IsDelete { get; set; }
    }
}
