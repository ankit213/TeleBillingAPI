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
        /// This method used for get excel mapping list 
        /// </summary>
        /// <returns></returns>
        Task<List<BillUploadListAC>> GetBillUploadedList();

         /// <summary>
        /// This method used for get excel mapping 
        /// </summary>
        /// <returns></returns>
        Task<List<MappingDetailAC>> GetExcelMapping(BillUploadAC billUploadModel);

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
        /// This method used for save Excel Log In database
        /// </summary>
        /// <param name="BillUploadAC"></param>
        /// <param name="fileNameGuid"></param>
        /// <param name="userId"></param>
        /// <returns>ExcelUplaodId PK</returns>
        Task<long> AddExcelUploadLog(BillUploadAC billUploadModel, string fileNameGuid, long userId);

        /// <summary>
        /// This method used for save Excel Detail Call Log In database
        /// </summary>
        /// <param name="List ofExcelDetail"></param>
        /// <returns>true if saved successful</returns>
        Task<bool> AddExcelDetail(List<ExcelDetail> excelDetailList);

        /// <summary>
        /// This method used for upload Excel for Mobility Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<MobilityUploadListAC>> ReadExcelForMobility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<MadaUploadListAC>> ReadExcelForMadaService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<InternetServiceUploadListAC>> ReadExcelForInternetServiceold(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

        /// <summary>
		/// This method used for delete exists excel mapping
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteExcelUplaod(long userId, long id);

        /// <summary>
        /// This method used for upload Excel for Mobility Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<MobilityUploadListAC>> ReadExcelForMobilityold(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

        /// <summary>
        /// This method used for upload Excel for Mada Provider Service in system
        /// </summary>
        /// <returns></returns>
        Task<ImportBillDetailAC<MadaUploadListAC>> ReadExcelForMadaServiceold(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC);

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
		/// <returns></returns>
		Task<ResponseAC> BillAllocation(BillAllocationAC billAllocationAC, long userId);

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

    }
}
