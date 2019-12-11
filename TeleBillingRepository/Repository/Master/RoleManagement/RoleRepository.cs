using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Master.RoleManagement
{
	public class RoleRepository : IRoleRepository {
		
		#region "Private Variable(s)"
		private readonly telebilling_v01Context  _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
	    private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;

		#endregion

		#region "Constructor"
		public RoleRepository(telebilling_v01Context  dbTeleBilling_V01Context,IMapper mapper, IStringConstant iStringConstant
			,ILogManagement ilogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
		}
		#endregion

		#region "Public Method(s)"

		public async Task<List<MenuLinkAC>> GetMenuListByRoleId(long id) {
			List<MenuLinkAC> menuLinkACList = new List<MenuLinkAC>();

			var moduleLink = (from mstModule in _dbTeleBilling_V01Context.MstModule join
						  linkModule in _dbTeleBilling_V01Context.MstLink on mstModule.ModuleId equals linkModule.ModuleId 
							   join roleRights in _dbTeleBilling_V01Context.MstRolerights on linkModule.LinkId equals roleRights.LinkId 
							   where roleRights.RoleId == id && roleRights.IsDelete == false && linkModule.IsActive && roleRights.IsView == true
							  select new NavigationMenuAC
							  {
								  ViewIndex = Convert.ToInt32(mstModule.ViewIndex),
								  IconName = mstModule.IconName,
								  IsSinglePage = linkModule.IsSinglePage,
								  ModuleId = mstModule.ModuleId,
								  ModuleName = mstModule.ModuleName,
								  ParentId = linkModule.ParentId,
								  RouteLink = linkModule.RouteLink,
								  LinkId = linkModule.LinkId,
								  Title = linkModule.Title,
								  IsAdd = roleRights.IsAdd,
								  IsEdit = roleRights.IsEdit,
								  IsDelete = roleRights.IsDelete,
								  IsView = roleRights.IsView,
								  IsReadOnly = roleRights.IsReadOnly,
								  IsEditable = roleRights.IsEditable,
								  IsChangeStatus = roleRights.IsChangeStatus,
								  LinkViewIndex = linkModule.ViewIndex
							  }).OrderBy(x=>x.ViewIndex).ThenBy(x=>x.LinkViewIndex).GroupBy(x=>x.ModuleId).ToList();

			foreach (var item in moduleLink)
			{
				foreach (var subItem in item)
				{
					if (menuLinkACList.Count(x => x.ModuleId == subItem.ModuleId) == 0)
					{
						MenuLinkAC menuLinkAC = new MenuLinkAC();

						if (subItem.IsSinglePage && subItem.IsView)
						{
							menuLinkAC.IconName = subItem.IconName;
							menuLinkAC.RouteLink = subItem.RouteLink;
							menuLinkAC.IsAdd = subItem.IsAdd;
						    menuLinkAC.IsEdit = subItem.IsEdit;
						    menuLinkAC.IsDelete = subItem.IsDelete;
						    menuLinkAC.IsView = subItem.IsView;
							menuLinkAC.IsReadOnly = subItem.IsReadOnly;
							menuLinkAC.IsEditable = subItem.IsEditable;
							menuLinkAC.IsChangeStatus = subItem.IsChangeStatus;
							menuLinkAC.Title = subItem.Title;
							menuLinkAC.Home = true;
							menuLinkAC.ModuleId = subItem.ModuleId;
							menuLinkACList.Add(menuLinkAC);
						}
						else if(subItem.IsView)
						{
							menuLinkAC.IconName = subItem.IconName;
							menuLinkAC.RouteLink = subItem.RouteLink;
							menuLinkAC.Title = subItem.ModuleName;
							menuLinkAC.IsAdd = subItem.IsAdd;
							menuLinkAC.IsEdit = subItem.IsEdit;
							menuLinkAC.IsEditable = subItem.IsEditable;
							menuLinkAC.IsDelete = subItem.IsDelete;
							menuLinkAC.IsReadOnly = subItem.IsReadOnly;
							menuLinkAC.IsView = subItem.IsView;
							menuLinkAC.IsChangeStatus = subItem.IsChangeStatus;
							menuLinkAC.Home = false;
							menuLinkAC.ModuleId = subItem.ModuleId;
							menuLinkAC.Children = new List<MenuLinkAC>();
							List<MstLink> linkList = await _dbTeleBilling_V01Context.MstLink.Where(x => x.ModuleId == item.Key && x.IsActive).OrderBy(x=>x.ViewIndex).ToListAsync();
							foreach (var subLink in linkList)
							{   var mstRoleRights = await _dbTeleBilling_V01Context.MstRolerights.FirstOrDefaultAsync(x => x.IsView && x.LinkId == subLink.LinkId && x.RoleId == id);
								if (mstRoleRights != null) { 
									MenuLinkAC newSubLink = new MenuLinkAC();
									newSubLink = _mapper.Map<MenuLinkAC>(mstRoleRights);
									newSubLink.Title = subLink.Title;
									newSubLink.RouteLink = subLink.RouteLink;
									newSubLink.Home = false;
									menuLinkAC.Children.Add(newSubLink);
								}
							}
							menuLinkACList.Add(menuLinkAC);
						}
					}
				}
			}
			return menuLinkACList;
		}


		public async Task<List<RoleAC>> GetRoleList() {
            try
            {
                var roleList = await _dbTeleBilling_V01Context.MstRole.Where(x => !x.IsDelete).OrderByDescending(x => x.RoleId).ToListAsync();
                return _mapper.Map<List<RoleAC>>(roleList);
            }
            catch (Exception e)
            {
                throw e;
            }
			
		}

		public async Task<RoleAC> GetRoleById(long roleId) {
			MstRole role = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => !x.IsDelete && x.RoleId == roleId);
			return _mapper.Map<RoleAC>(role);
		}

		public async Task<ResponseAC> AddRole(RoleAC roleAC, long userId, string loginUserName) {
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstRole.AnyAsync(x=>x.RoleName.ToLower() == roleAC.RoleName.Trim().ToLower() && !x.IsDelete)) {
				MstRole mstRole = new MstRole();
				mstRole.IsActive = true;
				mstRole.RoleName = roleAC.RoleName.Trim();
				mstRole.CreatedBy = userId;
				mstRole.CreatedDate = DateTime.Now;
				mstRole.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
                try
                {
                    await _dbTeleBilling_V01Context.AddAsync(mstRole);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddRole, loginUserName, userId, "Role(" + roleAC.RoleName.Trim() + ")", (int)EnumList.ActionTemplateTypes.Add, mstRole.RoleId);
				}
				catch (Exception e) 
                {

                    throw e;
                }
				responeAC.Message = _iStringConstant.RoleAddedSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				responeAC.Message = _iStringConstant.RoleExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return responeAC;
		}

		public async Task<ResponseAC> EditRole(RoleAC roleAC, long userId, string loginUserName) {
			 ResponseAC responeAC = new ResponseAC();
            try
            {
                if (!await _dbTeleBilling_V01Context.MstRole.AnyAsync(x => x.RoleId != roleAC.RoleId && x.RoleName.ToLower() == roleAC.RoleName.Trim().ToLower() && !x.IsDelete))
                {
                    MstRole mstRole = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => x.RoleId == roleAC.RoleId && !x.IsDelete);

                    #region Transaction Log Entry

                    if (mstRole.TransactionId == null)
                        mstRole.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                    var jsonSerailzeObj = JsonConvert.SerializeObject(mstRole);
                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstRole.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
                    #endregion

                    mstRole.RoleName = roleAC.RoleName.Trim();
                    mstRole.UpdatedBy = userId;
                    mstRole.UpdatedDate = DateTime.Now;
                    _dbTeleBilling_V01Context.Update(mstRole);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                    responeAC.Message = _iStringConstant.RoleUpdatedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditRole, loginUserName, userId, "Role(" + roleAC.RoleName.Trim() + ")", (int)EnumList.ActionTemplateTypes.Edit, mstRole.RoleId);

				}
				else
                {
                    responeAC.Message = _iStringConstant.RoleExists;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                }
            }
            catch(Exception e)
            {
                throw e;

            }
			
				return responeAC;

		}
		
		public async Task<bool> DeleteRole(long roleId,long userId, string loginUserName) {
			List<MstEmployee> mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.Where(x=>x.RoleId == roleId && x.IsActive && !x.IsDelete).ToListAsync();
			if (!mstEmployee.Any()) 
			{ 
				MstRole mstRole = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => x.RoleId == roleId);
				mstRole.IsDelete = true;
				mstRole.UpdatedBy = userId;
				mstRole.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstRole);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteRole, loginUserName, userId, "Role(" + mstRole.RoleName.Trim() + ")", (int)EnumList.ActionTemplateTypes.Delete, mstRole.RoleId);
				return true;
			}
			return false;
		}

		public async Task<bool> ChangeRoleStatus(long roleId,long userId, string loginUserName) {
			MstRole mstRole = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => x.RoleId == roleId);
			if (mstRole != null)
			{
				#region Transaction Log Entry
				if (mstRole.TransactionId == null)
					mstRole.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(mstRole);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstRole.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
				#endregion

				mstRole.IsActive = !mstRole.IsActive;
				mstRole.UpdatedBy = userId;
				mstRole.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstRole);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				if(mstRole.IsActive)
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ActiveRole, loginUserName, userId, "Role(" + mstRole.RoleName.Trim() + ")", (int)EnumList.ActionTemplateTypes.Active, mstRole.RoleId);
				else
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeactiveRole, loginUserName, userId, "Role(" + mstRole.RoleName.Trim() + ")", (int)EnumList.ActionTemplateTypes.Deactive, mstRole.RoleId);
				
				return true;
			}
			return false;
		}
		
		public async Task<List<RoleRightsAC>> GetRoleRights(long roleId)
		{
			List<RoleRightsAC> roleRightsAC = new List<RoleRightsAC>();
			List<MstLink> mstLinkList = await _dbTeleBilling_V01Context.MstLink.Where(x=>x.IsActive && x.ModuleId != 1).Include(x=>x.Module).OrderBy(x=>x.ViewIndex).ToListAsync(); 
			foreach(var link in mstLinkList)
			{
				RoleRightsAC roleRights = new RoleRightsAC();
				MstRolerights mstRoleRights = await _dbTeleBilling_V01Context.MstRolerights.FirstOrDefaultAsync(x => x.RoleId == roleId && x.LinkId == link.LinkId);
				if (mstRoleRights != null) {
					roleRights = _mapper.Map<RoleRightsAC>(mstRoleRights);
				}
				roleRights.LinkId = link.LinkId;
				roleRights.RoleId = roleId;
				roleRights.ModuleName = link.Module.ModuleName;
				roleRights.Title = link.Title;
				roleRightsAC.Add(roleRights);
			}
			return roleRightsAC;
		}
		
		public async Task<ResponseAC> UpdateRoleRights(long userId ,List<RoleRightsAC> roleRightsACList, string loginUserName)
		{
			ResponseAC responseAc = new ResponseAC();
			long roleId = roleRightsACList[0].RoleId;
			List<MstRolerights> roleRights = await _dbTeleBilling_V01Context.MstRolerights.Where(x=>x.RoleId == roleId).Include(x=>x.Role).ToListAsync();

			
			if (roleRights.Any()) {
				
				//#region Transaction Log Entry
				//var jsonSerailzeObj = JsonConvert.SerializeObject(roleRights);
				//await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(roleRights[0].TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				//#endregion

				_dbTeleBilling_V01Context.MstRolerights.RemoveRange(roleRights);
				_dbTeleBilling_V01Context.SaveChanges();
			}

			roleRights = new List<MstRolerights>();
			foreach(var item in roleRightsACList)
			{
				MstRolerights newRoleRights = new MstRolerights();
				newRoleRights = _mapper.Map<MstRolerights>(item);
				newRoleRights.CreatedBy = userId;
				newRoleRights.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				newRoleRights.CreatedDate = DateTime.Now;
				roleRights.Add(newRoleRights);
			}
			await _dbTeleBilling_V01Context.MstRolerights.AddRangeAsync(roleRights);
			_dbTeleBilling_V01Context.SaveChanges();
			responseAc.Message = _iStringConstant.RoleRightsUpdatedSuccessfully;
			responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ChangeRoleRights, loginUserName, userId,"Rolerights("+ roleRights[0].Role.RoleName +")", (int)EnumList.ActionTemplateTypes.Edit,null);
			return responseAc;
		}


		public async Task<List<ServiceTypeAC>> GetServiceTypes() {
			List<FixServicetype> serviceTypeACs = await _dbTeleBilling_V01Context.FixServicetype.OrderByDescending(x=>x.Id).ToListAsync();
			return _mapper.Map<List<ServiceTypeAC>>(serviceTypeACs);
		}

		#endregion

	}
}
