using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class PackageAC {

		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("providerid")]
		public long ProviderId { get; set; }

		[JsonProperty("servicetypeid")]
		public long ServiceTypeId { get; set; }

		[JsonProperty("providername")]
		public string ProviderName { get; set;}

		[JsonProperty("amount")]
		public decimal PackageAmount { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("startdate")]
		public DateTime StartDate {  get; set;}

		[JsonProperty("updateddate")]
		public DateTime? UpdatedDate { get; set; }

		[JsonProperty("isactive")]
		public bool IsActive { get; set; }

		[JsonProperty("servicetype")]
		public string ServiceType {  get; set; }
	}
}
