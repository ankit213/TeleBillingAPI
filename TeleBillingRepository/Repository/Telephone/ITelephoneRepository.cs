using System;
using System.Collections.Generic;
using System.Text;
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
		Task<List<TelephoneAC>> GetTelephoneList();

		/// <summary>
		/// This method used for add telephone
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="telephoneDetailAC"></param>
		/// <returns></returns>
		Task<ResponseAC> AddTelephone(long userId, TelephoneDetailAC telephoneDetailAC);

		/// <summary>
		/// This method used for edit exists telephone
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="telephoneDetailAC"></param>
		/// <returns></returns>
		Task<ResponseAC> UpdateTelephone(long userId, TelephoneDetailAC telephoneDetailAC);

		/// <summary>
		/// This method used for delete exists telephone
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteTelphone(long userId, long id);


		/// <summary>
		/// This method used for change status exists telephone
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> ChangeTelephoneStatus(long userId, long id);


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
		Task<List<AssignTelePhoneAC>> GetAssignedTelephoneList();

		/// <summary>
		/// This method used for assign new telephone
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="assignTelephoneDetailAC"></param>
		/// <returns></returns>
		Task<ResponseAC> AddAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC);

		/// <summary>
		/// This method used for update assigned telephone
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="assignTelephoneDetailAC"></param>
		/// <returns></returns>
		Task<ResponseAC> UpdateAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC);
		
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
		/// <returns></returns>
		Task<BulkAssignTelephoneResponseAC> UploadBulkAssignTelePhone(long userId ,ExcelUploadResponseAC exceluploadDetail);
	}
}
