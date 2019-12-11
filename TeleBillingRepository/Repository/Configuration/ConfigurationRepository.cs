using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Configuration
{
	public class ConfigurationRepository : IConfigurationRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private readonly IMapper _mapper;
		#endregion

		#region "Constructor"
		public ConfigurationRepository(telebilling_v01Context dbTeleBilling_V01Context, IStringConstant iStringConstant,
			ILogManagement iLogManagement, IMapper mapper)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion

		#region Public Method(s)

		#region Notification & Reminder
		public async Task<ResponseAC> AddConfiguration(long userId, TeleBillingUtility.Models.Configuration configuration, string loginUserName)
		{
			ResponseAC responseAC = new ResponseAC();
			if (configuration.Id == 0)
			{
				configuration.CreatedBy = 1;
				configuration.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				configuration.CreatedDate = DateTime.Now;
				await _dbTeleBilling_V01Context.AddAsync(configuration);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				responseAC.Message = _iStringConstant.ConfigurationAddedSuccessfully;
			}
			else
			{
				TeleBillingUtility.Models.Configuration configurationObj = await _dbTeleBilling_V01Context.Configuration.FirstAsync(x => x.Id == configuration.Id);

				#region Transaction Log Entry
				if (configurationObj.TransactionId == null)
					configurationObj.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(configurationObj);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(configurationObj.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion
				configurationObj = _mapper.Map(configuration, configurationObj);
				configurationObj.UpdatedBy = 1;
				configurationObj.UpdatedDate = DateTime.Now;

				_dbTeleBilling_V01Context.Update(configurationObj);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				responseAC.Message = _iStringConstant.ConfigurationUpdateSuccessfully;
			}
			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.UpdateReminderNotificaiton, loginUserName, userId, "Reminder & notification", (int)EnumList.ActionTemplateTypes.ReminderNotificaiton, configuration.Id);
			return responseAC;
		}

		public async Task<TeleBillingUtility.Models.Configuration> GetConfiguration()
		{
			return await _dbTeleBilling_V01Context.Configuration.FirstOrDefaultAsync();
		}
		#endregion

		#region Provider Wise Transaction 
		public async Task<List<ProviderWiseTransactionAC>> GetProviderWiseTransaction()
		{
			List<Transactiontypesetting> transactionTypeSettings = await _dbTeleBilling_V01Context.Transactiontypesetting.Where(x => !x.IsDelete).Include(x => x.Provider).Include(x => x.SetTypeAsNavigation).OrderByDescending(x => x.Id).ToListAsync();
			return _mapper.Map<List<ProviderWiseTransactionAC>>(transactionTypeSettings);
		}

		public async Task<ResponseAC> AddProviderWiseTransaction(long userId, ProviderWiseTransactionAC providerWiseTransactionAC, string loginUserName)
		{
			ResponseAC response = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Transactiontypesetting.AnyAsync(x => !x.IsDelete && x.ProviderId == providerWiseTransactionAC.ProviderId && x.TransactionType.ToLower().Trim() == providerWiseTransactionAC.TransactionType.ToLower().Trim()))
			{
				Transactiontypesetting transactionTypeSetting = _mapper.Map<Transactiontypesetting>(providerWiseTransactionAC);

				transactionTypeSetting.CreatedBy = userId;
				transactionTypeSetting.IsActive = true;
				transactionTypeSetting.CreatedDate = DateTime.Now;
				transactionTypeSetting.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				await _dbTeleBilling_V01Context.AddAsync(transactionTypeSetting);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				response.Message = _iStringConstant.ProviderWiseTransactionTypeAddedSuccessfully;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddProviderWiseTransactionType, loginUserName, userId, "Provider wise transaction(" + transactionTypeSetting.TransactionType + ")", (int)EnumList.ActionTemplateTypes.Add, transactionTypeSetting.Id);
			}
			else
			{
				response.Message = _iStringConstant.ProviderWiseTransactionTypeAlreadyExists;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return response;
		}

		public async Task<ResponseAC> UpdateProviderWiseTransaction(long userId, ProviderWiseTransactionAC providerWiseTransactionAC, string loginUserName)
		{
			ResponseAC response = new ResponseAC();
			if (!await _dbTeleBilling_V01Context.Transactiontypesetting.AnyAsync(x => !x.IsDelete && x.Id != providerWiseTransactionAC.Id && x.ProviderId == providerWiseTransactionAC.ProviderId && x.TransactionType.ToLower().Trim() == providerWiseTransactionAC.TransactionType.ToLower().Trim()))
			{
				Transactiontypesetting transactionTypeSetting = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.Id == providerWiseTransactionAC.Id);

				#region Transaction Log Entry
				if (transactionTypeSetting.TransactionId == null)
					transactionTypeSetting.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(transactionTypeSetting);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(transactionTypeSetting.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				transactionTypeSetting = _mapper.Map(providerWiseTransactionAC, transactionTypeSetting);
				transactionTypeSetting.UpdatedBy = userId;
				transactionTypeSetting.UpdatedDate = DateTime.Now;

				_dbTeleBilling_V01Context.Update(transactionTypeSetting);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				response.Message = _iStringConstant.ProviderWiseTransactionTypeUpdatedSuccessfully;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				//await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddProviderWiseTransactionType, loginUserName, userId, "Provider wise transaction(" + transactionTypeSetting.TransactionType + ")", (int)EnumList.ActionTemplateTypes.Edit, transactionTypeSetting.Id);
			}
			else
			{
				response.Message = _iStringConstant.ProviderWiseTransactionTypeAlreadyExists;
				response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
			}
			return response;
		}

		public async Task<bool> DeleteProviderWiseTransaction(long userId, long id, string loginUserName)
		{
			Transactiontypesetting transactionTypeSetting = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.Id == id);
			transactionTypeSetting.IsDelete = true;
			transactionTypeSetting.UpdatedBy = userId;
			transactionTypeSetting.UpdatedDate = DateTime.Now;

			_dbTeleBilling_V01Context.Update(transactionTypeSetting);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			//await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.provider, loginUserName, userId, "Provider wise transaction(" + transactionTypeSetting.TransactionType + ")", (int)EnumList.ActionTemplateTypes.Edit, transactionTypeSetting.Id);
			return true;
		}

		public async Task<ProviderWiseTransactionAC> GetProviderWiseTransactionById(long id)
		{
			ProviderWiseTransactionAC providerWiseTransaction = new ProviderWiseTransactionAC();

			Transactiontypesetting transactionTypeSetting = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.Id == id);
			providerWiseTransaction = _mapper.Map(transactionTypeSetting, providerWiseTransaction);
			return providerWiseTransaction;
		}

		public async Task<BulkAssignTelephoneResponseAC> BulkUploadProviderWiseTrans(long userId, ExcelUploadResponseAC exceluploadDetail, long providerId, string loginUserName)
		{
			BulkAssignTelephoneResponseAC bulkAssignTelephoneResponseAC = new BulkAssignTelephoneResponseAC();
			List<ExcelUploadResult> excelUploadResultList = new List<ExcelUploadResult>();
			List<Transactiontypesetting> transactionTypeSettingList = new List<Transactiontypesetting>();
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
							Transactiontypesetting transactionTypeSetting = new Transactiontypesetting();
							bulkAssignTelephoneResponseAC.TotalRecords += 1;
							string transactionType = workSheet.Cells[j + 1, 1].Value != null ? workSheet.Cells[j + 1, 1].Value.ToString() : string.Empty;
							if (!string.IsNullOrEmpty(transactionType))
							{
								if (!await _dbTeleBilling_V01Context.Transactiontypesetting.AnyAsync(x => x.ProviderId == providerId && !x.IsDelete && x.TransactionType.ToLower().Trim() == transactionType.ToLower().Trim()))
								{
									transactionTypeSetting.ProviderId = providerId;
									transactionTypeSetting.TransactionType = transactionType.Trim();
									transactionTypeSetting.CreatedBy = userId;
									transactionTypeSetting.CreatedDate = DateTime.Now;
									transactionTypeSetting.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

									bulkAssignTelephoneResponseAC.SuccessRecords += 1;
									transactionTypeSettingList.Add(transactionTypeSetting);

								}
								else
								{
									//PhoneNumber Not Exists
									bulkAssignTelephoneResponseAC.SkipRecords += 1;
									excelUploadResultList = AddedFileDataResponse(1, j + 1, transactionType, _iStringConstant.ProviderWiseTransactionTypeAlreadyExists, sheetName, excelUploadResultList);
								}
							}
							else
							{
								//PhoneNumber Not Exists
								bulkAssignTelephoneResponseAC.SkipRecords += 1;
								excelUploadResultList = AddedFileDataResponse(1, j + 1, transactionType, _iStringConstant.TransactionTypeIsEmpty, sheetName, excelUploadResultList);
							}
						}
					}
				}

				if (File.Exists(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid)))
					File.Delete(Path.Combine(exceluploadDetail.FilePath, exceluploadDetail.FileNameGuid));

				if (transactionTypeSettingList.Any())
				{
					await _dbTeleBilling_V01Context.AddRangeAsync(transactionTypeSettingList);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.BulkUploadTransactionType, loginUserName, userId, "Provider wise transaction", (int)EnumList.ActionTemplateTypes.Upload, null);
				}
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

		public async Task<ResponseAC> UpdateTransactionTypeSetting(long userId, ProviderWiseTransactionAC providerWiseTransactionAC, string loginUserName)
		{
			ResponseAC response = new ResponseAC();
			foreach (var item in providerWiseTransactionAC.TransactionTypeList)
			{
				Transactiontypesetting transactionTypeSetting = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == providerWiseTransactionAC.ProviderId && x.TransactionType.Trim().ToLower() == item.Name.Trim().ToLower() && x.Id == item.Id);
				transactionTypeSetting.SetTypeAs = providerWiseTransactionAC.SetTypeAs;
				transactionTypeSetting.UpdatedBy = userId;
				transactionTypeSetting.UpdatedDate = DateTime.Now;

				_dbTeleBilling_V01Context.Update(transactionTypeSetting);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				string setType= string.Empty;
				if(providerWiseTransactionAC.SetTypeAs == ((int)EnumList.AssignType.Employee))
					setType = "Personal";
				else
					setType = "Business";
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.SetTypeOfTransactionTypeSetting, loginUserName, userId, "Provider wise transaction(transaction type: '" + transactionTypeSetting.TransactionType + "'; and set as type: '"+setType+"')", (int)EnumList.ActionTemplateTypes.SetTransactionType, transactionTypeSetting.Id);
			}
			response.Message = _iStringConstant.TransactionTypeSettingUpdatedSuccessfully;
			response.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			return response;
		}

		public async Task<bool> ChangeProviderWiseTransactionStatus(long id, long userId, string loginUserName)
		{
			Transactiontypesetting transactiontypesetting = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.Id == id);
			if (transactiontypesetting != null)
			{
				#region Transaction Log Entry
				if (transactiontypesetting.TransactionId == null)
					transactiontypesetting.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(transactiontypesetting);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(transactiontypesetting.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.ChangeStatus), jsonSerailzeObj);
				#endregion

				transactiontypesetting.IsActive = !transactiontypesetting.IsActive;
				transactiontypesetting.UpdatedBy = userId;
				transactiontypesetting.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(transactiontypesetting);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				if (transactiontypesetting.IsActive)
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ActiveProviderWiseTransactionType, loginUserName, userId, "Provider wise transaction(" + transactiontypesetting.TransactionType + ")", (int)EnumList.ActionTemplateTypes.Active, transactiontypesetting.Id);
				else
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeactiveProviderWiseTransactinType, loginUserName, userId, "Provider wise transaction(" + transactiontypesetting.TransactionType + ")", (int)EnumList.ActionTemplateTypes.Deactive, transactiontypesetting.Id);

				return true;
			}
			return false;

		}

		#endregion

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
	}
}
