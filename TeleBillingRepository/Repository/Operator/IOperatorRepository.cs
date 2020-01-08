using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Operator
{
    public interface IOperatorRepository
    {
        /// <summary>
        /// This method used for get operator call log list 
        /// </summary>
        /// <returns></returns>
        Task<List<OperatorCallLogAC>> OperatorCallLogList();


        /// <summary>
        /// This method used for add operator call log 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="operatorCallLogDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> AddOperatorCallLog(long userId, OperatorCallLogDetailAC operatorCallLogDetailAC, string loginUserName);


        /// <summary>
        /// This method used for delete operator call log 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<bool> DeleteOperatorCallLog(long userId, long id, string loginUserName);


        /// <summary>
        /// This method used for edit call log operator
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="operatorCallLogDetailAC"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<ResponseAC> EditOperatorCallLog(long userId, OperatorCallLogDetailAC operatorCallLogDetailAC, string loginUserName);


        /// <summary>
        /// This method used for get operator call log 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<OperatorCallLogDetailAC> GetOperatorCallLog(long id);


        /// <summary>
        /// This method sued for bulk upload perator call log
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="exceluploadDetail"></param>
        /// <param name="loginUserName"></param>
        /// <returns></returns>
        Task<BulkAssignTelephoneResponseAC> BulkUploadOperatorCallLog(long userId, ExcelUploadResponseAC exceluploadDetail, string loginUserName);
    }
}
