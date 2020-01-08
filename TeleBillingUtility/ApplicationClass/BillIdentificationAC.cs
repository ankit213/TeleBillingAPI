using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillIdentificationAC
    {

        [JsonProperty("lstunassignedbill")]
        public List<UnAssignedBillAC> lstUnAssignedBill { get; set; }

        [JsonProperty("calltypeid")]
        public long CallTypeId { get; set; }

        [JsonProperty("callid")]
        public long CallId { get; set; }

        [JsonProperty("servicepackageamountdetails")]
        public List<ServicePackageAmountDetail> ServicePackageAmountDetail { get; set; }
    }


    public class ServicePackageAmountDetail
    {

        [JsonProperty("businesscharge")]
        public decimal BusinessCharge { get; set; }

        [JsonProperty("personaldeduction")]
        public decimal PersonalDeduction { get; set; }

        [JsonProperty("deductibleamount")]
        public decimal DeductibleAmount { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }

        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }
    }




}
