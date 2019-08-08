using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
	public class BulkAssignTelephoneResponseAC
	{
		[JsonProperty("totalrecords")]
		public long TotalRecords { get;set;}

		[JsonProperty("skiprecords")]
		public long SkipRecords { get; set; }

		[JsonProperty("successrecords")]
		public long SuccessRecords { get; set; }

		[JsonProperty("exceluploadresultlist")]
		public List<ExcelUploadResult> excelUploadResultList { get; set;}

	}


	public class ExcelUploadResult
	{
		[JsonProperty("celladdress")]
		public string CellAddress { get; set;}
		
		[JsonProperty("errormessage")]
		public string ErrorMessage { get; set; }

		[JsonProperty("sheetname")]
		public string SheetName {  get; set;}

		[JsonProperty("recorddetail")]
		public string RecordDetail { get; set;}
	}
}
