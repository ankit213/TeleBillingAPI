using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;


namespace TeleBillingRepository.Repository.BillProcess
{
	public interface IBillProcessRepository
	{

		/// <summary>
		///  This method used for get current bills
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<List<CurrentBillAC>> GetCurrentBills(long userId);


		/// <summary>
		/// This method used for get view bill detail by employee bill master id.
		/// </summary>
		/// <param name="employeebillmasterid"></param>
		/// <returns></returns>
		Task<ViewBillDetailAC> GetViewBillDetails(long employeebillmasterid);


		/// <summary>
		/// This method used for save bill identification 
		/// </summary>
		/// <param name="billIdentificationAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> BillIdentificationSave(BillIdentificationAC billIdentificationAC,long userId);


		/// <summary>
		/// This method used for bill process
		/// </summary>
		/// <param name="billIdentificationAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> BillProcess(BillIdentificationAC billIdentificationAC, long userId);


		/// <summary>
		/// This method used for get line manage approval list for logginuserId
		/// </summary>
		/// <param name="loginedUserId"></param>
		/// <returns></returns>
		Task<List<CurrentBillAC>> GetLineManagerApprovalList(long loginedUserId);


		/// <summary>
		/// This method used for employee bill approve by line manager
		/// </summary>
		/// <param name="lineManagerApprovalAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> LineManagerApproval(LineManagerApprovalAC lineManagerApprovalAC,long userId);

		/// <summary>
		/// This method used for get my staff bills
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<List<CurrentBillAC>> GetMyStaffBills(long userId);


		/// <summary>
		/// This method used for get previous period bills
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<List<CurrentBillAC>> GetPreviousPeriodBills(long userId);

		/// <summary>
		/// This method used for reidentification and return new employeebillid.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="employeebillmasterid"></param>
		/// <returns></returns>
		Task<long> ReIdentificationRequest(long userId, long employeebillmasterid);

		/// <summary>
		/// This method used for request to reimbursement request  
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="reImbursementRequestAC"></param>
		/// <returns></returns>
		Task<ResponseAC> ReImbursementRequest(long userId, ReImbursementRequestAC reImbursementRequestAC);

		/// <summary>
		/// This function used for get reimburse bills
		/// </summary>
		/// <returns></returns>
		Task<List<ReImburseBillsAC>> GetReImburseBills();

		/// <summary>
		/// This method used for approve re-imburse request 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="reImburseBillApprovalAC"></param>
		/// <returns></returns>
		Task<ResponseAC> ReImburseBillApproval(long userId, ReImburseBillApprovalAC reImburseBillApprovalAC);


		/// <summary>
		/// This mehtod used for get change bill status 
		/// </summary>
		/// <returns></returns>
		Task<List<ChangeBillStatusAC>> GetChangeBillStatusList();

		/// <summary>
		/// This method used for change bill status
		/// </summary>
		/// <param name="changeBillStatusACs"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> ChangeBillStatus(List<ChangeBillStatusAC> changeBillStatusACs, long userId);
	}
}
