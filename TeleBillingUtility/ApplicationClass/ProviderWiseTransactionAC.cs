using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class ProviderWiseTransactionAC {

		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("providerid")]
		public long ProviderId { get; set; }

		[JsonProperty("providername")]
		public string ProviderName { get; set;}

		[JsonProperty("transactiontype")]
		public string TransactionType { get; set; }

		[JsonProperty("settypeas")]
		public long? SetTypeAs { get; set; }

		[JsonProperty("typeas")]
		public string TypeAs { get; set; }

		[JsonProperty("transactiontypelist")]
		public List<DrpResponseAC> TransactionTypeList { get; set; }
	}
}
