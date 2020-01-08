using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class ExportMemoAC
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("refrenceno")]
        public string RefrenceNo { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("provider")]
        public string ProviderName { get; set; }

        [JsonProperty("totalamount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("ibancode")]
        public string Ibancode { get; set; }

        [JsonProperty("swiftcode")]
        public string Swiftcode { get; set; }

        [JsonProperty("accountnumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("countryname")]
        public string CountryName { get; set; }

        [JsonProperty("currencycode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("contractnumber")]
        public string ContractNumber { get; set; }

        [JsonProperty("exportmemobills")]
        public List<ExportMemoBillsAC> exportMemoBillsACs { get; set; }
    }

    public class ExportMemoBillsAC
    {
        [JsonProperty("description")]
        public string DescriptionService { get; set; }


        [JsonProperty("totalbillamount")]
        public decimal TotalBillAmount { get; set; }
    }
}
