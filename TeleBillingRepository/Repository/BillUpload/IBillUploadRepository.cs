using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.BillUpload
{
   public interface IBillUploadRepository
    {
        /// <summary>
        /// This method used for Get Bill Uploaded List
        /// </summary>
        /// <returns></returns>
        Task<List<BillUploadListAC>> GetBillUploadedList();
        /// <summary>
        /// Get Pbx Bill Uploaded List
        /// </summary>
        /// <returns></returns>
        Task<List<PbxBillUploadListAC>> GetPbxBillUploadedList();
         /// <summary>
         /// This method used for get excel mapping 
         /// </summary>
         /// <returns></returns>
        Task<List<MappingDetailAC>> GetExcelMapping(BillUploadAC billUploadModel);
        /// <summary>
        /// GetPbxExcelMapping
        /// </summary>
        /// <param name="PbxbillUploadModel"></param>
        /// <returns></returns>
        Task<List<MappingDetailPbxAC>> GetPbxExcelMapping(PbxBillUploadAC billUploadModel);

        /// <summary>
        /// This method used for upload Excel in system
        /// </summary>
        /// <returns></returns>
        ExcelUploadResponseAC UploadNewExcel(ExcelFileAC excelFileAC);

     
		/// <summary>
		/// This method used for get bill allocation list
		/// </summary>
		/// <param name="billAllocationAC"></param>
		/// <returns></returns>
        Task<BillAllocationListAC> GetBillAllocationList(BillAllocationAC billAllocationAC);

        /// <summary>
        /// ChekBillMergeId to check bill uploaded before or not for same month
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<long> ChekBillMergeId(long providerId, int month, int year);
        /// <summary>
        /// Chek Bill Merge Id Pbx  to check bill uploaded before or not for same month
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<long> ChekBillMergeIdPbx(long deviceId, int month, int year);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="billUploadModel"></param>
        /// <returns></returns>
        Task<long> CheckPBXBillCanMerged(PbxBillUploadAC billUploadModel);

        /// <summary>
        /// Check Bill Can Merged Return ExcecelUploadLog Id if able to merge.
        /// </summary>
        /// <param name="billUploadModel"></param>
        /// <returns></returns>
        Task<long> CheckBillCanMerged(BillUploadAC billUploadModel);

        /// <summary>
        /// Check Is Bill Allocated return true if allocated.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool CheckIsBillAllocated(long id);

        /// <summary>
        /// This method used for save Excel Log In database
        /// </summary>
        /// <param name="BillUploadAC"></param>
        /// <param name="fileNameGuid"></param>
        /// <param name="userId"></param>
        /// <returns>ExcelUplaodId PK</returns>
        Task<long> AddExcelUploadLog(BillUploadAC billUploadModel, string fileNameGuid, long userId);

        /// <summary>
        /// UpdateExcelUploadLog
        /// </summary>
        /// <param name="id"> upload id</param>
        /// <param name="userId"> login user id</param>
        /// <param name="count"> total count</param>
        /// <param name="amount">total amount</param>
        /// <returns></returns>
        Task<Exceluploadlog> UpdateExcelUploadLog(long id, long userId,long count, decimal amount,bool isSkypeData);

        /// <summary>
        /// used to remove temperory data ExcelUploadLog table before successful data read.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> RemoveExcelUploadLog(long id);

        /// <summary>
        /// AddExcelUploadLogPbx
        /// </summary>
        /// <param name="billUploadModel"></param>
        /// <param name="fileNameGuid"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<long> AddExcelUploadLogPbx(PbxBillUploadAC billUploadModel, string fileNameGuid, long userId);

        /// <summary>
        /// This method used for save Excel Detail Call Log In database
        /// </summary>
        /// <param name="List ofExcelDetail"></param>
        /// <returns>true if saved successful</returns>
        Task<bool> AddExcelDetail(List<Exceldetail> excelDetailList);


        /// <summary>
        /// This method used for save Excel Detail Call Log In database
        /// </summary>
        /// <param name="List of Excel Detail Error"></param>
        /// <returns>true if saved successful</returns>
        Task<bool> AddExcelDetailError(AllServiceTypeDataAC allServiceTypeData,string FileNameGuid);

        /// <summary>
        /// Export Mobility Error List
        /// </summary>
        /// <param name="fileGuidNo"></param>
        /// <returns></returns>
        List<MobilityExcelUploadDetailStringAC> ExportMobilityErrorList(string fileGuidNo);

        /// <summary>
        /// Add Skype Excel Detail 
        /// </summary>
        /// <param name="excelDetailList"></param>
        /// <returns></returns>
        Task<bool> AddSkypeExcelDetail(List<Skypeexceldetail> excelDetailList);


        /// <summary>
        /// AddPbxExcelDetail
        /// </summary>
        /// <param name="excelDetailList"></param>
        /// <returns></returns>
        Task<bool> AddPbxExcelDetail(List<Exceldetailpbx> excelDetailList);

        /// <summary>
        /// This method used for upload Excel for Mobility Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<MobilityUploadListAC>> ReadExcelForMobility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

      
        /// <summary>
        /// ReadExcelForPbx
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcel"></param>
        /// <param name="billUploadAC"></param>
        /// <returns></returns>
        Task<ImportBillDetailAC<PbxUploadListAC>> ReadExcelForPbx(string filepath, string filename, MappingDetailPbxAC mappingExcel, PbxBillUploadAC billUploadAC);

        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<MadaUploadListAC>> ReadExcelForMadaService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);


		/// <summary>
		/// This method used for delete exists excel upload
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> DeleteExcelUplaod(long userId, long id, string loginUserName);

		/// <summary>
		/// Approve Excel Upload Log
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> ApproveExcelUploadLog(long userId, long id, string loginUserName);
		/// <summary>
		/// Approve Excel Upload Pbx Log
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> ApproveExcelUploadPbxLog(long userId, long id, string loginUserName);

		/// <summary>
		/// This method used for delete exists pbx excel upload
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> DeletePbxExcelUplaod(long userId, long id, string loginUserName);

        
        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
         Task<ImportBillDetailAC<InternetServiceUploadListAC>> ReadExcelForInternetService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<DataCenterFacilityUploadListAC>> ReadExcelForDataCenterFacility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);



        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<ManagedHostingServiceUploadListAC>> ReadExcelForManagedHostingService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);



        /// <summary>
        /// get First Reading Index With Kems Multiple Service in on excel
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcellist"></param>
        /// <param name="billUploadAC"></param>
        /// <returns></returns>
        MultiServiceUploadAC getFirstReadingIndexWithService(string filepath, string filename, List<MappingDetailAC> mappingExcellist, BillUploadAC billUploadAC);

        /// <summary>
        /// get ReadingIndex With Service From Single Worksheet
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcellist"></param>
        /// <param name="billUploadAC"></param>
        /// <param name="ServiceTitle"></param>
        /// <param name="worksheetNo"></param>
        /// <returns></returns>
        MultiServiceUploadAC getReadingIndexWithServiceFromSingleWorksheet(string filepath, string filename, List<MappingDetailAC> mappingExcellist, BillUploadAC billUploadAC, string[] ServiceTitle, int worksheetNo = 1);

        /// <summary>
        /// GetServiceChargeType
        /// </summary>
        /// <param name="serviceTypeId"></param>
        /// <returns>true if Is Businees only </returns>
        Task<bool> GetServiceChargeType(long serviceTypeId);


          	/// <summary>
		/// This method used for assigne bills
		/// </summary>
		/// <param name="billAssigneAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> AssigneBills(BillAssigneAC billAssigneAC, string userId);
        
		/// <summary>
		/// This method used for get assigned bill list.
		/// </summary>
		/// <param name="employeeId"></param>
		/// <param name="exceluploadlogid"></param>
		/// <param name="businessunitId"></param>
		/// <returns></returns>
		Task<List<UnAssignedBillAC>> GetAssignedBillList(long employeeId, long exceluploadlogid, long businessunitId);

		/// <summary>
		/// This method used for assigned call log move to unassigne.
		/// </summary>
		/// <param name="unAssignedBillACs"></param>
		/// <returns></returns>
		Task<bool> UnAssgineCallLogs(List<UnAssignedBillAC> unAssignedBillACs);

		/// <summary>
		/// This method used for allocate the bill
		/// </summary>
		/// <param name="billAllocationAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> BillAllocation(BillAllocationAC billAllocationAC, long userId, string loginUserName);

        /// <summary>
        /// ReadExcelForInternetServiceMultiple
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcel"></param>
        /// <param name="singleWorksheetserviceList"></param>
        /// <param name="billUploadAC"></param>
        /// <param name="ReadingIndex"></param>
        /// <param name="ServiceTitle"></param>
        /// <param name="worksheetNo"></param>
        /// <returns></returns>
        Task<ImportBillDetailMultipleAC<InternetServiceUploadListAC>> ReadExcelForInternetServiceMultiple(string filepath, string filename, MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList, BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1);


        /// <summary>
        /// ReadExcelForInternetServiceMultiple
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcel"></param>
        /// <param name="singleWorksheetserviceList"></param>
        /// <param name="billUploadAC"></param>
        /// <param name="ReadingIndex"></param>
        /// <param name="ServiceTitle"></param>
        /// <param name="worksheetNo"></param>
        /// <returns></returns>
        Task<ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>> ReadExcelForDataCenterFacilityMultiple(string filepath, string filename, MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList, BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1);

        /// <summary>
        /// ReadExcelForManagedHostingServiceMultiple
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcel"></param>
        /// <param name="singleWorksheetserviceList"></param>
        /// <param name="billUploadAC"></param>
        /// <param name="ReadingIndex"></param>
        /// <param name="ServiceTitle"></param>
        /// <param name="worksheetNo"></param>
        /// <returns></returns>
        Task<ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>> ReadExcelForManagedHostingServiceMultiple(string filepath, string filename, MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList, BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1);

        /// <summary>
        /// Read Excel For Mobility,Voice Only,StaticIP,Internet Device Plan Offer Service Multiple 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcel"></param>
        /// <param name="singleWorksheetserviceList"></param>
        /// <param name="billUploadAC"></param>
        /// <param name="ReadingIndex"></param>
        /// <param name="ServiceTitle"></param>
        /// <param name="worksheetNo"></param>
        /// <param name="ServiceTypeId"></param>
        /// <returns></returns>
        Task<ImportBillDetailMultipleAC<MobilityUploadListAC>> ReadExcelForMobilityServiceMultiple
                    (string filepath, string filename,
                     MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
                     BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1
                     , long ServiceTypeId = 0);

        /// <summary>
        /// CheckMappingWithFileFormat
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="MaxWorkSheetNo"></param>
        /// <returns></returns>
        Task<SaveAllServiceExcelResponseAC> CheckMappingWithFileFormat(string filepath, string filename, long MaxWorkSheetNo);

        /// <summary>
        /// Read Excel For Voip
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="mappingExcel"></param>
        /// <param name="billUploadAC"></param>
        /// <returns></returns>
        Task<ImportBillDetailAC<VoipUploadListAC>> ReadExcelForVoip(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);
     }
}
