using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Telephone
{
    public interface ITelephoneRepository
    {
        /// <summary>
        /// This method used for get telephone list. 
        /// </summary>
        /// <returns></returns>
        Task<JqueryDataTablesPagedResults<TelephoneAC>> GetTelephoneList(JqueryDataTablesParameters param);



        /// <summary>
        /// This function used for export telephone list
        /// </summary>
        /// <returns></returns>
        List<ExportTelePhoneAC> GetTelephoneExportList();

        /// <summary>
        /// This method used for add telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="telephoneDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> AddTelephone(long userId, TelephoneDetailAC telephoneDetailAC, string loginUserName);

        /// <summary>
        /// This method used for edit exists telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="telephoneDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> UpdateTelephone(long userId, TelephoneDetailAC telephoneDetailAC, string loginUserName);

        /// <summary>
        /// This method used for delete exists telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> DeleteTelphone(long userId, long id, string loginUserName);


        /// <summary>
        /// This method used for change status exists telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> ChangeTelephoneStatus(long userId, long id, string loginUserName);


        /// <summary>
        /// This method used for get telephone by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TelephoneDetailAC> GetTelephoneById(long id);


        /// <summary>
        /// This method used for get assigned telephone list
        /// </summary>
        /// <returns></returns>
        JqueryDataTablesPagedResults<AssignTelePhoneAC> GetAssignedTelephoneList(JqueryDataTablesParameters param);

        /// <summary>
        /// Get Assigned Telephone Package detil List by assignTelephoneId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<AssignTelePhonePackageDetailAC>> GetAssignedTelephonePackageList(long id);

        /// <summary>
        /// This method used for assign new telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assignTelephoneDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> AddAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC, string loginUserName);

        /// <summary>
        /// This method used for update assigned telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assignTelephoneDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> UpdateAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC, string loginUserName);

        /// <summary>
        /// This method used for get assined telephone detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AssignTelephoneDetailAC> GetAssignedTelephoneById(long id);


        /// <summary>
        /// This method used for bulk assign telephone
        /// </summary>
        /// <param name="exceluploadDetail"></param>
        /// <param name="userId"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<BulkAssignTelephoneResponseAC> UploadBulkAssignTelePhone(long userId, ExcelUploadResponseAC exceluploadDetail, string loginUserName);


        /// <summary>
        /// This method used for delete assigned telephone
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> DeleteAssignedTelephone(long userId, long id, string loginUserName);


        /// <summary>
        /// This method used for export assigned telephone list
        /// </summary>
        /// <returns></returns>
        List<ExportAssignedTelePhoneAC> GetAssignedTelephoneExportList();
    }
}
