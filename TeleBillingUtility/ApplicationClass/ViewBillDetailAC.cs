using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class ViewBillDetailAC
    {

        [JsonProperty("lstunassignedbill")]
        public List<UnAssignedBillAC> lstUnAssignedBill { get; set; }

        [JsonProperty("packageservicelist")]
        public List<PackageServiceAC> PackageServiceList { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("isdeducateamount")]
        public bool IsDeducateAmount { get; set; }

        [JsonProperty("isdisplayonly")]
        public bool IsDisplayOnly { get; set; }

        [JsonProperty("employeebillstatus")]
        public int EmployeeBillStatus { get; set; }

        [JsonProperty("isreidentificationrequest")]
        public bool IsReIdentificationRequest { get; set; }

        [JsonProperty("totalbillamount")]
        public decimal TotalBillAmount { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }
    }

    public class PackageServiceAC
    {

        [JsonProperty("packageid")]
		public long? PackageId { get; set; }
		
        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceTypeName { get; set; }

        [JsonProperty("packagename")]
        public string PackageName { get; set; }

        [JsonProperty("packagelimitamount")]
        public decimal PackageLimitAmount { get; set; }

        [JsonProperty("deductionamount")]
        public decimal? DeductionAmount { get; set; }

        [JsonProperty("personalidentificationamount")]
        public decimal? PersonalIdentificationAmount { get; set; }

        [JsonProperty("businessidentificationamount")]
        public decimal? BusinessIdentificationAmount { get; set; }
    }
}
