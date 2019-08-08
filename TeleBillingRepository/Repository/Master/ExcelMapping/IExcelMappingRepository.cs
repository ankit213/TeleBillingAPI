using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Master.ExcelMapping
{
   public interface IExcelMappingRepository
    {

        /// <summary>
        /// This method used for get excel mapping list 
        /// </summary>
        /// <returns></returns>
        Task<List<ExcelMappingListAC>> GetExcelMappingList();

        /// <summary>
		/// This method used for delete exists excel mapping
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteExcelMapping(long userId, long id);

        /// <summary>
		/// This method used for add new ExcelMapping
		/// </summary>
		/// <param name="ExcelMappingAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> AddExcelMapping(ExcelMappingAC excelMappingAC, long userId);


        /// <summary>
        /// This method used for get excel mapping by id.
        /// </summary>
        /// <param name="excelMappingId"></param>
        /// <returns></returns>
        Task<ExcelMappingAC> GetExcelMappingById(long excelMappingId);


        /// <summary>
        /// This method used for edit ExcelMapping
        /// </summary>
        /// <param name="ExcelMappingAC"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseAC> EditExcelMapping(ExcelMappingAC excelMappingAC, long userId);



        /// <summary>
        /// This method used for get Pbx excel mapping list 
        /// </summary>
        /// <returns></returns>
        Task<List<PbxExcelMappingListAC>> GetPbxExcelMappingList();

        /// <summary>
		/// This method used for delete exists Pbx excel mapping
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeletePbxExcelMapping(long userId, long id);

        /// <summary>
		/// This method used for add new Pbx Excel Mapping
		/// </summary>
		/// <param name="ExcelMappingAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
	 	Task<ResponseAC> AddPbxExcelMapping(PbxExcelMappingAC excelMappingAC, long userId);


        /// <summary>
        /// This method used for get Pbx excel mapping by id.
        /// </summary>
        /// <param name="excelMappingId"></param>
        /// <returns></returns>
        Task<PbxExcelMappingAC> GetPbxExcelMappingById(long excelMappingId);


        /// <summary>
        /// This method used for edit Pbx Excel Mapping
        /// </summary>
        /// <param name="ExcelMappingAC"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
       Task<ResponseAC> EditPbxExcelMapping(PbxExcelMappingAC excelMappingAC, long userId);
                          
    }
}
