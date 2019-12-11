using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Master.RoleManagement
{
	public interface IRoleRepository {

		/// <summary>
		/// This method used for get menu list by role id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<List<MenuLinkAC>> GetMenuListByRoleId(long id);
		
		/// <summary>
		/// /This method used for get roles
		/// </summary>
		/// <returns></returns>
		Task<List<RoleAC>> GetRoleList();

		/// <summary>
		/// This method used for add new role
		/// </summary>
		/// <param name="roleAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> AddRole(RoleAC roleAC,long userId,string loginUserName);


		/// <summary>
		/// This method used for edit exists role
		/// </summary>
		/// <param name="roleAC"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> EditRole(RoleAC roleAC, long userId,string loginUserName);

		/// <summary>
		/// This method used for delete exitst role
		/// </summary>
		/// <param name="roleId"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> DeleteRole(long roleId, long userId, string loginUserName);

		/// <summary>
		/// This method used for change exitst role status
		/// </summary>
		/// <param name="roleId"></param>
		/// <param name="userId"></param>
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<bool> ChangeRoleStatus(long roleId, long userId, string loginUserName);


		/// <summary>
		/// This method used for get role by id.
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		Task<RoleAC> GetRoleById(long roleId);
		
		/// <summary>
		/// This method used for get role rights by role is.
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		Task<List<RoleRightsAC>> GetRoleRights(long roleId);


		/// <summary>
		/// This method used for update role rights
		/// </summary>
		/// <param name="roleRightsAC"></param>
		/// <param name="userId"></param> 
		/// <param name="loginUserName"></param>
		/// <returns></returns>
		Task<ResponseAC> UpdateRoleRights(long userId,List<RoleRightsAC> roleRightsAC, string loginUserName);
		
		
		/// <summary>
		/// This method used for get service type list
		/// </summary>
		/// <returns></returns>
		Task<List<ServiceTypeAC>> GetServiceTypes();
	}
}
