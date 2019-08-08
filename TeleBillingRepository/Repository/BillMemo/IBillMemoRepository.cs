using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.BillMemo
{
	public interface IBillMemoRepository
	{

		/// <summary>
		/// This method used for get memo list.
		/// </summary>
		/// <returns></returns>
		Task<List<BillMemoAC>> GetMemoList();
		

		/// <summary>
		/// This methods used for get bills for memo.
		/// </summary>
		/// <param name="month"></param>
		/// <param name="year"></param>
		/// <param name="providerid"></param>
		/// <returns></returns>
		Task<MemoBillResponse> GetMemoBills(int month, int year, int providerid);


		/// <summary>
		/// This method used for add new memo.
		/// </summary>
		/// <param name="memoAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> AddMemo(MemoAC memoAC, long userId);

		/// <summary>
		/// This method used for delete memo bills
		/// </summary>
		/// <param name="id"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<bool> DeleteMemoBills(long id, long userId);

		/// <summary>
		/// This method used for get approval memo list.
		/// </summary>
		/// <returns></returns>
		Task<List<BillMemoAC>> GetApprvoalMemoList();

		/// <summary>
		/// This method used for approe/reject memo request.
		/// </summary>
		/// <param name="memoApprovalAC"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<ResponseAC> MemoApproval(MemoApprovalAC memoApprovalAC, long userId);


		/// <summary>
		/// This method used for get memo list for accountant.
		/// </summary>
		/// <returns></returns>
		Task<List<BillMemoAC>> GetAccountMemoList();
		
	}
}
