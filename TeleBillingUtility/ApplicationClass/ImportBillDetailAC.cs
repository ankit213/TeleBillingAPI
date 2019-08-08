using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TeleBillingUtility.Models;

namespace TeleBillingUtility.ApplicationClass
{


    public class AllServiceTypeDataAC
    {

        public AllServiceTypeDataAC()
        {
            ValidList = new List<ExcelDetail>();
            InvalidList1 = new List<MobilityExcelUploadDetailStringAC>();
            InvalidList3 = new List<InternetServiceExcelUploadDetailStringAC>();
            InvalidList4 = new List<DataCenterFacilityExcelUploadDetailStringAC>();
            InvalidList5 = new List<ManagedHostingServiceExcelUploadDetailStringAC>();
            InvalidList9 = new List<MadaExcelUploadDetailStringAC>();
        }
        [JsonProperty("validlst")]
        public List<ExcelDetail> ValidList { get; set; }

        [JsonProperty("invalidlist1")]
        public List<MobilityExcelUploadDetailStringAC> InvalidList1 { get; set; }

        [JsonProperty("invalidlist3")]
        public List<InternetServiceExcelUploadDetailStringAC> InvalidList3 { get; set; }

        [JsonProperty("invalidlist4")]
        public List<DataCenterFacilityExcelUploadDetailStringAC> InvalidList4 { get; set; }

        [JsonProperty("invalidlist5")]
        public List<ManagedHostingServiceExcelUploadDetailStringAC> InvalidList5 { get; set; }

        [JsonProperty("invalidlist9")]
        public List<MadaExcelUploadDetailStringAC> InvalidList9 { get; set; }

    }

    public class SaveAllServiceExcelResponseAC
    {
        [JsonProperty("statuscode")]
        public long StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("totalamount")]
        public string TotalAmount { get; set; }

        [JsonProperty("totalvalidcount")]
        public string TotalValidCount { get; set; }

        [JsonProperty("invalidlist1")]
        public List<MobilityExcelUploadDetailStringAC> InvalidList1 { get; set; }

        [JsonProperty("invalidlist3")]
        public List<InternetServiceExcelUploadDetailStringAC> InvalidList3 { get; set; }

        [JsonProperty("invalidlist4")]
        public List<DataCenterFacilityExcelUploadDetailStringAC> InvalidList4 { get; set; }

        [JsonProperty("invalidlist5")]
        public List<ManagedHostingServiceExcelUploadDetailStringAC> InvalidList5 { get; set; }

        [JsonProperty("invalidlist9")]
        public List<MadaExcelUploadDetailStringAC> InvalidList9 { get; set; }
    }

    public class ImportBillDetailAC<T> where T : class
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("uploaddetail")]
        public ResponseDynamicDataAC<T> UploadData { get; set; }
    }

    public class ImportBillDetailMultipleAC<T> where T : class
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("readingindex")]
        public long ReadingIndex { get; set; }

        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }

        [JsonProperty("uploaddetail")]
        public ResponseDynamicDataAC<T> UploadData { get; set; }
    }

    public class SaveExcelResponseAC<T>
    {

        [JsonProperty("statuscode")]
        public long StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("totalamount")]
        public string TotalAmount { get; set; }

        [JsonProperty("totalvalidcount")]
        public string TotalValidCount { get; set; }

        [JsonProperty("uploaddetaillist")]
        public List<T> UploadDataList { get; set; }
    }
    
    public class ResponseDynamicDataAC<T> where T : class
    {
        private T _Data;

        public T Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
            }
        }
    }

    public class ResponseDynamicListDC<T>
    {
        private List<T> _Data = new List<T>();

     
        public List<T> Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
            }
        }
    }

    public class MobilityUploadListAC
    {
        [JsonProperty("validlst")]
        public List<ExcelDetail> ValidMobilityList { get; set; }

        [JsonProperty("invalidlist")]
        public List<MobilityExcelUploadDetailStringAC> InvalidMobilityList { get; set; }
    }

    public class MadaUploadListAC
    {
        [JsonProperty("validlst")]
        public List<ExcelDetail> ValidList { get; set; }

        [JsonProperty("invalidlist")]
        public List<MadaExcelUploadDetailStringAC> InvalidList { get; set; }
    }

    public class InternetServiceUploadListAC
    {
        [JsonProperty("validlst")]
        public List<ExcelDetail> ValidList { get; set; }

        [JsonProperty("invalidlist")]
        public List<InternetServiceExcelUploadDetailStringAC> InvalidList { get; set; }      
    }

    public class DataCenterFacilityUploadListAC
    {
        [JsonProperty("validlst")]
        public List<ExcelDetail> ValidList { get; set; }

        [JsonProperty("invalidlist")]
        public List<DataCenterFacilityExcelUploadDetailStringAC> InvalidList { get; set; }
       
    }

    public class ManagedHostingServiceUploadListAC
    {
        [JsonProperty("validlst")]
        public List<ExcelDetail> ValidList { get; set; }
       
        [JsonProperty("invalidlist")]
        public List<ManagedHostingServiceExcelUploadDetailStringAC> InvalidList { get; set; }
    }
    
    public  class MultiServiceUploadAC
    {
        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }

        [JsonProperty("readingindex")]
        public long ReadingIndex { get; set; }
    }
}
