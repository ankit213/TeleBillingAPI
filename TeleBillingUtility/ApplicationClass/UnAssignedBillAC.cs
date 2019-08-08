using Newtonsoft.Json;
using System;

namespace TeleBillingUtility.ApplicationClass {

	public class UnAssignedBillAC {

		[JsonProperty("exceluploadlogid")]
		public long ExcelUploadLogId { get; set; }

		[JsonProperty("exceldetailid")]
		public long ExcelDetailId { get; set;}

		[JsonProperty("calldate")]
		public DateTime CallDate { get; set;}

		[JsonProperty("calltime")]
		public TimeSpan? CallTime { get; set;}

		[JsonProperty("callduration")]
		public TimeSpan? CallDuration { get; set;}

		[JsonProperty("callamount")]
		public decimal CallAmount { get; set;}

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("transtype")]
		public string TransType { get;set;}

		[JsonProperty("servicetypeid")]
		public long ServiceTypeId { get; set; }
		
		[JsonProperty("servicetype")]
		public string ServiceTypeName { get; set; }

		[JsonProperty("callidentificationtype")]
		public long? CallIdentificationType { get; set;}

		[JsonProperty("comment")]
		public string Comment { get; set;}

		[JsonProperty("isautoassigned")]
		public bool IsAutoAssigned { get; set;}
		
	}
}
