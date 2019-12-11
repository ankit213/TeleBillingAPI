using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillUploadListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("billdate")] // Month Year
        public string BillDate { get; set; }

        [JsonProperty("uploadeddate")]
        public string UploadedDate { get; set; }

        [JsonProperty("totalrecord")]
        public long TotalRecord { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

        //[JsonProperty("isapproved")]
        //public bool IsApproved { get; set; }

        [JsonProperty("isapproved")]
        public bool IsApproved
        {

            get { return (IsApproved1 == 1 ? true : false); }
            set
            {
                IsApproved = value;
            }
        }

        [JsonProperty("isallocated")]
        public bool IsAllocated
        {

            get { return (IsAllocated1 == 1 ? true : false); }
            set
            {
                IsAllocated = value;
            }
        }

        public long IsApproved1 { get; set; }
        public long IsAllocated1 { get; set; }

    }

    public class PbxBillUploadListAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("billdate")] // Month Year
        public string BillDate { get; set; }

        [JsonProperty("uploadeddate")]
        public DateTime UploadedDate { get; set; }

        [JsonProperty("isapproved")]
        public bool? IsApproved { get; set; }
    }
}
