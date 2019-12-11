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
            ValidList = new List<Exceldetail>();
            ValidListSkype = new List<Skypeexceldetail>();
            ValidListPbx = new List<Exceldetailpbx>();
            InvalidList1 = new List<MobilityExcelUploadDetailStringAC>();

            InvalidList2 = new List<VoiceOnlyExcelUploadDetailStringAC>();
            InvalidList6 = new List<StaticIPExcelUploadDetailStringAC>();


            InvalidList3 = new List<InternetServiceExcelUploadDetailStringAC>();
            InvalidList4 = new List<DataCenterFacilityExcelUploadDetailStringAC>();
            InvalidList5 = new List<ManagedHostingServiceExcelUploadDetailStringAC>();
            InvalidList9 = new List<MadaExcelUploadDetailStringAC>();
            InvalidList7 = new List<VoipExcelUploadDetailStringAC>();
            InvalidListpbx = new List<PbxExcelUploadDetailStringAC>();

            InvalidListAllDB = new List<ExceldetailError>();
            InvalidListSkypeAllDB = new List<SkypeexceldetailError>();

        }
        [JsonProperty("validlst")]
        public List<Exceldetail> ValidList { get; set; }

        [JsonProperty("validlstskype")]
        public List<Skypeexceldetail> ValidListSkype { get; set; }

        [JsonProperty("validlstpbx")]
        public List<Exceldetailpbx> ValidListPbx { get; set; }

        [JsonProperty("invalidlist1")]
        public List<MobilityExcelUploadDetailStringAC> InvalidList1 { get; set; }

        [JsonProperty("invalidlist2")]
        public List<VoiceOnlyExcelUploadDetailStringAC> InvalidList2 { get; set; }

        [JsonProperty("invalidlist6")]
        public List<StaticIPExcelUploadDetailStringAC> InvalidList6 { get; set; }


        [JsonProperty("invalidlist3")]
        public List<InternetServiceExcelUploadDetailStringAC> InvalidList3 { get; set; }

        [JsonProperty("invalidlist4")]
        public List<DataCenterFacilityExcelUploadDetailStringAC> InvalidList4 { get; set; }

        [JsonProperty("invalidlist5")]
        public List<ManagedHostingServiceExcelUploadDetailStringAC> InvalidList5 { get; set; }

        [JsonProperty("invalidlist7")]
        public List<VoipExcelUploadDetailStringAC> InvalidList7 { get; set; }


        [JsonProperty("invalidlist9")]
        public List<MadaExcelUploadDetailStringAC> InvalidList9 { get; set; }

        [JsonProperty("invalidlistpbx")]
        public List<PbxExcelUploadDetailStringAC> InvalidListpbx { get; set; }


        [JsonProperty("invalidlistalldb")]
        public List<ExceldetailError> InvalidListAllDB { get; set; }


        [JsonProperty("invalidlistskypealldb")]
        public List<SkypeexceldetailError> InvalidListSkypeAllDB { get; set; }


    }

    public class SaveAllServiceExcelResponseAC
    {

        public SaveAllServiceExcelResponseAC()
        {            
            InvalidList1 = new List<MobilityExcelUploadDetailStringAC>();
            InvalidList2 = new List<VoiceOnlyExcelUploadDetailStringAC>();
            InvalidList6 = new List<StaticIPExcelUploadDetailStringAC>();
            InvalidList3 = new List<InternetServiceExcelUploadDetailStringAC>();
            InvalidList4 = new List<DataCenterFacilityExcelUploadDetailStringAC>();
            InvalidList5 = new List<ManagedHostingServiceExcelUploadDetailStringAC>();
            InvalidList9 = new List<MadaExcelUploadDetailStringAC>();
            InvalidList7 = new List<VoipExcelUploadDetailStringAC>();
            InvalidListPbx = new List<PbxExcelUploadDetailStringAC>();

            ExcelUploadId = 0;
            FileGuidNo = string.Empty;
            mode = 0;
        }

        [JsonProperty("exceluploadid")]
        public long ExcelUploadId { get; set; }

        [JsonProperty("statuscode")]
        public long StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("totalamount")]
        public string TotalAmount { get; set; }

        [JsonProperty("totalvalidcount")]
        public string TotalValidCount { get; set; }

        [JsonProperty("fileguidno")]
        public string FileGuidNo { get; set; }

        [JsonProperty("mode")]
        public int mode { get; set; }

        [JsonProperty("invalidlist1")]
        public List<MobilityExcelUploadDetailStringAC> InvalidList1 { get; set; }

        [JsonProperty("invalidlist2")]
        public List<VoiceOnlyExcelUploadDetailStringAC> InvalidList2 { get; set; }
        
        [JsonProperty("invalidlist6")]
        public List<StaticIPExcelUploadDetailStringAC> InvalidList6 { get; set; }


        [JsonProperty("invalidlist3")]
        public List<InternetServiceExcelUploadDetailStringAC> InvalidList3 { get; set; }

        [JsonProperty("invalidlist4")]
        public List<DataCenterFacilityExcelUploadDetailStringAC> InvalidList4 { get; set; }

        [JsonProperty("invalidlist5")]
        public List<ManagedHostingServiceExcelUploadDetailStringAC> InvalidList5 { get; set; }

        [JsonProperty("invalidlist7")]
        public List<VoipExcelUploadDetailStringAC> InvalidList7 { get; set; }

        [JsonProperty("invalidlist9")]
        public List<MadaExcelUploadDetailStringAC> InvalidList9 { get; set; }

        [JsonProperty("invalidlistpbx")]
        public List<PbxExcelUploadDetailStringAC> InvalidListPbx { get; set; }
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
        public MobilityUploadListAC()
        {
            ValidMobilityList = new List<Exceldetail>();
            InvalidMobilityList = new List<MobilityExcelUploadDetailStringAC>();
            InvalidListAllDB = new List<ExceldetailError>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidMobilityList { get; set; }

        [JsonProperty("invalidlist")]
        public List<MobilityExcelUploadDetailStringAC> InvalidMobilityList { get; set; }

        [JsonProperty("invalidlistalldb")]
        public List<ExceldetailError> InvalidListAllDB { get; set; }
    }
    
    public class StaticIPUploadListAC
    {
        public StaticIPUploadListAC()
        {
            ValidStaticIPList = new List<Exceldetail>();
            InvalidStaticIPList = new List<StaticIPExcelUploadDetailStringAC>();

        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidStaticIPList { get; set; }

        [JsonProperty("invalidlist")]
        public List<StaticIPExcelUploadDetailStringAC> InvalidStaticIPList { get; set; }
    }
    
    public class VoiceOnlyUploadListAC
    {

        public VoiceOnlyUploadListAC()
        {
            ValidVoiceOnlyList = new List<Exceldetail>();
            InvalidVoiceOnlyList = new List<VoiceOnlyExcelUploadDetailStringAC>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidVoiceOnlyList { get; set; }

        [JsonProperty("invalidlist")]
        public List<VoiceOnlyExcelUploadDetailStringAC> InvalidVoiceOnlyList { get; set; }
    }

    public class MadaUploadListAC
    {
        public MadaUploadListAC()
        {
            ValidList = new List<Exceldetail>();
            InvalidList = new List<MadaExcelUploadDetailStringAC>();
            InvalidListAllDB = new List<ExceldetailError>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidList { get; set; }

        [JsonProperty("invalidlist")]
        public List<MadaExcelUploadDetailStringAC> InvalidList { get; set; }

        [JsonProperty("invalidlistalldb")]
        public List<ExceldetailError> InvalidListAllDB { get; set; }
    }

    public class InternetServiceUploadListAC
    {
        public InternetServiceUploadListAC()
        {
            ValidList = new List<Exceldetail>();
            InvalidList = new List<InternetServiceExcelUploadDetailStringAC>();
            InvalidListAllDB = new List<ExceldetailError>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidList { get; set; }

        [JsonProperty("invalidlist")]
        public List<InternetServiceExcelUploadDetailStringAC> InvalidList { get; set; }

        [JsonProperty("invalidlistalldb")]
        public List<ExceldetailError> InvalidListAllDB { get; set; }
    }

    public class DataCenterFacilityUploadListAC
    {
        public DataCenterFacilityUploadListAC()
        {
            ValidList = new List<Exceldetail>();
            InvalidList = new List<DataCenterFacilityExcelUploadDetailStringAC>();
            InvalidListAllDB = new List<ExceldetailError>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidList { get; set; }

        [JsonProperty("invalidlist")]
        public List<DataCenterFacilityExcelUploadDetailStringAC> InvalidList { get; set; }

        [JsonProperty("invalidlistalldb")]
        public List<ExceldetailError> InvalidListAllDB { get; set; }
    }

    public class ManagedHostingServiceUploadListAC
    {
        public ManagedHostingServiceUploadListAC()
        {
            ValidList = new List<Exceldetail>();
            InvalidList = new List<ManagedHostingServiceExcelUploadDetailStringAC>();
            InvalidListAllDB = new List<ExceldetailError>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetail> ValidList { get; set; }
       
        [JsonProperty("invalidlist")]
        public List<ManagedHostingServiceExcelUploadDetailStringAC> InvalidList { get; set; }

        [JsonProperty("invalidlistalldb")]
        public List<ExceldetailError> InvalidListAllDB { get; set; }
    }
    
    public  class MultiServiceUploadAC
    {
        [JsonProperty("servicetypeid")]
        public long ServiceTypeId { get; set; }

        [JsonProperty("readingindex")]
        public long ReadingIndex { get; set; }
    }

    public class VoipUploadListAC
    {

        public VoipUploadListAC()
        {
            ValidVoipList = new List<Skypeexceldetail>();
            InvalidVoipList = new List<VoipExcelUploadDetailStringAC>();
        }

        [JsonProperty("validlst")]
        public List<Skypeexceldetail> ValidVoipList { get; set; }

        [JsonProperty("invalidlist")]
        public List<VoipExcelUploadDetailStringAC> InvalidVoipList { get; set; }
    }


    /*  PBX Upload AC Class */
    public class PbxUploadListAC
    {
       public  PbxUploadListAC()
        {
            ValidPbxList = new List<Exceldetailpbx>();
            InvalidPbxList = new List<PbxExcelUploadDetailStringAC>();
        }

        [JsonProperty("validlst")]
        public List<Exceldetailpbx> ValidPbxList { get; set; }

        [JsonProperty("invalidlist")]
        public List<PbxExcelUploadDetailStringAC> InvalidPbxList { get; set; }
    }
}
