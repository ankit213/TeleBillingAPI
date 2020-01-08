﻿using Newtonsoft.Json;

namespace TeleBillingUtility.ApplicationClass
{
    public class ExportMyStaffBillsAC
    {

        [JsonProperty("billnumber")]
        public string BillNumber { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("mobilenumber")]
        public string MobileNumber { get; set; }

        [JsonProperty("mobileassignedtype")]
        public string MobileAssignedType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("billstatus")]
        public string BillStatus { get; set; }

        [JsonProperty("isalreadyreimbursement")]
        public string IsAlreadyReImbursement { get; set; }

    }
}
