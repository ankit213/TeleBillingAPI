using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Telephone
{
	public class TelephoneRepository : ITelephoneRepository
	{

		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public TelephoneRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
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
		public async Task<List<TelephoneAC>> GetTelephoneList()
		{

			List<TelephoneNumber> lstTelephoneNumber = await _dbTeleBilling_V01Context.TelephoneNumber.Where(x => !x.IsDelete).Include(x => x.Provider).Include(x=>x.LineType).OrderByDescending(x => x.CreatedDate).ToListAsync();
			List<TelephoneAC> finalTelePhoneList = _mapper.Map<List<TelephoneAC>>(lstTelephoneNumber);

			foreach (var item in finalTelePhoneList)
			{
				if (await _dbTeleBilling_V01Context.TelephoneNumberAllocation.AnyAsync(x => x.TelephoneNumberId == item.Id && !x.IsDelete))
					item.IsAssigned = true;
				else
					item.IsAssigned = false;
			}

			return finalTelePhoneList;
		}

		public async Task<ResponseAC> AddTelephone(long userId, TelephoneDetailAC telephoneDetailAC)
		{
			ResponseAC responseAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.TelephoneNumber.AnyAsync(x => x.TelephoneNumber1.Trim() == telephoneDetailAC.TelephoneNumber1.Trim() && !x.IsDelete))
			{

				TelephoneNumber telephoneNumber = _mapper.Map<TelephoneNumber>(telephoneDetailAC);
				telephoneNumber.CreatedBy = userId;
				telephoneNumber.CreatedDate = DateTime.Now;
				telephoneNumber.IsActive = true;
				telephoneNumber.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				await _dbTeleBilling_V01Context.AddAsync(telephoneNumber);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

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

		public async Task<ResponseAC> UpdateTelephone(long userId, TelephoneDetailAC telephoneDetailAC)
		{
			ResponseAC responseAC = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.TelephoneNumber.AnyAsync(x => x.TelephoneNumber1.Trim() == telephoneDetailAC.TelephoneNumber1.Trim() && !x.IsDelete && x.Id != telephoneDetailAC.Id))
			{
				TelephoneNumber telephoneNumber = await _dbTeleBilling_V01Context.TelephoneNumber.FirstOrDefaultAsync(x => x.Id == telephoneDetailAC.Id);

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

		public async Task<bool> DeleteTelphone(long userId, long id)
		{
			TelephoneNumber telephoneNumber = await _dbTeleBilling_V01Context.TelephoneNumber.FirstOrDefaultAsync(x => x.Id == id);
			if (telephoneNumber != null)
			{
				telephoneNumber.IsDelete = true;
				telephoneNumber.UpdatedBy = userId;
				telephoneNumber.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(telephoneNumber);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			return false;
		}

		public async Task<bool> ChangeTelephoneStatus(long userId, long id)
		{
			TelephoneNumber telephoneNumber = await _dbTeleBilling_V01Context.TelephoneNumber.FirstOrDefaultAsync(x => x.Id == id);
			if (telephoneNumber != null)
			{
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
				return true;
			}
			return false;
		}

		public async Task<TelephoneDetailAC> GetTelephoneById(long id)
		{
			TelephoneNumber telephoneNumber = await _dbTeleBilling_V01Context.TelephoneNumber.FirstAsync(x => x.Id == id);
			return _mapper.Map<TelephoneDetailAC>(telephoneNumber);
		}

		#endregion

		#region Assign Telephone Management

		public async Task<List<AssignTelePhoneAC>> GetAssignedTelephoneList()
		{
			List<TelephoneNumberAllocation> telephoneNumberAllocations = await _dbTeleBilling_V01Context.TelephoneNumberAllocation.Where(x => !x.IsDelete).Include(x => x.TelephoneNumberNavigation).Include(x => x.AssignType).Include(x => x.Employee).Include(x => x.Employee.CostCenter).Include(x => x.Employee.Department).Include(x => x.LineStatus).ToListAsync();
			return _mapper.Map<List<AssignTelePhoneAC>>(telephoneNumberAllocations);
		}

		public async Task<ResponseAC> AddAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC)
		{
			ResponseAC responseAC = new ResponseAC();
			try
			{
				TelephoneNumberAllocation telephoneNumberAllocation = _mapper.Map<TelephoneNumberAllocation>(assignTelephoneDetailAC);
				telephoneNumberAllocation.CreatedBy = userId;
				telephoneNumberAllocation.CreatedDate = DateTime.Now;
				telephoneNumberAllocation.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				telephoneNumberAllocation.IsActive = true;
				await _dbTeleBilling_V01Context.AddAsync(telephoneNumberAllocation);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				List<TelePhoneNumberAllocationPackage> telePhoneNumberAllocationPackages = new List<TelePhoneNumberAllocationPackage>();
				foreach(var item in assignTelephoneDetailAC.TelePhonePackageDetails) {
					TelePhoneNumberAllocationPackage telePhoneNumberAllocationPackage = new TelePhoneNumberAllocationPackage();
					telePhoneNumberAllocationPackage.PackageId = item.PackageId;
					telePhoneNumberAllocationPackage.ServiceId = item.ServiceId;
					telePhoneNumberAllocationPackage.StartDate = item.StartDate.AddDays(1);
					telePhoneNumberAllocationPackage.EndDate = item.EndDate.AddDays(1);
					telePhoneNumberAllocationPackage.TelephoneNumberAllocationId = telephoneNumberAllocation.Id;
					telePhoneNumberAllocationPackages.Add(telePhoneNumberAllocationPackage);
				}

				await _dbTeleBilling_V01Context.AddRangeAsync(telePhoneNumberAllocationPackages);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.TelephoneAssignedSuccessfully;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return responseAC;
		}

		public async Task<ResponseAC> UpdateAssignedTelephone(long userId, AssignTelephoneDetailAC assignTelephoneDetailAC)
		{
			ResponseAC responseAC = new ResponseAC();
			TelephoneNumberAllocation telephoneNumberAllocation = await _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstAsync(x => x.Id == assignTelephoneDetailAC.Id);

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

			List<TelePhoneNumberAllocationPackage> telePhoneNumberAllocationPackages = await _dbTeleBilling_V01Context.TelePhoneNumberAllocationPackage.Where(x=>x.TelephoneNumberAllocationId == telephoneNumberAllocation.Id && !x.IsDelete).ToListAsync();
			foreach(var item in telePhoneNumberAllocationPackages) {
				item.IsDelete = true;
				item.UpdatedBy = userId;
				item.UpdatedDate =DateTime.Now;
			}

			_dbTeleBilling_V01Context.UpdateRange(telePhoneNumberAllocationPackages);
			_dbTeleBilling_V01Context.SaveChanges();

			#region Update Packages
		    telePhoneNumberAllocationPackages = new List<TelePhoneNumberAllocationPackage>();
			foreach (var item in assignTelephoneDetailAC.TelePhonePackageDetails)
			{
				TelePhoneNumberAllocationPackage telePhoneNumberAllocationPackage = new TelePhoneNumberAllocationPackage();
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

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.AssignedTelePhoneEditSuccessfully;
			return responseAC;
		}

		public async Task<AssignTelephoneDetailAC> GetAssignedTelephoneById(long id)
		{
			AssignTelephoneDetailAC assignTelephoneDetailAC = new AssignTelephoneDetailAC();

			TelephoneNumberAllocation telephoneNumberAllocation = await _dbTeleBilling_V01Context.TelephoneNumberAllocation.Include(x => x.Employee).Include(x => x.Employee.Department).FirstAsync(x => x.Id == id);
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


			List<TelePhoneNumberAllocationPackage> telePhoneNumberAllocationPackages = await _dbTeleBilling_V01Context.TelePhoneNumberAllocationPackage.Where(x => x.TelephoneNumberAllocationId == telephoneNumberAllocation.Id && !x.IsDelete).ToListAsync();
			assignTelephoneDetailAC.TelePhonePackageDetails = _mapper.Map<List<TelePhonePackageDetails>>(telePhoneNumberAllocationPackages);
			
			return assignTelephoneDetailAC;
		}

		public async Task<BulkAssignTelephoneResponseAC> UploadBulkAssignTelePhone(long userId, ExcelUploadResponseAC exceluploadDetail)
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
							TelephoneNumberAllocation telephoneNumberAllocation = new TelephoneNumberAllocation();
							TelePhoneNumberAllocationPackage telePhoneNumberAllocationPackage = new TelePhoneNumberAllocationPackage();
							bulkAssignTelephoneResponseAC.TotalRecords += 1;
							
							string mobileNumber =  workSheet.Cells[j + 1, 1].Value != null ? workSheet.Cells[j + 1, 1].Value.ToString() : "";
							TelephoneNumber telephoneNumber = await _dbTeleBilling_V01Context.TelephoneNumber.FirstOrDefaultAsync(x => x.TelephoneNumber1.ToLower().Trim() == mobileNumber.ToLower().Trim() && x.IsActive && !x.IsDelete);
							if (telephoneNumber != null)
							{
								 //if (!_dbTeleBilling_V01Context.TelephoneNumberAllocation.Any(x => x.TelephoneNumberId == telephoneNumber.Id && x.IsActive && !x.IsDelete))
								 //{
									string pfNumber = workSheet.Cells[j + 1, 3].Value != null ? workSheet.Cells[j + 1, 3].Value.ToString() : "";
									MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete && x.EmpPfnumber.ToLower().Trim() == pfNumber.ToLower().Trim());
									if (mstEmployee != null)
									{
										string packageName = workSheet.Cells[j + 1, 4].Value != null ? workSheet.Cells[j + 1, 4].Value.ToString() : "";
										ProviderPackage providerPackage = await _dbTeleBilling_V01Context.ProviderPackage.FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete && x.Name.ToLower().Trim() == packageName.ToLower().Trim());
										if (providerPackage != null)
										{
										  string startDate = workSheet.Cells[j + 1, 5].Value != null ? workSheet.Cells[j + 1, 5].Value.ToString() : "";
										  if (!string.IsNullOrEmpty(startDate))
											{
											  string endDate = workSheet.Cells[j + 1, 6].Value != null ? workSheet.Cells[j + 1, 6].Value.ToString() : "";
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
															FixLineStatus fixLineStatus = await _dbTeleBilling_V01Context.FixLineStatus.FirstOrDefaultAsync(x => x.IsActive && x.Name.ToLower().Trim() == lineStatus.ToLower().Trim());
															if (fixLineStatus != null)
															{
																TelephoneNumberAllocation  newTelephoneNumberAllocation = _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstOrDefault(x => x.TelephoneNumberId == telephoneNumber.Id && !x.IsDelete);
															    if(newTelephoneNumberAllocation != null) {
																	TelePhoneNumberAllocationPackage newTelePhoneNumberAllocationPackage = await _dbTeleBilling_V01Context.TelePhoneNumberAllocationPackage.FirstOrDefaultAsync(x=>x.TelephoneNumberAllocationId == newTelephoneNumberAllocation.Id && x.PackageId == providerPackage.Id && !x.IsDelete);
																	if(newTelePhoneNumberAllocationPackage == null) {
																		newTelePhoneNumberAllocationPackage = new TelePhoneNumberAllocationPackage();
																		newTelePhoneNumberAllocationPackage.ServiceId = providerPackage.ServiceTypeId;
																		newTelePhoneNumberAllocationPackage.StartDate = newStartDate;
																		newTelePhoneNumberAllocationPackage.EndDate = newEndDate;
																		newTelePhoneNumberAllocationPackage.PackageId = providerPackage.Id;
																		bulkAssignTelephoneResponseAC.SuccessRecords += 1;
																		newTelePhoneNumberAllocationPackage.TelephoneNumberAllocationId = newTelephoneNumberAllocation.Id;
																		await _dbTeleBilling_V01Context.AddAsync(newTelePhoneNumberAllocationPackage);
																		await _dbTeleBilling_V01Context.SaveChangesAsync();

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
								//PhoneNumber Not Exists
								bulkAssignTelephoneResponseAC.SkipRecords += 1;
								excelUploadResultList = AddedFileDataResponse(1, j + 1, mobileNumber, _iStringConstant.PhoneNumberNotExists, sheetName, excelUploadResultList);
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
