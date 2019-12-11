using AutoMapper;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Infrastructure;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Telephone
{
	public class TelephoneRepository : ITelephoneRepository
	{

		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		private readonly DAL _objDal = new DAL();
		private readonly DALMySql _objDalmysql = new DALMySql();
		#endregion

		#region "Constructor"
		public TelephoneRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
			ILogManagement iLogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion

		#region Public Method(s)

		#region Telephone Management
		public async Task<JqueryDataTablesPagedResults<TelephoneAC>> GetTelephoneList(JqueryDataTablesParameters param)
		{
			int skip = (param.Start / param.Length) * param.Length;
			int take = param.Length;
			int? sortColumnNumber = null;
			string sortType = string.Empty;
			if (param.Order.Length > 0)
			{
				sortColumnNumber = param.Order[0].Column;
				sortType = param.Order[0].Dir.ToString();
			}

			IQueryable<Telephonenumber> query = _dbTeleBilling_V01Context.Telephonenumber.Where(x => !x.IsDelete).Include(x => x.Provider).Include(x => x.LineType).OrderByDescending(x => x.Id).AsNoTracking();
			query = query.Where(x => x.Provider.Name.Contains(param.Search.Value) || x.TelephoneNumber1.Contains(param.Search.Value) || x.AccountNumber.Contains(param.Search.Value));

			var size = await query.CountAsync();

			var telphoneList = await query.Skip((param.Start / param.Length) * param.Length).Take(param.Length).ToArrayAsync();

			List<TelephoneAC> items = _mapper.Map<List<TelephoneAC>>(telphoneList);


			if (items.Any())
			{
				foreach (var item in items)
				{
					if (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumberId == item.Id && !x.IsDelete) != null)
						item.IsAssigned = true;
					else
						item.IsAssigned = false;
				}
			}

			return new JqueryDataTablesPagedResults<TelephoneAC>
			{
				Items = items,
				TotalSize = size
			};
		}

		public List<ExportTelePhoneAC> GetTelephoneExportList()
		{
			List<ExportTelePhoneAC> exportTelePhoneACs = new List<ExportTelePhoneAC>();
			DataSet ds = _objDalmysql.GetDataSet("usp_GetTelephoneListForExport");
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					exportTelePhoneACs = _objDal.ConvertDataTableToGenericList<ExportTelePhoneAC>(ds.Tables[0]).ToList();
				}
			}
			return exportTelePhoneACs;
		}

		public async Task<ResponseAC> AddTelephone(long userId, TelephoneDetailAC telephoneDetailAC, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Telephonenumber.AnyAsync(x => x.TelephoneNumber1.Trim() == telephoneDetailAC.TelephoneNumber1.Trim() && !x.IsDelete))
			{
				Telephonenumber telephoneNumber = _mapper.Map<Telephonenumber>(telephoneDetailAC);
				telephoneNumber.CreatedBy = userId;
				telephoneNumber.CreatedDate = DateTime.Now;
				telephoneNumber.IsActive = true;
				telephoneNumber.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				await _dbTeleBilling_V01Context.AddAsync(telephoneNumber);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddTelephone, loginUserName, userId, "Telephone(" + telephoneNumber.TelephoneNumber1 + ")", (int)EnumList.ActionTemplateTypes.Add, telephoneNumber.Id);
				responseAC.StatusCode = Convert.ToInt32(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.TelphoneAddedSuccessfully;
			}
			else
			{
				responseAC.StatusCode = Convert.ToInt32(EnumList.ResponseType.Error);
				responseAC.Message = _iStringConstant.TelphoneAlreadyExists;
			}
			return responseAC;
		}

		public async Task<ResponseAC> UpdateTelephone(long userId, TelephoneDetailAC telephoneDetailAC, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			if (await _dbTeleBilling_V01Context.Telephonenumber.FirstOrDefaultAsync(x => x.TelephoneNumber1.Trim() == telephoneDetailAC.TelephoneNumber1.Trim() && !x.IsDelete && x.Id != telephoneDetailAC.Id) == null)
			{
				Telephonenumber telephoneNumber = await _dbTeleBilling_V01Context.Telephonenumber.FirstOrDefaultAsync(x => x.Id == telephoneDetailAC.Id);

				#region Transaction Log Entry
				if (telephoneNumber.TransactionId == null)
					telephoneNumber.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(telephoneNumber);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(telephoneNumber.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				telephoneNumber = _mapper.Map(telephoneDetailAC, telephoneNumber);
				telephoneNumber.UpdatedBy = userId;
				telephoneNumber.UpdatedDate = DateTime.Now;


				_dbTeleBilling_V01Context.Update(telephoneNumber);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditTelephone, loginUserName, userId, "Telephone(" + telephoneNumber.TelephoneNumber1 + ")", (int)EnumList.ActionTemplateTypes.Edit, telephoneNumber.Id);
				responseAC.StatusCode = Convert.ToInt32(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.TelphoneAddedSuccessfully;
			}
			else
			{
				responseAC.StatusCode = Convert.ToInt32(EnumList.ResponseType.Error);
				responseAC.Message = _iStringConstant.TelphoneAlreadyExists;
			}
			return responseAC;
		}

		public async Task<bool> DeleteTelphone(long userId, long id, string loginUserName)
		{
			Telephonenumber telephoneNumber = await _dbTeleBilling_V01Context.Telephonenumber.FirstOrDefaultAsync(x => x.Id == id);
			SortedList sl = new SortedList();
			sl.Add("p_telephoneid", id);
			int result = Convert.ToInt16(_objDalmysql.ExecuteScaler("usp_GetTelephoneExists", sl));
			if (result == 0)
			{
				telephoneNumber.IsDelete = true;
				telephoneNumber.UpdatedBy = userId;
				telephoneNumber.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(telephoneNumber);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteTelephone, loginUserName, userId, "Telephone(" + telephoneNumber.TelephoneNumber1 + ")", (int)EnumList.ActionTemplateTypes.Delete, telephoneNumber.Id);
				return true;
			}
			return false;
		}

		public async Task<bool> ChangeTelephoneStatus(long userId, long id, string loginUserName)
		{
			Telephonenumber telephoneNumber = await _dbTeleBilling_V01Context.Telephonenumber.FirstOrDefaultAsync(x => x.Id == id);

			#region telephone already assigned it will not deactivated.
			if (telephoneNumber.IsActive)
			{
				Telephonenumberallocation telephonenumberallocation = await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.TelephoneNumberId == id);
				if (telephonenumberallocation != null)
					return false;
			}
			#endregion

			#region Transaction Log Entry
			if (telephoneNumber.TransactionId == null)
				telephoneNumber.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

			var jsonSerailzeObj = JsonConvert.SerializeObject(telephoneNumber);
			await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(telephoneNumber.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
			#endregion

			telephoneNumber.IsActive = !telephoneNumber.IsActive;
			telephoneNumber.UpdatedBy = userId;
			telephoneNumber.UpdatedDate = DateTime.Now;
			_dbTeleBilling_V01Context.Update(telephoneNumber);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			if (telephoneNumber.IsActive)
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ActiveTelephone, loginUserName, userId, "Telephone(" + telephoneNumber.TelephoneNumber1 + ")", (int)EnumList.ActionTemplateTypes.Active, telephoneNumber.Id);
			else
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeactiveTelephone, loginUserName, userId, "Telephone(" + telephoneNumber.TelephoneNumber1 + ")", (int)EnumList.ActionTemplateTypes.Deactive, telephoneNumber.Id);
			return true;
		}

		public async Task<TelephoneDetailAC> GetTelephoneById(long id)
		{
			Telephonenumber telephoneNumber = await _dbTeleBilling_V01Context.Telephonenumber.FirstAsync(x => x.Id == id);
			return _mapper.Map<TelephoneDetailAC>(telephoneNumber);
		}

		#endregion

		#region Assign Telephone Management

		public JqueryDataTablesPagedResults<AssignTelePhoneAC> GetAssignedTelephoneList(JqueryDataTablesParameters param)
		{
			List<AssignTelePhoneAC> assignTelePhonelist = new List<AssignTelePhoneAC>();

			long skipRecord = param.Start;
			int length = param.Length;
			int? sortColumnNumber = null;
			string sortType = string.Empty;
			string searchValue = param.Search.Value;
			int totalSize = 0;

			if (param.Order.Length > 0)
			{
				sortColumnNumber = param.Order[0].Column;
				sortType = param.Order[0].Dir.ToString();
			}

			SortedList sl = new SortedList();
			sl.Add("SkipRecord", skipRecord);
			sl.Add("Length", length);
			sl.Add("SearchValue", searchValue);
			DataSet ds = _objDalmysql.GetDataSet("usp_GetTelphoneAssignListWithPagging", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					assignTelePhonelist = _objDal.ConvertDataTableToGenericList<AssignTelePhoneAC>(ds.Tables[0]).ToList();
				}

				if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
				{
					totalSize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
				}
			}

			return new JqueryDataTablesPagedResults<AssignTelePhoneAC>
			{
				Items = assignTelePhonelist,
				TotalSize = totalSize
			};
		}

		public List<ExportAssignedTelePhoneAC> GetAssignedTelephoneExportList()
		{
			List<ExportAssignedTelePhoneAC> exportAssignedTelePhoneAC = new List<ExportAssignedTelePhoneAC>();
			DataSet ds = _objDalmysql.GetDataSet("usp_GetAssignedTelephoneListForExport");
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					exportAssignedTelePhoneAC = _objDal.ConvertDataTableToGenericList<ExportAssignedTelePhoneAC>(ds.Tables[0]).ToList();
				}
			}
			return exportAssignedTelePhoneAC;
		}

		public async Task<List<AssignTelePhonePackageDetailAC>> GetAssignedTelephonePackageList(long id)
		{
			List<AssignTelePhonePackageDetailAC> assignTelePhonePackagelist = new List<AssignTelePhonePackageDetailAC>();

			try
			{
				SortedList sl = new SortedList();
				sl.Add("allocationId", id);
				DataSet ds = _objDalmysql.GetDataSet("uspGetAssignPackageById", sl);

				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						assignTelePhonePackagelist = _objDal.ConvertDataTableToGenericList<AssignTelePhonePackageDetailAC>(ds.Tables[0]).ToList();
					}
				}

				if (assignTelePhonePackagelist != null)
				{
					if (assignTelePhonePackagelist.Count() > 0)
					{
						return assignTelePhonePackagelist;
					}
				}
				return new List<AssignTelePhonePackageDetailAC>();
			}
			catch (Exception)
			{
				return new List<AssignTelePhonePackageDetailAC>();
			}


		}

		public async Task<ResponseAC> AddAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			try
			{
				Telephonenumberallocation telephoneNumberAllocation = _mapper.Map<Telephonenumberallocation>(assignTelephoneDetailAC);
				telephoneNumberAllocation.CreatedBy = userId;
				telephoneNumberAllocation.CreatedDate = DateTime.Now;
				telephoneNumberAllocation.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				telephoneNumberAllocation.IsActive = true;
				await _dbTeleBilling_V01Context.AddAsync(telephoneNumberAllocation);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				List<Telephonenumberallocationpackage> telePhoneNumberAllocationPackages = new List<Telephonenumberallocationpackage>();
				foreach (var item in assignTelephoneDetailAC.TelePhonePackageDetails)
				{
					Telephonenumberallocationpackage telePhoneNumberAllocationPackage = new Telephonenumberallocationpackage();
					telePhoneNumberAllocationPackage.PackageId = item.PackageId;
					telePhoneNumberAllocationPackage.ServiceId = item.ServiceId;
					telePhoneNumberAllocationPackage.StartDate = item.StartDate.AddDays(1);
					telePhoneNumberAllocationPackage.EndDate = item.EndDate.AddDays(1);
					telePhoneNumberAllocationPackage.TelephoneNumberAllocationId = telephoneNumberAllocation.Id;
					telePhoneNumberAllocationPackages.Add(telePhoneNumberAllocationPackage);
				}

				await _dbTeleBilling_V01Context.AddRangeAsync(telePhoneNumberAllocationPackages);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AssignTelephone, loginUserName, userId, "Assign Telephone(" + telephoneNumberAllocation.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Assign, telephoneNumberAllocation.Id);

				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.TelephoneAssignedSuccessfully;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return responseAC;
		}

		public async Task<ResponseAC> UpdateAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			Telephonenumberallocation telephoneNumberAllocation = await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstAsync(x => x.Id == assignTelephoneDetailAC.Id);

			#region Transaction Log Entry
			if (telephoneNumberAllocation.TransactionId == null)
				telephoneNumberAllocation.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

			var jsonSerailzeObj = JsonConvert.SerializeObject(telephoneNumberAllocation);
			await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(telephoneNumberAllocation.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
			#endregion

			telephoneNumberAllocation = _mapper.Map(assignTelephoneDetailAC, telephoneNumberAllocation);
			telephoneNumberAllocation.UpdatedBy = userId;
			telephoneNumberAllocation.UpdatedDate = DateTime.Now;

			_dbTeleBilling_V01Context.Update(telephoneNumberAllocation);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			List<Telephonenumberallocationpackage> telePhoneNumberAllocationPackages = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage.Where(x => x.TelephoneNumberAllocationId == telephoneNumberAllocation.Id && !x.IsDelete).ToListAsync();
			foreach (var item in telePhoneNumberAllocationPackages)
			{
				item.IsDelete = true;
				item.UpdatedBy = userId;
				item.UpdatedDate = DateTime.Now;
			}

			_dbTeleBilling_V01Context.UpdateRange(telePhoneNumberAllocationPackages);
			_dbTeleBilling_V01Context.SaveChanges();

			#region Update Packages
			telePhoneNumberAllocationPackages = new List<Telephonenumberallocationpackage>();
			foreach (var item in assignTelephoneDetailAC.TelePhonePackageDetails)
			{
				Telephonenumberallocationpackage telePhoneNumberAllocationPackage = new Telephonenumberallocationpackage();
				telePhoneNumberAllocationPackage.PackageId = item.PackageId;
				telePhoneNumberAllocationPackage.ServiceId = item.ServiceId;
				telePhoneNumberAllocationPackage.StartDate = item.StartDate.AddDays(1);
				telePhoneNumberAllocationPackage.EndDate = item.EndDate.AddDays(1);
				telePhoneNumberAllocationPackage.TelephoneNumberAllocationId = telephoneNumberAllocation.Id;
				telePhoneNumberAllocationPackages.Add(telePhoneNumberAllocationPackage);
			}
			await _dbTeleBilling_V01Context.AddRangeAsync(telePhoneNumberAllocationPackages);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditAssignTelephone, loginUserName, userId, "Assign Telephone(" + telephoneNumberAllocation.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Edit, telephoneNumberAllocation.Id);

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.AssignedTelePhoneEditSuccessfully;
			return responseAC;
		}

		public async Task<AssignTelephoneDetailAC> GetAssignedTelephoneById(long id)
		{
			AssignTelephoneDetailAC assignTelephoneDetailAC = new AssignTelephoneDetailAC();

			Telephonenumberallocation telephoneNumberAllocation = await _dbTeleBilling_V01Context.Telephonenumberallocation.Include(x => x.Employee).Include(x => x.TelephoneNumberNavigation).Include(x => x.TelephoneNumberNavigation.Provider).Include(x => x.Employee.Department).FirstAsync(x => x.Id == id);
			assignTelephoneDetailAC = _mapper.Map(telephoneNumberAllocation, assignTelephoneDetailAC);
			assignTelephoneDetailAC.EmployeeData = new EmployeeAC();
			assignTelephoneDetailAC.TelephoneNumberData = new TelephoneNumberAC();
			assignTelephoneDetailAC.EmployeeData.Department = telephoneNumberAllocation.Employee.Department.Name;
			assignTelephoneDetailAC.EmployeeData.EmpPfnumber = telephoneNumberAllocation.EmpPfnumber;
			assignTelephoneDetailAC.EmployeeData.UserId = telephoneNumberAllocation.EmployeeId;
			assignTelephoneDetailAC.EmployeeData.FullName = telephoneNumberAllocation.Employee.FullName;
			assignTelephoneDetailAC.AssignTypeId = telephoneNumberAllocation.AssignTypeId;
			assignTelephoneDetailAC.TelephoneNumberData.Id = telephoneNumberAllocation.TelephoneNumberId;
			assignTelephoneDetailAC.TelephoneNumberData.TelephoneNumber1 = telephoneNumberAllocation.TelephoneNumber;
			assignTelephoneDetailAC.TelephoneNumberData.ProviderId = telephoneNumberAllocation.TelephoneNumberNavigation.ProviderId;
			assignTelephoneDetailAC.TelephoneNumberData.ProviderName = telephoneNumberAllocation.TelephoneNumberNavigation.Provider.Name;


			List<Telephonenumberallocationpackage> telePhoneNumberAllocationPackages = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage.Where(x => x.TelephoneNumberAllocationId == telephoneNumberAllocation.Id && !x.IsDelete).ToListAsync();
			assignTelephoneDetailAC.TelePhonePackageDetails = _mapper.Map<List<TelePhonePackageDetails>>(telePhoneNumberAllocationPackages);

			return assignTelephoneDetailAC;
		}

		public async Task<BulkAssignTelephoneResponseAC> UploadBulkAssignTelePhone(long userId, ExcelUploadResponseAC exceluploadDetail, string loginUserName)
		{
			BulkAssignTelephoneResponseAC bulkAssignTelephoneResponseAC = new BulkAssignTelephoneResponseAC();
			List<ExcelUploadResult> excelUploadResultList = new List<ExcelUploadResult>();
			FileInfo fileinfo = new FileInfo(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));

			try
			{
				using (ExcelPackage package = new ExcelPackage(fileinfo))
				{
					for (int i = 1; i <= package.Workbook.Worksheets.Count(); i++)
					{
						ExcelWorksheet workSheet = package.Workbook.Worksheets[i];
						string sheetName = package.Workbook.Worksheets[i].Name;
						int totalRows = workSheet.Dimension.Rows;
						int totalColums = workSheet.Dimension.Columns;
						for (int j = 1; j <= totalRows - 1; j++)
						{

							string mobileNumber = workSheet.Cells[j + 1, 1].Value != null ? workSheet.Cells[j + 1, 1].Value.ToString() : string.Empty;
							string assignType = workSheet.Cells[j + 1, 2].Value != null ? workSheet.Cells[j + 1, 2].Value.ToString() : string.Empty;
							string pfNumber = workSheet.Cells[j + 1, 3].Value != null ? workSheet.Cells[j + 1, 3].Value.ToString() : string.Empty;
							string packageName = workSheet.Cells[j + 1, 4].Value != null ? workSheet.Cells[j + 1, 4].Value.ToString() : string.Empty;
							string startDate = workSheet.Cells[j + 1, 5].Value != null ? workSheet.Cells[j + 1, 5].Value.ToString() : string.Empty;
							string endDate = workSheet.Cells[j + 1, 6].Value != null ? workSheet.Cells[j + 1, 6].Value.ToString() : string.Empty;

							if (!string.IsNullOrEmpty(mobileNumber) && !string.IsNullOrEmpty(assignType) && !string.IsNullOrEmpty(pfNumber)
								&& !string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(endDate))
							{
								Telephonenumberallocation telephoneNumberAllocation = new Telephonenumberallocation();
								Telephonenumberallocationpackage telePhoneNumberAllocationPackage = new Telephonenumberallocationpackage();
								bulkAssignTelephoneResponseAC.TotalRecords += 1;

								Telephonenumber telephoneNumber = await _dbTeleBilling_V01Context.Telephonenumber.FirstOrDefaultAsync(x => x.TelephoneNumber1.ToLower().Trim() == mobileNumber.ToLower().Trim() && x.IsActive && !x.IsDelete);
								if (telephoneNumber != null)
								{

									FixAssigntype fixAssigntype = await _dbTeleBilling_V01Context.FixAssigntype.FirstOrDefaultAsync(x => x.IsActive && x.Name == assignType);
									if (fixAssigntype != null)
									{

										MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete && x.EmpPfnumber.ToLower().Trim() == pfNumber.ToLower().Trim());
										if (mstEmployee != null)
										{

											Providerpackage providerPackage = await _dbTeleBilling_V01Context.Providerpackage.FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete && x.Name.ToLower().Trim() == packageName.ToLower().Trim());
											if (providerPackage != null)
											{
												if (!string.IsNullOrEmpty(startDate))
												{
													if (!string.IsNullOrEmpty(endDate))
													{
														DateTime newStartDate;
														DateTime newEndDate;
														if (DateTime.TryParse(startDate, out newStartDate))
														{
															String.Format("{0:d/MM/yyyy}", newStartDate);
															if (DateTime.TryParse(endDate, out newEndDate))
															{
																String.Format("{0:d/MM/yyyy}", newEndDate);
																string lineStatus = workSheet.Cells[j + 1, 7].Value != null ? workSheet.Cells[j + 1, 7].Value.ToString() : "";
																FixLinestatus fixLineStatus = await _dbTeleBilling_V01Context.FixLinestatus.FirstOrDefaultAsync(x => x.IsActive && x.Name.ToLower().Trim() == lineStatus.ToLower().Trim());
																if (fixLineStatus != null)
																{
																	if (telephoneNumber.ProviderId == providerPackage.ProviderId)
																	{
																		Telephonenumberallocation newTelephoneNumberAllocation = _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefault(x => x.TelephoneNumberId == telephoneNumber.Id && !x.IsDelete);
																		if (newTelephoneNumberAllocation != null)
																		{
																			Telephonenumberallocationpackage newTelePhoneNumberAllocationPackage = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage.FirstOrDefaultAsync(x => x.TelephoneNumberAllocationId == newTelephoneNumberAllocation.Id && x.PackageId == providerPackage.Id && !x.IsDelete);
																			if (newTelePhoneNumberAllocationPackage == null)
																			{
																				newTelePhoneNumberAllocationPackage = new Telephonenumberallocationpackage();
																				newTelePhoneNumberAllocationPackage.ServiceId = providerPackage.ServiceTypeId;
																				newTelePhoneNumberAllocationPackage.StartDate = newStartDate;
																				newTelePhoneNumberAllocationPackage.EndDate = newEndDate;
																				newTelePhoneNumberAllocationPackage.PackageId = providerPackage.Id;
																				bulkAssignTelephoneResponseAC.SuccessRecords += 1;
																				newTelePhoneNumberAllocationPackage.TelephoneNumberAllocationId = newTelephoneNumberAllocation.Id;

																				await _dbTeleBilling_V01Context.AddAsync(newTelePhoneNumberAllocationPackage);
																				await _dbTeleBilling_V01Context.SaveChangesAsync();

																				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AssignBulkTelephone, loginUserName, userId, "Telephone Assign(" + newTelephoneNumberAllocation.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Upload, newTelephoneNumberAllocation.Id);

																			}
																			else
																			{
																				//TelphoneNumbeAlready Assigned
																				bulkAssignTelephoneResponseAC.SkipRecords += 1;
																				string message = _iStringConstant.TelphoneNumberPackageAlreadyAssigned.Replace("{{$telephonenumber$}}", mobileNumber);
																				excelUploadResultList = AddedFileDataResponse(1, j + 1, packageName, message, sheetName, excelUploadResultList);
																			}
																		}
																		else
																		{
																			telephoneNumberAllocation.EmployeeId = mstEmployee.UserId;
																			telephoneNumberAllocation.EmpPfnumber = mstEmployee.EmpPfnumber;
																			telephoneNumberAllocation.TelephoneNumberId = telephoneNumber.Id;
																			telephoneNumberAllocation.TelephoneNumber = telephoneNumber.TelephoneNumber1;
																			telephoneNumberAllocation.LineStatusId = fixLineStatus.Id;
																			telephoneNumberAllocation.IsActive = true;
																			telephoneNumberAllocation.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
																			telephoneNumberAllocation.CreatedBy = userId;
																			telephoneNumberAllocation.CreatedDate = DateTime.Now;
																			telephoneNumberAllocation.AssignTypeId = fixAssigntype.Id;

																			bulkAssignTelephoneResponseAC.SuccessRecords += 1;
																			await _dbTeleBilling_V01Context.AddAsync(telephoneNumberAllocation);
																			await _dbTeleBilling_V01Context.SaveChangesAsync();

																			telePhoneNumberAllocationPackage.ServiceId = providerPackage.ServiceTypeId;
																			telePhoneNumberAllocationPackage.StartDate = newStartDate;
																			telePhoneNumberAllocationPackage.EndDate = newEndDate;
																			telePhoneNumberAllocationPackage.PackageId = providerPackage.Id;
																			telePhoneNumberAllocationPackage.TelephoneNumberAllocationId = telephoneNumberAllocation.Id;
																			await _dbTeleBilling_V01Context.AddAsync(telePhoneNumberAllocationPackage);
																			await _dbTeleBilling_V01Context.SaveChangesAsync();

																			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AssignBulkTelephone, loginUserName, userId, "Telephone Assign(" + newTelephoneNumberAllocation.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Upload, newTelephoneNumberAllocation.Id);
																		}
																	}
																	else
																	{
																		//Packge not same 
																		bulkAssignTelephoneResponseAC.SkipRecords += 1;
																		excelUploadResultList = AddedFileDataResponse(4, j + 1, "Given package not valid for mobilenumber(" + mobileNumber + ")", _iStringConstant.PackageNotMatchWithTelephoneNumber, sheetName, excelUploadResultList);
																	}
																}
																else
																{
																	//LineStatus Not Exists
																	bulkAssignTelephoneResponseAC.SkipRecords += 1;
																	excelUploadResultList = AddedFileDataResponse(7, j + 1, lineStatus, _iStringConstant.LineStatusNotExists, sheetName, excelUploadResultList);
																}
															}
															else
															{
																//End Date Not Valid
																bulkAssignTelephoneResponseAC.SkipRecords += 1;
																excelUploadResultList = AddedFileDataResponse(6, j + 1, endDate, _iStringConstant.EndDateNotValid, sheetName, excelUploadResultList);

															}
														}
														else
														{
															//Start Date Not Valid
															bulkAssignTelephoneResponseAC.SkipRecords += 1;
															excelUploadResultList = AddedFileDataResponse(5, j + 1, startDate, _iStringConstant.StartDateNotValid, sheetName, excelUploadResultList);

														}
													}
													else
													{
														//End Date Is Empty
														bulkAssignTelephoneResponseAC.SkipRecords += 1;
														excelUploadResultList = AddedFileDataResponse(6, j + 1, endDate, _iStringConstant.EndDateIsEmpty, sheetName, excelUploadResultList);
													}
												}
												else
												{
													//Start Date Is Empty
													bulkAssignTelephoneResponseAC.SkipRecords += 1;
													excelUploadResultList = AddedFileDataResponse(5, j, startDate, _iStringConstant.StartDateIsEmpty, sheetName, excelUploadResultList);
												}
											}
											else
											{
												//Package Not Exists
												bulkAssignTelephoneResponseAC.SkipRecords += 1;
												excelUploadResultList = AddedFileDataResponse(4, j + 1, packageName, _iStringConstant.PackageNotExists, sheetName, excelUploadResultList);
											}
										}
										else
										{
											//Employee Not Exists
											bulkAssignTelephoneResponseAC.SkipRecords += 1;
											excelUploadResultList = AddedFileDataResponse(3, j, pfNumber, _iStringConstant.EmployeeNotExists, sheetName, excelUploadResultList);
										}
										//else
										//{
										//TelphoneNumbeAlready Assigned
										//bulkAssignTelephoneResponseAC.SkipRecords += 1;
										//excelUploadResultList = AddedFileDataResponse(1, j + 1, mobileNumber, _iStringConstant.TelphoneNumberAlreadyAssigned, sheetName, excelUploadResultList);
										//}
									}
									else
									{
										//Employee Not Exists
										bulkAssignTelephoneResponseAC.SkipRecords += 1;
										excelUploadResultList = AddedFileDataResponse(2, j, assignType, _iStringConstant.AssignTypeNotExists, sheetName, excelUploadResultList);

									}
								}
								else
								{
									//PhoneNumber Not Exists
									bulkAssignTelephoneResponseAC.SkipRecords += 1;
									excelUploadResultList = AddedFileDataResponse(1, j + 1, mobileNumber, _iStringConstant.PhoneNumberNotExists, sheetName, excelUploadResultList);
								}
							}
						}
					}

				}

				if (File.Exists(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid)))
					File.Delete(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));

				bulkAssignTelephoneResponseAC.excelUploadResultList = excelUploadResultList;
				return bulkAssignTelephoneResponseAC;
			}
			catch (Exception ex)
			{
				if (File.Exists(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid)))
					File.Delete(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));
				throw ex;
			}
		}

		public async Task<bool> DeleteAssignedTelephone(long userId, long id, string loginUserName)
		{
			Telephonenumberallocation telephonenumberallocation = await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstAsync(x => x.Id == id);
			int employeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.CloseBill);
			if (!await _dbTeleBilling_V01Context.Employeebillmaster.AnyAsync(x => !x.IsDelete && x.EmployeeBillStatus != employeeBillStatus && x.TelephoneNumber == telephonenumberallocation.TelephoneNumber))
			{
				telephonenumberallocation.IsDelete = true;
				telephonenumberallocation.UpdatedBy = userId;
				telephonenumberallocation.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(telephonenumberallocation);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				List<Telephonenumberallocationpackage> telephonenumberallocationpackages = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage.Where(x => x.TelephoneNumberAllocationId == id && !x.IsDelete).ToListAsync();
				if (telephonenumberallocationpackages.Any())
				{
					telephonenumberallocationpackages = telephonenumberallocationpackages.Select(c => { c.IsDelete = true; c.UpdatedBy = userId ; c.UpdatedDate = DateTime.Now ; return c; }).ToList();
					_dbTeleBilling_V01Context.UpdateRange(telephonenumberallocationpackages);
					await _dbTeleBilling_V01Context.SaveChangesAsync();
				}

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteAssignTelephone, loginUserName, userId, "Assign Telephone(" + telephonenumberallocation.TelephoneNumber + ")", (int)EnumList.ActionTemplateTypes.Delete, telephonenumberallocation.Id);

				return true;
			}
			return false;
		}

		#endregion


		#region Private Method(s)

		/// <summary>
		/// This method used for create response list for bulk assign telephone.
		/// </summary>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="recordDetail"></param>
		/// <param name="errorMessage"></param>
		/// <param name="sheetName"></param>
		/// <param name="excelUploadResultList"></param>
		/// <returns></returns>
		private List<ExcelUploadResult> AddedFileDataResponse(int column, int row, string recordDetail, string errorMessage, string sheetName, List<ExcelUploadResult> excelUploadResultList)
		{
			ExcelUploadResult excelUploadResult = new ExcelUploadResult();
			excelUploadResult.CellAddress = "Column: " + column + ",Row: " + row;
			excelUploadResult.ErrorMessage = errorMessage;
			excelUploadResult.RecordDetail = recordDetail;
			excelUploadResult.SheetName = sheetName;
			excelUploadResultList.Add(excelUploadResult);
			return excelUploadResultList;
		}


		#endregion

		#endregion
	}
}
