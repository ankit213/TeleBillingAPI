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
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
	    private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public RoleRepository(TeleBilling_V01Context dbTeleBilling_V01Context,IMapper mapper, IStringConstant iStringConstant
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
							   join roleRights in _dbTeleBilling_V01Context.MstRoleRight on linkModule.LinkId equals roleRights.LinkId 
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
								  IsViewOnly = roleRights.IsViewOnly,
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
							menuLinkAC.IsViewOnly = subItem.IsViewOnly;
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
							//menuLinkAC.IsAdd = subItem.IsAdd;
							//menuLinkAC.IsEdit = subItem.IsEdit;
							//menuLinkAC.IsDelete = subItem.IsDelete;
							//menuLinkAC.IsView = subItem.IsView;
							//menuLinkAC.IsChangeStatus = subItem.IsChangeStatus;
							menuLinkAC.Home = false;
							menuLinkAC.ModuleId = subItem.ModuleId;
							menuLinkAC.Children = new List<MenuLinkAC>();
							List<MstLink> linkList = await _dbTeleBilling_V01Context.MstLink.Where(x => x.ModuleId == item.Key && x.IsActive).OrderBy(x=>x.ViewIndex).ToListAsync();
							foreach (var subLink in linkList)
							{   var mstRoleRights = await _dbTeleBilling_V01Context.MstRoleRight.FirstOrDefaultAsync(x => x.IsView && x.LinkId == subLink.LinkId && x.RoleId == id);
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
			var roleList = await _dbTeleBilling_V01Context.MstRole.Where(x=>!x.IsDelete).OrderByDescending(x=>x.CreatedDate).ToListAsync();
			return _mapper.Map<List<RoleAC>>(roleList);
		}

		public async Task<RoleAC> GetRoleById(long roleId) {
			MstRole role = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => !x.IsDelete && x.RoleId == roleId);
			return _mapper.Map<RoleAC>(role);
		}

		public async Task<ResponseAC> AddRole(RoleAC roleAC, long userId) {
			ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstRole.AnyAsync(x=>x.RoleName.ToLower() == roleAC.RoleName.Trim().ToLower() && !x.IsDelete)) {
				MstRole mstRole = new MstRole();
				mstRole.IsActive = true;
				mstRole.RoleName = roleAC.RoleName;
				mstRole.CreatedBy = userId;
				mstRole.CreatedDate = DateTime.Now;
				mstRole.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				
				await _dbTeleBilling_V01Context.AddAsync(mstRole);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
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

		public async Task<ResponseAC> EditRole(RoleAC roleAC, long userId) {
			 ResponseAC responeAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.MstRole.AnyAsync(x => x.RoleId != roleAC.RoleId && x.RoleName.ToLower() == roleAC.RoleName.Trim().ToLower() && !x.IsDelete))
			{
				MstRole mstRole = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => x.RoleId == roleAC.RoleId && !x.IsDelete);

				#region Transaction Log Entry

				if (mstRole.TransactionId == null)
					mstRole.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				
				var jsonSerailzeObj = JsonConvert.SerializeObject(mstRole);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mstRole.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				mstRole.RoleName = roleAC.RoleName;
				mstRole.UpdatedBy = userId;
				mstRole.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstRole);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				responeAC.Message = _iStringConstant.RoleUpdatedSuccessfully;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			}
			else
			{
				responeAC.Message = _iStringConstant.RoleExists;
				responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
				return responeAC;

		}
		
		public async Task<bool> DeleteRole(long roleId,long userId) {
			MstRole mstRole = await _dbTeleBilling_V01Context.MstRole.FirstOrDefaultAsync(x => x.RoleId == roleId);
			if(mstRole != null) {
				mstRole.IsDelete = true;
				mstRole.UpdatedBy = userId;
				mstRole.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(mstRole);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			return false;
		}

		public async Task<bool> ChangeRoleStatus(long roleId,long userId) {
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
				MstRoleRight mstRoleRights = await _dbTeleBilling_V01Context.MstRoleRight.FirstOrDefaultAsync(x => x.RoleId == roleId && x.LinkId == link.LinkId);
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
		
		public async Task<ResponseAC> UpdateRoleRights(long userId ,List<RoleRightsAC> roleRightsACList)
		{
			ResponseAC responseAc = new ResponseAC();
			long roleId = roleRightsACList[0].RoleId;
			List<MstRoleRight> roleRights = await _dbTeleBilling_V01Context.MstRoleRight.Where(x=>x.RoleId == roleId).ToListAsync();

			
			if (roleRights.Any()) {
				
				#region Transaction Log Entry
				var jsonSerailzeObj = JsonConvert.SerializeObject(roleRights);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(roleRights[0].TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				_dbTeleBilling_V01Context.MstRoleRight.RemoveRange(roleRights);
				_dbTeleBilling_V01Context.SaveChanges();
			}

			roleRights = new List<MstRoleRight>();
			foreach(var item in roleRightsACList)
			{
				MstRoleRight newRoleRights = new MstRoleRight();
				newRoleRights = _mapper.Map<MstRoleRight>(item);
				newRoleRights.CreatedBy = userId;
				newRoleRights.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				newRoleRights.CreatedDate = DateTime.Now;
				roleRights.Add(newRoleRights);
			}
			await _dbTeleBilling_V01Context.MstRoleRight.AddRangeAsync(roleRights);
			_dbTeleBilling_V01Context.SaveChanges();
			responseAc.Message = _iStringConstant.RoleRightsUpdatedSuccessfully;
			responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			return responseAc;
		}

		#endregion

	}
}
