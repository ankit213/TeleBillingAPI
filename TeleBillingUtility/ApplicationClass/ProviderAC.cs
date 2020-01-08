using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class ProviderAC
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("servicetypes")]
        public List<DrpResponseAC> ServiceTypes { get; set; }

        [JsonProperty("contractnumber")]
        public string ContractNumber { get; set; }

        [JsonProperty("countryid")]
        public long CountryId { get; set; }

        [JsonProperty("currencyid")]
        public long CurrencyId { get; set; }

        [JsonProperty("accountnumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("ibancode")]
        public string Ibancode { get; set; }

        [JsonProperty("swiftcode")]
        public string Swiftcode { get; set; }

        [JsonProperty("provideremail")]
        public string ProviderEmail { get; set; }

        [JsonProperty("providercontactdetails")]
        public List<ProviderContactDetailAC> ProviderContactDetailACList { get; set; }
    }

    public class ProviderContactDetailAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("contactnumbers")]
        public string ContactNumbers { get; set; }
    }
}
