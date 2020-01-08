using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class ProviderWiseTransactionAC
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("providername")]
        public string ProviderName { get; set; }

        [JsonProperty("transactiontype")]
        public string TransactionType { get; set; }

        [JsonProperty("settypeas")]
        public int? SetTypeAs { get; set; }

        [JsonProperty("typeas")]
        public string TypeAs { get; set; }

        [JsonProperty("transactiontypelist")]
        public List<DrpResponseAC> TransactionTypeList { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive { get; set; }
    }
}
