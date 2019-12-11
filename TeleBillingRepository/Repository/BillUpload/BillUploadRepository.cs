using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.Text.RegularExpressions;
using System.Data;
using TeleBillingUtility.Helpers;
using System.Collections;
using Microsoft.Extensions.Configuration;
using CsvHelper;
using TeleBillingUtility.Helpers.CommonFunction;
using NLog;
using Microsoft.AspNetCore.Http;


namespace TeleBillingRepository.Repository.BillUpload
{
	public class BillUploadRepository : IBillUploadRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		private IHostingEnvironment _hostingEnvironment;
		private readonly DAL _objDal = new DAL();
		private readonly DALMySql _objDalmysql = new DALMySql();
		private readonly IConfiguration _config;
        private readonly Logger _logger = LogManager.GetLogger("logger");
        #endregion

        #region "Constructor"

        public BillUploadRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
			, ILogManagement ilogManagement, IHostingEnvironment ihostingEnvironment, IConfiguration config)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_mapper = mapper;
			_iLogManagement = ilogManagement;
			_hostingEnvironment = ihostingEnvironment;
			_config = config;
		}
		#endregion

		#region Public Method(s)

		public async Task<List<BillUploadListAC>> GetBillUploadedList()
		{
			try
			{             

                List<BillUploadListAC> billUploadLists = new List<BillUploadListAC>();
                SortedList sl = new SortedList();
                sl.Add("SMonth", Convert.ToUInt64(0));
                sl.Add("SYear", Convert.ToUInt64(0));       
                sl.Add("ProviderId", 0);
                sl.Add("SkipRecord", 0);
                sl.Add("Length", 0);
                sl.Add("SearchValue", "");

                DataSet ds = _objDalmysql.GetDataSet("usp_GetBillUploadListWithPagging",sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						billUploadLists = _objDal.ConvertDataTableToGenericList<BillUploadListAC>(ds.Tables[0]).ToList();
					}
				}       
          
				return billUploadLists;
			}
			catch (Exception e)
			{
				// throw e;
				return new List<BillUploadListAC>();
			}

		}

		public async Task<List<PbxBillUploadListAC>> GetPbxBillUploadedList()
		{
			List<PbxBillUploadListAC> billUploadLists = new List<PbxBillUploadListAC>();
			// List<Exceluploadlogpbx> exceluploadlist = await _dbTeleBilling_V01Context.Exceluploadlogpbx.Where(x => !x.IsDelete).OrderByDescending(x => x.UploadDate).ToListAsync();
			List<Exceluploadlogpbx> exceluploadlist = await _dbTeleBilling_V01Context.Exceluploadlogpbx.Where(x => !x.IsDelete).Include(x => x.Device).OrderByDescending(x => x.Id).ToListAsync();
			foreach (var item in exceluploadlist)
			{
				PbxBillUploadListAC billUploadListAC = new PbxBillUploadListAC();
				billUploadListAC = _mapper.Map<PbxBillUploadListAC>(item);
				billUploadLists.Add(billUploadListAC);
			}
			return billUploadLists;
		}

		public async Task<bool> DeleteExcelUplaod(long userId, long id, string loginUserName)
		{
			Exceluploadlog excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlog.FirstOrDefaultAsync(x => x.Id == id);
			if (excelUploadLog != null)
			{
				excelUploadLog.IsDelete = true;
				excelUploadLog.UploadBy = userId;
				excelUploadLog.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(excelUploadLog);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				ExceluploadlogServicetype excelUploadServices = await _dbTeleBilling_V01Context.ExceluploadlogServicetype.FirstOrDefaultAsync(x => x.ExcelUploadLogId == id);

				if (excelUploadServices != null)
				{
					excelUploadServices.IsDelete = true;
					excelUploadServices.UpdatedBy = userId;
					excelUploadServices.UpdatedDate = DateTime.Now;
					_dbTeleBilling_V01Context.Update(excelUploadServices);
					await _dbTeleBilling_V01Context.SaveChangesAsync();

					#region Audit Log
						await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteBill, loginUserName, userId, "Uploaded bill(" + excelUploadLog.ExcelFileName + ")", (int)EnumList.ActionTemplateTypes.Delete, excelUploadLog.Id);
					#endregion

					return true;
				}

				return true;
			}
			return false;
		}

		public async Task<bool> ApproveExcelUploadLog(long userId, long id,string loginUserName)
		{
			Exceluploadlog excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlog.FirstOrDefaultAsync(x => x.Id == id);

            if (excelUploadLog != null)
            {
                if((excelUploadLog.MergedWithId ?? 0) > 0)
                {
                    id = excelUploadLog.MergedWithId?? id;
                }
            }

            List<Exceluploadlog> relateduploadList = new List<Exceluploadlog>();

            relateduploadList = await _dbTeleBilling_V01Context.Exceluploadlog.Where(x => x.Id == id || x.MergedWithId == id && !x.IsDelete).ToListAsync();

            if (relateduploadList != null)
            {
                List<Exceluploadlog> Exceluploadloglist = new List<Exceluploadlog>();
                foreach (var data in relateduploadList)
                {
                    data.IsApproved = true;
                    data.UploadBy = userId;
                    data.UpdatedDate = DateTime.Now;
                    Exceluploadloglist.Add(data);
                }
                _dbTeleBilling_V01Context.UpdateRange(Exceluploadloglist);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

				#region Audit Log
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ApproveUploadedBIll, loginUserName, userId, "Uploaded bill(" + excelUploadLog.ExcelFileName + ")", (int)EnumList.ActionTemplateTypes.Approve, excelUploadLog.Id);
				#endregion

				return true;
            }

            //if (excelUploadLog != null)
            //{
            //    excelUploadLog.IsApproved = true;
            //    excelUploadLog.UploadBy = userId;
            //    excelUploadLog.UpdatedDate = DateTime.Now;
            //    _dbTeleBilling_V01Context.Update(excelUploadLog);
            //    await _dbTeleBilling_V01Context.SaveChangesAsync();
            //    return true;
            //}
            return false;
        }

		public async Task<bool> ApproveExcelUploadPbxLog(long userId, long id, string loginUserName)
		{
			Exceluploadlogpbx excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlogpbx.FirstOrDefaultAsync(x => x.Id == id);

			if (excelUploadLog != null)
			{
				excelUploadLog.IsApproved = true;
				excelUploadLog.UploadBy = userId;
				excelUploadLog.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(excelUploadLog);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				#region Audit Log
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ApproveUploadedPBXBill, loginUserName, userId, "Uploaded pbx bill(" + excelUploadLog.ExcelFileName + ")", (int)EnumList.ActionTemplateTypes.Approve, excelUploadLog.Id);
				#endregion

				return true;
			}
			return false;
		}

		public bool CheckIsBillAllocated(long id)
		{
			if (id > 0)
			{
				SortedList sl = new SortedList();
				sl.Add("UploadLogId", id);
				int result = Convert.ToInt16(_objDalmysql.ExecuteScaler("usp_CheckExcelBillAllocated", sl));
				if (result == 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public async Task<long> ChekBillMergeId(long providerId, int month, int year)
		{
			long uploadId = 0;
			Exceluploadlog excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlog.FirstOrDefaultAsync(x => x.Month == month && x.Year == year && x.ProviderId == providerId && !x.IsDelete && !(x.IsMerge ?? false));
			if (excelUploadLog != null)
			{
				uploadId = excelUploadLog.Id;
			}
			return uploadId;
		}

		public async Task<long> ChekBillMergeIdPbx(long deviceId, int month, int year)
		{
			long uploadId = 0;
			Exceluploadlogpbx excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlogpbx.FirstOrDefaultAsync(x => x.Month == month && x.Year == year && x.DeviceId == deviceId && !x.IsDelete && !(x.IsMerge ?? false));
			if (excelUploadLog != null)
			{
				uploadId = excelUploadLog.Id;
			}
			return uploadId;
		}

        public async Task<long> CheckBillCanMergedOld(BillUploadAC billUploadModel)
        {
            try
            {
                List<Exceluploadlog> exceluploadlog = new List<Exceluploadlog>();
                if (billUploadModel != null)
                {
                    if (billUploadModel.MonthId > 0 && billUploadModel.ProviderId > 0 && billUploadModel.YearId > 0)
                    {
                        exceluploadlog = await _dbTeleBilling_V01Context.Exceluploadlog
                                           .Where(x => x.Month == billUploadModel.MonthId
                                            && x.Year == billUploadModel.YearId
                                            && x.ProviderId == billUploadModel.ProviderId
                                            && !x.IsDelete                                          
                                            && !(x.IsMerge ?? false)
                                            && (x.MergedWithId ?? 0) == 0
                                            ).ToListAsync();

						if (exceluploadlog != null)
						{
							if (exceluploadlog.Count() > 0)
							{

                              if (exceluploadlog.Count() > 1)
                                {
                                    //return -1;

                                foreach(var oldexefile in exceluploadlog)
                                    {
                                        long excelUploadId = oldexefile.Id;                                       
                                        List<ExceluploadlogServicetype> uploadedexcelserviceList = new List<ExceluploadlogServicetype>();
                                        uploadedexcelserviceList = await _dbTeleBilling_V01Context.ExceluploadlogServicetype
                                                                    .Where(x => x.ExcelUploadLogId == excelUploadId && !x.IsDelete)
                                                                    .ToListAsync();
                                        if (uploadedexcelserviceList != null)
                                        {
                                            if (uploadedexcelserviceList.Count() == billUploadModel.ServiceTypes.Count())
                                            {
                                                List<ExceluploadlogServicetype> differentServiceList = new List<ExceluploadlogServicetype>();

                                                differentServiceList = uploadedexcelserviceList
                                                                        .Where(x => !x.IsDelete
                                                                         && exceluploadlog.Select(e => e.Id).Contains(x.ExcelUploadLogId)
                                                                         && !billUploadModel.ServiceTypes.Select(s => s.Id).Contains(x.ServiceTypeId)
                                                                        ).ToList();

                                                if (differentServiceList != null && differentServiceList.Count() > 0)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    #region Old Code to merge
                                                    List<ExceluploadlogServicetype> excelserviceList = new List<ExceluploadlogServicetype>();

                                                    excelserviceList =  //await _dbTeleBilling_V01Context.ExceluploadlogServicetype
                                                                             uploadedexcelserviceList
                                                                            .Where(x => !x.IsDelete
                                                                             && exceluploadlog.Select(e => e.Id).Contains(x.ExcelUploadLogId)
                                                                             && billUploadModel.ServiceTypes.Select(s => s.Id).Contains(x.ServiceTypeId)
                                                                            ).ToList();

                                                    if (excelserviceList != null)
                                                    {
                                                        if (excelserviceList.Count() > 0)
                                                        {
                                                            #region --> Check Bill is not approved

                                                            var NotApproveBillUpload = exceluploadlog.FirstOrDefault(x => x.Id > 0 && (x.IsApproved ?? false) == true);

                                                            if (NotApproveBillUpload != null && NotApproveBillUpload.Id > 0)
                                                            {
                                                                return -1;
                                                            }

                                                            #endregion

                                                            return excelserviceList.Select(x => x.ExcelUploadLogId).FirstOrDefault();
                                                        }
                                                        else
                                                        {
                                                            return 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        return 0;
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }

                                      }
                                }
                                else
                                {
                                    long excelUploadId = 0;
                                    excelUploadId = exceluploadlog.Select(x => x.Id).FirstOrDefault();
                                    List<ExceluploadlogServicetype> uploadedexcelserviceList = new List<ExceluploadlogServicetype>();
                                    uploadedexcelserviceList = await _dbTeleBilling_V01Context.ExceluploadlogServicetype
                                                                .Where(x => x.ExcelUploadLogId == excelUploadId && !x.IsDelete)
                                                                .ToListAsync();
                                    if(uploadedexcelserviceList != null)
                                    {
                                         if(uploadedexcelserviceList.Count() == billUploadModel.ServiceTypes.Count())
                                        {
                                            List<ExceluploadlogServicetype> differentServiceList = new List<ExceluploadlogServicetype>();

                                            differentServiceList = uploadedexcelserviceList
                                                                    .Where(x => !x.IsDelete
                                                                     && exceluploadlog.Select(e => e.Id).Contains(x.ExcelUploadLogId)
                                                                     && !billUploadModel.ServiceTypes.Select(s => s.Id).Contains(x.ServiceTypeId)
                                                                    ).ToList();

                                            if(differentServiceList!=null  && differentServiceList.Count() > 0)
                                            {
                                                return 0;
                                            }
                                            else
                                            {
                                                #region Old Code to merge
                                                List<ExceluploadlogServicetype> excelserviceList = new List<ExceluploadlogServicetype>();

                                                excelserviceList =  //await _dbTeleBilling_V01Context.ExceluploadlogServicetype
                                                                         uploadedexcelserviceList
                                                                        .Where(x => !x.IsDelete
                                                                         && exceluploadlog.Select(e => e.Id).Contains(x.ExcelUploadLogId)
                                                                         && billUploadModel.ServiceTypes.Select(s => s.Id).Contains(x.ServiceTypeId)
                                                                        ).ToList();

                                                if (excelserviceList != null)
                                                {
                                                    if (excelserviceList.Count() > 0)
                                                    {
                                                        #region --> Check Bill is not approved

                                                        var NotApproveBillUpload = exceluploadlog.FirstOrDefault(x => x.Id > 0 && (x.IsApproved ?? false) == true);

                                                        if (NotApproveBillUpload != null && NotApproveBillUpload.Id > 0)
                                                        {
                                                            return -1;
                                                        }

                                                        #endregion

                                                        return excelserviceList.Select(x => x.ExcelUploadLogId).FirstOrDefault();
                                                    }
                                                    else
                                                    {
                                                        return 0;
                                                    }
                                                }
                                                else
                                                {
                                                    return 0;
                                                }
                                                #endregion
                                            }

                                        }

                                    }

                                }

                            

                            }
                            else
                            {
                                return 0;
                            }

                        }
                        else
                        {
                            return 0;
                        }
                    }

                }

                return 0;
            }
            catch (Exception e)
            {
                return 0;
            }

        }

        public async Task<long> CheckBillCanMerged(BillUploadAC billUploadModel)
        {
            try
            {
               
                if (billUploadModel != null)
                {
                    if (billUploadModel.MonthId > 0 && billUploadModel.ProviderId > 0 && billUploadModel.YearId > 0 && billUploadModel.ServiceTypes != null && billUploadModel.ServiceTypes.Count()>0)
                    {
                        try
                        {
                            string ServiceTypeIds = string.Empty;
                            List<long>  serviceidlst= new List<long>();
                            serviceidlst = billUploadModel.ServiceTypes.Select(x => x.Id).ToList();
                            ServiceTypeIds = string.Join(",", serviceidlst.Distinct().Select(x => x.ToString()).ToArray());
                             

                            List<ExcelUploadIdCountAC> oldexcelUploadIds = new List<ExcelUploadIdCountAC>();
                            SortedList sl = new SortedList();
                            sl.Add("BillMonth",Convert.ToUInt64(billUploadModel.MonthId));
                            sl.Add("BillYear", Convert.ToUInt64(billUploadModel.YearId));
                            sl.Add("ServiceTypesIdList", ServiceTypeIds);
                            sl.Add("ServiceCount", billUploadModel.ServiceTypes.Count());
                            sl.Add("ProviderId", billUploadModel.ProviderId);

                            DataSet ds = _objDalmysql.GetDataSet("usp_GetExcelUploadIdForMerge",sl);                         

                            if (ds != null)
                            {
                                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                                {
                                    oldexcelUploadIds = _objDal.ConvertDataTableToGenericList<ExcelUploadIdCountAC>(ds.Tables[0]).ToList();
                                }
                            }

                            if(oldexcelUploadIds!=null && oldexcelUploadIds.Count() > 0)
                            {
                                foreach(var olddata in oldexcelUploadIds)
                                {
                                    Exceluploadlog oldexceluploadlog = new Exceluploadlog();
                                    oldexceluploadlog = await _dbTeleBilling_V01Context.Exceluploadlog.FindAsync(olddata.Id);
                                    #region --> Check Bill is not approved

                                    if(oldexceluploadlog!=null && oldexceluploadlog.Id > 0)
                                    {
                                        if((oldexceluploadlog.IsApproved ?? false) == true)
                                        {
                                            return -1;
                                        }
                                        else
                                        {
                                            return oldexceluploadlog.Id;
                                        }
                                    }                                  
                                    #endregion
                                }
                               
                            }

                            return 0;
                           
                        }
                        catch (Exception e)
                        {
                            return -2;
                        }

                    }

				}

				return 0;
			}
			catch (Exception e)
			{
				return 0;
			}

		}

		public async Task<long> CheckPBXBillCanMerged(PbxBillUploadAC billUploadModel)
		{
			try
			{
				List<Exceluploadlogpbx> exceluploadlog = new List<Exceluploadlogpbx>();
				if (billUploadModel != null)
				{
					if (billUploadModel.MonthId > 0 && billUploadModel.DeviceId > 0 && billUploadModel.YearId > 0)
					{
						exceluploadlog = await _dbTeleBilling_V01Context.Exceluploadlogpbx
										   .Where(x => x.Month == billUploadModel.MonthId
											&& x.Year == billUploadModel.YearId
											&& x.DeviceId == billUploadModel.DeviceId
											&& !x.IsDelete
											//  && !(x.IsMerge ?? false)
											//  && (x.MergedWithId ?? 0) == 0
											).ToListAsync();

						if (exceluploadlog != null)
						{
							if (exceluploadlog.Count() > 0)
							{
								return exceluploadlog.Select(x => x.Id).FirstOrDefault();
							}
							else
							{
								return 0;
							}

						}
						else
						{
							return 0;
						}
					}

				}

				return 0;
			}
			catch (Exception e)
			{
				return 0;
			}

		}


		public async Task<bool> DeletePbxExcelUplaod(long userId, long id, string loginUserName)
		{
			Exceluploadlogpbx excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlogpbx.FirstOrDefaultAsync(x => x.Id == id);
			if (excelUploadLog != null)
			{
				excelUploadLog.IsDelete = true;
				excelUploadLog.UploadBy = userId;
				excelUploadLog.UpdatedDate = DateTime.Now;
				_dbTeleBilling_V01Context.Update(excelUploadLog);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				#region Audit Log
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeletePBXBill, loginUserName, userId, "Uploaded pbx bill("+ excelUploadLog.ExcelFileName + ")", (int)EnumList.ActionTemplateTypes.Delete, excelUploadLog.Id);
				#endregion

				return true;
			}
			return false;
		}

		public async Task<List<MappingDetailAC>> GetExcelMapping(BillUploadAC billUploadModel)
		{
			List<Mappingexcel> mappingExcel = new List<Mappingexcel>();
			List<MappingDetailAC> mappingDetails = new List<MappingDetailAC>();
			if (billUploadModel != null && billUploadModel.ProviderId > 0)
			{
				if (billUploadModel.ServiceTypes.Count > 0)
				{
					List<long> serviceTypeIds = new List<long>();
					serviceTypeIds = billUploadModel.ServiceTypes.Select(x => x.Id).ToList();
					mappingExcel = await _dbTeleBilling_V01Context.Mappingexcel.Where(x => x.ProviderId == billUploadModel.ProviderId && serviceTypeIds.Contains(x.ServiceTypeId) && !x.IsDelete && x.IsActive == true).Include(x => x.Mappingexcelcolumn).ToListAsync();
				}
			}

			if (mappingExcel != null && mappingExcel.Count > 0)
			{
				mappingDetails = _mapper.Map<List<MappingDetailAC>>(mappingExcel);
				foreach (var m in mappingDetails)
				{
					var mapDBData = await _dbTeleBilling_V01Context.Mappingexcelcolumn
											.Where(x => x.MappingExcelId == m.Id || (m.IsCommonMapped == true && x.MappingExcelId == m.MappedMappingId))
											.Include(x => x.MappingServiceTypeField)
											.Select(x => new DBFiledMappingAC
											{
												MappingDetailId = x.MappingExcelId,
												DBColumnName = x.MappingServiceTypeField.DbcolumnName,
												ExcelcolumnName = x.ExcelcolumnName,
												FormatField = x.FormatField
											})
											.ToListAsync();
					m.DBFiledMappingList = mapDBData.ToList();
				}
			}
			return mappingDetails;
		}

		public async Task<List<MappingDetailPbxAC>> GetPbxExcelMapping(PbxBillUploadAC billUploadModel)
		{
			List<MappingexcelPbx> mappingExcel = new List<MappingexcelPbx>();
			List<MappingDetailPbxAC> mappingDetails = new List<MappingDetailPbxAC>();
			if (billUploadModel != null)
			{
				if (billUploadModel.DeviceId > 0)
				{
					mappingExcel = await _dbTeleBilling_V01Context.MappingexcelPbx.Where(x => x.DeviceId == billUploadModel.DeviceId && !x.IsDelete && x.IsActive == true).Include(x => x.MappingexcelcolumnPbx).ToListAsync();
				}
			}

			if (mappingExcel != null && mappingExcel.Count > 0)
			{
				mappingDetails = _mapper.Map<List<MappingDetailPbxAC>>(mappingExcel);
				foreach (var m in mappingDetails)
				{
					var mapDBData = await _dbTeleBilling_V01Context.MappingexcelcolumnPbx
											.Where(x => x.MappingExcelId == m.Id)
											.Include(x => x.MappingServiceTypeField)
											.Select(x => new DBFiledMappingAC
											{
												MappingDetailId = x.MappingExcelId,
												DBColumnName = x.MappingServiceTypeField.DbcolumnName,
												ExcelcolumnName = x.ExcelcolumnName,
												FormatField = x.FormatField
											})
											.ToListAsync();
					m.DBFiledMappingList = mapDBData.ToList();
				}
			}
			return mappingDetails;
		}


		public ExcelUploadResponseAC UploadNewExcel(ExcelFileAC excelFileAC)
		{
			ExcelUploadResponseAC excelUploadResponse = new ExcelUploadResponseAC();
			try
			{
				#region --> Save File to temp Folder

				var file = excelFileAC.File;
				string folderName = excelFileAC.FolderName;
				string webRootPath = _hostingEnvironment.WebRootPath;
				string newPath = Path.Combine(webRootPath, folderName);
				string excelfilenameGuid = string.Empty;
				if (!Directory.Exists(newPath))
				{
					Directory.CreateDirectory(newPath);
				}
				if (file.Length > 0)
				{
					excelUploadResponse.FileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
					excelfilenameGuid = Guid.NewGuid().ToString() + "." + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Split(".")[1].Trim('"');
					// excelfilenameGuid = Guid.NewGuid().ToString() + ".xlsx";

					string fullPath = Path.Combine(newPath, excelfilenameGuid);
					using (var stream = new FileStream(fullPath, FileMode.Create))
					{
						file.CopyTo(stream);
					}
				}

				excelUploadResponse.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				excelUploadResponse.Message = "File uplaoded successfully";
				excelUploadResponse.FileNameGuid = excelfilenameGuid;
				excelUploadResponse.FilePath = newPath;
				#endregion
			}
			catch (Exception e)
			{
				excelUploadResponse.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
				excelUploadResponse.Message = "Error while excel uploading :" + e.Message;
				return excelUploadResponse;
			}

			return excelUploadResponse;
		}

		public string getAddress(List<DBFiledMappingAC> DBFiledMappingList, string dbColumn, int index)
		{
			string cellAddress = string.Empty;
			if (DBFiledMappingList != null)
			{
				cellAddress = DBFiledMappingList.Where(x => x.DBColumnName == dbColumn).Select(x => x.ExcelcolumnName).FirstOrDefault();
				cellAddress = Convert.ToString(cellAddress) + Convert.ToString(index);
			}
			return cellAddress;
		}

		public string getFormatField(List<DBFiledMappingAC> DBFiledMappingList, string dbColumn)
		{
			string formatField = string.Empty;
			if (DBFiledMappingList != null)
			{
				formatField = DBFiledMappingList.Where(x => x.DBColumnName == dbColumn).Select(x => x.FormatField).FirstOrDefault();
				formatField = Convert.ToString(formatField);
			}
			return formatField;
		}

		public string checkPriceValidation(string dbColumn, string Value, Type valueType, bool isRequired)
		{
			string ErrorMessage = "valid";
			string PriceValueStr = Value;
			try
			{
				if (!string.IsNullOrEmpty(PriceValueStr))
				{

					if ((valueType == typeof(string)))
					{
						decimal number1;
						double number2;
						long number3;
						if ((!decimal.TryParse(PriceValueStr, out number1)) && (!double.TryParse(PriceValueStr, out number2)) && (!long.TryParse(PriceValueStr, out number3)))
						{
							ErrorMessage = dbColumn + " must be numeric ! ";
						}
					}
					else
					{
						if ((valueType != typeof(decimal)) && (valueType != typeof(double)) && (valueType != typeof(long)))
						{
							ErrorMessage = dbColumn + " must be numeric ! ";
						}

					}
				}
				else
				{
					if (isRequired)
						ErrorMessage = dbColumn + " doesnot exists ! ";

				}
				return ErrorMessage;
			}
			catch (Exception)
			{
				ErrorMessage = dbColumn + " must be numeric ! ";
				return ErrorMessage;
			}

		}

		public CallerDetailResponseAC getCallerReceiverNumber(string calldetail)
		{
			CallerDetailResponseAC keyVal = new CallerDetailResponseAC();
			try
			{
				if (!string.IsNullOrEmpty(calldetail))
				{
					keyVal.CallerNumber = calldetail;
					keyVal.ReceiverNumber = calldetail;
				}
				else
				{
					return keyVal;
				}

				#region sepration of  call detail
				// string mobileData = "From: +99051000265027 To: +97143188807";
				string CallFrom = "";
				string CallTo = "";
				calldetail = calldetail.Trim();
				CallFrom = calldetail.Replace("From: +", "");
				int pos = CallFrom.IndexOf("To: +");
				if (pos >= 0)
				{
					// String after founder  
					string afterFounder = CallFrom.Remove(pos);
					CallFrom = afterFounder;
					// Remove everything before founder but include founder
					int pos2 = calldetail.IndexOf("To: +");
					string beforeFounder = calldetail.Remove(0, pos2);
					CallTo = beforeFounder.Replace("To: +", "");
				}
				#endregion
				if (CallFrom.Length > 0)
					keyVal.CallerNumber = CallFrom.Trim();

				if (CallTo.Length > 0)
					keyVal.ReceiverNumber = CallTo.Trim();


				return keyVal;
			}
			catch (Exception)
			{
				return keyVal;

			}
		}

		public async Task<bool> GetServiceChargeType(long serviceTypeId)
		{
			bool IsBusiness = await _dbTeleBilling_V01Context.FixServicetype.Where(x => x.Id == serviceTypeId).Select(x => x.IsBusinessOnly).FirstOrDefaultAsync();

			if (IsBusiness)
				return true;
			else
				return false;
		}

		public async Task<BillAllocationListAC> GetBillAllocationList(long providerId, int month, int year)
		{
			BillAllocationListAC billAllocationResponse = new BillAllocationListAC();
			Exceluploadlog excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlog.FirstOrDefaultAsync(x => x.Month == month && x.Year == year && x.ProviderId == providerId);
			if (excelUploadLog != null)
			{



			}
			return billAllocationResponse;
		}


		public async Task<long> AddExcelUploadLog(BillUploadAC billUploadModel, string fileNameGuid, long userId)
		{
			try
			{
				Exceluploadlog excelUploadLog = new Exceluploadlog();
				excelUploadLog.ProviderId = billUploadModel.ProviderId;
				excelUploadLog.Month = billUploadModel.MonthId;
				excelUploadLog.Year = billUploadModel.YearId;
				excelUploadLog.IsPbxupload = billUploadModel.DeviceId > 0 ? true : false;
				excelUploadLog.ExcelFileName = billUploadModel.ExcelFileName1 ?? fileNameGuid;
				excelUploadLog.FileNameGuid = fileNameGuid;
				excelUploadLog.UploadBy = userId;
				excelUploadLog.UploadDate = DateTime.Now;
				excelUploadLog.IsDelete = false;
				excelUploadLog.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
                excelUploadLog.CurrencyId = await _dbTeleBilling_V01Context.Provider.Where(x => x.Id == billUploadModel.ProviderId && x.IsActive && !x.IsDelete).Select(x => x.CurrencyId).FirstOrDefaultAsync();


                if (billUploadModel.MergedWithId > 0)
				{
					excelUploadLog.IsMerge = true;
					excelUploadLog.MergedWithId = billUploadModel.MergedWithId;
				}

				await _dbTeleBilling_V01Context.AddAsync(excelUploadLog);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				List<ExceluploadlogServicetype> excelUploadLogServiceType = new List<ExceluploadlogServicetype>();
				foreach (var services in billUploadModel.ServiceTypes)
				{
					excelUploadLogServiceType.Add(new ExceluploadlogServicetype
					{
						ServiceTypeId = services.Id,
						ExcelUploadLogId = excelUploadLog.Id
					});
				}

				await _dbTeleBilling_V01Context.AddRangeAsync(excelUploadLogServiceType);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				return excelUploadLog.Id;

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        public async Task<Exceluploadlog> UpdateExcelUploadLog(long id, long userId,long count,decimal amount,bool isSkypeData)
        {
            try
            {
                Exceluploadlog excelUploadLog = new Exceluploadlog();

                if (id > 0)
                {
                    excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlog.FindAsync(id);
                    if (excelUploadLog != null)
                    {

                        if(excelUploadLog.Id > 0)
                        {
                            if (isSkypeData)
                            {                               
                                excelUploadLog.TotalImportedBillAmount = await _dbTeleBilling_V01Context.Skypeexceldetail.Where(x => x.ExcelUploadLogId == id ).SumAsync(x => x.CallAmount)??0;
                                excelUploadLog.TotalRecordImportCount = await _dbTeleBilling_V01Context.Skypeexceldetail.Where(x => x.ExcelUploadLogId == id).CountAsync();
                            }
                            else
                            {
                                excelUploadLog.TotalImportedBillAmount = await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == id || x.MergeExcelUploadId == id).SumAsync(x => x.CallAmount) ?? 0;
                                excelUploadLog.TotalRecordImportCount = await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == id || x.MergeExcelUploadId == id).CountAsync();
                            }
                        }
                        //excelUploadLog.TotalRecordImportCount = excelUploadLog.TotalRecordImportCount + Convert.ToInt16(count);
                        //excelUploadLog.TotalImportedBillAmount = excelUploadLog.TotalImportedBillAmount + Convert.ToDecimal(amount);
                        excelUploadLog.UpdatedBy = userId;
                        excelUploadLog.UpdatedDate = DateTime.Now;
                        _dbTeleBilling_V01Context.Update(excelUploadLog);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();
                        return excelUploadLog;
                    }
                }
                               

                return excelUploadLog;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RemoveExcelUploadLog(long id)
        {
            Exceluploadlog excelUploadLog = new Exceluploadlog();
            if (id > 0)
            {
                excelUploadLog = await _dbTeleBilling_V01Context.Exceluploadlog.FindAsync(id);
                if (excelUploadLog != null)
                {

                    List<ExceluploadlogServicetype> excel_service_list = new List<ExceluploadlogServicetype>();
                    excel_service_list = await _dbTeleBilling_V01Context.ExceluploadlogServicetype.Where(x => x.ExcelUploadLogId == id).ToListAsync();
                    if(excel_service_list != null && excel_service_list.Count() >0)
                    {
                        _dbTeleBilling_V01Context.RemoveRange(excel_service_list);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();
                    }

                    _dbTeleBilling_V01Context.Remove(excelUploadLog);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        public async Task<long> AddExcelUploadLogPbx(PbxBillUploadAC billUploadModel, string fileNameGuid, long userId)
		{
			Exceluploadlogpbx excelUploadLog = new Exceluploadlogpbx();
			excelUploadLog.DeviceId = billUploadModel.DeviceId;
			excelUploadLog.Month = billUploadModel.MonthId;
			excelUploadLog.Year = billUploadModel.YearId;
			excelUploadLog.DeviceId = billUploadModel.DeviceId;
			excelUploadLog.ExcelFileName = billUploadModel.ExcelFileName1 ?? fileNameGuid;
			excelUploadLog.FileNameGuid = fileNameGuid;
			excelUploadLog.UploadBy = userId;
			excelUploadLog.UploadDate = DateTime.Now;
			excelUploadLog.IsDelete = false;
			excelUploadLog.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
			await _dbTeleBilling_V01Context.AddAsync(excelUploadLog);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			return excelUploadLog.Id;
		}

		public async Task<bool> AddExcelDetail(List<Exceldetail> excelDetailList)
		{
			try
			{
				await _dbTeleBilling_V01Context.AddRangeAsync(excelDetailList);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public async Task<bool> AddSkypeExcelDetail(List<Skypeexceldetail> excelDetailList)
		{
			try
			{
				await _dbTeleBilling_V01Context.AddRangeAsync(excelDetailList);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public async Task<bool> AddPbxExcelDetail(List<Exceldetailpbx> excelDetailList)
		{
			try
			{
				await _dbTeleBilling_V01Context.AddRangeAsync(excelDetailList);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}



        // new method to add Error data
        public async Task<bool> AddExcelDetailError(AllServiceTypeDataAC allServiceTypeData, string FileNameGuid)
        {        

            if (allServiceTypeData != null)
            {
                if (allServiceTypeData.InvalidListSkypeAllDB.Count() > 0)
                {
                    try
                    {
                        await _dbTeleBilling_V01Context.AddRangeAsync(allServiceTypeData.InvalidListSkypeAllDB);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                if (allServiceTypeData.InvalidListAllDB.Count() > 0)
                {

                    try
                    {
                        await _dbTeleBilling_V01Context.AddRangeAsync(allServiceTypeData.InvalidListAllDB);
                        await _dbTeleBilling_V01Context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }

                }
            }

            return true;

        }

        // Export  Error data | add | 03122019
        public  List<MobilityExcelUploadDetailStringAC> ExportMobilityErrorList(string fileGuidNo)
        {
            List<MobilityExcelUploadDetailStringAC> _searchDataList = new List<MobilityExcelUploadDetailStringAC>();
            try
            {
                List<ExceldetailError> datalistError = new List<ExceldetailError>();
                datalistError =  _dbTeleBilling_V01Context.ExceldetailError.Where(x => x.FileGuidNo == fileGuidNo).ToList();
                if(datalistError!=null && datalistError.Count() > 0)
                {
                    _searchDataList = _mapper.Map<List<MobilityExcelUploadDetailStringAC>>(datalistError);
                }
                return _searchDataList;

            }
            catch (Exception e)
            {
                return new List<MobilityExcelUploadDetailStringAC>();
            }

        }

        //  new Methods based on Npoi packagefor .xls and excel reader
        public async Task<ImportBillDetailAC<MobilityUploadListAC>> ReadExcelForMobility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
		{
			ImportBillDetailAC<MobilityUploadListAC> importBillDetail = new ImportBillDetailAC<MobilityUploadListAC>();
			List<Exceldetail> datalist = new List<Exceldetail>();
			List<MobilityExcelUploadDetailStringAC> datalistInvalid = new List<MobilityExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

			try
			{

				MobilityUploadListAC mobilityUploadListAC = new MobilityUploadListAC();
				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.Mobility);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				string DuractionSecondsStr = string.Empty;
				long ServiceTypeId = (long)EnumList.ServiceType.Mobility;


				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help   To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 1 : 0);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;
							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

                                    LogManager.Configuration.Variables["user"] = "";
                                    LogManager.Configuration.Variables["stepno"] = "5";
                                    _logger.Info("Start Reading data one by one and validate with database.");

                                    for (int j = readingIndex; j < rowcount; j++)
									{
										var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

										if (itemvalue == null || itemvalue.Count() == 0)
										{
											continue;
										}

										if (itemvalue.Count() > 0)
										{
											var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

											#region --> Coomon Data Variable declaration 
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											bool isBusinessTransType = false; // If type is business Call Time,Duration is not required 
											TimeSpan? _CallTime = null;
											ServiceTypeId = (long)EnumList.ServiceType.Mobility;
											string descriptionText = string.Empty;
											string descriptionTextStr = string.Empty;
											string CallTransTypeStr = string.Empty;
											string CallNumberStr = string.Empty;
											string CallAmountStr = string.Empty;
											string CallTimeStr = string.Empty;
											string CallDateStr = string.Empty;
											string CallerNameStr = string.Empty;
											string SubscriptionTypeStr = string.Empty;
											string CallDataKBStr = string.Empty;
											string MessageCountStr = string.Empty;
											#endregion
											try
											{

												// ------------- Need to check if Static ip In description -----
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress5 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex5 = getAlphabatefromIndex(Convert.ToChar(fieldAddress5.ToLower()));
													string fieldValue5 = string.Empty;
													if (filedAddressIndex5 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex5);
														var value = rowDataItems[index];
														fieldValue5 = Convert.ToString(value);
													}
													descriptionTextStr = fieldValue5;
													descriptionText = fieldValue5;

													if (!string.IsNullOrEmpty(descriptionText))
													{
														descriptionText = Regex.Replace(descriptionText, @"s", "");
														descriptionText = descriptionText.Replace(" ", "");
														descriptionText = descriptionText.Trim().ToUpper();

														string StaticIP = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_StaticIP");
														string VoiceOnly = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_VoiceOnly");
														string InternetPlanDevice = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_InternetPlannDevice");

														List<string> InternetPlannTagIds = InternetPlanDevice.Split(',').ToList();

														if (descriptionText.Contains(StaticIP))
														{
															if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.StaticIP).Count() > 0)
															{
																ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
															}
															else
															{
																csv.Dispose();

																if (File.Exists(Path.Combine(filepath, filename)))
																	File.Delete(Path.Combine(filepath, filename));

																importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
																importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
																importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

																return importBillDetail;
															}
														}

														else if (descriptionText.Contains(VoiceOnly))
														{
															if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.VoiceOnly).Count() > 0)
															{
																ServiceTypeId = (long)EnumList.ServiceType.VoiceOnly;
															}
															else
															{
																csv.Dispose();

																if (File.Exists(Path.Combine(filepath, filename)))
																	File.Delete(Path.Combine(filepath, filename));

																importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
																importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
																importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

																return importBillDetail;
															}
														}

														else if (InternetPlannTagIds.Any(w => descriptionText.Contains(w)))
														{
															if (billUploadAC.ServiceTypes.Where(x => x.Id == 6).Count() > 0)
															{
																ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
															}
															else
															{
																csv.Dispose();

																if (File.Exists(Path.Combine(filepath, filename)))
																	File.Delete(Path.Combine(filepath, filename));

																importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
																importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
																importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

																return importBillDetail;
															}
														}
													}

												}
												//--------------- End : need to check static Ip in description ----

												#region --> TransType Validation 

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
													string fieldValue0 = string.Empty;
													if (filedAddressIndex0 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex0);
														var value = rowDataItems[index];
														fieldValue0 = Convert.ToString(value);
													}

													CallTransTypeStr = fieldValue0;// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);
													Transactiontypesetting transtypeDetail = new Transactiontypesetting();

													if (!string.IsNullOrEmpty(CallTransTypeStr))
													{
														transtypeDetail = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == CallTransTypeStr.Trim().ToLower());
														if (transtypeDetail != null)
														{
															if (transtypeDetail.Id > 0 && transtypeDetail.SetTypeAs != null)
															{
																isBusinessTransType = (transtypeDetail.SetTypeAs == (int)EnumList.CallType.Business ? true : false);
															}
															else
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Trans Type does not set exists system ! ";
															}
														}
														else
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Trans Type is not defined!";
														}
													}
													else
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Trans Type doesnot exists in excel ! ";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Trans Type mapping exists ! ";
												}
												#endregion


												#region --> Call Date Required and Format Validation Part

												string fieldAddress = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDate").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex = getAlphabatefromIndex(Convert.ToChar(fieldAddress.ToLower()));
												string fieldValue = string.Empty;
												if (filedAddressIndex != null)
												{
													int index = Convert.ToInt16(filedAddressIndex);
													var value = rowDataItems[index];
													fieldValue = Convert.ToString(value);
												}
												var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
												CallDateStr = fieldValue;// getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
																		 // string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
												if (!string.IsNullOrEmpty(CallDateStr))
												{

													bool isvalidDate = true;
													isvalidDate = CheckDate(CallDateStr);

													if (!isvalidDate)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
													}


													//DateTime dt;
													//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy", "dd-MM-yyyy", "dd-MM-yyyy HH:mm:ss" };
													//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
													//}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

												}
												#endregion

												#region --> Call Time Required and Format Validation Part

												string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallTime").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
												string fieldValue1 = string.Empty;
												if (filedAddressIndex1 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex1);
													var value = rowDataItems[index];
													fieldValue1 = Convert.ToString(value);
												}


												var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
												CallTimeStr = fieldValue1;// getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
												string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
												if (!string.IsNullOrEmpty(CallTimeStr) && (CallTimeStr != "00:00:00.000000" && CallTimeStr != "00:00:00"))
												{
													bool isvalidTime = true;

													CallTimeStr = CallTimeStr.Replace(".", ":");
													isvalidTime = CheckDate(CallTimeStr);

													if (!isvalidTime)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
													}

													//DateTime dt;
													//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
													//             "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" ,"dd-MM-yyyy"};
													//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
													//}
													else
													{
														// _CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
														_CallTime = Convert.ToDateTime(CallTimeStr).TimeOfDay;

													}
												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now time not required !
														//  ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";
													}
													else
													{
														_CallTime = null;
													}

												}
												#endregion

												#region --> Call Duration Required and Format Validation Part

												string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDuration").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
												string fieldValue2 = string.Empty;
												if (filedAddressIndex2 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex2);
													var value = rowDataItems[index];
													fieldValue2 = Convert.ToString(value);
												}


												long DuractionSeconds = 0;
												string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
												var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

												string CallDurationStr = fieldValue2;// getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));
												DuractionSecondsStr = CallDurationStr;
												if (!string.IsNullOrEmpty(CallDurationStr))
												//   && (CallDurationStr != "00:00:00.000000" && CallDurationStr != "00:00:00")
												{

													if (durationFormat == "seconds")
													{
														long number;
														if (!long.TryParse(CallDurationStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															DuractionSeconds = Convert.ToInt64(CallDurationStr);
														}
													}
													else
													{
														if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
														{
															DateTime dt1 = DateTime.Parse(CallDurationStr);
															CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
														}
														//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

														DateTime dt;
														string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
														if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															TimeSpan ts = TimeSpan.Parse(CallDurationStr);
															double totalSeconds = ts.TotalSeconds;
															DuractionSeconds = Convert.ToInt64(totalSeconds);
														}
													}

												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now Duration not required !
														// ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";
													}
													else
													{
														DuractionSeconds = 0;
													}

												}
												#endregion

												#region --> Call Amount Required and Format Validation Part

												string fieldAddress3 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex3 = getAlphabatefromIndex(Convert.ToChar(fieldAddress3.ToLower()));
												string fieldValue3 = string.Empty;
												if (filedAddressIndex3 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex3);
													var value = rowDataItems[index];
													fieldValue3 = Convert.ToString(value);
												}

												CallAmountStr = fieldValue3; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
												if (!string.IsNullOrEmpty(CallAmountStr))
												{
													decimal number;
													if (!decimal.TryParse(CallAmountStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
												}
												#endregion

												#region --> Call Number Required and Format Validation Part
												string fieldAddress4 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerNumber").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex4 = getAlphabatefromIndex(Convert.ToChar(fieldAddress4.ToLower()));
												string fieldValue4 = string.Empty;
												if (filedAddressIndex4 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex4);
													var value = rowDataItems[index];
													fieldValue4 = Convert.ToString(value);
												}


												CallNumberStr = fieldValue4; // getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
												if (!string.IsNullOrEmpty(CallNumberStr))
												{

													if (!(CallNumberStr.Length > 5))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid. ";
													}
													else
													{
														Telephonenumberallocation telephoneNumber = new Telephonenumberallocation();
														telephoneNumber = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == CallNumberStr && !x.IsDelete && x.IsActive));

														if (telephoneNumber != null)
														{
															Telephonenumberallocationpackage packageData = new Telephonenumberallocationpackage();
															packageData = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage
																				.Where(x => (  // x.ServiceId == (long)EnumList.ServiceType.Mobility ||
																				x.ServiceId == ServiceTypeId)
																				&& x.TelephoneNumberAllocationId == telephoneNumber.Id && !x.IsDelete).FirstOrDefaultAsync();

															if (telephoneNumber.EmployeeId <= 0)
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not allocated to employee!";
															}

															if (packageData == null)
															{

																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Caller Number does not allocated mobility package!";

															}

														}
														else
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
														}

													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
												}
												#endregion

												#region --> MessageCount numeric Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MessageCount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress6 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MessageCount").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex6 = getAlphabatefromIndex(Convert.ToChar(fieldAddress6.ToLower()));
													string fieldValue6 = string.Empty;
													if (filedAddressIndex6 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex6);
														var value = rowDataItems[index];
														fieldValue6 = Convert.ToString(value);
													}
													MessageCountStr = fieldValue6; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
													if (!string.IsNullOrEmpty(MessageCountStr))
													{
														long number;
														if (!long.TryParse(MessageCountStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Message Count format is not valid";
														}
													}

												}

												#endregion

												#region --> CallDataKB numeric Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDataKB").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress7 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDataKB").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex7 = getAlphabatefromIndex(Convert.ToChar(fieldAddress7.ToLower()));
													string fieldValue7 = string.Empty;
													if (filedAddressIndex7 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex7);
														var value = rowDataItems[index];
														fieldValue7 = Convert.ToString(value);
													}
													CallDataKBStr = fieldValue7; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
													if (!string.IsNullOrEmpty(CallDataKBStr))
													{
														decimal number;
														if (!decimal.TryParse(CallDataKBStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " Call Data KB format is not valid";
														}
													}
												}

												#endregion

												#region --> Other Optional Fileds
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
													string fieldValue8 = string.Empty;
													if (filedAddressIndex8 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex8);
														var value = rowDataItems[index];
														fieldValue8 = Convert.ToString(value);
													}
													CallerNameStr = fieldValue8;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
													string fieldValue9 = string.Empty;
													if (filedAddressIndex9 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex9);
														var value = rowDataItems[index];
														fieldValue9 = Convert.ToString(value);
													}
													SubscriptionTypeStr = fieldValue9;
												}
												#endregion


												if (IsFullValid)
												{
													Exceldetail data = new Exceldetail();
													// --> Required Field Data--------------------
													string callTransactionType = CallTransTypeStr;
												// need to opt.
                                                    data.CallTransactionTypeId = (await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == callTransactionType.Trim().ToLower()))?.Id;
													data.TransType = callTransactionType;
													data.CallerNumber = CallNumberStr;
													data.CallAmount = Convert.ToDecimal(CallAmountStr);
													data.CallDate = Convert.ToDateTime(CallDateStr);

													if (_CallTime != null)
													{
														data.CallTime = _CallTime;
													}
													// Call duration hh:mm:ss to long convert and stored
													data.CallDuration = DuractionSeconds;

													// --> Optional Field Data--------------------
													data.Description = descriptionTextStr;
													data.CallerName = CallerNameStr;
													data.SubscriptionType = SubscriptionTypeStr;
													data.MessageCount = string.IsNullOrEmpty(MessageCountStr) ? 0 : Convert.ToInt32(MessageCountStr);
													data.CallDataKB = string.IsNullOrEmpty(CallDataKBStr) ? 0 : Convert.ToDecimal(CallDataKBStr);
													data.CallWithinGroup = false;
													data.SiteName = string.Empty;
													data.GroupDetail = string.Empty;
													data.Bandwidth = string.Empty;
													data.MonthlyPrice = null;
													data.CommentOnPrice = string.Empty;
													data.CommentOnBandwidth = string.Empty;
													data.ReceiverNumber = string.Empty;
													data.ReceiverName = string.Empty;

													data.EmployeeId = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == data.CallerNumber && !x.IsDelete && x.IsActive))?.EmployeeId;
													data.ServiceTypeId = ServiceTypeId; // (long)EnumList.ServiceType.Mobility;
													data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
													data.ExcelUploadLogId = 0;
													MstEmployee mstemp = new MstEmployee();
													mstemp = (await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == data.EmployeeId));
													if (mstemp != null)
													{
														data.BusinessUnitId = mstemp.BusinessUnitId;
														data.CostCenterId = mstemp.CostCenterId;
													}
													data.ExcelUploadLogId = 0;
													data.GroupId = null;
													data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
													data.AssignType = null;

													if (isBusinessOnly)
													{
														data.AssignType = (long)EnumList.AssignType.Business;
													}
													else
													{
														//if anount is negative than it will be always assigned to Business charge 
														if (data.CallAmount < 0)
														{
															data.AssignType = (long)EnumList.AssignType.Business;
														}
														else
														{
															#region Get Assign Type Logic from Trans Type
															if (data.CallTransactionTypeId > 0)
															{
																long? ChargeType = (await _dbTeleBilling_V01Context.Transactiontypesetting.FindAsync(data.CallTransactionTypeId))?.SetTypeAs;
																if (ChargeType > 0)
																{
																	data.AssignType = ChargeType;
																}
																else
																{
																	if (data.EmployeeId > 0)
																	{
																		data.AssignType = (long)EnumList.AssignType.Employee;
																	}
																	else if (data.BusinessUnitId > 0)
																	{
																		data.AssignType = (long)EnumList.AssignType.Business;
																	}
																}
															}
															#endregion
														}
													}


													datalist.Add(data);
												}
												else
												{
													datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
													{
														CallerName = CallerNameStr,
														CallType = CallTransTypeStr,
														Description = descriptionTextStr,
														CallerNumber = CallNumberStr,
														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														CallDataKB = CallDataKBStr,
														MessageCount = MessageCountStr,
														ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        FileGuidNo= filename,
                                                        ServiceTypeId= ServiceTypeId,
                                                        CallerName = CallerNameStr,
                                                        TransType = CallTransTypeStr,
                                                        Description = descriptionTextStr,
                                                        CallerNumber = CallNumberStr,
                                                        CallAmount = CallAmountStr,
                                                        CallDate = CallDateStr,
                                                        CallTime = CallTimeStr,
                                                        CallDuration = DuractionSecondsStr,
                                                        CallDataKb = CallDataKBStr,
                                                        MessageCount = MessageCountStr,
                                                        ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                    });

                                                }

											}
											catch (Exception e)
											{
												if (e.GetType() != typeof(System.NullReferenceException))
												{

													datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
													{
														CallerName = CallerNameStr,
														CallType = CallTransTypeStr,
														Description = descriptionTextStr,
														CallerNumber = CallNumberStr,
														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														CallDataKB = CallDataKBStr,
														MessageCount = MessageCountStr,
														ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        FileGuidNo = filename,
                                                        ServiceTypeId = ServiceTypeId,
                                                        CallerName = CallerNameStr,
                                                        TransType = CallTransTypeStr,
                                                        Description = descriptionTextStr,
                                                        CallerNumber = CallNumberStr,
                                                        CallAmount = CallAmountStr,
                                                        CallDate = CallDateStr,
                                                        CallTime = CallTimeStr,
                                                        CallDuration = DuractionSecondsStr,
                                                        CallDataKb = CallDataKBStr,
                                                        MessageCount = MessageCountStr,
                                                        ErrorSummary =  "At :" + j.ToString() + " " + "Error :" + e.Message
                                                    });
                                                }

											}

										}

									}

                                    LogManager.Configuration.Variables["user"] = "";
                                    LogManager.Configuration.Variables["stepno"] = "6";
                                    _logger.Info("Over: Reading data one by one and validate with database.");
                                }
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						}

						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets > 0 && hssfwb.NumberOfSheets <= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}

						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}

						}
						IRow headerRow = sheet.GetRow(0);
						if (headerRow == null)
						{
							headerRow = sheet.GetRow(readingIndex);
						}
						if (headerRow != null)
						{
							int cellCount = headerRow.LastCellNum;
							int rowcount = sheet.LastRowNum;

                            LogManager.Configuration.Variables["user"] = "";
                            LogManager.Configuration.Variables["stepno"] = "5";
                            _logger.Info("Start Reading data one by one and validate with database.");
                            // ---- Global Variable 
                            long CurrencyId = 0;
                            CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId ??0;
                            // ---- Global Variable 

                            for (int j = readingIndex; j <= rowcount + 1; j++)
							{
								int intIndex = 0;
								intIndex = (j > 0 ? j - 1 : j);
								IRow row = sheet.GetRow(intIndex);

                                // -------- Common variable Data ------------
                                long?_CallTransactionTypeId = 0;
                                long? _EmployeeId = 0;
                                long? _BusinessUnitId = 0;
                                long? _CostCenterId = 0;
                                long? _ChargeType = 0;
                                //------------ validate variable-----------------------

                                string _TransTypestr = string.Empty;
                                string _CallerNumberstr = string.Empty;
                                string _CallAmountStr = string.Empty;
                                string _CallDatestr = string.Empty;
                                string _CallTimestr = string.Empty;

                                // ----- End : Common Varaiable Data --------


                                if (row == null || (row.Cells.All(d => d.CellType == CellType.Blank)))
								{
									continue;
								}
								else
								{
									string CallDataKBStr = string.Empty;
									string MessageCountStr = string.Empty;

									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;
										bool isBusinessTransType = false; // If type is business Call Time,Duration is not required 
										TimeSpan? _CallTime = null;
										ServiceTypeId = (long)EnumList.ServiceType.Mobility;

										// ------------- Need to check if Static ip In description -----
										string descriptionText = string.Empty;
										DateTime _callDate = new DateTime();



										if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
										{
											descriptionText = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(descriptionText))
											{

												descriptionText = Regex.Replace(descriptionText, @"s", "");
												descriptionText = descriptionText.Replace(" ", "");
												descriptionText = descriptionText.Trim().ToUpper();

												string StaticIP = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_StaticIP");
												string VoiceOnly = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_VoiceOnly");
												string InternetPlanDevice = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_InternetPlannDevice");

												List<string> InternetPlannTagIds = InternetPlanDevice.Split(',').ToList();

												if (descriptionText.Contains(StaticIP))
												{
													if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.StaticIP).Count() > 0)
													{
														ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
													}
													else
													{
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
														importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

														return importBillDetail;
													}
												}

												else if (descriptionText.Contains(VoiceOnly))
												{
													if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.VoiceOnly).Count() > 0)
													{
														ServiceTypeId = (long)EnumList.ServiceType.VoiceOnly;
													}
													else
													{

														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
														importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

														return importBillDetail;
													}
												}

												else if (InternetPlannTagIds.Any(w => descriptionText.Contains(w)))
												{
													if (billUploadAC.ServiceTypes.Where(x => x.Id == 6).Count() > 0)
													{
														ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
													}
													else
													{
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
														importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

														return importBillDetail;
													}
												}
											}

										}

										//--------------- End : need to check static Ip in description ----

										#region --> TransType Validation 
										string CallTransTypeStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);
										Transactiontypesetting transtypeDetail = new Transactiontypesetting();
                                        _TransTypestr = CallTransTypeStr;

                                        if (!string.IsNullOrEmpty(CallTransTypeStr))
										{
                                            ///agagaggaag
											transtypeDetail = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == CallTransTypeStr.Trim().ToLower());
                                               if (transtypeDetail != null)
											{
												if (transtypeDetail.Id > 0 && transtypeDetail.SetTypeAs != null)
												{
                                                    _CallTransactionTypeId = transtypeDetail.Id;
                                                    _ChargeType = transtypeDetail.SetTypeAs;

                                                    isBusinessTransType = (transtypeDetail.SetTypeAs == (int)EnumList.CallType.Business ? true : false);
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Trans Type does not set exists system ! ";
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Trans Type is not defined!";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Trans Type doesnot exists in excel ! ";
										}

										#endregion

										#region --> Call Date Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
										string CallDateStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
                                        //  string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
                                        _CallDatestr = CallDateStr;

                                        if (!string.IsNullOrEmpty(CallDateStr))
										{

											bool isvalidDate = true;
											isvalidDate = CheckDate(CallDateStr);

											if (!isvalidDate)
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
											}

											//DateTime dt;
											//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy" };
											//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
											//{
											//    IsFullValid = false;
											//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
											//}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

										}
										#endregion

										#region --> Call Time Required and Format Validation Part

										var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
										string CallTimeStr = getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
                                        // string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
                                        _CallTimestr = CallTimeStr;
										if (!string.IsNullOrEmpty(CallTimeStr) && (CallTimeStr != "00:00:00.000000" && CallTimeStr != "00:00:00"))
										{
											bool isvalidTime = true;
											isvalidTime = CheckDate(CallTimeStr);

											if (!isvalidTime)
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
											}
											else
											{
												_CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;

											}


											//DateTime dt;
											//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
											//                 "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
											//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
											//{
											//    IsFullValid = false;
											//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
											//}
											//else
											//{
											//    _CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
											//}
										}
										else
										{
											if (!isBusinessTransType)
											{
												// IsFullValid = false; from now time not required !
												//  ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";
											}
											else
											{
												_CallTime = null;
											}

										}
										#endregion

										#region --> Call Duration Required and Format Validation Part
										long DuractionSeconds = 0;
										string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
										var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

										string CallDurationStr = getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));
										DuractionSecondsStr = CallDurationStr;
										if (!string.IsNullOrEmpty(CallDurationStr))
										//   && (CallDurationStr != "00:00:00.000000" && CallDurationStr != "00:00:00")
										{

											if (durationFormat == "seconds")
											{
												long number;
												if (!long.TryParse(CallDurationStr, out number))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
												}
												else
												{
													DuractionSeconds = Convert.ToInt64(CallDurationStr);
												}
											}
											else
											{
												if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
												{
													DateTime dt1 = DateTime.Parse(CallDurationStr);
													CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
												}
												//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

												DateTime dt;
												string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
												if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
												}
												else
												{
													TimeSpan ts = TimeSpan.Parse(CallDurationStr);
													double totalSeconds = ts.TotalSeconds;
													DuractionSeconds = Convert.ToInt64(totalSeconds);
												}
											}

										}
										else
										{
											if (!isBusinessTransType)
											{
												// IsFullValid = false; from now Duration not required !
												// ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";
											}
											else
											{
												DuractionSeconds = 0;
											}

										}
										#endregion

										#region --> Call Amount Required and Format Validation Part

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
                                        //string CallAmountStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value.ToString());
                                        _CallAmountStr = CallAmountStr;
                                        if (!string.IsNullOrEmpty(CallAmountStr))
										{
											decimal number;
											if (!decimal.TryParse(CallAmountStr, out number))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
										}
										#endregion

										#region --> Call Number Required and Format Validation Part
										string CallNumberStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
                                        //string CallNumberStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value.ToString());

                                        _CallerNumberstr = CallNumberStr;
                                        if (!string.IsNullOrEmpty(CallNumberStr))
										{

											if (!(CallNumberStr.Length > 5))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid. ";
											}
											else
											{
												Telephonenumberallocation telephoneNumber = new Telephonenumberallocation();
												telephoneNumber = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == CallNumberStr && !x.IsDelete && x.IsActive));

												if (telephoneNumber != null)
												{
													Telephonenumberallocationpackage packageData = new Telephonenumberallocationpackage();
													packageData = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage
																		   .Where(x => (x.ServiceId == ServiceTypeId)
																		   && x.TelephoneNumberAllocationId == telephoneNumber.Id && !x.IsDelete).FirstOrDefaultAsync();

                                                    if (telephoneNumber.EmployeeId > 0 && packageData!=null && packageData.Id >0)
                                                    {
                                                        _EmployeeId = telephoneNumber.EmployeeId;
                                                        MstEmployee mstemp = new MstEmployee();
                                                        mstemp = (await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == telephoneNumber.EmployeeId));
                                                        if (mstemp != null)
                                                        {
                                                            _BusinessUnitId = mstemp.BusinessUnitId;
                                                            _CostCenterId = mstemp.CostCenterId;
                                                        }
                                                    }

													if (telephoneNumber.EmployeeId <= 0)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not allocated to employee!";
													}

													if (packageData == null)
													{

														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Caller Number does not allocated mobility package!";

													}

												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
												}

											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
										}
										#endregion

										#region --> MessageCount numeric Format Validation Part
										if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MessageCount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
										{
											MessageCountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MessageCount", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(MessageCountStr))
											{
												long number;
												if (!long.TryParse(MessageCountStr, out number))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Message Count format is not valid";
												}
											}

										}

										#endregion

										#region --> CallDataKB numeric Format Validation Part
										if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDataKB").Select(x => x.ExcelcolumnName).FirstOrDefault()))
										{
											CallDataKBStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDataKB", j), sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(CallDataKBStr))
											{
												decimal number;
												if (!decimal.TryParse(CallDataKBStr, out number))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Call Data KB format is not valid";
												}
											}
										}

										#endregion


										if (IsFullValid)
										{
											Exceldetail data = new Exceldetail();
                                            // --> Required Field Data--------------------
                                             data.CallTransactionTypeId = _CallTransactionTypeId;// (await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == callTransactionType.Trim().ToLower()))?.Id;
											data.TransType = _TransTypestr;
                                            data.CallerNumber = _CallerNumberstr;// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);

                                            data.CallAmount = string.IsNullOrEmpty(_CallAmountStr) ? 0 : Convert.ToDecimal(_CallAmountStr);
                                            data.CallDate =  string.IsNullOrEmpty(_CallDatestr) ? data.CallDate : Convert.ToDateTime(_CallDatestr);
                                          
											if (_CallTime != null)
											{
                                                data.CallTime = _CallTime;// Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
											}
											// Call duration hh:mm:ss to long convert and stored
											data.CallDuration = DuractionSeconds;

											// --> Optional Field Data--------------------

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												data.CallerName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet, (long)EnumList.SupportDataType.String);
											}

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												data.Description = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet, (long)EnumList.SupportDataType.String);
											}
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												data.SubscriptionType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SubscriptionType", j), sheet, (long)EnumList.SupportDataType.String);
											}

											data.MessageCount = string.IsNullOrEmpty(MessageCountStr) ? 0 : Convert.ToInt32(MessageCountStr);
											data.CallDataKB = string.IsNullOrEmpty(CallDataKBStr) ? 0 : Convert.ToDecimal(CallDataKBStr);

											data.CallWithinGroup = false;
											data.SiteName = string.Empty;
											data.GroupDetail = string.Empty;
											data.Bandwidth = string.Empty;
											data.MonthlyPrice = null;
											data.CommentOnPrice = string.Empty;
											data.CommentOnBandwidth = string.Empty;
											data.ReceiverNumber = string.Empty;
											data.ReceiverName = string.Empty;

                                            data.EmployeeId = _EmployeeId;// (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == data.CallerNumber && !x.IsDelete && x.IsActive))?.EmployeeId;
											data.ServiceTypeId = ServiceTypeId; // (long)EnumList.ServiceType.Mobility;
                                            data.CurrencyId = CurrencyId;// (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
											data.ExcelUploadLogId = 0;
											data.BusinessUnitId = _BusinessUnitId;
										    data.CostCenterId = _CostCenterId;										
											data.ExcelUploadLogId = 0;
											data.GroupId = null;
											data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
											data.AssignType = null;

											if (isBusinessOnly)
											{
												data.AssignType = (long)EnumList.AssignType.Business;
											}
											else
											{
												//if anount is negative than it will be always assigned to Business charge 
												if (data.CallAmount < 0)
												{
													data.AssignType = (long)EnumList.AssignType.Business;
												}
												else
												{
													#region Get Assign Type Logic from Trans Type
													if (data.CallTransactionTypeId > 0)
													{
														if (_ChargeType > 0)
														{
															data.AssignType = _ChargeType;
														}
														else
														{
															if (data.EmployeeId > 0)
															{
																data.AssignType = (long)EnumList.AssignType.Employee;
															}
															else if (data.BusinessUnitId > 0)
															{
																data.AssignType = (long)EnumList.AssignType.Business;
															}
														}
													}
													#endregion
												}
											}



											datalist.Add(data);
										}
										else
										{
											string dDescription = string.Empty;
											string dCallername = string.Empty;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);

											}
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dCallername = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet);
											}

											datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
											{
												CallerName = dCallername, //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet),
												CallType =_TransTypestr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet),
												Description = dDescription,
												CallerNumber =_CallerNumberstr,//   getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
												CallAmount = _CallAmountStr ,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
												CallDate =_CallDatestr ,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
												CallTime = _CallTimestr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
												CallDuration = DuractionSecondsStr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
												CallDataKB = CallDataKBStr,
												MessageCount = MessageCountStr,
												ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                FileGuidNo = filename,
                                                ServiceTypeId = ServiceTypeId,
                                                CallerName = dCallername,
                                                TransType = _TransTypestr,
                                                Description = dDescription,
                                                CallerNumber = _CallerNumberstr,
                                                CallAmount = _CallAmountStr,
                                                CallDate = _CallDatestr,
                                                CallTime = _CallTimestr,
                                                CallDuration = DuractionSecondsStr,
                                                CallDataKb = CallDataKBStr,
                                                MessageCount = MessageCountStr,
                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });

                                        }
									}
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											string dDescription = string.Empty;
											string dCallername = string.Empty;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);
											}
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dCallername = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet);
											}

											datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
											{
												CallerName = dCallername, //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet),
												CallType =  _TransTypestr ,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet),
												Description = dDescription,
                                                CallerNumber = _CallerNumberstr,//   getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
                                                CallAmount = _CallAmountStr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
                                                CallDate = _CallDatestr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
                                                CallTime = _CallTimestr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
                                                CallDuration = DuractionSecondsStr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
                                                CallDataKB = CallDataKBStr,
                                                MessageCount = MessageCountStr,
												ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                FileGuidNo = filename,
                                                ServiceTypeId = ServiceTypeId,
                                                CallerName = dCallername,
                                                TransType = _TransTypestr,
                                                Description = dDescription,
                                                CallerNumber = _CallerNumberstr,
                                                CallAmount = _CallAmountStr,
                                                CallDate = _CallDatestr,
                                                CallTime = _CallTimestr,
                                                CallDuration = DuractionSecondsStr,
                                                CallDataKb = CallDataKBStr,
                                                MessageCount = MessageCountStr,
                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
									}
								}


							}

                            LogManager.Configuration.Variables["user"] = "";
                            LogManager.Configuration.Variables["stepno"] = "6";
                            _logger.Info("Over: Reading data one by one and validate with database.");
                        }

					}
				} // end of file format reader

				mobilityUploadListAC.InvalidMobilityList = datalistInvalid;
				mobilityUploadListAC.ValidMobilityList = datalist;
                mobilityUploadListAC.InvalidListAllDB = datalistError;


                ResponseDynamicDataAC<MobilityUploadListAC> responseData = new ResponseDynamicDataAC<MobilityUploadListAC>();
				responseData.Data = mobilityUploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
                    // File.Delete(Path.Combine(filepath, filename));

                    #endregion

                LogManager.Configuration.Variables["user"] = "";
                LogManager.Configuration.Variables["stepno"] = "7";
                _logger.Info("REPO :Return valid & invalid list");

                return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}
		}

		public async Task<ImportBillDetailAC<MadaUploadListAC>> ReadExcelForMadaService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
		{
			ImportBillDetailAC<MadaUploadListAC> importBillDetail = new ImportBillDetailAC<MadaUploadListAC>();
			List<Exceldetail> datalist = new List<Exceldetail>();
			List<MadaExcelUploadDetailStringAC> datalistInvalid = new List<MadaExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

            try
			{
				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.GeneralServiceMada);
				long ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada;
				MadaUploadListAC madaUploadListAC = new MadaUploadListAC();

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);


				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 1 : 0);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;
							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();
									for (int j = readingIndex; j < rowcount; j++)
									{
										var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

										if (itemvalue == null || itemvalue.Count() == 0)
										{
											continue;
										}

										if (itemvalue.Count() > 0)
										{
											var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

											#region --> Coomon Data Variable declaration 
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											string CallAmountStr = string.Empty;
											string MonthlyPriceStr = string.Empty;
											string FinalAnnualChargesKDStr = string.Empty;
											string InitialDiscountedMonthlyPriceKDStr = string.Empty;
											string InitialDiscountedAnnualPriceKDStr = string.Empty;
											string InitialDiscountedSavingMonthlyKDStr = string.Empty;
											string InitialDiscountedSavingYearlyKDStr = string.Empty;
											string SiteNameStr = string.Empty;
											string BandwidthStr = string.Empty;
											string ServiceDetailStr = string.Empty;
											string CostCentreStr = string.Empty;

											#endregion
											try
											{
												#region --> Site Name Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
													string fieldValue0 = string.Empty;
													if (filedAddressIndex0 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex0);
														var value = rowDataItems[index];
														fieldValue0 = Convert.ToString(value);
													}

													SiteNameStr = fieldValue0;
													if (string.IsNullOrEmpty(SiteNameStr))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
													}

												}
												else
												{
													csv.Dispose();
													if (File.Exists(Path.Combine(filepath, filename)))
														File.Delete(Path.Combine(filepath, filename));

													importBillDetail.Message = "Site Name mapping doesnot exists !";
													importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
													return importBillDetail;
												}
												#endregion

												#region --> Bandwidth Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
													string fieldValue01 = string.Empty;
													if (filedAddressIndex01 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex01);
														var value = rowDataItems[index];
														fieldValue01 = Convert.ToString(value);
													}

													BandwidthStr = fieldValue01;
													if (string.IsNullOrEmpty(BandwidthStr))
													{
														if (BandwidthStr.GetType() != typeof(string))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
														}
													}

												}
												else
												{
													csv.Dispose();
													if (File.Exists(Path.Combine(filepath, filename)))
														File.Delete(Path.Combine(filepath, filename));

													importBillDetail.Message = "Bandwidth mapping doesnot exists !";
													importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
													return importBillDetail;
												}
												#endregion

												#region --> Price Validatition Required and Format Validation Part
												string PriceStr = string.Empty;
												string Message = string.Empty;
												Type valueType;

												#region --> Call Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
													string fieldValue1 = string.Empty;
													if (filedAddressIndex1 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex1);
														var value = rowDataItems[index];
														fieldValue1 = Convert.ToString(value);
													}

													CallAmountStr = fieldValue1;
													if (!string.IsNullOrEmpty(CallAmountStr))
													{
														PriceStr = CallAmountStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
													else
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
													}
												}
												else
												{
													csv.Dispose();
													if (File.Exists(Path.Combine(filepath, filename)))
														File.Delete(Path.Combine(filepath, filename));

													importBillDetail.Message = "Call Amount mapping doesnot exists !";
													importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
													return importBillDetail;
												}
												#endregion

												#region --> Monthly Price Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
													string fieldValue2 = string.Empty;
													if (filedAddressIndex2 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex2);
														var value = rowDataItems[index];
														fieldValue2 = Convert.ToString(value);
													}

													MonthlyPriceStr = fieldValue2;
													if (!string.IsNullOrEmpty(MonthlyPriceStr))
													{
														PriceStr = MonthlyPriceStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												#region -->FinalAnnualChargesKD Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "FinalAnnualChargesKD").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress3 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "FinalAnnualChargesKD").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex3 = getAlphabatefromIndex(Convert.ToChar(fieldAddress3.ToLower()));
													string fieldValue3 = string.Empty;
													if (filedAddressIndex3 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex3);
														var value = rowDataItems[index];
														fieldValue3 = Convert.ToString(value);
													}

													FinalAnnualChargesKDStr = fieldValue3;
													if (!string.IsNullOrEmpty(FinalAnnualChargesKDStr))
													{
														PriceStr = FinalAnnualChargesKDStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("FinalAnnualChargesKD", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												#region -->InitialDiscountedMonthlyPriceKD Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedMonthlyPriceKD").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress4 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedMonthlyPriceKD").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex4 = getAlphabatefromIndex(Convert.ToChar(fieldAddress4.ToLower()));
													string fieldValue4 = string.Empty;
													if (filedAddressIndex4 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex4);
														var value = rowDataItems[index];
														fieldValue4 = Convert.ToString(value);
													}

													InitialDiscountedMonthlyPriceKDStr = fieldValue4;
													if (!string.IsNullOrEmpty(InitialDiscountedMonthlyPriceKDStr))
													{
														PriceStr = InitialDiscountedMonthlyPriceKDStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("InitialDiscountedMonthlyPriceKD", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												#region -->InitialDiscountedAnnualPriceKD Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedAnnualPriceKD").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress5 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedAnnualPriceKD").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex5 = getAlphabatefromIndex(Convert.ToChar(fieldAddress5.ToLower()));
													string fieldValue5 = string.Empty;
													if (filedAddressIndex5 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex5);
														var value = rowDataItems[index];
														fieldValue5 = Convert.ToString(value);
													}

													InitialDiscountedAnnualPriceKDStr = fieldValue5;
													if (!string.IsNullOrEmpty(InitialDiscountedAnnualPriceKDStr))
													{
														PriceStr = InitialDiscountedAnnualPriceKDStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("InitialDiscountedAnnualPriceKD", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												#region -->InitialDiscountedSavingMonthlyKD Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedSavingMonthlyKD").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress6 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedSavingMonthlyKD").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex6 = getAlphabatefromIndex(Convert.ToChar(fieldAddress6.ToLower()));
													string fieldValue6 = string.Empty;
													if (filedAddressIndex6 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex6);
														var value = rowDataItems[index];
														fieldValue6 = Convert.ToString(value);
													}

													InitialDiscountedSavingMonthlyKDStr = fieldValue6;
													if (!string.IsNullOrEmpty(InitialDiscountedSavingMonthlyKDStr))
													{
														PriceStr = InitialDiscountedSavingMonthlyKDStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("InitialDiscountedSavingMonthlyKD", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												#region -->InitialDiscountedSavingYearlyKD Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedSavingYearlyKD").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress7 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "InitialDiscountedSavingYearlyKD").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex7 = getAlphabatefromIndex(Convert.ToChar(fieldAddress7.ToLower()));
													string fieldValue7 = string.Empty;
													if (filedAddressIndex7 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex7);
														var value = rowDataItems[index];
														fieldValue7 = Convert.ToString(value);
													}

													InitialDiscountedSavingYearlyKDStr = fieldValue7;
													if (!string.IsNullOrEmpty(InitialDiscountedSavingYearlyKDStr))
													{
														PriceStr = InitialDiscountedSavingYearlyKDStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("InitialDiscountedSavingYearlyKD", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												// end of price validation region here
												#endregion

												#region --> Other Optional Fileds
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ServiceDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ServiceDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
													string fieldValue8 = string.Empty;
													if (filedAddressIndex8 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex8);
														var value = rowDataItems[index];
														fieldValue8 = Convert.ToString(value);
													}
													ServiceDetailStr = fieldValue8;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CostCentre").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CostCentre").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
													string fieldValue9 = string.Empty;
													if (filedAddressIndex9 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex9);
														var value = rowDataItems[index];
														fieldValue9 = Convert.ToString(value);
													}
													CostCentreStr = fieldValue9;
												}
												#endregion


												if (IsFullValid)
												{
													Exceldetail data = new Exceldetail();
													data.SiteName = SiteNameStr;
													data.ServiceDetail = ServiceDetailStr;
													data.Bandwidth = BandwidthStr;
													data.CostCentre = CostCentreStr;
													data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
													data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
													data.FinalAnnualChargesKd = (string.IsNullOrEmpty(FinalAnnualChargesKDStr) ? 0 : Convert.ToDecimal(FinalAnnualChargesKDStr));
													data.InitialDiscountedMonthlyPriceKd = (string.IsNullOrEmpty(InitialDiscountedMonthlyPriceKDStr) ? 0 : Convert.ToDecimal(InitialDiscountedMonthlyPriceKDStr));
													data.InitialDiscountedAnnualPriceKd = (string.IsNullOrEmpty(InitialDiscountedAnnualPriceKDStr) ? 0 : Convert.ToDecimal(InitialDiscountedAnnualPriceKDStr));
													data.InitialDiscountedSavingMonthlyKd = (string.IsNullOrEmpty(InitialDiscountedSavingMonthlyKDStr) ? 0 : Convert.ToDecimal(InitialDiscountedSavingMonthlyKDStr));
													data.InitialDiscountedSavingYearlyKd = (string.IsNullOrEmpty(InitialDiscountedSavingYearlyKDStr) ? 0 : Convert.ToDecimal(InitialDiscountedSavingYearlyKDStr));

													data.ExcelUploadLogId = 0;
													data.ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada;
													data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
													data.GroupId = null;
													data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
													datalist.Add(data);
												}
												else
												{
													datalistInvalid.Add(new MadaExcelUploadDetailStringAC
													{
														SiteName = SiteNameStr,
														ServiceDetail = ServiceDetailStr,
														Bandwidth = BandwidthStr,
														CostCentre = CostCentreStr,
														MonthlyPrice = MonthlyPriceStr,
														FinalAnnualChargesKd = FinalAnnualChargesKDStr,
														InitialDiscountedMonthlyPriceKd = InitialDiscountedMonthlyPriceKDStr,
														InitialDiscountedAnnualPriceKd = InitialDiscountedAnnualPriceKDStr,
														InitialDiscountedSavingMonthlyKd = InitialDiscountedSavingMonthlyKDStr,
														InitialDiscountedSavingYearlyKd = InitialDiscountedSavingYearlyKDStr,
														CallAmount = CallAmountStr,
														ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId=0,
                                                        FileGuidNo=filename,
                                                        ServiceTypeId= (long)EnumList.ServiceType.GeneralServiceMada,
                                                        SiteName = SiteNameStr,
                                                        ServiceDetail = ServiceDetailStr,
                                                        Bandwidth = BandwidthStr,
                                                        CostCentre = CostCentreStr,
                                                        MonthlyPrice = MonthlyPriceStr,
                                                        FinalAnnualChargesKd = FinalAnnualChargesKDStr,
                                                        InitialDiscountedMonthlyPriceKd = InitialDiscountedMonthlyPriceKDStr,
                                                        InitialDiscountedAnnualPriceKd = InitialDiscountedAnnualPriceKDStr,
                                                        InitialDiscountedSavingMonthlyKd = InitialDiscountedSavingMonthlyKDStr,
                                                        InitialDiscountedSavingYearlyKd = InitialDiscountedSavingYearlyKDStr,
                                                        CallAmount = CallAmountStr,

                                                        ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                    });
                                                }

											}
											catch (Exception e)
											{
												if (e.GetType() != typeof(System.NullReferenceException))
												{
													datalistInvalid.Add(new MadaExcelUploadDetailStringAC
													{
														SiteName = SiteNameStr,
														ServiceDetail = ServiceDetailStr,
														Bandwidth = BandwidthStr,
														CostCentre = CostCentreStr,
														MonthlyPrice = MonthlyPriceStr,
														FinalAnnualChargesKd = FinalAnnualChargesKDStr,
														InitialDiscountedMonthlyPriceKd = InitialDiscountedMonthlyPriceKDStr,
														InitialDiscountedAnnualPriceKd = InitialDiscountedAnnualPriceKDStr,
														InitialDiscountedSavingMonthlyKd = InitialDiscountedSavingMonthlyKDStr,
														InitialDiscountedSavingYearlyKd = InitialDiscountedSavingYearlyKDStr,
														CallAmount = CallAmountStr,
														ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        FileGuidNo = filename,
                                                        ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada,
                                                        SiteName = SiteNameStr,
                                                        ServiceDetail = ServiceDetailStr,
                                                        Bandwidth = BandwidthStr,
                                                        CostCentre = CostCentreStr,
                                                        MonthlyPrice = MonthlyPriceStr,
                                                        FinalAnnualChargesKd = FinalAnnualChargesKDStr,
                                                        InitialDiscountedMonthlyPriceKd = InitialDiscountedMonthlyPriceKDStr,
                                                        InitialDiscountedAnnualPriceKd = InitialDiscountedAnnualPriceKDStr,
                                                        InitialDiscountedSavingMonthlyKd = InitialDiscountedSavingMonthlyKDStr,
                                                        InitialDiscountedSavingYearlyKd = InitialDiscountedSavingYearlyKDStr,
                                                        CallAmount = CallAmountStr,

                                                        ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                    });
                                                }

											}

										}

									}

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);

						}

						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						IRow headerRow = sheet.GetRow(0);

						if (headerRow != null)
						{
							int cellCount = headerRow.LastCellNum;
							int rowcount = sheet.LastRowNum;
							for (int j = readingIndex; j <= rowcount + 1; j++)
							{
								int intIndex = 0;
								intIndex = (j > 0 ? j - 1 : j);
								IRow row = sheet.GetRow(intIndex);

								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}
								else
								{
									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;


										#region --> Site Name Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
										string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
										if (string.IsNullOrEmpty(SiteNameStr))
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";
										}

										#endregion

										#region --> Bandwidth  Validation Part

										var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
										string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
										if (string.IsNullOrEmpty(BandwidthStr))
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Bandwidth doesnot exists ! ";
										}

										#endregion

										#region --> Price Validatition Required and Format Validation Part
										string PriceStr = string.Empty;
										string Message = string.Empty;
										Type valueType;

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
										if (Message != "valid")
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " " + Message;
										}

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										Message = checkPriceValidation("FinalAnnualChargesKD", PriceStr, valueType, false);
										if (Message != "valid")
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " " + Message;
										}

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										Message = checkPriceValidation("InitialDiscountedAnnualPriceKD", PriceStr, valueType, false);
										if (Message != "valid")
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " " + Message;
										}

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										Message = checkPriceValidation("InitialDiscountedMonthlyPriceKD", PriceStr, valueType, false);
										if (Message != "valid")
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " " + Message;
										}

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										Message = checkPriceValidation("InitialDiscountedSavingMonthlyKD", PriceStr, valueType, false);
										if (Message != "valid")
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " " + Message;
										}

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										Message = checkPriceValidation("InitialDiscountedSavingYearlyKD", PriceStr, valueType, false);
										if (Message != "valid")
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " " + Message;
										}

										#endregion

										//Addede On : 2019-10-14
										#region --> Call Amount Required and Format Validation Part  

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

										valueType = CallAmountStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";
										}

										#endregion


										if (IsFullValid)
										{
											Exceldetail data = new Exceldetail();
											// --> Required Field Data--------------------
											data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
											data.ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String);
											data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
											data.CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String);

											data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
											data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));


											data.InitialDiscountedAnnualPriceKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number));
											data.InitialDiscountedMonthlyPriceKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number));
											data.InitialDiscountedSavingMonthlyKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number));
											data.InitialDiscountedSavingYearlyKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number));
											data.FinalAnnualChargesKd = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number));

											data.ExcelUploadLogId = 0;
											data.ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada;
											data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
											data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
											//  data.AssignType = (long)EnumList.AssignType.Business;
											data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);

											datalist.Add(data);
										}
										else
										{

											datalistInvalid.Add(new MadaExcelUploadDetailStringAC
											{
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedAnnualPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedMonthlyPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedSavingMonthlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedSavingYearlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												FinalAnnualChargesKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number)),

												ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                FileGuidNo = filename,
                                                ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada,

                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedAnnualPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedMonthlyPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedSavingMonthlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedSavingYearlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                FinalAnnualChargesKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number)),


                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });

                                        }
									}
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											datalistInvalid.Add(new MadaExcelUploadDetailStringAC
											{
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedAnnualPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedMonthlyPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedSavingMonthlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												InitialDiscountedSavingYearlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
												FinalAnnualChargesKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number)),

												ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                FileGuidNo = filename,
                                                ServiceTypeId = (long)EnumList.ServiceType.GeneralServiceMada,

                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                ServiceDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ServiceDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                CostCentre = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CostCentre", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedAnnualPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedAnnualPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedMonthlyPriceKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedMonthlyPriceKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedSavingMonthlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingMonthlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                InitialDiscountedSavingYearlyKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "InitialDiscountedSavingYearlyKD", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                FinalAnnualChargesKd = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "FinalAnnualChargesKD", j), sheet, (long)EnumList.SupportDataType.Number)),


                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });

                                        }
									}
								}


							}
						}

					}
				}
				madaUploadListAC.InvalidList = datalistInvalid;
				madaUploadListAC.ValidList = datalist;
                madaUploadListAC.InvalidListAllDB = datalistError;

				ResponseDynamicDataAC<MadaUploadListAC> responseData = new ResponseDynamicDataAC<MadaUploadListAC>();
				responseData.Data = madaUploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}

		public async Task<ImportBillDetailAC<InternetServiceUploadListAC>> ReadExcelForInternetService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
		{
			ImportBillDetailAC<InternetServiceUploadListAC> importBillDetail = new ImportBillDetailAC<InternetServiceUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<InternetServiceExcelUploadDetailStringAC> datalistInvalid = new List<InternetServiceExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

			try
			{
				InternetServiceUploadListAC UploadListAC = new InternetServiceUploadListAC();

				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.InternetService);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					string TitleHeader = string.Empty;
					string TitleReadingColumn = string.Empty;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
						TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
						TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;

							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

									#region --> Find Actual reading Index from Title Address Index
									if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
									{
										if (TitleReadingColumn.All(Char.IsLetter))
										{
											for (int j = readingIndex; j <= rowcount + 1; j++)
											{
												int intIndex = 0;
												intIndex = (j > 0 ? j - 1 : j);

												int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);
												var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

												if (itemvalue == null || itemvalue.Count() == 0)
												{
													continue;
												}
												else
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.ToLower()));
													string titleValue = string.Empty;
													if (titleAddressIndex != null)
													{
														int index = Convert.ToInt16(titleAddressIndex);
														var value = rowDataItems[index];
														titleValue = Convert.ToString(value);
													}
													if (titleValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
													{
														readingIndex = intIndex + 1;
														break;
													}
												}
											}
										}
										else if (TitleReadingColumn.All(Char.IsLetterOrDigit) && TitleReadingColumn.Length > 1)
										{
											//var dynCR = new CellReference(TitleReadingColumn);
											//IRow row = sheet.GetRow(dynCR.Row);
											//var cell = row.GetCell(dynCR.Col);
											//string strValue = Convert.ToString(row.GetCell(dynCR.Col));
											//if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
											//{
											//    readingIndex = dynCR.Row + 1;
											//}

											string indexStr = "0";
											indexStr = string.Join("", TitleReadingColumn.ToCharArray().Where(Char.IsDigit));
											int indexINT = Convert.ToInt32(indexStr);
											indexINT = indexINT == 0 ? indexINT : indexINT - 1;

											for (int j = indexINT - 1; j <= indexINT; j++)
											{
												var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

												if (itemvalue == null || itemvalue.Count() == 0)
												{
													continue;
												}

												if (itemvalue.Count() > 0)
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.Substring(0, 1).ToLower()));
													string titleValue = string.Empty;
													if (titleAddressIndex != null)
													{
														int index = Convert.ToInt16(titleAddressIndex);
														var value = rowDataItems[index];
														titleValue = Convert.ToString(value);
													}
													if (titleValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
													{
														readingIndex = indexINT + 1;
														break;
													}
												}
											}
										}


										#endregion


										for (int j = readingIndex; j < rowcount; j++)
										{
											var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

											if (itemvalue == null || itemvalue.Count() == 0)
											{
												continue;
											}

											if (itemvalue.Count() > 0)
											{
												var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

												#region --> Coomon Data Variable declaration 
												bool IsFullValid = true;
												string ErrorMessageSummary = string.Empty;
												string CallAmountStr = string.Empty;
												string MonthlyPriceStr = string.Empty;
												string SiteNameStr = string.Empty;
												string BandwidthStr = string.Empty;
												string GroupDetailStr = string.Empty;
												string BusinessUnitStr = string.Empty;
												string CommentOnPriceStr = string.Empty;
												string CommentOnBandwidthStr = string.Empty;

												#endregion
												try
												{
													#region --> Site Name Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
														string fieldValue0 = string.Empty;
														if (filedAddressIndex0 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex0);
															var value = rowDataItems[index];
															fieldValue0 = Convert.ToString(value);
														}

														SiteNameStr = fieldValue0;
														if (string.IsNullOrEmpty(SiteNameStr))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
														}

													}
													else
													{
														csv.Dispose();
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Site Name mapping doesnot exists !";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
														return importBillDetail;
													}
													#endregion

													#region --> Bandwidth Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
														string fieldValue01 = string.Empty;
														if (filedAddressIndex01 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex01);
															var value = rowDataItems[index];
															fieldValue01 = Convert.ToString(value);
														}

														BandwidthStr = fieldValue01;
														if (string.IsNullOrEmpty(BandwidthStr))
														{
															if (BandwidthStr.GetType() != typeof(string))
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
															}
														}

													}
													else
													{
														csv.Dispose();
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Bandwidth mapping doesnot exists !";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
														return importBillDetail;
													}
													#endregion

													#region --> Price Validatition Required and Format Validation Part
													string PriceStr = string.Empty;
													string Message = string.Empty;
													Type valueType;

													#region --> Call Amount Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
														string fieldValue1 = string.Empty;
														if (filedAddressIndex1 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex1);
															var value = rowDataItems[index];
															fieldValue1 = Convert.ToString(value);
														}

														CallAmountStr = fieldValue1;
														if (!string.IsNullOrEmpty(CallAmountStr))
														{
															PriceStr = CallAmountStr;
															valueType = PriceStr.GetType();
															Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
															if (Message != "valid")
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + " " + Message;
															}
														}
														else
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
														}
													}
													else
													{
														csv.Dispose();
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Call Amount mapping doesnot exists !";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
														return importBillDetail;
													}
													#endregion

													#region --> Monthly Price Amount Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
														string fieldValue2 = string.Empty;
														if (filedAddressIndex2 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex2);
															var value = rowDataItems[index];
															fieldValue2 = Convert.ToString(value);
														}

														MonthlyPriceStr = fieldValue2;
														if (!string.IsNullOrEmpty(MonthlyPriceStr))
														{
															PriceStr = MonthlyPriceStr;
															valueType = PriceStr.GetType();
															Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
															if (Message != "valid")
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + " " + Message;
															}
														}
													}
													#endregion

													// end of price validation region here
													#endregion

													#region --> Other Optional Fileds
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
														string fieldValue8 = string.Empty;
														if (filedAddressIndex8 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex8);
															var value = rowDataItems[index];
															fieldValue8 = Convert.ToString(value);
														}
														GroupDetailStr = fieldValue8;
													}

													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
														string fieldValue9 = string.Empty;
														if (filedAddressIndex9 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex9);
															var value = rowDataItems[index];
															fieldValue9 = Convert.ToString(value);
														}
														BusinessUnitStr = fieldValue9;
													}

													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
														string fieldValue10 = string.Empty;
														if (filedAddressIndex10 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex10);
															var value = rowDataItems[index];
															fieldValue10 = Convert.ToString(value);
														}
														CommentOnPriceStr = fieldValue10;
													}

													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
														string fieldValue11 = string.Empty;
														if (filedAddressIndex11 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex11);
															var value = rowDataItems[index];
															fieldValue11 = Convert.ToString(value);
														}
														CommentOnBandwidthStr = fieldValue11;
													}
													#endregion


													if (IsFullValid)
													{
														Exceldetail data = new Exceldetail();
														data.SiteName = SiteNameStr;
														data.GroupDetail = GroupDetailStr;
														data.Bandwidth = BandwidthStr;
														data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
														data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
														data.BusinessUnit = BusinessUnitStr;
														data.CommentOnBandwidth = CommentOnBandwidthStr;
														data.CommentOnPrice = CommentOnPriceStr;

														data.ExcelUploadLogId = 0;
														data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
														data.CurrencyId = 1;
														long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
														data.CurrencyId = providerCurrencyId;

														data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
														// data.AssignType = (long)EnumList.AssignType.Business;
														data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
														datalist.Add(data);
													}
													else
													{
														datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
														{
															SiteName = SiteNameStr,
															ServiceName = _iStringConstant.InternetService,
															GroupDetail = GroupDetailStr,
															Bandwidth = BandwidthStr,
															BusinessUnit = BusinessUnitStr,
															MonthlyPrice = MonthlyPriceStr,
															CallAmount = CallAmountStr,
															CommentOnBandwidth = CommentOnBandwidthStr,
															CommentOnPrice = CommentOnPriceStr,
															ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
														});

                                                        datalistError.Add(new ExceldetailError
                                                        {
                                                            ExcelUploadLogId = 0,
                                                            ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                            FileGuidNo = filename,

                                                            SiteName = SiteNameStr,
                                                            ServiceDetail = _iStringConstant.DataCenterFacility,
                                                            GroupDetail = GroupDetailStr,
                                                            Bandwidth = BandwidthStr,
                                                            BusinessUnit = BusinessUnitStr,
                                                            MonthlyPrice = MonthlyPriceStr,
                                                            CallAmount = CallAmountStr,
                                                            CommentOnBandwidth = CommentOnBandwidthStr,
                                                            CommentOnPrice = CommentOnPriceStr,

                                                            ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                        });
                                                    }

												}
												catch (Exception e)
												{
													if (e.GetType() != typeof(System.NullReferenceException))
													{
														datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
														{
															SiteName = SiteNameStr,
															ServiceName = _iStringConstant.InternetService,
															GroupDetail = GroupDetailStr,
															Bandwidth = BandwidthStr,
															BusinessUnit = BusinessUnitStr,
															MonthlyPrice = MonthlyPriceStr,
															CallAmount = CallAmountStr,
															CommentOnBandwidth = CommentOnBandwidthStr,
															CommentOnPrice = CommentOnPriceStr,
															ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
														});

                                                        datalistError.Add(new ExceldetailError
                                                        {
                                                            ExcelUploadLogId = 0,
                                                            ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                            FileGuidNo = filename,

                                                            SiteName = SiteNameStr,
                                                            ServiceDetail = _iStringConstant.DataCenterFacility,
                                                            GroupDetail = GroupDetailStr,
                                                            Bandwidth = BandwidthStr,
                                                            BusinessUnit = BusinessUnitStr,
                                                            MonthlyPrice = MonthlyPriceStr,
                                                            CallAmount = CallAmountStr,
                                                            CommentOnBandwidth = CommentOnBandwidthStr,
                                                            CommentOnPrice = CommentOnPriceStr,

                                                            ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                        });
                                                    }

												}

											}

										}

									}
								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
						}

						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						IRow headerRow = sheet.GetRow(0);


						if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
						{
							if (TitleReadingColumn.All(Char.IsLetter))
							{
								int rowcount = sheet.LastRowNum;
								for (int j = readingIndex; j <= rowcount + 1; j++)
								{
									int intIndex = 0;
									intIndex = (j > 0 ? j - 1 : j);
									IRow row = sheet.GetRow(intIndex);
									int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

									if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

									else
									{
										string strValue = Convert.ToString(row.GetCell(ColNumber));
										if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
										{
											readingIndex = intIndex + 1;
											break;
										}
									}
								}

							}
							else if (TitleReadingColumn.All(Char.IsLetterOrDigit) && TitleReadingColumn.Length > 1)
							{
								var dynCR = new CellReference(TitleReadingColumn);
								IRow row = sheet.GetRow(dynCR.Row);
								var cell = row.GetCell(dynCR.Col);
								string strValue = Convert.ToString(row.GetCell(dynCR.Col));
								if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
								{
									readingIndex = dynCR.Row + 1;
								}
							}

						}

						if (headerRow != null)
						{

							int rowcount = sheet.LastRowNum + 1;
							for (int j = readingIndex + 1; j <= rowcount; j++)
							{
								IRow row = sheet.GetRow(readingIndex);

								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}
								else
								{
									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;


										#region --> Site Name Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
										string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
										if (!string.IsNullOrEmpty(SiteNameStr))
										{
											if (SiteNameStr.GetType() != typeof(string))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

										}
										#endregion

										#region --> Bandwidth  Validation Part

										var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
										string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
										if (!string.IsNullOrEmpty(BandwidthStr))
										{
											if (BandwidthStr.GetType() != typeof(string))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
											}
										}

										#endregion

										#region --> Price Validatition Required and Format Validation Part
										string PriceStr = string.Empty;
										string Message = string.Empty;
										Type valueType;

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

										}



										#endregion

										//Addede On : 2019-10-14
										#region --> Call Amount Required and Format Validation Part  

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

										valueType = CallAmountStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";

										}



										#endregion

										if (IsFullValid)
										{
											Exceldetail data = new Exceldetail();
											// --> Required Field Data--------------------
											data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
											data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
											data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
											data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
											data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
											data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));


											data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
											data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

											data.ExcelUploadLogId = 0;
											data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
											data.CurrencyId = 1;
											long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
											data.CurrencyId = providerCurrencyId;

											data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
											// data.AssignType = (long)EnumList.AssignType.Business;
											data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
											datalist.Add(data);
										}
										else
										{

											datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
											{
												// --> Required Field Data--------------------
												ServiceName = _iStringConstant.InternetService,
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
												CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

												ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                FileGuidNo = filename,

                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });

                                        }
									}
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
											{
												// --> Required Field Data--------------------
												ServiceName = _iStringConstant.InternetService,
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
												CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

												ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                FileGuidNo = filename,

                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
									}
								}


							}
						}

					}
				}
				UploadListAC.InvalidList = datalistInvalid;
				UploadListAC.ValidList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;

				ResponseDynamicDataAC<InternetServiceUploadListAC> responseData = new ResponseDynamicDataAC<InternetServiceUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}

		public async Task<ImportBillDetailAC<DataCenterFacilityUploadListAC>> ReadExcelForDataCenterFacility(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
		{
			ImportBillDetailAC<DataCenterFacilityUploadListAC> importBillDetail = new ImportBillDetailAC<DataCenterFacilityUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<DataCenterFacilityExcelUploadDetailStringAC> datalistInvalid = new List<DataCenterFacilityExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

			try
			{
				DataCenterFacilityUploadListAC UploadListAC = new DataCenterFacilityUploadListAC();
				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.DataCenterFacility);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					string TitleHeader = string.Empty;
					string TitleReadingColumn = string.Empty;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
						TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
						TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;

							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

									#region --> Find Actual reading Index from Title Address Index
									if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
									{
										if (TitleReadingColumn.All(Char.IsLetter))
										{
											for (int j = readingIndex; j <= rowcount + 1; j++)
											{
												int intIndex = 0;
												intIndex = (j > 0 ? j - 1 : j);

												int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);
												var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

												if (itemvalue == null || itemvalue.Count() == 0)
												{
													continue;
												}
												else
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.ToLower()));
													string titleValue = string.Empty;
													if (titleAddressIndex != null)
													{
														int index = Convert.ToInt16(titleAddressIndex);
														var value = rowDataItems[index];
														titleValue = Convert.ToString(value);
													}
													if (titleValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
													{
														readingIndex = intIndex + 1;
														break;
													}
												}
											}
										}
										else if (TitleReadingColumn.All(Char.IsLetterOrDigit) && TitleReadingColumn.Length > 1)
										{
											//var dynCR = new CellReference(TitleReadingColumn);
											//IRow row = sheet.GetRow(dynCR.Row);
											//var cell = row.GetCell(dynCR.Col);
											//string strValue = Convert.ToString(row.GetCell(dynCR.Col));
											//if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
											//{
											//    readingIndex = dynCR.Row + 1;
											//}

											string indexStr = "0";
											indexStr = string.Join("", TitleReadingColumn.ToCharArray().Where(Char.IsDigit));
											int indexINT = Convert.ToInt32(indexStr);
											indexINT = indexINT == 0 ? indexINT : indexINT - 1;

											for (int j = indexINT - 1; j <= indexINT; j++)
											{
												var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

												if (itemvalue == null || itemvalue.Count() == 0)
												{
													continue;
												}

												if (itemvalue.Count() > 0)
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.Substring(0, 1).ToLower()));
													string titleValue = string.Empty;
													if (titleAddressIndex != null)
													{
														int index = Convert.ToInt16(titleAddressIndex);
														var value = rowDataItems[index];
														titleValue = Convert.ToString(value);
													}
													if (titleValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
													{
														readingIndex = indexINT + 1;
														break;
													}
												}
											}
										}


										#endregion


										for (int j = readingIndex; j < rowcount; j++)
										{
											var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

											if (itemvalue == null || itemvalue.Count() == 0)
											{
												continue;
											}

											if (itemvalue.Count() > 0)
											{
												var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

												#region --> Coomon Data Variable declaration 
												bool IsFullValid = true;
												string ErrorMessageSummary = string.Empty;
												string CallAmountStr = string.Empty;
												string MonthlyPriceStr = string.Empty;
												string SiteNameStr = string.Empty;
												string BandwidthStr = string.Empty;
												string GroupDetailStr = string.Empty;
												string BusinessUnitStr = string.Empty;
												string CommentOnPriceStr = string.Empty;
												string CommentOnBandwidthStr = string.Empty;

												#endregion
												try
												{
													#region --> Site Name Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
														string fieldValue0 = string.Empty;
														if (filedAddressIndex0 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex0);
															var value = rowDataItems[index];
															fieldValue0 = Convert.ToString(value);
														}

														SiteNameStr = fieldValue0;
														if (string.IsNullOrEmpty(SiteNameStr))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
														}

													}
													else
													{
														csv.Dispose();
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Site Name mapping doesnot exists !";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
														return importBillDetail;
													}
													#endregion

													#region --> Bandwidth Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
														string fieldValue01 = string.Empty;
														if (filedAddressIndex01 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex01);
															var value = rowDataItems[index];
															fieldValue01 = Convert.ToString(value);
														}

														BandwidthStr = fieldValue01;
														if (string.IsNullOrEmpty(BandwidthStr))
														{
															if (BandwidthStr.GetType() != typeof(string))
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
															}
														}

													}
													else
													{
														csv.Dispose();
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Bandwidth mapping doesnot exists !";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
														return importBillDetail;
													}
													#endregion

													#region --> Price Validatition Required and Format Validation Part
													string PriceStr = string.Empty;
													string Message = string.Empty;
													Type valueType;

													#region --> Call Amount Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
														string fieldValue1 = string.Empty;
														if (filedAddressIndex1 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex1);
															var value = rowDataItems[index];
															fieldValue1 = Convert.ToString(value);
														}

														CallAmountStr = fieldValue1;
														if (!string.IsNullOrEmpty(CallAmountStr))
														{
															PriceStr = CallAmountStr;
															valueType = PriceStr.GetType();
															Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
															if (Message != "valid")
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + " " + Message;
															}
														}
														else
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
														}
													}
													else
													{
														csv.Dispose();
														if (File.Exists(Path.Combine(filepath, filename)))
															File.Delete(Path.Combine(filepath, filename));

														importBillDetail.Message = "Call Amount mapping doesnot exists !";
														importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
														return importBillDetail;
													}
													#endregion

													#region --> Monthly Price Amount Required and Format Validation Part
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
														string fieldValue2 = string.Empty;
														if (filedAddressIndex2 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex2);
															var value = rowDataItems[index];
															fieldValue2 = Convert.ToString(value);
														}

														MonthlyPriceStr = fieldValue2;
														if (!string.IsNullOrEmpty(MonthlyPriceStr))
														{
															PriceStr = MonthlyPriceStr;
															valueType = PriceStr.GetType();
															Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
															if (Message != "valid")
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + " " + Message;
															}
														}
													}
													#endregion

													// end of price validation region here
													#endregion

													#region --> Other Optional Fileds
													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
														string fieldValue8 = string.Empty;
														if (filedAddressIndex8 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex8);
															var value = rowDataItems[index];
															fieldValue8 = Convert.ToString(value);
														}
														GroupDetailStr = fieldValue8;
													}

													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
														string fieldValue9 = string.Empty;
														if (filedAddressIndex9 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex9);
															var value = rowDataItems[index];
															fieldValue9 = Convert.ToString(value);
														}
														BusinessUnitStr = fieldValue9;
													}

													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
														string fieldValue10 = string.Empty;
														if (filedAddressIndex10 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex10);
															var value = rowDataItems[index];
															fieldValue10 = Convert.ToString(value);
														}
														CommentOnPriceStr = fieldValue10;
													}

													if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
													{
														string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
														int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
														string fieldValue11 = string.Empty;
														if (filedAddressIndex11 != null)
														{
															int index = Convert.ToInt16(filedAddressIndex11);
															var value = rowDataItems[index];
															fieldValue11 = Convert.ToString(value);
														}
														CommentOnBandwidthStr = fieldValue11;
													}
													#endregion


													if (IsFullValid)
													{
														Exceldetail data = new Exceldetail();
														data.SiteName = SiteNameStr;
														data.GroupDetail = GroupDetailStr;
														data.Bandwidth = BandwidthStr;
														data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
														data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
														data.BusinessUnit = BusinessUnitStr;
														data.CommentOnBandwidth = CommentOnBandwidthStr;
														data.CommentOnPrice = CommentOnPriceStr;

														data.ExcelUploadLogId = 0;
														data.ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility;

														data.CurrencyId = 1;
														long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
														data.CurrencyId = providerCurrencyId;

														data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
														// data.AssignType = (long)EnumList.AssignType.Business;
														data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
														datalist.Add(data);
													}
													else
													{
														datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
														{
															SiteName = SiteNameStr,
															ServiceName = _iStringConstant.DataCenterFacility,
															GroupDetail = GroupDetailStr,
															Bandwidth = BandwidthStr,
															BusinessUnit = BusinessUnitStr,
															MonthlyPrice = MonthlyPriceStr,
															CallAmount = CallAmountStr,
															CommentOnBandwidth = CommentOnBandwidthStr,
															CommentOnPrice = CommentOnPriceStr,
															ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
														});

                                                        datalistError.Add(new ExceldetailError
                                                        {
                                                            ExcelUploadLogId=0,
                                                            ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                            FileGuidNo=filename,

                                                            SiteName = SiteNameStr,
                                                            ServiceDetail = _iStringConstant.DataCenterFacility,
                                                            GroupDetail = GroupDetailStr,
                                                            Bandwidth = BandwidthStr,
                                                            BusinessUnit = BusinessUnitStr,
                                                            MonthlyPrice = MonthlyPriceStr,
                                                            CallAmount = CallAmountStr,
                                                            CommentOnBandwidth = CommentOnBandwidthStr,
                                                            CommentOnPrice = CommentOnPriceStr,

                                                            ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                        });


                                                    }

												}
												catch (Exception e)
												{
													if (e.GetType() != typeof(System.NullReferenceException))
													{
														datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
														{
															SiteName = SiteNameStr,
															ServiceName = _iStringConstant.DataCenterFacility,
															GroupDetail = GroupDetailStr,
															Bandwidth = BandwidthStr,
															BusinessUnit = BusinessUnitStr,
															MonthlyPrice = MonthlyPriceStr,
															CallAmount = CallAmountStr,
															CommentOnBandwidth = CommentOnBandwidthStr,
															CommentOnPrice = CommentOnPriceStr,
															ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
														});
													}

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                        FileGuidNo = filename,

                                                        SiteName = SiteNameStr,
                                                        ServiceDetail = _iStringConstant.DataCenterFacility,
                                                        GroupDetail = GroupDetailStr,
                                                        Bandwidth = BandwidthStr,
                                                        BusinessUnit = BusinessUnitStr,
                                                        MonthlyPrice = MonthlyPriceStr,
                                                        CallAmount = CallAmountStr,
                                                        CommentOnBandwidth = CommentOnBandwidthStr,
                                                        CommentOnPrice = CommentOnPriceStr,

                                                        ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                    });

                                                }

											}

										}

									}
								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
						}

						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						IRow headerRow = sheet.GetRow(0);


						if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
						{
							if (TitleReadingColumn.All(Char.IsLetter))
							{
								int rowcount = sheet.LastRowNum;
								for (int j = readingIndex; j <= rowcount + 1; j++)
								{
									int intIndex = 0;
									intIndex = (j > 0 ? j - 1 : j);
									IRow row = sheet.GetRow(intIndex);
									int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

									if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

									else
									{
										string strValue = Convert.ToString(row.GetCell(ColNumber));
										if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
										{
											readingIndex = intIndex + 1;
											break;
										}
									}
								}

							}
							else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
							{
								var dynCR = new CellReference(TitleReadingColumn);
								if (dynCR.Row >= 0)
								{
									IRow row = sheet.GetRow(dynCR.Row);
									var cell = row.GetCell(dynCR.Col);
									string strValue = Convert.ToString(row.GetCell(dynCR.Col));
									if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
									{
										readingIndex = dynCR.Row + 1;
									}
								}

							}


						}

						if (headerRow != null)
						{

							int rowcount = sheet.LastRowNum + 1;
							for (int j = readingIndex + 1; j <= rowcount; j++)
							{
								IRow row = sheet.GetRow(readingIndex);

								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}
								else
								{
									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;


										#region --> Site Name Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
										string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
										if (!string.IsNullOrEmpty(SiteNameStr))
										{
											if (SiteNameStr.GetType() != typeof(string))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

										}
										#endregion

										#region --> Bandwidth  Validation Part

										var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
										string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
										if (!string.IsNullOrEmpty(BandwidthStr))
										{
											if (BandwidthStr.GetType() != typeof(string))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
											}
										}

										#endregion

										#region --> Price Validatition Required and Format Validation Part
										string PriceStr = string.Empty;
										string Message = string.Empty;
										Type valueType;

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

										}



										#endregion

										//Addede On : 2019-10-14
										#region --> Call Amount Required and Format Validation Part  

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

										valueType = CallAmountStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";

										}



										#endregion

										if (IsFullValid)
										{
											Exceldetail data = new Exceldetail();
											// --> Required Field Data--------------------
											data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
											data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
											data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
											data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
											data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
											data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));


											data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
											data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

											data.ExcelUploadLogId = 0;
											data.ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility;
											data.CurrencyId = 1;
											long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
											data.CurrencyId = providerCurrencyId;

											data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
											// data.AssignType = (long)EnumList.AssignType.Business;
											data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
											datalist.Add(data);
										}
										else
										{

											datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
											{
												// --> Required Field Data--------------------
												ServiceName = _iStringConstant.DataCenterFacility,
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
												CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

												ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
											});


                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                FileGuidNo = filename,

                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });
                                        }
									}
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
											{
												// --> Required Field Data--------------------
												ServiceName = _iStringConstant.DataCenterFacility,
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
												CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

												ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                FileGuidNo = filename,

                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
									}
								}


							}
						}

					}
				}
				UploadListAC.InvalidList = datalistInvalid;
				UploadListAC.ValidList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;

				ResponseDynamicDataAC<DataCenterFacilityUploadListAC> responseData = new ResponseDynamicDataAC<DataCenterFacilityUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}



		public async Task<ImportBillDetailAC<ManagedHostingServiceUploadListAC>> ReadExcelForManagedHostingService(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
		{
			ImportBillDetailAC<ManagedHostingServiceUploadListAC> importBillDetail = new ImportBillDetailAC<ManagedHostingServiceUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<ManagedHostingServiceExcelUploadDetailStringAC> datalistInvalid = new List<ManagedHostingServiceExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

            try
			{
				ManagedHostingServiceUploadListAC UploadListAC = new ManagedHostingServiceUploadListAC();
				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.ManagedHostingService);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					string TitleHeader = string.Empty;
					string TitleReadingColumn = string.Empty;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
						TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
						TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;

							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

									for (int j = readingIndex; j < rowcount; j++)
									{
										var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

										if (itemvalue == null || itemvalue.Count() == 0)
										{
											continue;
										}

										if (itemvalue.Count() > 0)
										{
											var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

											#region --> Coomon Data Variable declaration 
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											string CallAmountStr = string.Empty;
											string MonthlyPriceStr = string.Empty;
											string SiteNameStr = string.Empty;
											string BandwidthStr = string.Empty;
											string GroupDetailStr = string.Empty;
											string BusinessUnitStr = string.Empty;
											string CommentOnPriceStr = string.Empty;
											string CommentOnBandwidthStr = string.Empty;

											#endregion
											try
											{
												#region --> Site Name Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
													string fieldValue0 = string.Empty;
													if (filedAddressIndex0 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex0);
														var value = rowDataItems[index];
														fieldValue0 = Convert.ToString(value);
													}

													SiteNameStr = fieldValue0;
													if (string.IsNullOrEmpty(SiteNameStr))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
													}

												}
												else
												{
													csv.Dispose();
													if (File.Exists(Path.Combine(filepath, filename)))
														File.Delete(Path.Combine(filepath, filename));

													importBillDetail.Message = "Site Name mapping doesnot exists !";
													importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
													return importBillDetail;
												}
												#endregion

												#region --> Bandwidth Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
													string fieldValue01 = string.Empty;
													if (filedAddressIndex01 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex01);
														var value = rowDataItems[index];
														fieldValue01 = Convert.ToString(value);
													}

													BandwidthStr = fieldValue01;
													if (string.IsNullOrEmpty(BandwidthStr))
													{
														if (BandwidthStr.GetType() != typeof(string))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
														}
													}

												}
												else
												{
													csv.Dispose();
													if (File.Exists(Path.Combine(filepath, filename)))
														File.Delete(Path.Combine(filepath, filename));

													importBillDetail.Message = "Bandwidth mapping doesnot exists !";
													importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
													return importBillDetail;
												}
												#endregion

												#region --> Price Validatition Required and Format Validation Part
												string PriceStr = string.Empty;
												string Message = string.Empty;
												Type valueType;

												#region --> Call Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
													string fieldValue1 = string.Empty;
													if (filedAddressIndex1 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex1);
														var value = rowDataItems[index];
														fieldValue1 = Convert.ToString(value);
													}

													CallAmountStr = fieldValue1;
													if (!string.IsNullOrEmpty(CallAmountStr))
													{
														PriceStr = CallAmountStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
													else
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
													}
												}
												else
												{
													csv.Dispose();
													if (File.Exists(Path.Combine(filepath, filename)))
														File.Delete(Path.Combine(filepath, filename));

													importBillDetail.Message = "Call Amount mapping doesnot exists !";
													importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
													return importBillDetail;
												}
												#endregion

												#region --> Monthly Price Amount Required and Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
													string fieldValue2 = string.Empty;
													if (filedAddressIndex2 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex2);
														var value = rowDataItems[index];
														fieldValue2 = Convert.ToString(value);
													}

													MonthlyPriceStr = fieldValue2;
													if (!string.IsNullOrEmpty(MonthlyPriceStr))
													{
														PriceStr = MonthlyPriceStr;
														valueType = PriceStr.GetType();
														Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
														if (Message != "valid")
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " " + Message;
														}
													}
												}
												#endregion

												// end of price validation region here
												#endregion

												#region --> Other Optional Fileds
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
													string fieldValue8 = string.Empty;
													if (filedAddressIndex8 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex8);
														var value = rowDataItems[index];
														fieldValue8 = Convert.ToString(value);
													}
													GroupDetailStr = fieldValue8;
												}

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
													string fieldValue9 = string.Empty;
													if (filedAddressIndex9 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex9);
														var value = rowDataItems[index];
														fieldValue9 = Convert.ToString(value);
													}
													BusinessUnitStr = fieldValue9;
												}

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
													string fieldValue10 = string.Empty;
													if (filedAddressIndex10 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex10);
														var value = rowDataItems[index];
														fieldValue10 = Convert.ToString(value);
													}
													CommentOnPriceStr = fieldValue10;
												}

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
													string fieldValue11 = string.Empty;
													if (filedAddressIndex11 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex11);
														var value = rowDataItems[index];
														fieldValue11 = Convert.ToString(value);
													}
													CommentOnBandwidthStr = fieldValue11;
												}
												#endregion


												if (IsFullValid)
												{
													Exceldetail data = new Exceldetail();
													data.SiteName = SiteNameStr;
													data.GroupDetail = GroupDetailStr;
													data.Bandwidth = BandwidthStr;
													data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
													data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
													data.BusinessUnit = BusinessUnitStr;
													data.CommentOnBandwidth = CommentOnBandwidthStr;
													data.CommentOnPrice = CommentOnPriceStr;

													data.ExcelUploadLogId = 0;
													data.ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService;
													data.CurrencyId = 1;
													long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
													data.CurrencyId = providerCurrencyId;
													data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
													// data.AssignType = (long)EnumList.AssignType.Business;
													data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
													datalist.Add(data);
												}
												else
												{
													datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
													{
														SiteName = SiteNameStr,
														ServiceName = _iStringConstant.ManagedHostingService,
														GroupDetail = GroupDetailStr,
														Bandwidth = BandwidthStr,
														BusinessUnit = BusinessUnitStr,
														MonthlyPrice = MonthlyPriceStr,
														CallAmount = CallAmountStr,
														CommentOnBandwidth = CommentOnBandwidthStr,
														CommentOnPrice = CommentOnPriceStr,
														ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                        FileGuidNo = filename,

                                                        SiteName = SiteNameStr,
                                                        ServiceDetail = _iStringConstant.ManagedHostingService,
                                                        GroupDetail = GroupDetailStr,
                                                        Bandwidth = BandwidthStr,
                                                        BusinessUnit = BusinessUnitStr,
                                                        MonthlyPrice = MonthlyPriceStr,
                                                        CallAmount = CallAmountStr,
                                                        CommentOnBandwidth = CommentOnBandwidthStr,
                                                        CommentOnPrice = CommentOnPriceStr,

                                                        ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                    });
                                                }

											}
											catch (Exception e)
											{
												if (e.GetType() != typeof(System.NullReferenceException))
												{
													datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
													{
														SiteName = SiteNameStr,
														ServiceName = _iStringConstant.ManagedHostingService,
														GroupDetail = GroupDetailStr,
														Bandwidth = BandwidthStr,
														BusinessUnit = BusinessUnitStr,
														MonthlyPrice = MonthlyPriceStr,
														CallAmount = CallAmountStr,
														CommentOnBandwidth = CommentOnBandwidthStr,
														CommentOnPrice = CommentOnPriceStr,
														ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                        FileGuidNo = filename,

                                                        SiteName = SiteNameStr,
                                                        ServiceDetail = _iStringConstant.ManagedHostingService,
                                                        GroupDetail = GroupDetailStr,
                                                        Bandwidth = BandwidthStr,
                                                        BusinessUnit = BusinessUnitStr,
                                                        MonthlyPrice = MonthlyPriceStr,
                                                        CallAmount = CallAmountStr,
                                                        CommentOnBandwidth = CommentOnBandwidthStr,
                                                        CommentOnPrice = CommentOnPriceStr,

                                                        ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                    });
                                                }

											}

										}

									}

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
						}

						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						IRow headerRow = sheet.GetRow(0);


						if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
						{
							if (TitleReadingColumn.All(Char.IsLetter))
							{
								int rowcount = sheet.LastRowNum;
								for (int j = readingIndex; j <= rowcount + 1; j++)
								{
									int intIndex = 0;
									intIndex = (j > 0 ? j - 1 : j);
									IRow row = sheet.GetRow(intIndex);
									int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

									if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

									else
									{
										string strValue = Convert.ToString(row.GetCell(ColNumber));
										if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
										{
											readingIndex = intIndex + 1;
											break;
										}
									}
								}

							}
							else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
							{
								var dynCR = new CellReference(TitleReadingColumn);
								if (dynCR.Row >= 0)
								{
									IRow row = sheet.GetRow(dynCR.Row);
									var cell = row.GetCell(dynCR.Col);
									string strValue = Convert.ToString(row.GetCell(dynCR.Col));
									if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
									{
										readingIndex = dynCR.Row + 1;
									}
								}

							}


						}

						if (headerRow != null)
						{

							int rowcount = sheet.LastRowNum + 1;
							for (int j = readingIndex + 1; j <= rowcount; j++)
							{
								IRow row = sheet.GetRow(readingIndex);

								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}
								else
								{
									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;


										#region --> Site Name Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
										string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
										if (!string.IsNullOrEmpty(SiteNameStr))
										{
											if (SiteNameStr.GetType() != typeof(string))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

										}
										#endregion

										#region --> Bandwidth  Validation Part

										var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
										string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
										if (!string.IsNullOrEmpty(BandwidthStr))
										{
											if (BandwidthStr.GetType() != typeof(string))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
											}
										}

										#endregion

										#region --> Price Validatition Required and Format Validation Part
										string PriceStr = string.Empty;
										string Message = string.Empty;
										Type valueType;

										PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
										valueType = PriceStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

										}



										#endregion

										//Addede On : 2019-10-14
										#region --> Call Amount Required and Format Validation Part  

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

										valueType = CallAmountStr.GetType();
										if (!string.IsNullOrEmpty(PriceStr))
										{
											Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
											if (Message != "valid")
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " " + Message;
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";

										}



										#endregion

										if (IsFullValid)
										{
											Exceldetail data = new Exceldetail();
											// --> Required Field Data--------------------
											data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
											data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
											data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
											data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
											data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
											data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));


											data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
											data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

											data.ExcelUploadLogId = 0;
											data.ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService;
											data.CurrencyId = 1;
											long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
											data.CurrencyId = providerCurrencyId;
											data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
											// data.AssignType = (long)EnumList.AssignType.Business;
											data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
											datalist.Add(data);
										}
										else
										{

											datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
											{
												// --> Required Field Data--------------------
												ServiceName = _iStringConstant.ManagedHostingService,
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
												CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

												ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
											});


                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                FileGuidNo = filename,

                                                ServiceDetail = _iStringConstant.ManagedHostingService,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                            });


                                        }
                                    }
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
											{
												// --> Required Field Data--------------------
												ServiceName = _iStringConstant.ManagedHostingService,
												SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
												GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
												Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
												BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

												CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
												MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
												CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
												CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

												ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
											});

                                            datalistError.Add(new ExceldetailError
                                            {
                                                ExcelUploadLogId = 0,
                                                ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                FileGuidNo = filename,

                                                ServiceDetail = _iStringConstant.ManagedHostingService,
                                                SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                            });
                                        }
									}
								}


							}
						}

					}
				}
				UploadListAC.InvalidList = datalistInvalid;
				UploadListAC.ValidList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;

				ResponseDynamicDataAC<ManagedHostingServiceUploadListAC> responseData = new ResponseDynamicDataAC<ManagedHostingServiceUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}



		private string getValueFromExcel(string dynamicRef, ISheet sheet, long type = 0)
		{
			var dynCR = new CellReference(dynamicRef);
			var row = sheet.GetRow(dynCR.Row);
			var cell = row.GetCell(dynCR.Col);
			string strValueofmyname = string.Empty;
			// IRow rows = sheet.GetRow(dynCR.Row);
			try
			{
				if (row == null)
				{
					strValueofmyname = "Invalid Type";
				}
				if (row.Cells.All(d => d.CellType == CellType.Blank))
				{
					strValueofmyname = "";
				}
				else if (cell.CellType == CellType.Formula)
				{
					if (cell.CachedFormulaResultType == CellType.Numeric)
					{
						strValueofmyname = Convert.ToString(cell.NumericCellValue);
					}
					else if (cell.CachedFormulaResultType == CellType.String)
					{
						strValueofmyname = Convert.ToString(cell.StringCellValue);
					}
					else if (cell.CachedFormulaResultType == CellType.Boolean)
					{
						strValueofmyname = Convert.ToString(cell.BooleanCellValue);
					}
					else if (cell.CachedFormulaResultType == CellType.Blank)
					{
						strValueofmyname = string.Empty;
					}
					else if (cell.CachedFormulaResultType == CellType.Error)
					{
						strValueofmyname = Convert.ToString(cell.ErrorCellValue);
					}
					else if (cell.CachedFormulaResultType == CellType.Unknown)
					{
						strValueofmyname = Convert.ToString(row.GetCell(dynCR.Col));
					}
				}
				else
				{
					strValueofmyname = Convert.ToString(row.GetCell(dynCR.Col));

					if (type == (long)EnumList.SupportDataType.Date) // date
					{
						strValueofmyname = Convert.ToString(cell.DateCellValue);
					}
					if (type == (long)EnumList.SupportDataType.Time) //  time
					{
						strValueofmyname = Convert.ToString(cell.DateCellValue);
						DateTime dt1 = DateTime.Parse(strValueofmyname);
						strValueofmyname = String.Format("{0:HH:mm:ss}", dt1);
					}
					if (type == (long)EnumList.SupportDataType.Number) // numeric amount any
					{
						strValueofmyname = Convert.ToString(cell.NumericCellValue);
					}
				}

			}
			catch (Exception)
			{
				strValueofmyname = Convert.ToString(row.GetCell(dynCR.Col));
			}

			return strValueofmyname;
		}

		private bool CheckDate(String date)

		{

			try

			{

				DateTime dt = DateTime.Parse(date);

				return true;

			}
			catch

			{

				return false;

			}

		}


		private string RemoveSpecialChars(string str)
		{
			// Create  a string array and add the special characters you want to remove except ".".
			string[] chars = new string[] { ",", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]", "<", ">" };
			//Iterate the number of times based on the String array length.
			for (int i = 0; i < chars.Length; i++)
			{
				if (str.Contains(chars[i]))
				{
					str = str.Replace(chars[i], "");
				}
			}
			return str;
		}

		public MultiServiceUploadAC getFirstReadingIndexWithService(string filepath, string filename, List<MappingDetailAC> mappingExcellist, BillUploadAC billUploadAC)
		{
			MultiServiceUploadAC readingData = new MultiServiceUploadAC();
			try
			{
				int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
				MappingDetailAC mappingDetail = new MappingDetailAC();

				#region --> If we got1 service without title than call function for that service

				if (ServiceWithoutTitleCount == 1)
				{
					mappingDetail = mappingExcellist.FirstOrDefault(x => !x.HaveTitle && x.TitleName == "");
					readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

					if (mappingDetail != null)
					{
						if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
						{
							readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
						}
						else
						{
							readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
						}

					}

					return readingData;
				}  // End of  if (ServiceWithoutTitleCount == 1)

				#endregion

				#region --> If all service have Title than get first minimum worksheet no's service details

				if (ServiceWithoutTitleCount == 0)
				{

					int worksheetno = 0;


					if (mappingExcellist != null)
					{
						mappingDetail = mappingExcellist.OrderBy(x => x.WorkSheetNo).FirstOrDefault();
						worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);

						List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == worksheetno).ToList();

						#region --> Only One service in single worksheet 
						if (SingleWorksheetservice.Count() == 1)
						{
							mappingDetail = SingleWorksheetservice.FirstOrDefault();

							if (mappingDetail != null)
							{
								readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

								if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
								{
									readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
								}
								else if (mappingDetail.HaveHeader)
								{
									readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
								}
								else if (mappingDetail.HaveTitle)
								{
									#region--> read excel and get Title Index Row                                    
									ISheet sheet;
									string sFileExtension = Path.GetExtension(filename).ToLower();
									string fullPath = Path.Combine(filepath, filename);
									int readingIndex = 0;
									using (var stream = new FileStream(fullPath, FileMode.Open))
									{

										string TitleHeader = string.Empty;
										string TitleReadingColumn = string.Empty;
										if (mappingDetail != null)
										{
											worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);
											readingIndex = (mappingDetail.HaveHeader ? 2 : 1);
											TitleHeader = (mappingDetail.HaveTitle ? mappingDetail.TitleName : "");
											TitleReadingColumn = (mappingDetail.ExcelColumnNameForTitle);
										}

										stream.Position = 0;
										if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
										{
											HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
											sheet = hssfwb.GetSheetAt(worksheetno - 1);
										}
										else
										{
											XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
											sheet = hssfwb.GetSheetAt(worksheetno - 1);
										}
										IRow headerRow = sheet.GetRow(0);

										if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
										{
											if (TitleReadingColumn.All(Char.IsLetter))
											{
												int rowcount = sheet.LastRowNum;
												for (int j = readingIndex; j <= rowcount + 1; j++)
												{
													int intIndex = 0;
													intIndex = (j > 0 ? j - 1 : j);
													IRow row = sheet.GetRow(intIndex);
													int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

													if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

													else
													{
														string strValue = Convert.ToString(row.GetCell(ColNumber));
														if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
														{
															readingIndex = intIndex + 1;
															break;
														}
													}
												}

											}
											else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
											{
												var dynCR = new CellReference(TitleReadingColumn);
												if (dynCR.Row >= 0)
												{
													IRow row = sheet.GetRow(dynCR.Row);
													var cell = row.GetCell(dynCR.Col);
													string strValue = Convert.ToString(row.GetCell(dynCR.Col));
													if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
													{
														readingIndex = dynCR.Row + 1;
													}
												}
											}

										}

									}
									#endregion

									if (mappingDetail.HaveTitle && mappingDetail.HaveHeader)
									{
										readingData.ReadingIndex = readingIndex + 1; //Add one for Header
									}
									else
									{
										readingData.ReadingIndex = readingIndex;
									}
								}

								return readingData;
							}

						} // end of if (SingleWorksheetservice.Count() == 1)

						#endregion

						#region --> Multiple service in  single worksheet 

						if (SingleWorksheetservice.Count() > 1)
						{
							// Get All service Title in list to find first index
							string[] ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();

							#region--> read excel and get Title Index Row                                    
							ISheet sheet;
							string sFileExtension = Path.GetExtension(filename).ToLower();
							string fullPath = Path.Combine(filepath, filename);
							int readingIndex = 0;
							using (var stream = new FileStream(fullPath, FileMode.Open))
							{

								if (mappingDetail != null)
								{
									worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);
									readingIndex = 1; // as we dont know which service will be first so will start from 1.                                    
								}
								stream.Position = 0;
								if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
								{
									HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
									sheet = hssfwb.GetSheetAt(worksheetno - 1);
								}
								else
								{
									XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
									sheet = hssfwb.GetSheetAt(worksheetno - 1);
								}
								IRow headerRow = sheet.GetRow(0);

								if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
								{
									int rowcount = sheet.LastRowNum;
									for (int j = readingIndex; j <= rowcount + 1; j++)
									{
										int intIndex = 0;
										intIndex = (j > 0 ? j - 1 : j);
										IRow row = sheet.GetRow(intIndex);

										if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

										else
										{
											int TitleColumNo = 0;
											// find service title nd column wise first index
											foreach (var service in SingleWorksheetservice
												.Where(x => x.WorkSheetNo == worksheetno)
												.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
											{

												if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
												{
													TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
													if (row == null || (row.GetCell(TitleColumNo) != null && (row.GetCell(TitleColumNo) != null && row.GetCell(TitleColumNo).CellType == CellType.Blank))) continue;
													else
													{
														string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
														if (!string.IsNullOrEmpty(getStrVal))
														{
															bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
															if (ServiceTitle.Any(getStrVal.Contains))
															{
																readingIndex = intIndex + 1;
																readingData.ServiceTypeId = service.ServiceTypeId;
																readingData.ReadingIndex = readingIndex;
																return readingData;
																//  break;
															}
															else
															{
																continue;
															}
														}
													}

												}

												else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
												{
													var dynCR = new CellReference(service.ExcelColumnNameForTitle);
													if (dynCR.Row >= 0)
													{
														IRow rowR = sheet.GetRow(dynCR.Row);
														var cell = rowR.GetCell(dynCR.Col);
														string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
														if (!string.IsNullOrEmpty(getStrVal))
														{
															bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
															if (ServiceTitle.Any(getStrVal.Contains))
															{
																readingIndex = dynCR.Row + 1;
																readingData.ServiceTypeId = service.ServiceTypeId;
																readingData.ReadingIndex = readingIndex;
																return readingData;
																//  break;
															}
															else
															{
																continue;
															}
														}

													}
												}

											} // end of for loop

										} // end of else


									} // end of forloop of reading index
								} // end of if  (ServiceTitle != null)

							} // end of using 

						} // end of  if (SingleWorksheetservice.Count() > 1)

						#endregion


					}

					#endregion

				} // end of   if (mappingExcellist != null)

				#endregion
			}
			catch (Exception)
			{
				readingData.ReadingIndex = 0;
				return readingData;
			}
			return readingData;
		}


		public async Task<ImportBillDetailAC<PbxUploadListAC>> ReadExcelForPbx(string filepath, string filename, MappingDetailPbxAC mappingExcel, PbxBillUploadAC billUploadAC)
		{
			ImportBillDetailAC<PbxUploadListAC> importBillDetail = new ImportBillDetailAC<PbxUploadListAC>();
			List<Exceldetailpbx> datalist = new List<Exceldetailpbx>();
			List<PbxExcelUploadDetailStringAC> datalistInvalid = new List<PbxExcelUploadDetailStringAC>();

			try
			{
				PbxUploadListAC pbxUploadListAC = new PbxUploadListAC();
				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.VOIP);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help   To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 1 : 0);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							//csv.Configuration.HasHeaderRecord = false;
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;
							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();
								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();
									for (int j = readingIndex; j < rowcount; j++)
									{
										var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

										if (itemvalue == null || itemvalue.Count() == 0)
										{
											continue;
										}

										if (itemvalue.Count() > 0)
										{
											var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

											#region --> Coomon Data Variable declaration 
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											bool isBusinessTransType = false; // If type is business Call Time,Duration is not required 
											TimeSpan? _CallTime = null;
											string CallNumberStr = string.Empty;
											string CallAmountStr = string.Empty;
											string CallTimeStr = string.Empty;
											string CallDateStr = string.Empty;

											string ConnectingPartyStr = string.Empty;
											string Name1Str = string.Empty;
											string Name2Str = string.Empty;
											string Name3Str = string.Empty;
											string Name4Str = string.Empty;
											string OtherPartyStr = string.Empty;

											string CodeNumberStr = string.Empty;
											string ClassificationCodeStr = string.Empty;
											string CallTypeStr = string.Empty;
											string PlaceStr = string.Empty;

											string BandStr = string.Empty;
											string RateStr = string.Empty;
											string DestinationTypeStr = string.Empty;
											string DistantNumberStr = string.Empty;
											string RingingTimeStr = string.Empty;
											string DescriptionStr = string.Empty;
											string DuractionSecondsStr = string.Empty;
											#endregion
											try
											{

												#region --> Call Date Required and Format Validation Part

												string fieldAddress = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDate").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex = getAlphabatefromIndex(Convert.ToChar(fieldAddress.ToLower()));
												string fieldValue = string.Empty;
												if (filedAddressIndex != null)
												{
													int index = Convert.ToInt16(filedAddressIndex);
													var value = rowDataItems[index];
													fieldValue = Convert.ToString(value);
												}
												var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
												CallDateStr = fieldValue;// getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
																		 // string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
												if (!string.IsNullOrEmpty(CallDateStr))
												{

													bool isvalidDate = true;
													isvalidDate = CheckDate(CallDateStr);

													if (!isvalidDate)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
													}


													//DateTime dt;
													//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy", "dd-MM-yyyy", "dd-MM-yyyy HH:mm:ss" };
													//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
													//}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

												}
												#endregion

												#region --> Call Time Required and Format Validation Part

												string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallTime").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
												string fieldValue1 = string.Empty;
												if (filedAddressIndex1 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex1);
													var value = rowDataItems[index];
													fieldValue1 = Convert.ToString(value);
												}


												var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
												CallTimeStr = fieldValue1;// getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
												string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
												if (!string.IsNullOrEmpty(CallTimeStr) && (CallTimeStr != "00:00:00.000000" && CallTimeStr != "00:00:00"))
												{
													bool isvalidTime = true;

													CallTimeStr = CallTimeStr.Replace(".", ":");
													isvalidTime = CheckDate(CallTimeStr);

													if (!isvalidTime)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
													}

													//DateTime dt;
													//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
													//             "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" ,"dd-MM-yyyy"};
													//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
													//}
													else
													{
														// _CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
														_CallTime = Convert.ToDateTime(CallTimeStr).TimeOfDay;

													}
												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now time not required !
														//  ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";
													}
													else
													{
														_CallTime = null;
													}

												}
												#endregion

												#region --> Call Duration Required and Format Validation Part

												string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDuration").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
												string fieldValue2 = string.Empty;
												if (filedAddressIndex2 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex2);
													var value = rowDataItems[index];
													fieldValue2 = Convert.ToString(value);
												}


												long DuractionSeconds = 0;
												string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
												var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

												string CallDurationStr = fieldValue2;// getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));
												DuractionSecondsStr = CallDurationStr;
												if (!string.IsNullOrEmpty(CallDurationStr))
												//   && (CallDurationStr != "00:00:00.000000" && CallDurationStr != "00:00:00")
												{

													if (durationFormat == "seconds")
													{
														long number;
														if (!long.TryParse(CallDurationStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															DuractionSeconds = Convert.ToInt64(CallDurationStr);
														}
													}
													else
													{
														if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
														{
															DateTime dt1 = DateTime.Parse(CallDurationStr);
															CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
														}
														//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

														DateTime dt;
														string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
														if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															TimeSpan ts = TimeSpan.Parse(CallDurationStr);
															double totalSeconds = ts.TotalSeconds;
															DuractionSeconds = Convert.ToInt64(totalSeconds);
														}
													}

												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now Duration not required !
														// ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";
													}
													else
													{
														DuractionSeconds = 0;
													}

												}
												#endregion

												#region --> Call Amount Required and Format Validation Part

												string fieldAddress3 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex3 = getAlphabatefromIndex(Convert.ToChar(fieldAddress3.ToLower()));
												string fieldValue3 = string.Empty;
												if (filedAddressIndex3 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex3);
													var value = rowDataItems[index];
													fieldValue3 = Convert.ToString(value);
												}

												CallAmountStr = fieldValue3; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
												if (!string.IsNullOrEmpty(CallAmountStr))
												{
													decimal number;
													if (!decimal.TryParse(CallAmountStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
												}
												#endregion

												if (IsFullValid)
												{
													int year = 0;
													// Additional Check for all Data is valid nd not null 
													if (CallDateStr != null)
													{
														DateTime oDate = DateTime.Parse(CallDateStr);
														year = oDate.Year;
													}

													// if (CallDateStr == "01-01-0001 00:00:00"    || CallDateStr == "01-01-0001 00:00:00" 
													//     || CallDateStr == "1899-12-31 00:00:00" || CallDateStr == "1907-05-28 00:00:00"
													//     || CallDateStr == "1907-05-22 00:00:00" || CallDateStr == "1900-10-03 00:00:00"
													//     || CallDateStr == "1900-01-06 00:00:00" )
													if (year <= (int)EnumList.ValidYear.MinYear)
													{
														IsFullValid = false;
													}
												}

												#region --> Other Optional Fileds
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ConnectingParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ConnectingParty").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
													string fieldValue8 = string.Empty;
													if (filedAddressIndex8 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex8);
														var value = rowDataItems[index];
														fieldValue8 = Convert.ToString(value);
													}
													ConnectingPartyStr = fieldValue8;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name1").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name1").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
													string fieldValue9 = string.Empty;
													if (filedAddressIndex9 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex9);
														var value = rowDataItems[index];
														fieldValue9 = Convert.ToString(value);
													}
													Name1Str = fieldValue9;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name2").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name2").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
													string fieldValue10 = string.Empty;
													if (filedAddressIndex10 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex10);
														var value = rowDataItems[index];
														fieldValue10 = Convert.ToString(value);
													}
													Name2Str = fieldValue10;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name3").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name3").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
													string fieldValue11 = string.Empty;
													if (filedAddressIndex11 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex11);
														var value = rowDataItems[index];
														fieldValue11 = Convert.ToString(value);
													}
													Name3Str = fieldValue11;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name4").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress12 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name4").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex12 = getAlphabatefromIndex(Convert.ToChar(fieldAddress12.ToLower()));
													string fieldValue12 = string.Empty;
													if (filedAddressIndex12 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex12);
														var value = rowDataItems[index];
														fieldValue12 = Convert.ToString(value);
													}
													Name4Str = fieldValue12;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "OtherParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress13 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "OtherParty").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex13 = getAlphabatefromIndex(Convert.ToChar(fieldAddress13.ToLower()));
													string fieldValue13 = string.Empty;
													if (filedAddressIndex13 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex13);
														var value = rowDataItems[index];
														fieldValue13 = Convert.ToString(value);
													}
													OtherPartyStr = fieldValue13;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CodeNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress14 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CodeNumber").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex14 = getAlphabatefromIndex(Convert.ToChar(fieldAddress14.ToLower()));
													string fieldValue14 = string.Empty;
													if (filedAddressIndex14 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex14);
														var value = rowDataItems[index];
														fieldValue14 = Convert.ToString(value);
													}
													CodeNumberStr = fieldValue14;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ClassificationCode").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress15 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ClassificationCode").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex15 = getAlphabatefromIndex(Convert.ToChar(fieldAddress15.ToLower()));
													string fieldValue15 = string.Empty;
													if (filedAddressIndex15 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex15);
														var value = rowDataItems[index];
														fieldValue15 = Convert.ToString(value);
													}
													ClassificationCodeStr = fieldValue15;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress16 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex16 = getAlphabatefromIndex(Convert.ToChar(fieldAddress16.ToLower()));
													string fieldValue16 = string.Empty;
													if (filedAddressIndex16 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex16);
														var value = rowDataItems[index];
														fieldValue16 = Convert.ToString(value);
													}
													CallTypeStr = fieldValue16;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Place").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress17 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Place").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex17 = getAlphabatefromIndex(Convert.ToChar(fieldAddress17.ToLower()));
													string fieldValue17 = string.Empty;
													if (filedAddressIndex17 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex17);
														var value = rowDataItems[index];
														fieldValue17 = Convert.ToString(value);
													}
													PlaceStr = fieldValue17;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Band").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress18 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Band").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex18 = getAlphabatefromIndex(Convert.ToChar(fieldAddress18.ToLower()));
													string fieldValue18 = string.Empty;
													if (filedAddressIndex18 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex18);
														var value = rowDataItems[index];
														fieldValue18 = Convert.ToString(value);
													}
													BandStr = fieldValue18;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Rate").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress19 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Rate").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex19 = getAlphabatefromIndex(Convert.ToChar(fieldAddress19.ToLower()));
													string fieldValue19 = string.Empty;
													if (filedAddressIndex19 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex19);
														var value = rowDataItems[index];
														fieldValue19 = Convert.ToString(value);
													}
													RateStr = fieldValue19;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DestinationType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress20 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DestinationType").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex20 = getAlphabatefromIndex(Convert.ToChar(fieldAddress20.ToLower()));
													string fieldValue20 = string.Empty;
													if (filedAddressIndex20 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex20);
														var value = rowDataItems[index];
														fieldValue20 = Convert.ToString(value);
													}
													DestinationTypeStr = fieldValue20;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DistantNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress21 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DistantNumber").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex21 = getAlphabatefromIndex(Convert.ToChar(fieldAddress21.ToLower()));
													string fieldValue21 = string.Empty;
													if (filedAddressIndex21 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex21);
														var value = rowDataItems[index];
														fieldValue21 = Convert.ToString(value);
													}
													DistantNumberStr = fieldValue21;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "RingingTime").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress22 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "RingingTime").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex22 = getAlphabatefromIndex(Convert.ToChar(fieldAddress22.ToLower()));
													string fieldValue22 = string.Empty;
													if (filedAddressIndex22 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex22);
														var value = rowDataItems[index];
														fieldValue22 = Convert.ToString(value);
													}
													RingingTimeStr = fieldValue22;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress23 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex23 = getAlphabatefromIndex(Convert.ToChar(fieldAddress23.ToLower()));
													string fieldValue23 = string.Empty;
													if (filedAddressIndex23 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex23);
														var value = rowDataItems[index];
														fieldValue23 = Convert.ToString(value);
													}
													DescriptionStr = fieldValue23;
												}

												#endregion


												if (IsFullValid)
												{
													Exceldetailpbx data = new Exceldetailpbx();
													// --> Required Field Data--------------------

													data.CallAmount = Convert.ToDecimal(CallAmountStr);
													data.CallDate = Convert.ToDateTime(CallDateStr);
													if (_CallTime != null)
													{
														data.CallTime = _CallTime;
													}
													data.CallDuration = DuractionSeconds; // Call duration hh:mm:ss to long convert and stored

													// --> Optional Field Data--------------------
													data.ConnectingParty = ConnectingPartyStr;
													data.Name1 = Name1Str;
													data.Name2 = Name2Str;
													data.Name3 = Name3Str;
													data.Name4 = Name4Str;
													data.OtherParty = OtherPartyStr;
													data.CodeNumber = CodeNumberStr;
													data.ClassificationCode = ClassificationCodeStr;
													data.CallType = CallTypeStr;
													data.Place = PlaceStr;
													data.Band = BandStr;
													data.Rate = RateStr;
													data.DestinationType = DestinationTypeStr;
													data.DistantNumber = DistantNumberStr;
													data.RingingTime = string.IsNullOrEmpty(RingingTimeStr) ? 0 : Convert.ToInt32(RingingTimeStr);
													data.ExcelUploadLogId = 0;
													data.CurrencyId = 1;
													//  data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.DeviceId && !x.IsDelete && x.IsActive))?.CurrencyId;
													// PBX File Currency issue pending.
													datalist.Add(data);
												}
												else
												{

													datalistInvalid.Add(new PbxExcelUploadDetailStringAC
													{

														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														ConnectingParty = ConnectingPartyStr,
														Name1 = Name1Str,
														Name2 = Name2Str,
														Name3 = Name3Str,
														Name4 = Name4Str,
														OtherParty = OtherPartyStr,
														CodeNumber = CodeNumberStr,
														ClassificationCode = ClassificationCodeStr,
														CallType = CallTypeStr,
														Place = PlaceStr,
														Band = BandStr,
														Rate = RateStr,
														DestinationType = DestinationTypeStr,
														DistantNumber = DistantNumberStr,
														RingingTime = RingingTimeStr,

														ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
													});
												}

											}
											catch (Exception e)
											{
												if (e.GetType() != typeof(System.NullReferenceException))
												{
													datalistInvalid.Add(new PbxExcelUploadDetailStringAC
													{

														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														ConnectingParty = ConnectingPartyStr,
														Name1 = Name1Str,
														Name2 = Name2Str,
														Name3 = Name3Str,
														Name4 = Name4Str,
														OtherParty = OtherPartyStr,
														CodeNumber = CodeNumberStr,
														ClassificationCode = ClassificationCodeStr,
														CallType = CallTypeStr,
														Place = PlaceStr,
														Band = BandStr,
														Rate = RateStr,
														DestinationType = DestinationTypeStr,
														DistantNumber = DistantNumberStr,
														RingingTime = RingingTimeStr,
														ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
													});
												}

											}

										}

									}

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}

						}
					}
					catch (Exception ex)
					{
						if (File.Exists(Path.Combine(filepath, filename)))
							File.Delete(Path.Combine(filepath, filename));

						importBillDetail.Message = "Error during reading csv file.";// + e.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;

					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);

						}

						stream.Position = 0;

						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);

							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}

						}
						else if (sFileExtension == ".xlsx")//This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
						else
						{
							sheet = null;
							importBillDetail.Message = "File extension is invalid";
							importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
							importBillDetail.UploadData = new ResponseDynamicDataAC<PbxUploadListAC>();
							return importBillDetail;
						}
						IRow headerRow = sheet.GetRow(0);

						if (headerRow != null)
						{
							int cellCount = headerRow.LastCellNum;
							int rowcount = sheet.LastRowNum;
							for (int j = readingIndex; j <= rowcount + 1; j++)
							{
								int intIndex = 0;
								intIndex = (j > 0 ? j - 1 : j);
								IRow row = sheet.GetRow(intIndex);

								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}
								else
								{
									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;

										#region --> Call Date Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
										string CallDateStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
										// string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");

										if (!string.IsNullOrEmpty(CallDateStr))
										{
											bool isvalidDate = true;
											isvalidDate = CheckDate(CallDateStr);

											if (!isvalidDate)
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
											}

											//DateTime dt;
											//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy" };
											//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
											//{
											//    IsFullValid = false;
											//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
											//}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

										}
										#endregion

										#region --> Call Time Required and Format Validation Part

										var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
										string CallTimeStr = getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
										// string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
										if (!string.IsNullOrEmpty(CallTimeStr))
										{
											bool isvalidTime = true;
											isvalidTime = CheckDate(CallTimeStr);

											if (!isvalidTime)
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
											}


											//DateTime dt;
											//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
											//                     "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
											//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
											//{
											//    IsFullValid = false;
											//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
											//}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";

										}
										#endregion

										#region --> Call Duration Required and Format Validation Part
										long DuractionSeconds = 0;
										string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
										var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

										string CallDurationStr = getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));

										if (!string.IsNullOrEmpty(CallDurationStr))
										{

											if (durationFormat == "seconds")
											{
												long number;
												if (!long.TryParse(CallDurationStr, out number))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
												}
											}
											else
											{
												if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
												{
													DateTime dt1 = DateTime.Parse(CallDurationStr);
													CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
												}
												//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

												DateTime dt;
												string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
												if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
												}
												else
												{
													TimeSpan ts = TimeSpan.Parse(CallDurationStr);
													double totalSeconds = ts.TotalSeconds;
													DuractionSeconds = Convert.ToInt64(totalSeconds);
												}
											}

										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";

										}
										#endregion

										#region --> Call Amount Required and Format Validation Part

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
										//string CallAmountStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value.ToString());
										if (!string.IsNullOrEmpty(CallAmountStr))
										{
											decimal number;
											if (!decimal.TryParse(CallAmountStr, out number))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
										}
										#endregion


										if (IsFullValid)
										{
											int year = 0;
											// Additional Check for all Data is valid nd not null 
											if (CallDateStr != null)
											{
												DateTime oDate = DateTime.Parse(CallDateStr);
												year = oDate.Year;
											}

											// if (CallDateStr == "01-01-0001 00:00:00"    || CallDateStr == "01-01-0001 00:00:00" 
											//     || CallDateStr == "1899-12-31 00:00:00" || CallDateStr == "1907-05-28 00:00:00"
											//     || CallDateStr == "1907-05-22 00:00:00" || CallDateStr == "1900-10-03 00:00:00"
											//     || CallDateStr == "1900-01-06 00:00:00" )
											if (year <= (int)EnumList.ValidYear.MinYear)
											{
												IsFullValid = false;
											}
										}

										if (IsFullValid)
										{


											Exceldetailpbx data = new Exceldetailpbx();
											string CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
											CallAmount = RemoveSpecialChars(CallAmount);

											data.CallAmount = Convert.ToDecimal(CallAmount);
											data.CallDate = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date));
											data.CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
											// Call duration hh:mm:ss to long convert and stored
											data.CallDuration = DuractionSeconds;

											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ConnectingParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.ConnectingParty = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ConnectingParty", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name1").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name1 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name1", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "OtherParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.OtherParty = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "OtherParty", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name2").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name2 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name2", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CodeNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.CodeNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CodeNumber", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name3").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name3 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name3", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ClassificationCode").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.ClassificationCode = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ClassificationCode", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name4").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name4 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name4", j), sheet, (long)EnumList.SupportDataType.String);


											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.CallType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Place").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Place = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Place", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Band").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Band = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Band", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Rate").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Rate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Rate", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DestinationType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.DestinationType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "DestinationType", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DistantNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.DistantNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "DistantNumber", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "RingingTime").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.RingingTime = Convert.ToInt16(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "RingingTime", j), sheet, (long)EnumList.SupportDataType.Number));

											data.ExcelUploadLogId = 0;
											data.CurrencyId = 1;
											//  data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.DeviceId && !x.IsDelete && x.IsActive))?.CurrencyId;

											datalist.Add(data);
										}
										else
										{
											string dDescription = string.Empty;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);

											}
											PbxExcelUploadDetailStringAC data = new PbxExcelUploadDetailStringAC();
											data.CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet);
											data.CallDate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date);
											data.CallTime = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time);
											data.CallDuration = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time);

											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ConnectingParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.ConnectingParty = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ConnectingParty", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name1").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name1 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name1", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "OtherParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.OtherParty = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "OtherParty", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name2").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name2 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name2", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CodeNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.CodeNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CodeNumber", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name3").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name3 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name3", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ClassificationCode").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.ClassificationCode = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ClassificationCode", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name4").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name4 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name4", j), sheet, (long)EnumList.SupportDataType.String);


											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.CallType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Place").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Place = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Place", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Band").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Band = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Band", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Rate").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Rate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Rate", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DestinationType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.DestinationType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "DestinationType", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DistantNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.DistantNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "DistantNumber", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "RingingTime").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.RingingTime = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "RingingTime", j), sheet, (long)EnumList.SupportDataType.Number));

											data.ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary;

											datalistInvalid.Add(data);
										}
									}
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											string dDescription = string.Empty;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);

											}
											PbxExcelUploadDetailStringAC data = new PbxExcelUploadDetailStringAC();
											data.CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet);
											data.CallDate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date);
											data.CallTime = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time);
											data.CallDuration = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time);

											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ConnectingParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.ConnectingParty = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ConnectingParty", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name1").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name1 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name1", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "OtherParty").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.OtherParty = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "OtherParty", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name2").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name2 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name2", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CodeNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.CodeNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CodeNumber", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name3").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name3 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name3", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ClassificationCode").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.ClassificationCode = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ClassificationCode", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Name4").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Name4 = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Name4", j), sheet, (long)EnumList.SupportDataType.String);


											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.CallType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Place").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Place = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Place", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Band").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Band = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Band", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Rate").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.Rate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Rate", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DestinationType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.DestinationType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "DestinationType", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "DistantNumber").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.DistantNumber = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "DistantNumber", j), sheet, (long)EnumList.SupportDataType.String);

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "RingingTime").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												data.RingingTime = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "RingingTime", j), sheet, (long)EnumList.SupportDataType.Number));

											data.ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message;
											datalistInvalid.Add(data);

										}
									}
								}

							}
						}



					}
				}

				pbxUploadListAC.InvalidPbxList = datalistInvalid;
				pbxUploadListAC.ValidPbxList = datalist;
				ResponseDynamicDataAC<PbxUploadListAC> responseData = new ResponseDynamicDataAC<PbxUploadListAC>();
				responseData.Data = pbxUploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#endregion

				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}
		}


		#region --> Multiple Service functions 

		public MultiServiceUploadAC getReadingIndexWithServiceFromSingleWorksheet(string filepath, string filename, List<MappingDetailAC> mappingExcellist, BillUploadAC billUploadAC, string[] ServiceTitle, int worksheetNo = 1)
		{
			MultiServiceUploadAC readingData = new MultiServiceUploadAC();
			readingData.ReadingIndex = 0;
			readingData.ServiceTypeId = 0;
			try
			{
				int ServiceWithoutTitleCount = mappingExcellist.Where(x => !x.HaveTitle && x.TitleName == "").Count();
				MappingDetailAC mappingDetail = new MappingDetailAC();

				#region --> If we got1 service without title than call function for that service

				if (ServiceWithoutTitleCount == 1)
				{
					mappingDetail = mappingExcellist.FirstOrDefault(x => !x.HaveTitle && x.TitleName == "");
					readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

					if (mappingDetail != null)
					{
						if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
						{
							readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
						}
						else
						{
							readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
							// Date : 2019-09-25 Not Sure about reading Index logic
							readingData.ReadingIndex = (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn) ? Convert.ToInt16(mappingDetail.ExcelReadingColumn) : readingData.ReadingIndex);
						}

					}

					return readingData;
				}  // End of  if (ServiceWithoutTitleCount == 1)

				#endregion

				#region --> If all service have Title than get first minimum worksheet no's service details

				if (ServiceWithoutTitleCount == 0)
				{
					string sFileExtension = Path.GetExtension(filename).ToLower();
					string fullPath = Path.Combine(filepath, filename);

					int worksheetno = 0;
					worksheetno = worksheetNo;

					if (mappingExcellist != null)
					{

						if (sFileExtension == ".csv")
						{
							int readingIndexCsv = 0;
							string TitleHeader = string.Empty;
							string TitleReadingColumn = string.Empty;

							string csvpath = fullPath;
							try
							{
								using (var reader = new StreamReader(csvpath))
								using (var csv = new CsvReader(reader))
								{
									csv.Configuration.HasHeaderRecord = false;

									try
									{
										var csvRecords = csv.GetRecords<dynamic>().ToList();

										if (csvRecords != null)
										{
											int rowcount = csvRecords.Count();

											if (rowcount > 0)
											{
												if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
												{

													for (int j = readingIndexCsv; j < rowcount; j++)
													{
														var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();
														int intIndex = readingIndexCsv;
														if (itemvalue == null || itemvalue.Count() == 0)
														{
															continue;
														}
														int TitleColumNo = 0;
														// find service title nd column wise first index
														#region --> Forloop forget title from each mapped column at current row
														foreach (var service in mappingExcellist
																		.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
														{

															if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
															{
																TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);

																if (itemvalue == null || itemvalue.Count() == 0)
																{
																	continue;
																}
																else
																{
																	var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

																	int? titleAddressIndex = TitleColumNo;
																	string titleValue = string.Empty;
																	if (titleAddressIndex != null)
																	{
																		int index = Convert.ToInt16(titleAddressIndex);
																		var value = rowDataItems[index];
																		titleValue = Convert.ToString(value);
																	}
																	string getStrVal = titleValue;
																	if (!string.IsNullOrEmpty(getStrVal))
																	{
																		bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																		if (containsAny)
																		{
																			readingIndexCsv = intIndex + 1;
																			readingData.ServiceTypeId = mappingExcellist.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;
																			readingData.ReadingIndex = readingIndexCsv;
																			return readingData;
																			//  break;
																		}
																		else
																		{
																			continue;
																		}
																	}
																}

															}

															else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
															{

																string indexStr = "0";
																indexStr = string.Join("", TitleReadingColumn.ToCharArray().Where(Char.IsDigit));
																int indexINT = Convert.ToInt32(indexStr);
																indexINT = indexINT == 0 ? indexINT : indexINT - 1;

																if (indexINT == j)
																{
																	if (itemvalue == null || itemvalue.Count() == 0)
																	{
																		continue;
																	}

																	if (itemvalue.Count() > 0)
																	{
																		var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

																		int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.Substring(0, 1).ToLower()));
																		string titleValue = string.Empty;
																		if (titleAddressIndex != null)
																		{
																			int index = Convert.ToInt16(titleAddressIndex);
																			var value = rowDataItems[index];
																			titleValue = Convert.ToString(value);
																		}
																		string getStrVal = titleValue;
																		if (!string.IsNullOrEmpty(getStrVal))
																		{
																			bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																			if (containsAny)
																			{
																				readingIndexCsv = intIndex + 1;
																				readingData.ServiceTypeId = mappingExcellist.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;
																				readingData.ReadingIndex = readingIndexCsv;
																				return readingData;
																				//  break;
																			}
																			else
																			{
																				continue;
																			}
																		}
																	}
																}


															}

														} // end of for loop
														#endregion
													}

												}

											}
										}
									}
									catch (Exception e)
									{
										readingData.ReadingIndex = 0;
										return readingData;
									}
								}
							}
							catch (Exception e)
							{
								readingData.ReadingIndex = 0;
								return readingData;
							}

						}
						else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
						{
							List<MappingDetailAC> SingleWorksheetservice = mappingExcellist.Where(x => x.WorkSheetNo == worksheetno).ToList();

							#region --> Only One service in single worksheet 
							if (SingleWorksheetservice.Count() == 1)
							{
								mappingDetail = SingleWorksheetservice.FirstOrDefault();

								if (mappingDetail != null)
								{
									readingData.ServiceTypeId = mappingDetail.ServiceTypeId;

									if (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn))
									{
										readingData.ReadingIndex = Convert.ToInt64(mappingDetail.ExcelReadingColumn);
									}
									else if (mappingDetail.HaveHeader)
									{
										readingData.ReadingIndex = (mappingDetail.HaveHeader ? 2 : 1);
									}
									else if (mappingDetail.HaveTitle)
									{
										#region--> read excel and get Title Index Row                                    
										ISheet sheet;
										//string sFileExtension = Path.GetExtension(filename).ToLower();
										//string fullPath = Path.Combine(filepath, filename);
										int readingIndex = 0;
										using (var stream = new FileStream(fullPath, FileMode.Open))
										{

											string TitleHeader = string.Empty;
											string TitleReadingColumn = string.Empty;
											if (mappingDetail != null)
											{
												worksheetno = Convert.ToInt16(mappingDetail.WorkSheetNo);
												readingIndex = (mappingDetail.HaveHeader ? 2 : 1);

												// 2019-09-25 I M Not sure for Reading index logic
												readingIndex = (!string.IsNullOrEmpty(mappingDetail.ExcelReadingColumn) ? Convert.ToInt16(mappingDetail.ExcelReadingColumn) : readingIndex);

												TitleHeader = (mappingDetail.HaveTitle ? mappingDetail.TitleName : "");
												TitleReadingColumn = (mappingDetail.ExcelColumnNameForTitle);
											}

											stream.Position = 0;
											if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
											{
												HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
												sheet = hssfwb.GetSheetAt(worksheetno - 1);
											}
											else
											{
												XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
												sheet = hssfwb.GetSheetAt(worksheetno - 1);
											}
											IRow headerRow = sheet.GetRow(0);

											if ((!string.IsNullOrEmpty(TitleHeader)) && (!string.IsNullOrEmpty(TitleReadingColumn)))
											{
												if (TitleReadingColumn.All(Char.IsLetter))
												{
													int rowcount = sheet.LastRowNum;
													for (int j = readingIndex; j <= rowcount + 1; j++)
													{
														int intIndex = 0;
														intIndex = (j > 0 ? j - 1 : j);
														IRow row = sheet.GetRow(intIndex);
														int ColNumber = CellReference.ConvertColStringToIndex(TitleReadingColumn);

														if (row == null || row.GetCell(ColNumber).CellType == CellType.Blank) continue;

														else
														{
															string strValue = Convert.ToString(row.GetCell(ColNumber));
															if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
															{
																readingIndex = intIndex + 1;
																break;
															}
														}
													}

												}
												else if (TitleReadingColumn.All(Char.IsLetterOrDigit))
												{
													var dynCR = new CellReference(TitleReadingColumn);
													if (dynCR.Row >= 0)
													{
														IRow row = sheet.GetRow(dynCR.Row);
														var cell = row.GetCell(dynCR.Col);
														string strValue = Convert.ToString(row.GetCell(dynCR.Col));
														if (strValue.ToLower().Trim() == TitleHeader.ToLower().Trim())
														{
															readingIndex = dynCR.Row + 1;
														}
													}
												}

											}

										}
										#endregion

										if (mappingDetail.HaveTitle && mappingDetail.HaveHeader)
										{
											readingData.ReadingIndex = readingIndex + 1; //Add one for Header
										}
										else
										{
											readingData.ReadingIndex = readingIndex;
										}
									}

									return readingData;
								}

							} // end of if (SingleWorksheetservice.Count() == 1)

							#endregion

							#region --> Multiple service in  single worksheet 

							if (SingleWorksheetservice.Count() > 1)
							{
								// Get All service Title in list to find first index
								// string[] ServiceTitle = SingleWorksheetservice.Select(i => i.TitleName.ToString().ToLower().Trim()).ToArray();

								#region--> read excel and get Title Index Row                                    
								ISheet sheet;
								int readingIndex = 0;
								using (var stream = new FileStream(fullPath, FileMode.Open))
								{
									readingIndex = 0;
									worksheetno = worksheetNo;

									stream.Position = 0;
									if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
									{
										HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
										sheet = hssfwb.GetSheetAt(worksheetno - 1);
									}
									else
									{
										XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
										sheet = hssfwb.GetSheetAt(worksheetno - 1);
									}
									IRow headerRow = sheet.GetRow(0);

									if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
									{
										int rowcount = sheet.LastRowNum;
										for (int j = readingIndex; j <= rowcount + 1; j++)
										{
											int intIndex = j;
											// intIndex = (j > 0 ? j - 1 : j);
											IRow row = sheet.GetRow(intIndex);

											if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

											else
											{
												int TitleColumNo = 0;
												// find service title nd column wise first index
												foreach (var service in SingleWorksheetservice
													.Where(x => x.WorkSheetNo == worksheetno)
													.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
												{

													if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
													{
														TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
														if (row == null || (row.GetCell(TitleColumNo) != null && row.GetCell(TitleColumNo).CellType == CellType.Blank)) continue;
														else
														{
															string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
															if (!string.IsNullOrEmpty(getStrVal))
															{
																bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																if (containsAny)
																{
																	readingIndex = intIndex + 1;
																	readingData.ServiceTypeId = SingleWorksheetservice.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;
																	readingData.ReadingIndex = readingIndex;
																	return readingData;
																	//  break;
																}
																else
																{
																	continue;
																}
															}
														}

													}

													else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
													{
														var dynCR = new CellReference(service.ExcelColumnNameForTitle);
														if (dynCR.Row >= 0)
														{
															IRow rowR = sheet.GetRow(dynCR.Row);
															var cell = rowR.GetCell(dynCR.Col);
															string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
															if (!string.IsNullOrEmpty(getStrVal))
															{
																bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																if (containsAny)
																{
																	readingIndex = dynCR.Row + 1;
																	readingData.ServiceTypeId = service.ServiceTypeId;
																	readingData.ServiceTypeId = SingleWorksheetservice.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																	readingData.ReadingIndex = readingIndex;
																	return readingData;
																	//  break;
																}
																else
																{
																	continue;
																}
															}

														}
													}

												} // end of for loop

											} // end of else


										} // end of forloop of reading index
									} // end of if  (ServiceTitle != null)

								} // end of using 

							} // end of  if (SingleWorksheetservice.Count() > 1)

							#endregion
						}
						#endregion
					}
				} // end of   if (mappingExcellist != null)

				#endregion
			}
			catch (Exception e)
			{
				readingData.ReadingIndex = 0;
				return readingData;
			}
			return readingData;
		}


		public async Task<ImportBillDetailMultipleAC<InternetServiceUploadListAC>> ReadExcelForInternetServiceMultiple
						(string filepath, string filename,
						 MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
						 BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1)
		{
			ImportBillDetailMultipleAC<InternetServiceUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<InternetServiceUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<InternetServiceExcelUploadDetailStringAC> datalistInvalid = new List<InternetServiceExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

            try
			{
				InternetServiceUploadListAC UploadListAC = new InternetServiceUploadListAC();

				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.InternetService);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);

				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					string TitleHeader = string.Empty;
					string TitleReadingColumn = string.Empty;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
						TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);

						#region --> set reading index for current Service type

						if (ReadingIndex > 0)
						{
							readingIndex = (int)ReadingIndex;
						}
						else
						{
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
							// SPECIALLY FOR csv READER ONLY
							readingIndex = (readingIndex > 1 ? readingIndex : 0);
						}

						#endregion
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;

							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

									if (rowcount > 0)
									{
										bool IsServiceTitleRow = false;

										for (int j = readingIndex; j < rowcount; j++)
										{
											bool IsSameServiceTitleRow = false;

											var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();
											int intIndex = readingIndex;
											if (itemvalue == null || itemvalue.Count() == 0)
											{
												continue;
											}

											#region --> Check next row having any title for next service or not 

											else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
											{
												int TitleColumNo = 0;
												// find service title nd column wise first index
												#region --> Forloop forget title from each mapped column at current row
												foreach (var service in singleWorksheetserviceList
															   .Where(x => x.WorkSheetNo == worksheetno)
																.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
												{

													if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
													{
														TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);

														if (itemvalue == null || itemvalue.Count() == 0)
														{
															continue;
														}
														else
														{
															var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

															int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.ToLower()));
															string titleValue = string.Empty;
															if (titleAddressIndex != null)
															{
																int index = Convert.ToInt16(titleAddressIndex);
																var value = rowDataItems[index];
																titleValue = Convert.ToString(value);
															}

															string getStrVal = titleValue;
															if (!string.IsNullOrEmpty(getStrVal))
															{
																if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
																{
																	bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																	if (containsAny)
																	{
																		readingIndex = j + 1;
																		importBillDetail.ServiceTypeId = service.ServiceTypeId;
																		importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																		importBillDetail.ReadingIndex = readingIndex;
																		rowcount = 0; // to stop the loop
																		IsServiceTitleRow = true;
																		break;
																		//return importBillDetail;

																	}
																}
																else
																{
																	// IsServiceTitleRow = true; // just to avoid error of Totle row
																	//break;
																	IsSameServiceTitleRow = true;
																	continue;
																}

															}
														}

													}

													else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
													{

														string indexStr = "0";
														indexStr = string.Join("", TitleReadingColumn.ToCharArray().Where(Char.IsDigit));
														int indexINT = Convert.ToInt32(indexStr);
														indexINT = indexINT == 0 ? indexINT : indexINT - 1;

														if (indexINT == j)
														{
															if (itemvalue == null || itemvalue.Count() == 0)
															{
																continue;
															}

															if (itemvalue.Count() > 0)
															{
																var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

																int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.Substring(0, 1).ToLower()));
																string titleValue = string.Empty;
																if (titleAddressIndex != null)
																{
																	int index = Convert.ToInt16(titleAddressIndex);
																	var value = rowDataItems[index];
																	titleValue = Convert.ToString(value);
																}
																string getStrVal = titleValue;
																if (!string.IsNullOrEmpty(getStrVal))
																{
																	if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
																	{
																		bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																		if (containsAny)
																		{
																			readingIndex = j + 1;
																			importBillDetail.ServiceTypeId = service.ServiceTypeId;
																			importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																			importBillDetail.ReadingIndex = readingIndex;
																			rowcount = 0;// to stop the loop
																			IsServiceTitleRow = true;
																			break;
																			// return importBillDetail;                                             
																		}
																	}
																	else
																	{
																		IsServiceTitleRow = true; // just to avoid error of Totle row
																		break;
																	}

																}
															}
														}


													}

												} // end of for loop
												#endregion

											} // end of if  (ServiceTitle != null)

											#endregion

											if (!IsServiceTitleRow && !IsSameServiceTitleRow)
											{
												if (itemvalue.Count() > 0)
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													#region --> Coomon Data Variable declaration 
													bool IsFullValid = true;
													string ErrorMessageSummary = string.Empty;
													string CallAmountStr = string.Empty;
													string MonthlyPriceStr = string.Empty;
													string SiteNameStr = string.Empty;
													string BandwidthStr = string.Empty;
													string GroupDetailStr = string.Empty;
													string BusinessUnitStr = string.Empty;
													string CommentOnPriceStr = string.Empty;
													string CommentOnBandwidthStr = string.Empty;

													#endregion
													try
													{
														#region --> Site Name Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
															string fieldValue0 = string.Empty;
															if (filedAddressIndex0 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex0);
																var value = rowDataItems[index];
																fieldValue0 = Convert.ToString(value);
															}

															SiteNameStr = fieldValue0;
															if (string.IsNullOrEmpty(SiteNameStr))
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
															}

														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Site Name mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Bandwidth Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
															string fieldValue01 = string.Empty;
															if (filedAddressIndex01 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex01);
																var value = rowDataItems[index];
																fieldValue01 = Convert.ToString(value);
															}

															BandwidthStr = fieldValue01;
															if (string.IsNullOrEmpty(BandwidthStr))
															{
																if (BandwidthStr.GetType() != typeof(string))
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
																}
															}

														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Bandwidth mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Price Validatition Required and Format Validation Part
														string PriceStr = string.Empty;
														string Message = string.Empty;
														Type valueType;

														#region --> Call Amount Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
															string fieldValue1 = string.Empty;
															if (filedAddressIndex1 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex1);
																var value = rowDataItems[index];
																fieldValue1 = Convert.ToString(value);
															}

															CallAmountStr = fieldValue1;
															if (!string.IsNullOrEmpty(CallAmountStr))
															{
																PriceStr = CallAmountStr;
																valueType = PriceStr.GetType();
																Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
																if (Message != "valid")
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + " " + Message;
																}
															}
															else
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
															}
														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Call Amount mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Monthly Price Amount Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
															string fieldValue2 = string.Empty;
															if (filedAddressIndex2 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex2);
																var value = rowDataItems[index];
																fieldValue2 = Convert.ToString(value);
															}

															MonthlyPriceStr = fieldValue2;
															if (!string.IsNullOrEmpty(MonthlyPriceStr))
															{
																PriceStr = MonthlyPriceStr;
																valueType = PriceStr.GetType();
																Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
																if (Message != "valid")
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + " " + Message;
																}
															}
														}
														#endregion

														// end of price validation region here
														#endregion

														#region --> Other Optional Fileds
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
															string fieldValue8 = string.Empty;
															if (filedAddressIndex8 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex8);
																var value = rowDataItems[index];
																fieldValue8 = Convert.ToString(value);
															}
															GroupDetailStr = fieldValue8;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
															string fieldValue9 = string.Empty;
															if (filedAddressIndex9 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex9);
																var value = rowDataItems[index];
																fieldValue9 = Convert.ToString(value);
															}
															BusinessUnitStr = fieldValue9;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
															string fieldValue10 = string.Empty;
															if (filedAddressIndex10 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex10);
																var value = rowDataItems[index];
																fieldValue10 = Convert.ToString(value);
															}
															CommentOnPriceStr = fieldValue10;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
															string fieldValue11 = string.Empty;
															if (filedAddressIndex11 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex11);
																var value = rowDataItems[index];
																fieldValue11 = Convert.ToString(value);
															}
															CommentOnBandwidthStr = fieldValue11;
														}
														#endregion


														if (IsFullValid)
														{
															Exceldetail data = new Exceldetail();
															data.SiteName = SiteNameStr;
															data.GroupDetail = GroupDetailStr;
															data.Bandwidth = BandwidthStr;
															data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
															data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
															data.BusinessUnit = BusinessUnitStr;
															data.CommentOnBandwidth = CommentOnBandwidthStr;
															data.CommentOnPrice = CommentOnPriceStr;

															data.ExcelUploadLogId = 0;
															data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
															data.CurrencyId = 1;
															long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
															data.CurrencyId = providerCurrencyId;

															data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
															// data.AssignType = (long)EnumList.AssignType.Business;
															data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
															datalist.Add(data);
														}
														else
														{
															datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
															{
																SiteName = SiteNameStr,
																ServiceName = _iStringConstant.InternetService,
																GroupDetail = GroupDetailStr,
																Bandwidth = BandwidthStr,
																BusinessUnit = BusinessUnitStr,
																MonthlyPrice = MonthlyPriceStr,
																CallAmount = CallAmountStr,
																CommentOnBandwidth = CommentOnBandwidthStr,
																CommentOnPrice = CommentOnPriceStr,
																ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
															});

                                                            datalistError.Add(new ExceldetailError
                                                            {
                                                                ExcelUploadLogId = 0,
                                                                ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                                FileGuidNo = filename,

                                                                SiteName = SiteNameStr,
                                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                                GroupDetail = GroupDetailStr,
                                                                Bandwidth = BandwidthStr,
                                                                BusinessUnit = BusinessUnitStr,
                                                                MonthlyPrice = MonthlyPriceStr,
                                                                CallAmount = CallAmountStr,
                                                                CommentOnBandwidth = CommentOnBandwidthStr,
                                                                CommentOnPrice = CommentOnPriceStr,

                                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                            });
                                                        }

													}
													catch (Exception e)
													{
														if (e.GetType() != typeof(System.NullReferenceException))
														{
															datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
															{
																SiteName = SiteNameStr,
																ServiceName = _iStringConstant.InternetService,
																GroupDetail = GroupDetailStr,
																Bandwidth = BandwidthStr,
																BusinessUnit = BusinessUnitStr,
																MonthlyPrice = MonthlyPriceStr,
																CallAmount = CallAmountStr,
																CommentOnBandwidth = CommentOnBandwidthStr,
																CommentOnPrice = CommentOnPriceStr,
																ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
															});

                                                            datalistError.Add(new ExceldetailError
                                                            {
                                                                ExcelUploadLogId = 0,
                                                                ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                                FileGuidNo = filename,

                                                                SiteName = SiteNameStr,
                                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                                GroupDetail = GroupDetailStr,
                                                                Bandwidth = BandwidthStr,
                                                                BusinessUnit = BusinessUnitStr,
                                                                MonthlyPrice = MonthlyPriceStr,
                                                                CallAmount = CallAmountStr,
                                                                CommentOnBandwidth = CommentOnBandwidthStr,
                                                                CommentOnPrice = CommentOnPriceStr,

                                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                            });

                                                        }

													}

												}
											}
											else
											{
												if (importBillDetail.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
													IsSameServiceTitleRow = true;
											}

										}
									} // end of if row count > 0

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = worksheetNo;
						int readingIndex = ReadingIndex;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							#region --> set reading index for current Service type
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							if (ReadingIndex < 0)
							{
								readingIndex = (mappingExcel.HaveHeader ? 2 : 1);

								TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
							}
							else
							{
								readingIndex = (int)ReadingIndex;
							}

							#endregion
						}

						#region --> Read  .xls or .xlsx File Using NPOI Package Class
						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						#endregion

						if (sheet.LastRowNum > 0)
						{
							int rowcount = sheet.LastRowNum + 1;
							bool IsServiceTitleRow = false;
							for (int j = readingIndex; j <= rowcount; j++)
							{
								IsServiceTitleRow = false;
								int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
								IRow row = sheet.GetRow(intIndex);
								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}

								#region --> Check next row having any title for next service or not 
								else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
								{
									int TitleColumNo = 0;
									// find service title nd column wise first index
									#region --> Forloop forget title from each mapped column at current row
									foreach (var service in singleWorksheetserviceList
												   .Where(x => x.WorkSheetNo == worksheetno)
													.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
									{

										if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
										{
											TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
											if (row == null || (row.GetCell(TitleColumNo) != null && row.GetCell(TitleColumNo).CellType == CellType.Blank)) continue;
											else
											{
												string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = intIndex + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0; // to stop the loop
															IsServiceTitleRow = true;
															break;
															//return importBillDetail;

														}
													}
													else
													{
														IsServiceTitleRow = true; // just to avoid error of Totle row
														break;
													}

												}
											}

										}

										else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
										{
											var dynCR = new CellReference(service.ExcelColumnNameForTitle);
											if (dynCR.Row >= 0)
											{
												IRow rowR = sheet.GetRow(dynCR.Row);
												var cell = rowR.GetCell(dynCR.Col);
												string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = dynCR.Row + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0;// to stop the loop
															IsServiceTitleRow = true;
															break;
															// return importBillDetail;                                             
														}
													}
													else
													{
														IsServiceTitleRow = true; // just to avoid error of Totle row
														break;
													}

												}

											}
										}

									} // end of for loop
									#endregion

								} // end of if  (ServiceTitle != null)

								#endregion

								if (!IsServiceTitleRow)
								{
									if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
									{
										try
										{
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;

											#region --> Site Name Required and Format Validation Part

											var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
											string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(SiteNameStr))
											{
												if (SiteNameStr.GetType() != typeof(string))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

											}
											#endregion

											#region --> Bandwidth  Validation Part

											var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
											string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(BandwidthStr))
											{
												if (BandwidthStr.GetType() != typeof(string))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
												}
											}

											#endregion

											#region --> Price Validatition Required and Format Validation Part
											string PriceStr = string.Empty;
											string Message = string.Empty;
											Type valueType;

											PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
											valueType = PriceStr.GetType();
											if (!string.IsNullOrEmpty(PriceStr))
											{
												Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
												if (Message != "valid")
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " " + Message;
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

											}



											#endregion

											//Addede On : 2019-10-14
											#region --> Call Amount Required and Format Validation Part  

											string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

											valueType = CallAmountStr.GetType();
											if (!string.IsNullOrEmpty(PriceStr))
											{
												Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
												if (Message != "valid")
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " " + Message;
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";

											}



											#endregion


											if (IsFullValid)
											{
												Exceldetail data = new Exceldetail();
												// --> Required Field Data--------------------
												data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
												data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
												data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
												data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
												data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
												data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));

												data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
												data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

												data.ExcelUploadLogId = 0;
												data.ServiceTypeId = (long)EnumList.ServiceType.InternetService;
												data.CurrencyId = 1;
												long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
												data.CurrencyId = providerCurrencyId;
												data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
												// data.AssignType = (long)EnumList.AssignType.Business;
												data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
												datalist.Add(data);
											}
											else
											{

												datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
												{
													// --> Required Field Data--------------------
													ServiceName = _iStringConstant.InternetService,
													SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
													GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
													Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
													BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

													CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
													MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
													CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
													CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

													ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                    FileGuidNo = filename,

                                                    ServiceDetail = _iStringConstant.DataCenterFacility,
                                                    SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                    GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                    Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                    CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                    CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                    ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                });

                                            }

                                        }
										catch (Exception e)
										{
											if (e.GetType() != typeof(System.NullReferenceException))
											{
												datalistInvalid.Add(new InternetServiceExcelUploadDetailStringAC
												{
													// --> Required Field Data--------------------
													ServiceName = _iStringConstant.InternetService,
													SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
													GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
													Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
													BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

													CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
													MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
													CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
													CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

													ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    ServiceTypeId = (long)EnumList.ServiceType.InternetService,
                                                    FileGuidNo = filename,

                                                    ServiceDetail = _iStringConstant.DataCenterFacility,
                                                    SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                    GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                    Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                    CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                    CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    
                                                    ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                });

                                            }
										}
									}
								} // end of if (!IsServiceTitleRow)

							}
						}

					}

				}
				UploadListAC.InvalidList = datalistInvalid;
				UploadListAC.ValidList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;
				ResponseDynamicDataAC<InternetServiceUploadListAC> responseData = new ResponseDynamicDataAC<InternetServiceUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				// if (File.Exists(Path.Combine(filepath, filename)))
				//    File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}



		public async Task<ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>> ReadExcelForDataCenterFacilityMultiple
						(string filepath, string filename,
						 MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
						 BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1)
		{
			ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<DataCenterFacilityUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<DataCenterFacilityExcelUploadDetailStringAC> datalistInvalid = new List<DataCenterFacilityExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

            try
			{
				DataCenterFacilityUploadListAC UploadListAC = new DataCenterFacilityUploadListAC();

				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.DataCenterFacility);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					string TitleHeader = string.Empty;
					string TitleReadingColumn = string.Empty;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
						TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);

						#region --> set reading index for current Service type

						if (ReadingIndex > 0)
						{
							readingIndex = (int)ReadingIndex;
						}
						else
						{
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
							// SPECIALLY FOR csv READER ONLY
							readingIndex = (readingIndex > 1 ? readingIndex : 0);
						}

						#endregion
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;

							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

									if (rowcount > 0)
									{
										bool IsServiceTitleRow = false;

										for (int j = readingIndex; j < rowcount; j++)
										{
											bool IsSameServiceTitleRow = false;

											var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();
											int intIndex = readingIndex;
											if (itemvalue == null || itemvalue.Count() == 0)
											{
												continue;
											}

											#region --> Check next row having any title for next service or not 

											else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
											{
												int TitleColumNo = 0;
												// find service title nd column wise first index
												#region --> Forloop forget title from each mapped column at current row
												foreach (var service in singleWorksheetserviceList
															   .Where(x => x.WorkSheetNo == worksheetno)
																.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
												{

													if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
													{
														TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);

														if (itemvalue == null || itemvalue.Count() == 0)
														{
															continue;
														}
														else
														{
															var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

															int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.ToLower()));
															string titleValue = string.Empty;
															if (titleAddressIndex != null)
															{
																int index = Convert.ToInt16(titleAddressIndex);
																var value = rowDataItems[index];
																titleValue = Convert.ToString(value);
															}

															string getStrVal = titleValue;
															if (!string.IsNullOrEmpty(getStrVal))
															{
																if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
																{
																	bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																	if (containsAny)
																	{
																		readingIndex = j + 1;
																		importBillDetail.ServiceTypeId = service.ServiceTypeId;
																		importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																		importBillDetail.ReadingIndex = readingIndex;
																		rowcount = 0; // to stop the loop
																		IsServiceTitleRow = true;
																		break;
																		//return importBillDetail;

																	}
																}
																else
																{
																	// IsServiceTitleRow = true; // just to avoid error of Totle row
																	//break;
																	IsSameServiceTitleRow = true;
																	continue;
																}

															}
														}

													}

													else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
													{

														string indexStr = "0";
														indexStr = string.Join("", TitleReadingColumn.ToCharArray().Where(Char.IsDigit));
														int indexINT = Convert.ToInt32(indexStr);
														indexINT = indexINT == 0 ? indexINT : indexINT - 1;

														if (indexINT == j)
														{
															if (itemvalue == null || itemvalue.Count() == 0)
															{
																continue;
															}

															if (itemvalue.Count() > 0)
															{
																var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

																int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.Substring(0, 1).ToLower()));
																string titleValue = string.Empty;
																if (titleAddressIndex != null)
																{
																	int index = Convert.ToInt16(titleAddressIndex);
																	var value = rowDataItems[index];
																	titleValue = Convert.ToString(value);
																}
																string getStrVal = titleValue;
																if (!string.IsNullOrEmpty(getStrVal))
																{
																	if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
																	{
																		bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																		if (containsAny)
																		{
																			readingIndex = j + 1;
																			importBillDetail.ServiceTypeId = service.ServiceTypeId;
																			importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																			importBillDetail.ReadingIndex = readingIndex;
																			rowcount = 0;// to stop the loop
																			IsServiceTitleRow = true;
																			break;
																			// return importBillDetail;                                             
																		}
																	}
																	else
																	{
																		IsServiceTitleRow = true; // just to avoid error of Totle row
																		break;
																	}

																}
															}
														}


													}

												} // end of for loop
												#endregion

											} // end of if  (ServiceTitle != null)

											#endregion

											if (!IsServiceTitleRow && !IsSameServiceTitleRow)
											{
												if (itemvalue.Count() > 0)
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													#region --> Coomon Data Variable declaration 
													bool IsFullValid = true;
													string ErrorMessageSummary = string.Empty;
													string CallAmountStr = string.Empty;
													string MonthlyPriceStr = string.Empty;
													string SiteNameStr = string.Empty;
													string BandwidthStr = string.Empty;
													string GroupDetailStr = string.Empty;
													string BusinessUnitStr = string.Empty;
													string CommentOnPriceStr = string.Empty;
													string CommentOnBandwidthStr = string.Empty;

													#endregion
													try
													{
														#region --> Site Name Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
															string fieldValue0 = string.Empty;
															if (filedAddressIndex0 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex0);
																var value = rowDataItems[index];
																fieldValue0 = Convert.ToString(value);
															}

															SiteNameStr = fieldValue0;
															if (string.IsNullOrEmpty(SiteNameStr))
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
															}

														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Site Name mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Bandwidth Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
															string fieldValue01 = string.Empty;
															if (filedAddressIndex01 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex01);
																var value = rowDataItems[index];
																fieldValue01 = Convert.ToString(value);
															}

															BandwidthStr = fieldValue01;
															if (string.IsNullOrEmpty(BandwidthStr))
															{
																if (BandwidthStr.GetType() != typeof(string))
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
																}
															}

														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Bandwidth mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Price Validatition Required and Format Validation Part
														string PriceStr = string.Empty;
														string Message = string.Empty;
														Type valueType;

														#region --> Call Amount Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
															string fieldValue1 = string.Empty;
															if (filedAddressIndex1 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex1);
																var value = rowDataItems[index];
																fieldValue1 = Convert.ToString(value);
															}

															CallAmountStr = fieldValue1;
															if (!string.IsNullOrEmpty(CallAmountStr))
															{
																PriceStr = CallAmountStr;
																valueType = PriceStr.GetType();
																Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
																if (Message != "valid")
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + " " + Message;
																}
															}
															else
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
															}
														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Call Amount mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Monthly Price Amount Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
															string fieldValue2 = string.Empty;
															if (filedAddressIndex2 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex2);
																var value = rowDataItems[index];
																fieldValue2 = Convert.ToString(value);
															}

															MonthlyPriceStr = fieldValue2;
															if (!string.IsNullOrEmpty(MonthlyPriceStr))
															{
																PriceStr = MonthlyPriceStr;
																valueType = PriceStr.GetType();
																Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
																if (Message != "valid")
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + " " + Message;
																}
															}
														}
														#endregion

														// end of price validation region here
														#endregion

														#region --> Other Optional Fileds
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
															string fieldValue8 = string.Empty;
															if (filedAddressIndex8 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex8);
																var value = rowDataItems[index];
																fieldValue8 = Convert.ToString(value);
															}
															GroupDetailStr = fieldValue8;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
															string fieldValue9 = string.Empty;
															if (filedAddressIndex9 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex9);
																var value = rowDataItems[index];
																fieldValue9 = Convert.ToString(value);
															}
															BusinessUnitStr = fieldValue9;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
															string fieldValue10 = string.Empty;
															if (filedAddressIndex10 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex10);
																var value = rowDataItems[index];
																fieldValue10 = Convert.ToString(value);
															}
															CommentOnPriceStr = fieldValue10;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
															string fieldValue11 = string.Empty;
															if (filedAddressIndex11 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex11);
																var value = rowDataItems[index];
																fieldValue11 = Convert.ToString(value);
															}
															CommentOnBandwidthStr = fieldValue11;
														}
														#endregion


														if (IsFullValid)
														{
															Exceldetail data = new Exceldetail();
															data.SiteName = SiteNameStr;
															data.GroupDetail = GroupDetailStr;
															data.Bandwidth = BandwidthStr;
															data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
															data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
															data.BusinessUnit = BusinessUnitStr;
															data.CommentOnBandwidth = CommentOnBandwidthStr;
															data.CommentOnPrice = CommentOnPriceStr;

															data.ExcelUploadLogId = 0;
															data.ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility;

															data.CurrencyId = 1;
															long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
															data.CurrencyId = providerCurrencyId;

															data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
															// data.AssignType = (long)EnumList.AssignType.Business;
															data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
															datalist.Add(data);
														}
														else
														{
															datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
															{
																SiteName = SiteNameStr,
																ServiceName = _iStringConstant.DataCenterFacility,
																GroupDetail = GroupDetailStr,
																Bandwidth = BandwidthStr,
																BusinessUnit = BusinessUnitStr,
																MonthlyPrice = MonthlyPriceStr,
																CallAmount = CallAmountStr,
																CommentOnBandwidth = CommentOnBandwidthStr,
																CommentOnPrice = CommentOnPriceStr,
																ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
															});

                                                            datalistError.Add(new ExceldetailError
                                                            {
                                                                ExcelUploadLogId = 0,
                                                                ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                                FileGuidNo = filename,

                                                                SiteName = SiteNameStr,
                                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                                GroupDetail = GroupDetailStr,
                                                                Bandwidth = BandwidthStr,
                                                                BusinessUnit = BusinessUnitStr,
                                                                MonthlyPrice = MonthlyPriceStr,
                                                                CallAmount = CallAmountStr,
                                                                CommentOnBandwidth = CommentOnBandwidthStr,
                                                                CommentOnPrice = CommentOnPriceStr,

                                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                            });
                                                        }

													}
													catch (Exception e)
													{
														if (e.GetType() != typeof(System.NullReferenceException))
														{
															datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
															{
																SiteName = SiteNameStr,
																ServiceName = _iStringConstant.DataCenterFacility,
																GroupDetail = GroupDetailStr,
																Bandwidth = BandwidthStr,
																BusinessUnit = BusinessUnitStr,
																MonthlyPrice = MonthlyPriceStr,
																CallAmount = CallAmountStr,
																CommentOnBandwidth = CommentOnBandwidthStr,
																CommentOnPrice = CommentOnPriceStr,
																ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
															});

                                                            datalistError.Add(new ExceldetailError
                                                            {
                                                                ExcelUploadLogId = 0,
                                                                ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                                FileGuidNo = filename,

                                                                SiteName = SiteNameStr,
                                                                ServiceDetail = _iStringConstant.DataCenterFacility,
                                                                GroupDetail = GroupDetailStr,
                                                                Bandwidth = BandwidthStr,
                                                                BusinessUnit = BusinessUnitStr,
                                                                MonthlyPrice = MonthlyPriceStr,
                                                                CallAmount = CallAmountStr,
                                                                CommentOnBandwidth = CommentOnBandwidthStr,
                                                                CommentOnPrice = CommentOnPriceStr,

                                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                            });
                                                        }

													}

												}
											}
											else
											{
												if (importBillDetail.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
													IsSameServiceTitleRow = true;
											}

										}
									} // end of if row count > 0

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = worksheetNo;
						int readingIndex = ReadingIndex;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							#region --> set reading index for current Service type
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							if (ReadingIndex < 0)
							{
								readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
								TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
							}
							else
							{
								readingIndex = (int)ReadingIndex;
							}

							#endregion
						}

						#region --> Read  .xls or .xlsx File Using NPOI Package Class
						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						#endregion

						if (sheet.LastRowNum > 0)
						{
							int rowcount = sheet.LastRowNum + 1;
							bool IsServiceTitleRow = false;
							for (int j = readingIndex; j <= rowcount; j++)
							{
								IsServiceTitleRow = false;
								int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
								IRow row = sheet.GetRow(intIndex);
								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}

								#region --> Check next row having any title for next service or not 
								else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
								{
									int TitleColumNo = 0;
									// find service title nd column wise first index
									#region --> Forloop forget title from each mapped column at current row
									foreach (var service in singleWorksheetserviceList
												   .Where(x => x.WorkSheetNo == worksheetno)
													.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
									{

										if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
										{
											TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
											if (row == null || (row.GetCell(TitleColumNo) != null && row.GetCell(TitleColumNo).CellType == CellType.Blank)) continue;
											else
											{
												string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = intIndex + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0;// to stop the loop
															IsServiceTitleRow = true;
															break;
															//return importBillDetail;

														}
													}
													else
													{
														IsServiceTitleRow = true;
														break;
													}

												}

											}

										}

										else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
										{
											var dynCR = new CellReference(service.ExcelColumnNameForTitle);
											if (dynCR.Row >= 0)
											{
												IRow rowR = sheet.GetRow(dynCR.Row);
												var cell = rowR.GetCell(dynCR.Col);
												string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = dynCR.Row + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0;// to stop the loop
															IsServiceTitleRow = true;
															break;
															// return importBillDetail;                                             
														}
													}
													else
													{
														IsServiceTitleRow = true;
														break;
													}

												}

											}
										}

									} // end of for loop
									#endregion

								} // end of if  (ServiceTitle != null)

								#endregion
								if (!IsServiceTitleRow)
								{
									if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
									{
										try
										{
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;

											#region --> Site Name Required and Format Validation Part

											var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
											string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(SiteNameStr))
											{
												if (SiteNameStr.GetType() != typeof(string))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

											}
											#endregion

											#region --> Bandwidth  Validation Part

											var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
											string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(BandwidthStr))
											{
												if (BandwidthStr.GetType() != typeof(string))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
												}
											}

											#endregion

											#region --> Price Validatition Required and Format Validation Part
											string PriceStr = string.Empty;
											string Message = string.Empty;
											Type valueType;

											PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
											valueType = PriceStr.GetType();
											if (!string.IsNullOrEmpty(PriceStr))
											{
												Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
												if (Message != "valid")
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " " + Message;
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

											}



											#endregion

											//Addede On : 2019-10-14
											#region --> Call Amount Required and Format Validation Part  

											string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

											valueType = CallAmountStr.GetType();
											if (!string.IsNullOrEmpty(PriceStr))
											{
												Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
												if (Message != "valid")
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " " + Message;
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";

											}



											#endregion


											if (IsFullValid)
											{
												Exceldetail data = new Exceldetail();
												// --> Required Field Data--------------------
												data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
												data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
												data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
												data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
												data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
												data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));

												data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
												data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

												data.ExcelUploadLogId = 0;
												data.ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility;

												data.CurrencyId = 1;
												long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
												data.CurrencyId = providerCurrencyId;

												data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
												// data.AssignType = (long)EnumList.AssignType.Business;
												data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
												datalist.Add(data);
											}
											else
											{

												datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
												{
													// --> Required Field Data--------------------
													ServiceName = _iStringConstant.DataCenterFacility,
													SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
													GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
													Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
													BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

													CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
													MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
													CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
													CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

													ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
												});


                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                    FileGuidNo = filename,

                                                    ServiceDetail = _iStringConstant.DataCenterFacility,
                                                    SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                    GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                    Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                    CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                    CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                    ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                });

                                            }

										}
										catch (Exception e)
										{
											if (e.GetType() != typeof(System.NullReferenceException))
											{
												datalistInvalid.Add(new DataCenterFacilityExcelUploadDetailStringAC
												{
													// --> Required Field Data--------------------
													ServiceName = _iStringConstant.DataCenterFacility,
													SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
													GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
													Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
													BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

													CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
													MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
													CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
													CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

													ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    ServiceTypeId = (long)EnumList.ServiceType.DataCenterFacility,
                                                    FileGuidNo = filename,

                                                    ServiceDetail = _iStringConstant.DataCenterFacility,
                                                    SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                    GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                    Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                    CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                    CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                    ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                });
                                            }
										}
									}
								}

							}
						}

					}
				}

				UploadListAC.InvalidList = datalistInvalid;
				UploadListAC.ValidList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;

				ResponseDynamicDataAC<DataCenterFacilityUploadListAC> responseData = new ResponseDynamicDataAC<DataCenterFacilityUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				// if (File.Exists(Path.Combine(filepath, filename)))
				//    File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}



		public async Task<ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>> ReadExcelForManagedHostingServiceMultiple
						(string filepath, string filename,
						 MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
						 BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1)
		{
			ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<ManagedHostingServiceUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<ManagedHostingServiceExcelUploadDetailStringAC> datalistInvalid = new List<ManagedHostingServiceExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

            try
			{
				ManagedHostingServiceUploadListAC UploadListAC = new ManagedHostingServiceUploadListAC();

				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.ManagedHostingService);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);

				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					string TitleHeader = string.Empty;
					string TitleReadingColumn = string.Empty;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
						TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);

						#region --> set reading index for current Service type

						if (ReadingIndex > 0)
						{
							readingIndex = (int)ReadingIndex;
						}
						else
						{
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
							// SPECIALLY FOR csv READER ONLY
							readingIndex = (readingIndex > 1 ? readingIndex : 0);
						}

						#endregion
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;

							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();

									if (rowcount > 0)
									{
										bool IsServiceTitleRow = false;

										for (int j = readingIndex; j < rowcount; j++)
										{
											bool IsSameServiceTitleRow = false;

											var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();
											int intIndex = readingIndex;
											if (itemvalue == null || itemvalue.Count() == 0)
											{
												continue;
											}

											#region --> Check next row having any title for next service or not 

											else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
											{
												int TitleColumNo = 0;
												// find service title nd column wise first index
												#region --> Forloop forget title from each mapped column at current row
												foreach (var service in singleWorksheetserviceList
															   .Where(x => x.WorkSheetNo == worksheetno)
																.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
												{

													if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
													{
														TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);

														if (itemvalue == null || itemvalue.Count() == 0)
														{
															continue;
														}
														else
														{
															var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

															int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.ToLower()));
															string titleValue = string.Empty;
															if (titleAddressIndex != null)
															{
																int index = Convert.ToInt16(titleAddressIndex);
																var value = rowDataItems[index];
																titleValue = Convert.ToString(value);
															}

															string getStrVal = titleValue;
															if (!string.IsNullOrEmpty(getStrVal))
															{
																if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
																{
																	bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																	if (containsAny)
																	{
																		readingIndex = j + 1;
																		importBillDetail.ServiceTypeId = service.ServiceTypeId;
																		importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																		importBillDetail.ReadingIndex = readingIndex;
																		rowcount = 0; // to stop the loop
																		IsServiceTitleRow = true;
																		break;
																		//return importBillDetail;

																	}
																}
																else
																{
																	// IsServiceTitleRow = true; // just to avoid error of Totle row
																	//break;
																	IsSameServiceTitleRow = true;
																	continue;
																}

															}
														}

													}

													else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
													{

														string indexStr = "0";
														indexStr = string.Join("", TitleReadingColumn.ToCharArray().Where(Char.IsDigit));
														int indexINT = Convert.ToInt32(indexStr);
														indexINT = indexINT == 0 ? indexINT : indexINT - 1;

														if (indexINT == j)
														{
															if (itemvalue == null || itemvalue.Count() == 0)
															{
																continue;
															}

															if (itemvalue.Count() > 0)
															{
																var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

																int? titleAddressIndex = getAlphabatefromIndex(Convert.ToChar(TitleReadingColumn.Substring(0, 1).ToLower()));
																string titleValue = string.Empty;
																if (titleAddressIndex != null)
																{
																	int index = Convert.ToInt16(titleAddressIndex);
																	var value = rowDataItems[index];
																	titleValue = Convert.ToString(value);
																}
																string getStrVal = titleValue;
																if (!string.IsNullOrEmpty(getStrVal))
																{
																	if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
																	{
																		bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
																		if (containsAny)
																		{
																			readingIndex = j + 1;
																			importBillDetail.ServiceTypeId = service.ServiceTypeId;
																			importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																			importBillDetail.ReadingIndex = readingIndex;
																			rowcount = 0;// to stop the loop
																			IsServiceTitleRow = true;
																			break;
																			// return importBillDetail;                                             
																		}
																	}
																	else
																	{
																		IsServiceTitleRow = true; // just to avoid error of Totle row
																		break;
																	}

																}
															}
														}


													}

												} // end of for loop
												#endregion

											} // end of if  (ServiceTitle != null)

											#endregion

											if (!IsServiceTitleRow && !IsSameServiceTitleRow)
											{
												if (itemvalue.Count() > 0)
												{
													var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

													#region --> Coomon Data Variable declaration 
													bool IsFullValid = true;
													string ErrorMessageSummary = string.Empty;
													string CallAmountStr = string.Empty;
													string MonthlyPriceStr = string.Empty;
													string SiteNameStr = string.Empty;
													string BandwidthStr = string.Empty;
													string GroupDetailStr = string.Empty;
													string BusinessUnitStr = string.Empty;
													string CommentOnPriceStr = string.Empty;
													string CommentOnBandwidthStr = string.Empty;

													#endregion
													try
													{
														#region --> Site Name Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SiteName").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
															string fieldValue0 = string.Empty;
															if (filedAddressIndex0 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex0);
																var value = rowDataItems[index];
																fieldValue0 = Convert.ToString(value);
															}

															SiteNameStr = fieldValue0;
															if (string.IsNullOrEmpty(SiteNameStr))
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Site Name doesnot exists ! ";
															}

														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Site Name mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Bandwidth Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress01 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Bandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex01 = getAlphabatefromIndex(Convert.ToChar(fieldAddress01.ToLower()));
															string fieldValue01 = string.Empty;
															if (filedAddressIndex01 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex01);
																var value = rowDataItems[index];
																fieldValue01 = Convert.ToString(value);
															}

															BandwidthStr = fieldValue01;
															if (string.IsNullOrEmpty(BandwidthStr))
															{
																if (BandwidthStr.GetType() != typeof(string))
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + "Bandwidth doesnot exists ! ";
																}
															}

														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Bandwidth mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Price Validatition Required and Format Validation Part
														string PriceStr = string.Empty;
														string Message = string.Empty;
														Type valueType;

														#region --> Call Amount Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
															string fieldValue1 = string.Empty;
															if (filedAddressIndex1 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex1);
																var value = rowDataItems[index];
																fieldValue1 = Convert.ToString(value);
															}

															CallAmountStr = fieldValue1;
															if (!string.IsNullOrEmpty(CallAmountStr))
															{
																PriceStr = CallAmountStr;
																valueType = PriceStr.GetType();
																Message = checkPriceValidation("CallAmount", PriceStr, valueType, false);
																if (Message != "valid")
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + " " + Message;
																}
															}
															else
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Call Amount doesnot exists ! ";
															}
														}
														else
														{
															csv.Dispose();
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Call Amount mapping doesnot exists !";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
															return importBillDetail;
														}
														#endregion

														#region --> Monthly Price Amount Required and Format Validation Part
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MonthlyPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
															string fieldValue2 = string.Empty;
															if (filedAddressIndex2 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex2);
																var value = rowDataItems[index];
																fieldValue2 = Convert.ToString(value);
															}

															MonthlyPriceStr = fieldValue2;
															if (!string.IsNullOrEmpty(MonthlyPriceStr))
															{
																PriceStr = MonthlyPriceStr;
																valueType = PriceStr.GetType();
																Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
																if (Message != "valid")
																{
																	IsFullValid = false;
																	ErrorMessageSummary = ErrorMessageSummary + " " + Message;
																}
															}
														}
														#endregion

														// end of price validation region here
														#endregion

														#region --> Other Optional Fileds
														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "GroupDetail").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
															string fieldValue8 = string.Empty;
															if (filedAddressIndex8 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex8);
																var value = rowDataItems[index];
																fieldValue8 = Convert.ToString(value);
															}
															GroupDetailStr = fieldValue8;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "BusinessUnit").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
															string fieldValue9 = string.Empty;
															if (filedAddressIndex9 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex9);
																var value = rowDataItems[index];
																fieldValue9 = Convert.ToString(value);
															}
															BusinessUnitStr = fieldValue9;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress10 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnPrice").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex10 = getAlphabatefromIndex(Convert.ToChar(fieldAddress10.ToLower()));
															string fieldValue10 = string.Empty;
															if (filedAddressIndex10 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex10);
																var value = rowDataItems[index];
																fieldValue10 = Convert.ToString(value);
															}
															CommentOnPriceStr = fieldValue10;
														}

														if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault()))
														{
															string fieldAddress11 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CommentOnBandwidth").Select(x => x.ExcelcolumnName).FirstOrDefault();
															int? filedAddressIndex11 = getAlphabatefromIndex(Convert.ToChar(fieldAddress11.ToLower()));
															string fieldValue11 = string.Empty;
															if (filedAddressIndex11 != null)
															{
																int index = Convert.ToInt16(filedAddressIndex11);
																var value = rowDataItems[index];
																fieldValue11 = Convert.ToString(value);
															}
															CommentOnBandwidthStr = fieldValue11;
														}
														#endregion


														if (IsFullValid)
														{
															Exceldetail data = new Exceldetail();
															data.SiteName = SiteNameStr;
															data.GroupDetail = GroupDetailStr;
															data.Bandwidth = BandwidthStr;
															data.CallAmount = (string.IsNullOrEmpty(CallAmountStr) ? 0 : Convert.ToDecimal(CallAmountStr));
															data.MonthlyPrice = (string.IsNullOrEmpty(MonthlyPriceStr) ? 0 : Convert.ToDecimal(MonthlyPriceStr));
															data.BusinessUnit = BusinessUnitStr;
															data.CommentOnBandwidth = CommentOnBandwidthStr;
															data.CommentOnPrice = CommentOnPriceStr;

															data.ExcelUploadLogId = 0;
															data.ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService;

															data.CurrencyId = 1;
															long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
															data.CurrencyId = providerCurrencyId;

															data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
															// data.AssignType = (long)EnumList.AssignType.Business;
															data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
															datalist.Add(data);
														}
														else
														{
															datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
															{
																SiteName = SiteNameStr,
																ServiceName = _iStringConstant.ManagedHostingService,
																GroupDetail = GroupDetailStr,
																Bandwidth = BandwidthStr,
																BusinessUnit = BusinessUnitStr,
																MonthlyPrice = MonthlyPriceStr,
																CallAmount = CallAmountStr,
																CommentOnBandwidth = CommentOnBandwidthStr,
																CommentOnPrice = CommentOnPriceStr,
																ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
															});

                                                            datalistError.Add(new ExceldetailError
                                                            {
                                                                ExcelUploadLogId = 0,
                                                                ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                                FileGuidNo = filename,

                                                                SiteName = SiteNameStr,
                                                                ServiceDetail = _iStringConstant.ManagedHostingService,
                                                                GroupDetail = GroupDetailStr,
                                                                Bandwidth = BandwidthStr,
                                                                BusinessUnit = BusinessUnitStr,
                                                                MonthlyPrice = MonthlyPriceStr,
                                                                CallAmount = CallAmountStr,
                                                                CommentOnBandwidth = CommentOnBandwidthStr,
                                                                CommentOnPrice = CommentOnPriceStr,

                                                                ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                            });

                                                        }

                                                    }
													catch (Exception e)
													{
														if (e.GetType() != typeof(System.NullReferenceException))
														{
															datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
															{
																SiteName = SiteNameStr,
																ServiceName = _iStringConstant.ManagedHostingService,
																GroupDetail = GroupDetailStr,
																Bandwidth = BandwidthStr,
																BusinessUnit = BusinessUnitStr,
																MonthlyPrice = MonthlyPriceStr,
																CallAmount = CallAmountStr,
																CommentOnBandwidth = CommentOnBandwidthStr,
																CommentOnPrice = CommentOnPriceStr,
																ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
															});

                                                            datalistError.Add(new ExceldetailError
                                                            {
                                                                ExcelUploadLogId = 0,
                                                                ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                                FileGuidNo = filename,

                                                                SiteName = SiteNameStr,
                                                                ServiceDetail = _iStringConstant.ManagedHostingService,
                                                                GroupDetail = GroupDetailStr,
                                                                Bandwidth = BandwidthStr,
                                                                BusinessUnit = BusinessUnitStr,
                                                                MonthlyPrice = MonthlyPriceStr,
                                                                CallAmount = CallAmountStr,
                                                                CommentOnBandwidth = CommentOnBandwidthStr,
                                                                CommentOnPrice = CommentOnPriceStr,

                                                                ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                            });
                                                        }

													}

												}
											}
											else
											{
												if (importBillDetail.ServiceTypeId == (long)EnumList.ServiceType.InternetService)
													IsSameServiceTitleRow = true;
											}

										}
									} // end of if row count > 0

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}

				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = worksheetNo;
						int readingIndex = ReadingIndex;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							#region --> set reading index for current Service type
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							if (ReadingIndex < 0)
							{
								readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
								TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
							}
							else
							{
								readingIndex = (int)ReadingIndex;
							}

							#endregion
						}

						#region --> Read  .xls or .xlsx File Using NPOI Package Class
						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						#endregion

						if (sheet.LastRowNum > 0)
						{
							int rowcount = sheet.LastRowNum + 1;
							bool IsServiceTitleRow = false;
							for (int j = readingIndex; j <= rowcount; j++)
							{
								IsServiceTitleRow = false;
								int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
								IRow row = sheet.GetRow(intIndex);
								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}

								#region --> Check next row having any title for next service or not 
								else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
								{
									int TitleColumNo = 0;
									// find service title nd column wise first index
									#region --> Forloop forget title from each mapped column at current row
									foreach (var service in singleWorksheetserviceList
												   .Where(x => x.WorkSheetNo == worksheetno)
													.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
									{

										if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
										{
											TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
											try
											{
												if (row == null || (row.GetCell(TitleColumNo) != null && (row.GetCell(TitleColumNo) != null && row.GetCell(TitleColumNo).CellType == CellType.Blank))) continue;
												else
												{
													string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
													if (!string.IsNullOrEmpty(getStrVal))
													{
														if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
														{
															bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
															if (containsAny)
															{
																readingIndex = intIndex + 1;
																importBillDetail.ServiceTypeId = service.ServiceTypeId;
																importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

																importBillDetail.ReadingIndex = readingIndex;
																rowcount = 0;// to stop the loop
																IsServiceTitleRow = true;
																break;
																//return importBillDetail;

															}
														}
														else
														{
															IsServiceTitleRow = true;
															break;
														}

													}

												}
											}
											catch (Exception ex)
											{

											}


										}

										else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
										{
											var dynCR = new CellReference(service.ExcelColumnNameForTitle);
											if (dynCR.Row >= 0)
											{
												IRow rowR = sheet.GetRow(dynCR.Row);
												var cell = rowR.GetCell(dynCR.Col);
												string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = dynCR.Row + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0;// to stop the loop
															IsServiceTitleRow = true;
															break;
															// return importBillDetail;                                             
														}
													}
													else
													{
														IsServiceTitleRow = true;
														break;
													}

												}

											}
										}

									} // end of for loop
									#endregion

								} // end of if  (ServiceTitle != null)

								#endregion
								if (!IsServiceTitleRow)
								{
									if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
									{
										try
										{
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;

											#region --> Site Name Required and Format Validation Part

											var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "SiteName", j);
											string SiteNameStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(SiteNameStr))
											{
												if (SiteNameStr.GetType() != typeof(string))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Site Name not not valid ! ";
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Site Name doesnot exists ! ";

											}
											#endregion

											#region --> Bandwidth  Validation Part

											var dynamicRefBandwidth = getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j);
											string BandwidthStr = getValueFromExcel(dynamicRefBandwidth, sheet, (long)EnumList.SupportDataType.String);
											if (!string.IsNullOrEmpty(BandwidthStr))
											{
												if (BandwidthStr.GetType() != typeof(string))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " Bandwidth not not valid ! ";
												}
											}

											#endregion

											#region --> Price Validatition Required and Format Validation Part
											string PriceStr = string.Empty;
											string Message = string.Empty;
											Type valueType;

											PriceStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number);
											valueType = PriceStr.GetType();
											if (!string.IsNullOrEmpty(PriceStr))
											{
												Message = checkPriceValidation("MonthlyPrice", PriceStr, valueType, false);
												if (Message != "valid")
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " " + Message;
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Monthly Price doesnot exists ! ";

											}



											#endregion

											//Addede On : 2019-10-14
											#region --> Call Amount Required and Format Validation Part  

											string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);

											valueType = CallAmountStr.GetType();
											if (!string.IsNullOrEmpty(PriceStr))
											{
												Message = checkPriceValidation("CallAmount", CallAmountStr, valueType, false);
												if (Message != "valid")
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + " " + Message;
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + " Amount doesnot exists ! ";

											}



											#endregion

											if (IsFullValid)
											{
												Exceldetail data = new Exceldetail();
												// --> Required Field Data--------------------
												data.SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String);
												data.GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String);
												data.Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String);
												data.BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String);
												data.MonthlyPrice = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number));
												data.CallAmount = Convert.ToDecimal(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number));


												data.CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String);
												data.CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String);

												data.ExcelUploadLogId = 0;
												data.ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService;

												data.CurrencyId = 1;
												long? providerCurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
												data.CurrencyId = providerCurrencyId;

												data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
												// data.AssignType = (long)EnumList.AssignType.Business;                                            
												data.AssignType = (isBusinessOnly ? (long)EnumList.AssignType.Business : (long)EnumList.AssignType.Employee);
												datalist.Add(data);
											}
											else
											{

												datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
												{
													// --> Required Field Data--------------------
													ServiceName = _iStringConstant.ManagedHostingService,
													SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
													GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
													Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
													BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

													CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
													MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
													CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
													CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

													ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                    FileGuidNo = filename,

                                                    ServiceDetail = _iStringConstant.ManagedHostingService,
                                                    SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                    GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                    Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                    CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                    CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                    ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                });
                                            }

										}
										catch (Exception e)
										{
											if (e.GetType() != typeof(System.NullReferenceException))
											{
												datalistInvalid.Add(new ManagedHostingServiceExcelUploadDetailStringAC
												{
													// --> Required Field Data--------------------
													ServiceName = _iStringConstant.ManagedHostingService,
													SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
													GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
													Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
													BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

													CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
													MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
													CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
													CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),

													ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    ServiceTypeId = (long)EnumList.ServiceType.ManagedHostingService,
                                                    FileGuidNo = filename,

                                                    ServiceDetail = _iStringConstant.ManagedHostingService,
                                                    SiteName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SiteName", j), sheet, (long)EnumList.SupportDataType.String),
                                                    GroupDetail = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "GroupDetail", j), sheet, (long)EnumList.SupportDataType.String),
                                                    Bandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Bandwidth", j), sheet, (long)EnumList.SupportDataType.String),
                                                    BusinessUnit = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "BusinessUnit", j), sheet, (long)EnumList.SupportDataType.String),

                                                    CallAmount = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    MonthlyPrice = Convert.ToString(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MonthlyPrice", j), sheet, (long)EnumList.SupportDataType.Number)),
                                                    CommentOnPrice = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnPrice", j), sheet, (long)EnumList.SupportDataType.String),
                                                    CommentOnBandwidth = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CommentOnBandwidth", j), sheet, (long)EnumList.SupportDataType.String),


                                                    ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                });
                                            }
										}
									}
								}

							}
						}

					}

				}
				UploadListAC.InvalidList = datalistInvalid;
				UploadListAC.ValidList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;
				ResponseDynamicDataAC<ManagedHostingServiceUploadListAC> responseData = new ResponseDynamicDataAC<ManagedHostingServiceUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				// if (File.Exists(Path.Combine(filepath, filename)))
				//    File.Delete(Path.Combine(filepath, filename));

				#endregion


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}


		public async Task<ImportBillDetailMultipleAC<MobilityUploadListAC>> ReadExcelForMobilityServiceMultiple
					(string filepath, string filename,
					 MappingDetailAC mappingExcel, List<MappingDetailAC> singleWorksheetserviceList,
					 BillUploadAC billUploadAC, int ReadingIndex, string[] ServiceTitle, int worksheetNo = 1
					 , long ServiceTypeId = 0)
		{
			ImportBillDetailMultipleAC<MobilityUploadListAC> importBillDetail = new ImportBillDetailMultipleAC<MobilityUploadListAC>();

			List<Exceldetail> datalist = new List<Exceldetail>();
			List<MobilityExcelUploadDetailStringAC> datalistInvalid = new List<MobilityExcelUploadDetailStringAC>();
            List<ExceldetailError> datalistError = new List<ExceldetailError>();

			try
			{
				MobilityUploadListAC UploadListAC = new MobilityUploadListAC();

				//  bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.Mobility);
				bool isBusinessOnly = await GetServiceChargeType(ServiceTypeId);

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				string DuractionSecondsStr = string.Empty;



				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help   To read CSV File Multiple

					//  Note :  In CSV READ FOR Mobility,StaticIP,Voice Only, Internet & Device  Offer We have to read it From Bill Text !
					//          So, I don't Include logic for finding next service By Finding Title.

					int worksheetno = 0;
					int readingIndex = 0;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 1 : 0);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
					}

					string csvpath = fullPath;
					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;
							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();
                                    LogManager.Configuration.Variables["user"] = "";
                                    LogManager.Configuration.Variables["stepno"] = "5";
                                    _logger.Info("Start Reading data one by one and validate with database.");

                                    // ---- Global Variable 
                                    long CurrencyId = 0;
                                    CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId ?? 0;
                                    // ---- Global Variable 

                                    for (int j = readingIndex; j < rowcount; j++)
									{
										var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

										if (itemvalue == null || itemvalue.Count() == 0)
										{
											continue;
										}

										if (itemvalue.Count() > 0)
										{
											var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

											#region --> Coomon Data Variable declaration 
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											bool isBusinessTransType = false; // If type is business Call Time,Duration is not required 
											TimeSpan? _CallTime = null;
											ServiceTypeId = (long)EnumList.ServiceType.Mobility;
											string descriptionText = string.Empty;
											string descriptionTextStr = string.Empty;
											string CallTransTypeStr = string.Empty;
											string CallNumberStr = string.Empty;
											string CallAmountStr = string.Empty;
											string CallTimeStr = string.Empty;
											string CallDateStr = string.Empty;
											string CallerNameStr = string.Empty;
											string SubscriptionTypeStr = string.Empty;
											string CallDataKBStr = string.Empty;
											string MessageCountStr = string.Empty;
											#endregion

											try
											{

												// ------------- Need to check if Static ip In description -----
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress5 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex5 = getAlphabatefromIndex(Convert.ToChar(fieldAddress5.ToLower()));
													string fieldValue5 = string.Empty;
													if (filedAddressIndex5 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex5);
														var value = rowDataItems[index];
														fieldValue5 = Convert.ToString(value);
													}
													descriptionTextStr = fieldValue5;
													descriptionText = fieldValue5;

													if (!string.IsNullOrEmpty(descriptionText))
													{
														descriptionText = Regex.Replace(descriptionText, @"s", "");
														descriptionText = descriptionText.Replace(" ", "");
														descriptionText = descriptionText.Trim().ToUpper();

														string StaticIP = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_StaticIP");
														string VoiceOnly = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_VoiceOnly");
														string InternetPlanDevice = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_InternetPlannDevice");

														List<string> InternetPlannTagIds = InternetPlanDevice.Split(',').ToList();

														if (descriptionText.Contains(StaticIP))
														{
															if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.StaticIP).Count() > 0)
															{
																ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
															}
															else
															{
																csv.Dispose();

																if (File.Exists(Path.Combine(filepath, filename)))
																	File.Delete(Path.Combine(filepath, filename));

																importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
																importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
																importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

																return importBillDetail;
															}
														}

														else if (descriptionText.Contains(VoiceOnly))
														{
															if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.VoiceOnly).Count() > 0)
															{
																ServiceTypeId = (long)EnumList.ServiceType.VoiceOnly;
															}
															else
															{
																csv.Dispose();

																if (File.Exists(Path.Combine(filepath, filename)))
																	File.Delete(Path.Combine(filepath, filename));

																importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
																importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
																importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

																return importBillDetail;
															}
														}

														else if (InternetPlannTagIds.Any(w => descriptionText.Contains(w)))
														{
															if (billUploadAC.ServiceTypes.Where(x => x.Id == 6).Count() > 0)
															{
																ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
															}
															else
															{
																csv.Dispose();

																if (File.Exists(Path.Combine(filepath, filename)))
																	File.Delete(Path.Combine(filepath, filename));

																importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
																importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
																importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

																return importBillDetail;
															}
														}
													}

												}
												//--------------- End : need to check static Ip in description ----

												#region --> TransType Validation 

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallType").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
													string fieldValue0 = string.Empty;
													if (filedAddressIndex0 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex0);
														var value = rowDataItems[index];
														fieldValue0 = Convert.ToString(value);
													}

													CallTransTypeStr = fieldValue0;// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);
													Transactiontypesetting transtypeDetail = new Transactiontypesetting();

													if (!string.IsNullOrEmpty(CallTransTypeStr))
													{
														transtypeDetail = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == CallTransTypeStr.Trim().ToLower());
														if (transtypeDetail != null)
														{
															if (transtypeDetail.Id > 0 && transtypeDetail.SetTypeAs != null)
															{
																isBusinessTransType = (transtypeDetail.SetTypeAs == (int)EnumList.CallType.Business ? true : false);
															}
															else
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Trans Type does not set exists system ! ";
															}
														}
														else
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Trans Type is not defined!";
														}
													}
													else
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Trans Type doesnot exists in excel ! ";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Trans Type mapping exists ! ";
												}
												#endregion

												#region --> Call Date Required and Format Validation Part

												string fieldAddress = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDate").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex = getAlphabatefromIndex(Convert.ToChar(fieldAddress.ToLower()));
												string fieldValue = string.Empty;
												if (filedAddressIndex != null)
												{
													int index = Convert.ToInt16(filedAddressIndex);
													var value = rowDataItems[index];
													fieldValue = Convert.ToString(value);
												}
												var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
												CallDateStr = fieldValue;// getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
																		 // string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
												if (!string.IsNullOrEmpty(CallDateStr))
												{

													bool isvalidDate = true;
													isvalidDate = CheckDate(CallDateStr);

													if (!isvalidDate)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
													}


													//DateTime dt;
													//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy", "dd-MM-yyyy", "dd-MM-yyyy HH:mm:ss" };
													//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
													//}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

												}
												#endregion

												#region --> Call Time Required and Format Validation Part

												string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallTime").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
												string fieldValue1 = string.Empty;
												if (filedAddressIndex1 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex1);
													var value = rowDataItems[index];
													fieldValue1 = Convert.ToString(value);
												}


												var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
												CallTimeStr = fieldValue1;// getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
												string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
												if (!string.IsNullOrEmpty(CallTimeStr) && (CallTimeStr != "00:00:00.000000" && CallTimeStr != "00:00:00"))
												{


													bool isvalidTime = true;
													isvalidTime = CheckDate(CallTimeStr);

													if (!isvalidTime)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
													}

													//DateTime dt;
													//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
													//             "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" ,"dd-MM-yyyy"};
													//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
													//}
													else
													{
														// _CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
														_CallTime = Convert.ToDateTime(CallTimeStr).TimeOfDay;

													}
												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now time not required !
														//  ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";
													}
													else
													{
														_CallTime = null;
													}

												}
												#endregion

												#region --> Call Duration Required and Format Validation Part

												string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDuration").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
												string fieldValue2 = string.Empty;
												if (filedAddressIndex2 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex2);
													var value = rowDataItems[index];
													fieldValue2 = Convert.ToString(value);
												}


												long DuractionSeconds = 0;
												string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
												var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

												string CallDurationStr = fieldValue2;// getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));
												DuractionSecondsStr = CallDurationStr;
												if (!string.IsNullOrEmpty(CallDurationStr))
												//   && (CallDurationStr != "00:00:00.000000" && CallDurationStr != "00:00:00")
												{

													if (durationFormat == "seconds")
													{
														long number;
														if (!long.TryParse(CallDurationStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															DuractionSeconds = Convert.ToInt64(CallDurationStr);
														}
													}
													else
													{
														if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
														{
															DateTime dt1 = DateTime.Parse(CallDurationStr);
															CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
														}
														//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

														DateTime dt;
														string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
														if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															TimeSpan ts = TimeSpan.Parse(CallDurationStr);
															double totalSeconds = ts.TotalSeconds;
															DuractionSeconds = Convert.ToInt64(totalSeconds);
														}
													}

												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now Duration not required !
														// ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";
													}
													else
													{
														DuractionSeconds = 0;
													}

												}
												#endregion

												#region --> Call Amount Required and Format Validation Part

												string fieldAddress3 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex3 = getAlphabatefromIndex(Convert.ToChar(fieldAddress3.ToLower()));
												string fieldValue3 = string.Empty;
												if (filedAddressIndex3 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex3);
													var value = rowDataItems[index];
													fieldValue3 = Convert.ToString(value);
												}

												CallAmountStr = fieldValue3; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
												if (!string.IsNullOrEmpty(CallAmountStr))
												{
													decimal number;
													if (!decimal.TryParse(CallAmountStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
												}
												#endregion

												#region --> Call Number Required and Format Validation Part
												string fieldAddress4 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerNumber").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex4 = getAlphabatefromIndex(Convert.ToChar(fieldAddress4.ToLower()));
												string fieldValue4 = string.Empty;
												if (filedAddressIndex4 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex4);
													var value = rowDataItems[index];
													fieldValue4 = Convert.ToString(value);
												}


												CallNumberStr = fieldValue4; // getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
												if (!string.IsNullOrEmpty(CallNumberStr))
												{

													if (!(CallNumberStr.Length > 5))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid. ";
													}
													else
													{
														Telephonenumberallocation telephoneNumber = new Telephonenumberallocation();
														telephoneNumber = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == CallNumberStr && !x.IsDelete && x.IsActive));

														if (telephoneNumber != null)
														{
															Telephonenumberallocationpackage packageData = new Telephonenumberallocationpackage();
															packageData = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage
																				.Where(x => (  // x.ServiceId == (long)EnumList.ServiceType.Mobility ||
																				x.ServiceId == ServiceTypeId)
																				&& x.TelephoneNumberAllocationId == telephoneNumber.Id && !x.IsDelete).FirstOrDefaultAsync();

															if (telephoneNumber.EmployeeId <= 0)
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not allocated to employee!";
															}

															if (packageData == null)
															{

																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Caller Number does not allocated mobility package!";

															}

														}
														else
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
														}

													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
												}
												#endregion

												#region --> MessageCount numeric Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MessageCount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress6 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MessageCount").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex6 = getAlphabatefromIndex(Convert.ToChar(fieldAddress6.ToLower()));
													string fieldValue6 = string.Empty;
													if (filedAddressIndex6 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex6);
														var value = rowDataItems[index];
														fieldValue6 = Convert.ToString(value);
													}
													MessageCountStr = fieldValue6; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
													if (!string.IsNullOrEmpty(MessageCountStr))
													{
														long number;
														if (!long.TryParse(MessageCountStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Message Count format is not valid";
														}
													}

												}

												#endregion

												#region --> CallDataKB numeric Format Validation Part
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDataKB").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress7 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDataKB").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex7 = getAlphabatefromIndex(Convert.ToChar(fieldAddress7.ToLower()));
													string fieldValue7 = string.Empty;
													if (filedAddressIndex7 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex7);
														var value = rowDataItems[index];
														fieldValue7 = Convert.ToString(value);
													}
													CallDataKBStr = fieldValue7; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
													if (!string.IsNullOrEmpty(CallDataKBStr))
													{
														decimal number;
														if (!decimal.TryParse(CallDataKBStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + " Call Data KB format is not valid";
														}
													}
												}

												#endregion

												#region --> Other Optional Fileds
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress8 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex8 = getAlphabatefromIndex(Convert.ToChar(fieldAddress8.ToLower()));
													string fieldValue8 = string.Empty;
													if (filedAddressIndex8 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex8);
														var value = rowDataItems[index];
														fieldValue8 = Convert.ToString(value);
													}
													CallerNameStr = fieldValue8;
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress9 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex9 = getAlphabatefromIndex(Convert.ToChar(fieldAddress9.ToLower()));
													string fieldValue9 = string.Empty;
													if (filedAddressIndex9 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex9);
														var value = rowDataItems[index];
														fieldValue9 = Convert.ToString(value);
													}
													SubscriptionTypeStr = fieldValue9;
												}
												#endregion


												if (IsFullValid)
												{
													Exceldetail data = new Exceldetail();
													// --> Required Field Data--------------------
													string callTransactionType = CallTransTypeStr;
													data.CallTransactionTypeId = (await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == callTransactionType.Trim().ToLower()))?.Id;
													data.TransType = callTransactionType;
													data.CallerNumber = CallNumberStr;
													data.CallAmount = Convert.ToDecimal(CallAmountStr);
													data.CallDate = Convert.ToDateTime(CallDateStr);

													if (_CallTime != null)
													{
														data.CallTime = _CallTime;
													}
													// Call duration hh:mm:ss to long convert and stored
													data.CallDuration = DuractionSeconds;

													// --> Optional Field Data--------------------
													data.Description = descriptionTextStr;
													data.CallerName = CallerNameStr;
													data.SubscriptionType = SubscriptionTypeStr;
													data.MessageCount = string.IsNullOrEmpty(MessageCountStr) ? 0 : Convert.ToInt32(MessageCountStr);
													data.CallDataKB = string.IsNullOrEmpty(CallDataKBStr) ? 0 : Convert.ToDecimal(CallDataKBStr);
													data.CallWithinGroup = false;
													data.SiteName = string.Empty;
													data.GroupDetail = string.Empty;
													data.Bandwidth = string.Empty;
													data.MonthlyPrice = null;
													data.CommentOnPrice = string.Empty;
													data.CommentOnBandwidth = string.Empty;
													data.ReceiverNumber = string.Empty;
													data.ReceiverName = string.Empty;

													data.EmployeeId = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == data.CallerNumber && !x.IsDelete && x.IsActive))?.EmployeeId;
													data.ServiceTypeId = ServiceTypeId; // (long)EnumList.ServiceType.Mobility;
													data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
													data.ExcelUploadLogId = 0;
													MstEmployee mstemp = new MstEmployee();
													mstemp = (await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == data.EmployeeId));
													if (mstemp != null)
													{
														data.BusinessUnitId = mstemp.BusinessUnitId;
														data.CostCenterId = mstemp.CostCenterId;
													}
													data.ExcelUploadLogId = 0;
													data.GroupId = null;
													data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
													data.AssignType = null;

													if (isBusinessOnly)
													{
														data.AssignType = (long)EnumList.AssignType.Business;
													}
													else
													{
														//if anount is negative than it will be always assigned to Business charge 
														if (data.CallAmount < 0)
														{
															data.AssignType = (long)EnumList.AssignType.Business;
														}
														else
														{
															#region Get Assign Type Logic from Trans Type
															if (data.CallTransactionTypeId > 0)
															{
																long? ChargeType = (await _dbTeleBilling_V01Context.Transactiontypesetting.FindAsync(data.CallTransactionTypeId))?.SetTypeAs;
																if (ChargeType > 0)
																{
																	data.AssignType = ChargeType;
																}
																else
																{
																	if (data.EmployeeId > 0)
																	{
																		data.AssignType = (long)EnumList.AssignType.Employee;
																	}
																	else if (data.BusinessUnitId > 0)
																	{
																		data.AssignType = (long)EnumList.AssignType.Business;
																	}
																}
															}
															#endregion
														}
													}


													datalist.Add(data);
												}
												else
												{
													datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
													{
														CallerName = CallerNameStr,
														CallType = CallTransTypeStr,
														Description = descriptionTextStr,
														CallerNumber = CallNumberStr,
														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														CallDataKB = CallDataKBStr,
														MessageCount = MessageCountStr,
														ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        FileGuidNo = filename,
                                                        ServiceTypeId = ServiceTypeId,
                                                        CallerName = CallerNameStr,
                                                        TransType = CallTransTypeStr,
                                                        Description = descriptionTextStr,
                                                        CallerNumber = CallNumberStr,
                                                        CallAmount = CallAmountStr,
                                                        CallDate = CallDateStr,
                                                        CallTime = CallTimeStr,
                                                        CallDuration = DuractionSecondsStr,
                                                        CallDataKb = CallDataKBStr,
                                                        MessageCount = MessageCountStr,
                                                        ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                    });

                                                }

											}
											catch (Exception e)
											{
												if (e.GetType() != typeof(System.NullReferenceException))
												{
													datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
													{
														CallerName = CallerNameStr,
														CallType = CallTransTypeStr,
														Description = descriptionTextStr,
														CallerNumber = CallNumberStr,
														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														CallDataKB = CallDataKBStr,
														MessageCount = MessageCountStr,
														ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
													});

                                                    datalistError.Add(new ExceldetailError
                                                    {
                                                        ExcelUploadLogId = 0,
                                                        FileGuidNo = filename,
                                                        ServiceTypeId = ServiceTypeId,
                                                        CallerName = CallerNameStr,
                                                        TransType = CallTransTypeStr,
                                                        Description = descriptionTextStr,
                                                        CallerNumber = CallNumberStr,
                                                        CallAmount = CallAmountStr,
                                                        CallDate = CallDateStr,
                                                        CallTime = CallTimeStr,
                                                        CallDuration = DuractionSecondsStr,
                                                        CallDataKb = CallDataKBStr,
                                                        MessageCount = MessageCountStr,
                                                        ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                    });
                                                }

											}

										}

									}
                                    LogManager.Configuration.Variables["user"] = "";
                                    LogManager.Configuration.Variables["stepno"] = "6";
                                    _logger.Info("Over: Reading data one by one and validate with database.");
                                }
								else
								{
									csv.Dispose();

									if (File.Exists(Path.Combine(filepath, filename)))
										File.Delete(Path.Combine(filepath, filename));

									importBillDetail.Message = "Data not found in csv file.";
									importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
									return importBillDetail;
								}

							}

							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}

						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}


				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = worksheetNo;
						int readingIndex = ReadingIndex;
						string TitleHeader = string.Empty;
						string TitleReadingColumn = string.Empty;
						if (mappingExcel != null)
						{
							#region --> set reading index for current Service type
							TitleHeader = (mappingExcel.HaveTitle ? mappingExcel.TitleName : "");
							if (ReadingIndex < 0)
							{
								readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
								TitleReadingColumn = (mappingExcel.ExcelColumnNameForTitle);
							}
							else
							{
								readingIndex = (int)ReadingIndex;
							}

							#endregion
						}

						#region --> Read  .xls or .xlsx File Using NPOI Package Class
						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							if (hssfwb.NumberOfSheets >= 0 && hssfwb.NumberOfSheets >= (worksheetno - 1))
								sheet = hssfwb.GetSheetAt(worksheetno - 1);
							else
							{
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Worksheet No." + Convert.ToString(worksheetno) + " is out of range!";
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}
						}
						#endregion

						if (sheet.LastRowNum > 0)
						{
							int rowcount = sheet.LastRowNum + 1;
							bool IsServiceTitleRow = false;
                            LogManager.Configuration.Variables["user"] = "";
                            LogManager.Configuration.Variables["stepno"] = "5";
                            _logger.Info("Start Reading data one by one and validate with database.");

                            // ---- Global Variable 
                            long CurrencyId = 0;
                            CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId ?? 0;
                            // ---- Global Variable 

                            for (int j = readingIndex; j <= rowcount; j++)
							{

                                // -------- Common variable Data ------------
                                long? _CallTransactionTypeId = 0;
                                long? _EmployeeId = 0;
                                long? _BusinessUnitId = 0;
                                long? _CostCenterId = 0;
                                long? _ChargeType = 0;
                                //------------ validate variable-----------------------

                                string _TransTypestr = string.Empty;
                                string _CallerNumberstr = string.Empty;
                                string _CallAmountStr = string.Empty;
                                string _CallDatestr = string.Empty;
                                string _CallTimestr = string.Empty;

                                // ----- End : Common Varaiable Data --------


                                IsServiceTitleRow = false;
								int intIndex = (j > 0 ? j - 1 : j); // getRow index start from 0 nad Address call start from 1
								IRow row = sheet.GetRow(intIndex);
								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}

								#region --> Check next row having any title for next service or not 
								else if ((ServiceTitle != null) && (ServiceTitle.Count() > 0))
								{
									int TitleColumNo = 0;
									// find service title nd column wise first index
									#region --> Forloop forget title from each mapped column at current row
									foreach (var service in singleWorksheetserviceList
												   .Where(x => x.WorkSheetNo == worksheetno)
													.OrderBy(x => x.ExcelColumnNameForTitle).ToList())
									{

										if (service.ExcelColumnNameForTitle.All(Char.IsLetter))
										{
											TitleColumNo = CellReference.ConvertColStringToIndex(service.ExcelColumnNameForTitle);
											if (row == null || (row.GetCell(TitleColumNo) != null && row.GetCell(TitleColumNo).CellType == CellType.Blank)) continue;
											else
											{
												string getStrVal = Convert.ToString(row.GetCell(TitleColumNo));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = intIndex + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0;// to stop the loop
															IsServiceTitleRow = true;
															break;
															//return importBillDetail;

														}
													}
													else
													{
														IsServiceTitleRow = true;
														break;
													}

												}

											}

										}

										else if (service.ExcelColumnNameForTitle.All(Char.IsLetterOrDigit))
										{
											var dynCR = new CellReference(service.ExcelColumnNameForTitle);
											if (dynCR.Row >= 0)
											{
												IRow rowR = sheet.GetRow(dynCR.Row);
												var cell = rowR.GetCell(dynCR.Col);
												string getStrVal = Convert.ToString(row.GetCell(dynCR.Col));
												if (!string.IsNullOrEmpty(getStrVal))
												{
													if (TitleHeader.ToLower().Trim() != getStrVal.ToLower().Trim()) // for same service title do not check
													{
														bool containsAny = ServiceTitle.Any(getStrVal.ToLower().Trim().Contains);
														if (containsAny)
														{
															readingIndex = dynCR.Row + 1;
															importBillDetail.ServiceTypeId = service.ServiceTypeId;
															importBillDetail.ServiceTypeId = singleWorksheetserviceList.FirstOrDefault(x => x.TitleName.ToLower().Trim() == getStrVal.ToLower().Trim()).ServiceTypeId;

															importBillDetail.ReadingIndex = readingIndex;
															rowcount = 0;// to stop the loop
															IsServiceTitleRow = true;
															break;
															// return importBillDetail;                                             
														}
													}
													else
													{
														IsServiceTitleRow = true;
														break;
													}

												}

											}
										}

									} // end of for loop
									#endregion

								} // end of if  (ServiceTitle != null)

								#endregion
								if (!IsServiceTitleRow)
								{
									if (row != null || row.Cells.All(d => d.CellType != CellType.Blank))
									{
										string CallDataKBStr = string.Empty;
										string MessageCountStr = string.Empty;
										try
										{

											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											bool isBusinessTransType = false; // If type is business Call Time,Duration is not required 
											TimeSpan? _CallTime = null;


											// ------------- Need to check if Static ip In description -----
											string descriptionText = string.Empty;

											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												descriptionText = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet, (long)EnumList.SupportDataType.String);
												if (!string.IsNullOrEmpty(descriptionText))
												{

													descriptionText = Regex.Replace(descriptionText, @"s", "");
													descriptionText = descriptionText.Replace(" ", "");
													descriptionText = descriptionText.Trim().ToUpper();

													string StaticIP = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_StaticIP");
													string VoiceOnly = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_VoiceOnly");
													string InternetPlanDevice = _config.GetValue<string>("ServiceIdentificationKeyWord:KeyWord_InternetPlannDevice");

													List<string> InternetPlannTagIds = InternetPlanDevice.Split(',').ToList();

													if (descriptionText.Contains(StaticIP.ToUpper()))
													{
														if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.StaticIP).Count() > 0)
														{
															ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
														}
														else
														{
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
															importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

															return importBillDetail;
														}
													}

													else if (descriptionText.Contains(VoiceOnly.ToUpper()))
													{
														if (billUploadAC.ServiceTypes.Where(x => x.Id == (long)EnumList.ServiceType.VoiceOnly).Count() > 0)
														{
															ServiceTypeId = (long)EnumList.ServiceType.VoiceOnly;
														}
														else
														{
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
															importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

															return importBillDetail;
														}
													}

													else if (InternetPlannTagIds.Any(w => descriptionText.Contains(w.ToUpper())))
													{
														if (billUploadAC.ServiceTypes.Where(x => x.Id == 6).Count() > 0)
														{
															ServiceTypeId = (long)EnumList.ServiceType.StaticIP;
														}
														else
														{
															if (File.Exists(Path.Combine(filepath, filename)))
																File.Delete(Path.Combine(filepath, filename));

															importBillDetail.Message = "Sorry, We could not upload file with multiple service. Please select required service and try again.  ";
															importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.MultipleServiceFound);
															importBillDetail.UploadData = new ResponseDynamicDataAC<MobilityUploadListAC>();

															return importBillDetail;
														}
													}
												}

											}

											//--------------- End : need to check static Ip in description ----

											#region --> TransType Validation 
											string CallTransTypeStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet, (long)EnumList.SupportDataType.String);
											Transactiontypesetting transtypeDetail = new Transactiontypesetting();
                                            _TransTypestr = CallTransTypeStr;
                                            if (!string.IsNullOrEmpty(CallTransTypeStr))
											{
                                               

                                                transtypeDetail = await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == CallTransTypeStr.Trim().ToLower());
												if (transtypeDetail != null)
												{
                                                    if (transtypeDetail.Id > 0 && transtypeDetail.SetTypeAs != null)
                                                    {
                                                        _CallTransactionTypeId = transtypeDetail.Id;
                                                        _ChargeType = transtypeDetail.SetTypeAs;

                                                        isBusinessTransType = (transtypeDetail.SetTypeAs == (int)EnumList.CallType.Business ? true : false);
                                                    }
                                                    else
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Trans Type does not set exists system ! ";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Trans Type is not defined!";
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Trans Type doesnot exists in excel ! ";
											}

											#endregion

											#region --> Call Date Required and Format Validation Part

											var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
											string CallDateStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
                                            // string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
                                            _CallDatestr = CallDateStr;
                                            if (!string.IsNullOrEmpty(CallDateStr))
											{

												bool isvalidDate = true;
												isvalidDate = CheckDate(CallDateStr);

												if (!isvalidDate)
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
												}

												//DateTime dt;
												//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy" };
												//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
												//{
												//    IsFullValid = false;
												//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
												//}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

											}
											#endregion

											#region --> Call Time Required and Format Validation Part

											var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
											string CallTimeStr = getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
                                            // string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
                                            _CallTimestr = CallTimeStr;
                                            if (!string.IsNullOrEmpty(CallTimeStr) && (CallTimeStr != "00:00:00.000000" && CallTimeStr != "00:00:00"))
											{

												bool isvalidTime = true;
												isvalidTime = CheckDate(CallTimeStr);

												if (!isvalidTime)
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
												}
												else
												{
													_CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;

												}
												//DateTime dt;
												//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
												//                 "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
												//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
												//{
												//    IsFullValid = false;
												//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
												//}
												//else
												//{
												//    _CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
												//}
											}
											else
											{
												if (!isBusinessTransType)
												{
													// IsFullValid = false; from now time not required !
													//  ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";
												}
												else
												{
													_CallTime = null;
												}

											}
											#endregion

											#region --> Call Duration Required and Format Validation Part
											long DuractionSeconds = 0;
											string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
											var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

											string CallDurationStr = getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));
											DuractionSecondsStr = CallDurationStr;
											if (!string.IsNullOrEmpty(CallDurationStr))
											//   && (CallDurationStr != "00:00:00.000000" && CallDurationStr != "00:00:00")
											{

												if (durationFormat == "seconds")
												{
													long number;
													if (!long.TryParse(CallDurationStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
													}
													else
													{
														DuractionSeconds = Convert.ToInt64(CallDurationStr);
													}
												}
												else
												{
													if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
													{
														DateTime dt1 = DateTime.Parse(CallDurationStr);
														CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
													}
													//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

													DateTime dt;
													string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
													if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
													}
													else
													{
														TimeSpan ts = TimeSpan.Parse(CallDurationStr);
														double totalSeconds = ts.TotalSeconds;
														DuractionSeconds = Convert.ToInt64(totalSeconds);
													}
												}

											}
											else
											{
												if (!isBusinessTransType)
												{
													// IsFullValid = false; from now Duration not required !
													// ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";
												}
												else
												{
													DuractionSeconds = 0;
												}

											}
											#endregion

											#region --> Call Amount Required and Format Validation Part

											string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
                                            //string CallAmountStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value.ToString());
                                            _CallAmountStr = CallAmountStr;
                                            if (!string.IsNullOrEmpty(CallAmountStr))
											{
												decimal number;
												if (!decimal.TryParse(CallAmountStr, out number))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
											}
											#endregion

											#region --> Call Number Required and Format Validation Part
											string CallNumberStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
                                            //string CallNumberStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", i)].Value.ToString());
                                            _CallerNumberstr = CallNumberStr;
                                            if (!string.IsNullOrEmpty(CallNumberStr))
											{

												if (!(CallNumberStr.Length > 5))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid. ";
												}
												else
												{
													Telephonenumberallocation telephoneNumber = new Telephonenumberallocation();
													telephoneNumber = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == CallNumberStr && !x.IsDelete && x.IsActive));

													if (telephoneNumber != null)
													{
														Telephonenumberallocationpackage packageData = new Telephonenumberallocationpackage();
														//packageData = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage.Where(x => x.ServiceId == (long)EnumList.ServiceType.Mobility && x.TelephoneNumberAllocationId == telephoneNumber.Id && !x.IsDelete).FirstOrDefaultAsync();
														packageData = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage
																			   .Where(x => (x.ServiceId == ServiceTypeId)
																			   && x.TelephoneNumberAllocationId == telephoneNumber.Id && !x.IsDelete).FirstOrDefaultAsync();
                                                       
                                                        if (telephoneNumber.EmployeeId > 0 && packageData != null && packageData.Id > 0)
                                                        {
                                                            _EmployeeId = telephoneNumber.EmployeeId;
                                                            MstEmployee mstemp = new MstEmployee();
                                                            mstemp = (await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == telephoneNumber.EmployeeId));
                                                            if (mstemp != null)
                                                            {
                                                                _BusinessUnitId = mstemp.BusinessUnitId;
                                                                _CostCenterId = mstemp.CostCenterId;
                                                            }
                                                        }


                                                        if (telephoneNumber.EmployeeId <= 0)
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not allocated to employee!";
														}

														if (packageData == null)
														{
															string serviceName = CommonFunction.GetFixServiceList().Where(x => x.Id == ServiceTypeId).Select(x => x.Name).FirstOrDefault().ToString();
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Caller Number does not allocated " + serviceName + " package!";
														}

													}
													else
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
													}

												}
											}
											else
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
											}
											#endregion

											#region --> MessageCount numeric Format Validation Part
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "MessageCount").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												MessageCountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "MessageCount", j), sheet, (long)EnumList.SupportDataType.String);

												if (!string.IsNullOrEmpty(MessageCountStr))
												{
													long number;
													if (!long.TryParse(MessageCountStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Message Count format is not valid";
													}
												}

											}

											#endregion

											#region --> CallDataKB numeric Format Validation Part
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDataKB").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												CallDataKBStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDataKB", j), sheet, (long)EnumList.SupportDataType.String);
												if (!string.IsNullOrEmpty(CallDataKBStr))
												{
													decimal number;
													if (!decimal.TryParse(CallDataKBStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + " Call Data KB format is not valid";
													}
												}
											}

											#endregion




											if (IsFullValid)
											{
												Exceldetail data = new Exceldetail();


												// --> Required Field Data--------------------											
                                                data.CallTransactionTypeId = _CallTransactionTypeId;// (await _dbTeleBilling_V01Context.Transactiontypesetting.FirstOrDefaultAsync(x => x.ProviderId == billUploadAC.ProviderId && !x.IsDelete && x.TransactionType.Trim().ToLower() == callTransactionType.Trim().ToLower()))?.Id;
                                                data.TransType = _TransTypestr;
                                                data.CallerNumber = _CallerNumberstr;//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
                                                data.CallAmount = string.IsNullOrEmpty(_CallAmountStr) ? 0 : Convert.ToDecimal(_CallAmountStr);
                                                data.CallDate = string.IsNullOrEmpty(_CallDatestr) ? data.CallDate : Convert.ToDateTime(_CallDatestr);

                                                if (_CallTime != null)
												{
                                                    data.CallTime = _CallTime;// Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
												}
												// Call duration hh:mm:ss to long convert and stored
												data.CallDuration = DuractionSeconds;

												// --> Optional Field Data--------------------

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													data.CallerName = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet, (long)EnumList.SupportDataType.String);
												}

												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													data.Description = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet, (long)EnumList.SupportDataType.String);
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "SubscriptionType").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													data.SubscriptionType = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "SubscriptionType", j), sheet, (long)EnumList.SupportDataType.String);
												}

												data.MessageCount = string.IsNullOrEmpty(MessageCountStr) ? 0 : Convert.ToInt32(MessageCountStr);
												data.CallDataKB = string.IsNullOrEmpty(CallDataKBStr) ? 0 : Convert.ToDecimal(CallDataKBStr);

												data.CallWithinGroup = false;
												data.SiteName = string.Empty;
												data.GroupDetail = string.Empty;
												data.Bandwidth = string.Empty;
												data.MonthlyPrice = null;
												data.CommentOnPrice = string.Empty;
												data.CommentOnBandwidth = string.Empty;
												data.ReceiverNumber = string.Empty;
												data.ReceiverName = string.Empty;
                                                data.EmployeeId = _EmployeeId;// (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == data.CallerNumber && !x.IsDelete && x.IsActive))?.EmployeeId;
                                                data.ServiceTypeId = ServiceTypeId; // (long)EnumList.ServiceType.Mobility;
                                                data.CurrencyId = CurrencyId;// (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
                                                data.ExcelUploadLogId = 0;
                                                data.BusinessUnitId = _BusinessUnitId;
                                                data.CostCenterId = _CostCenterId;
                                                data.ExcelUploadLogId = 0;
												data.GroupId = null;
												data.IsAssigned = (data.EmployeeId > 0 || data.BusinessUnitId > 0) ? true : false;
												data.AssignType = null;

												if (isBusinessOnly)
												{
													data.AssignType = (long)EnumList.AssignType.Business;
												}
												else
												{
													//if anount is negative than it will be always assigned to Business charge 
													if (data.CallAmount < 0)
													{
														data.AssignType = (long)EnumList.AssignType.Business;
													}
													else
													{
														#region Get Assign Type Logic from Trans Type
														if (data.CallTransactionTypeId > 0)
														{
                                                            if (_ChargeType > 0)
                                                            {
                                                                data.AssignType = _ChargeType;
                                                            }
                                                            else
															{
																if (data.EmployeeId > 0)
																{
																	data.AssignType = (long)EnumList.AssignType.Employee;
																}
																else if (data.BusinessUnitId > 0)
																{
																	data.AssignType = (long)EnumList.AssignType.Business;
																}
															}
														}
														#endregion
													}
												}

												datalist.Add(data);

											}
											else
											{

												string dDescription = string.Empty;
												string dCallername = string.Empty;
												// --> Optional Field Data--------------------
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);

												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													dCallername = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet);
												}

												datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
												{
													CallerName = dCallername, //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet),
                                                    CallType = _TransTypestr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet),
                                                    Description = dDescription,
                                                    CallerNumber = _CallerNumberstr,//   getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
                                                    CallAmount = _CallAmountStr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
                                                    CallDate = _CallDatestr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
                                                    CallTime = _CallTimestr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
                                                    CallDuration = DuractionSecondsStr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
													CallDataKB = CallDataKBStr,
													MessageCount = MessageCountStr,
													ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    FileGuidNo = filename,
                                                    ServiceTypeId = ServiceTypeId,
                                                    CallerName = dCallername,
                                                    TransType = _TransTypestr,
                                                    Description = dDescription,
                                                    CallerNumber = _CallerNumberstr,
                                                    CallAmount = _CallAmountStr,
                                                    CallDate = _CallDatestr,
                                                    CallTime = _CallTimestr,
                                                    CallDuration = DuractionSecondsStr,
                                                    CallDataKb = CallDataKBStr,
                                                    MessageCount = MessageCountStr,
                                                    ErrorSummary = "At :" + j.ToString() + " " + ErrorMessageSummary
                                                });

                                            }

										}
										catch (Exception e)
										{


											if (e.GetType() != typeof(System.NullReferenceException))
											{
												string dDescription = string.Empty;
												string dCallername = string.Empty;
												// --> Optional Field Data--------------------
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);
												}
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerName").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													dCallername = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet);
												}

												datalistInvalid.Add(new MobilityExcelUploadDetailStringAC
												{
													CallerName = dCallername, //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerName", j), sheet),
                                                    CallType = _TransTypestr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallType", j), sheet),
                                                    Description = dDescription,
                                                    CallerNumber = _CallerNumberstr,//   getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
                                                    CallAmount = _CallAmountStr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
                                                    CallDate = _CallDatestr,//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
                                                    CallTime = _CallTimestr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
                                                    CallDuration = DuractionSecondsStr,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
													CallDataKB = CallDataKBStr,
													MessageCount = MessageCountStr,
													ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
												});

                                                datalistError.Add(new ExceldetailError
                                                {
                                                    ExcelUploadLogId = 0,
                                                    FileGuidNo = filename,
                                                    ServiceTypeId = ServiceTypeId,
                                                    CallerName = dCallername,
                                                    TransType = _TransTypestr,
                                                    Description = dDescription,
                                                    CallerNumber = _CallerNumberstr,
                                                    CallAmount = _CallAmountStr,
                                                    CallDate = _CallDatestr,
                                                    CallTime = _CallTimestr,
                                                    CallDuration = DuractionSecondsStr,
                                                    CallDataKb = CallDataKBStr,
                                                    MessageCount = MessageCountStr,
                                                    ErrorSummary = "At :" + j.ToString() + " " + "Error :" + e.Message
                                                });
                                            }

										}
									}
								}

							}
                            LogManager.Configuration.Variables["user"] = "";
                            LogManager.Configuration.Variables["stepno"] = "6";
                            _logger.Info("Over: Reading data one by one and validate with database.");
                        }

					}
				}

				UploadListAC.InvalidMobilityList = datalistInvalid;
				UploadListAC.ValidMobilityList = datalist;
                UploadListAC.InvalidListAllDB = datalistError;

				ResponseDynamicDataAC<MobilityUploadListAC> responseData = new ResponseDynamicDataAC<MobilityUploadListAC>();
				responseData.Data = UploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}


				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}

		}



		public async Task<ImportBillDetailAC<VoipUploadListAC>> ReadExcelForVoip(string filepath, string filename, MappingDetailAC mappingExcel, BillUploadAC billUploadAC)
		{
			ImportBillDetailAC<VoipUploadListAC> importBillDetail = new ImportBillDetailAC<VoipUploadListAC>();
			List<Skypeexceldetail> datalist = new List<Skypeexceldetail>();
			List<VoipExcelUploadDetailStringAC> datalistInvalid = new List<VoipExcelUploadDetailStringAC>();

			try
			{
				VoipUploadListAC voipUploadListAC = new VoipUploadListAC();
				bool isBusinessOnly = await GetServiceChargeType((long)EnumList.ServiceType.VOIP);
				string _CallerNumber = string.Empty;
				string _ReceiverNumber = string.Empty;
				CallerDetailResponseAC _callDt = new CallerDetailResponseAC();

				#region --> Read Excel file   
				ISheet sheet;
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);

				if (sFileExtension == ".csv")
				{
					#region -- > Ankit Code Help   To read CSV File

					int worksheetno = 0;
					int readingIndex = 0;
					if (mappingExcel != null)
					{
						worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
						readingIndex = (mappingExcel.HaveHeader ? 1 : 0);
						readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);
						// SPECIALLY FOR csv READER ONLY
						readingIndex = (readingIndex > 1 ? readingIndex : 0);
					}
					string csvpath = fullPath;

					try
					{
						using (var reader = new StreamReader(csvpath))
						using (var csv = new CsvReader(reader))
						{
							csv.Configuration.HasHeaderRecord = mappingExcel.HaveHeader;
							try
							{
								var csvRecords = csv.GetRecords<dynamic>().ToList();

								if (csvRecords != null)
								{
									int rowcount = csvRecords.Count();
									for (int j = readingIndex; j < rowcount; j++)
									{
										var itemvalue = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Where(x => x.Value.ToString() != "").ToList();

										if (itemvalue == null || itemvalue.Count() == 0)
										{
											continue;
										}

										if (itemvalue.Count() > 0)
										{
											var rowDataItems = ((IDictionary<string, object>)csvRecords.ElementAtOrDefault(j)).Values.ToList();

											#region --> Coomon Data Variable declaration 
											bool IsFullValid = true;
											string ErrorMessageSummary = string.Empty;
											bool isBusinessTransType = false; // If type is business Call Time,Duration is not required 
											TimeSpan? _CallTime = null;
											string descriptionText = string.Empty;
											string descriptionTextStr = string.Empty;
											string CallTransTypeStr = string.Empty;
											string CallNumberStr = string.Empty;
											string CallAmountStr = string.Empty;
											string CallTimeStr = string.Empty;
											string DuractionSecondsStr = string.Empty;
											string CallDateStr = string.Empty;
											string CallerNameStr = string.Empty;
											string ReceivNumberStr = string.Empty;

											#endregion
											try
											{

												#region --> description get from cell
												if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
												{
													string fieldAddress0 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault();
													int? filedAddressIndex0 = getAlphabatefromIndex(Convert.ToChar(fieldAddress0.ToLower()));
													string fieldValue0 = string.Empty;
													if (filedAddressIndex0 != null)
													{
														int index = Convert.ToInt16(filedAddressIndex0);
														var value = rowDataItems[index];
														fieldValue0 = Convert.ToString(value);
													}
													descriptionTextStr = fieldValue0;
													descriptionText = fieldValue0;
												}
												#endregion

												#region --> Call Date Required and Format Validation Part

												string fieldAddress = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDate").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex = getAlphabatefromIndex(Convert.ToChar(fieldAddress.ToLower()));
												string fieldValue = string.Empty;
												if (filedAddressIndex != null)
												{
													int index = Convert.ToInt16(filedAddressIndex);
													var value = rowDataItems[index];
													fieldValue = Convert.ToString(value);
												}
												var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
												CallDateStr = fieldValue;
												if (!string.IsNullOrEmpty(CallDateStr))
												{

													bool isvalidDate = true;
													isvalidDate = CheckDate(CallDateStr);

													if (!isvalidDate)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
													}


													//DateTime dt;
													//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy", "dd-MM-yyyy", "dd-MM-yyyy HH:mm:ss" };
													//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
													//}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

												}
												#endregion

												#region --> Call Time Required and Format Validation Part

												string fieldAddress1 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallTime").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex1 = getAlphabatefromIndex(Convert.ToChar(fieldAddress1.ToLower()));
												string fieldValue1 = string.Empty;
												if (filedAddressIndex1 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex1);
													var value = rowDataItems[index];
													fieldValue1 = Convert.ToString(value);
												}


												var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
												CallTimeStr = fieldValue1;// getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
												string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
												if (!string.IsNullOrEmpty(CallTimeStr) && (CallTimeStr != "00:00:00.000000" && CallTimeStr != "00:00:00"))
												{
													bool isvalidTime = true;

													CallTimeStr = CallTimeStr.Replace(".", ":");
													isvalidTime = CheckDate(CallTimeStr);

													if (!isvalidTime)
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
													}

													//DateTime dt;
													//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
													//             "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss" ,"dd-MM-yyyy"};
													//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
													//{
													//    IsFullValid = false;
													//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
													//}
													else
													{
														// _CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
														_CallTime = Convert.ToDateTime(CallTimeStr).TimeOfDay;

													}
												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now time not required !
														//  ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";
													}
													else
													{
														_CallTime = null;
													}

												}
												#endregion

												#region --> Call Duration Required and Format Validation Part

												string fieldAddress2 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallDuration").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex2 = getAlphabatefromIndex(Convert.ToChar(fieldAddress2.ToLower()));
												string fieldValue2 = string.Empty;
												if (filedAddressIndex2 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex2);
													var value = rowDataItems[index];
													fieldValue2 = Convert.ToString(value);
												}


												long DuractionSeconds = 0;
												string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
												var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

												string CallDurationStr = fieldValue2;// getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));


												DuractionSecondsStr = CallDurationStr;
												if (!string.IsNullOrEmpty(CallDurationStr))
												//   && (CallDurationStr != "00:00:00.000000" && CallDurationStr != "00:00:00")
												{

													if (durationFormat == "seconds")
													{
														long number;
														if (!long.TryParse(CallDurationStr, out number))
														{
															IsFullValid = false;
															ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
														}
														else
														{
															DuractionSeconds = Convert.ToInt64(CallDurationStr);
														}
													}
													else
													{
														if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
														{
															DateTime dt1 = DateTime.Parse(CallDurationStr);
															CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
														}

														else if (CallDurationStr.Contains('.'))
														{
															DateTime dtDiff;
															string[] formatsff = { "mm:ss.f", "mm:ss.ff", "mm:ss.fff", "hh:mm:ss.f", "hh:mm:ss.ff", "hh:mm:ss.fff" };
															if (DateTime.TryParseExact(CallDurationStr, formatsff, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtDiff))
															{
																CallDurationStr = "2019-11-02 00:" + fieldValue2.Split('.')[0];
																DateTime dt1 = DateTime.ParseExact(CallDurationStr, "yyyy-MM-dd HH:mm:ss", null);
																CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
																TimeSpan ts = TimeSpan.Parse(CallDurationStr);
																double totalSeconds = ts.TotalSeconds;
																DuractionSeconds = Convert.ToInt64(totalSeconds);
															}
														}
														else
														{
															DateTime dt;
															string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss", "hh:mm:ss tt", "hh:mm:ss", "mm:ss", "hh:mm:ss", "mm:ss" };
															if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
															{
																IsFullValid = false;
																ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
															}
															else
															{
																TimeSpan ts = TimeSpan.Parse(CallDurationStr);
																double totalSeconds = ts.TotalSeconds;
																DuractionSeconds = Convert.ToInt64(totalSeconds);
															}
														}

													}

												}
												else
												{
													if (!isBusinessTransType)
													{
														// IsFullValid = false; from now Duration not required !
														// ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";
													}
													else
													{
														DuractionSeconds = 0;
													}

												}
												#endregion

												#region --> Call Amount Required and Format Validation Part

												string fieldAddress3 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallAmount").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex3 = getAlphabatefromIndex(Convert.ToChar(fieldAddress3.ToLower()));
												string fieldValue3 = string.Empty;
												if (filedAddressIndex3 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex3);
													var value = rowDataItems[index];
													fieldValue3 = Convert.ToString(value);
												}

												CallAmountStr = fieldValue3;
												CallAmountStr = RemoveSpecialChars(CallAmountStr);
												if (!string.IsNullOrEmpty(CallAmountStr))
												{
													decimal number;
													if (!decimal.TryParse(CallAmountStr, out number))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
												}
												#endregion

												#region --> Call Number Required and Format Validation Part
												string fieldAddress4 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "CallerNumber").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex4 = getAlphabatefromIndex(Convert.ToChar(fieldAddress4.ToLower()));
												string callernumberFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallerNumber");
												string fieldValue4 = string.Empty;
												if (filedAddressIndex4 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex4);
													var value = rowDataItems[index];
													fieldValue4 = Convert.ToString(value);
												}


												CallNumberStr = fieldValue4; // getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
												if (!string.IsNullOrEmpty(CallNumberStr))
												{
													if (callernumberFormat == "From: +XXXXXXXXXXXXX To: +XXXXXXXXXX")
													{
														_callDt = getCallerReceiverNumber(CallNumberStr);
														_CallerNumber = _callDt.CallerNumber;
														_ReceiverNumber = _callDt.ReceiverNumber;
													}
													else if (callernumberFormat == "onlynumber(XXXXXXXXXX)")
													{
														_CallerNumber = CallNumberStr;
													}
													else // Other pending
													{
														_CallerNumber = CallNumberStr;

													}

													if (!(_CallerNumber.Length > 5))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
												}
												#endregion

												#region --> Receiver Number Required and Format Validation Part
												string fieldAddress5 = mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "ReceiverNumber").Select(x => x.ExcelcolumnName).FirstOrDefault();
												int? filedAddressIndex5 = getAlphabatefromIndex(Convert.ToChar(fieldAddress5.ToLower()));
												string receivernumberFormat = getFormatField(mappingExcel.DBFiledMappingList, "ReceiverNumber");
												string fieldValue5 = string.Empty;
												if (filedAddressIndex5 != null)
												{
													int index = Convert.ToInt16(filedAddressIndex5);
													var value = rowDataItems[index];
													fieldValue5 = Convert.ToString(value);
												}


												ReceivNumberStr = fieldValue5;

												if (!string.IsNullOrEmpty(ReceivNumberStr))
												{
													if (receivernumberFormat == "From: +XXXXXXXXXXXXX To: +XXXXXXXXXX")
													{
														_callDt = getCallerReceiverNumber(ReceivNumberStr);
														_CallerNumber = _callDt.CallerNumber;
														_ReceiverNumber = _callDt.ReceiverNumber;
													}
													else if (receivernumberFormat == "onlynumber(XXXXXXXXXX)")
													{
														_ReceiverNumber = ReceivNumberStr;
													}
													else // Other pending
													{
														_ReceiverNumber = ReceivNumberStr;

													}
													if (!(_ReceiverNumber.Length > 5))
													{
														IsFullValid = false;
														ErrorMessageSummary = ErrorMessageSummary + "Receiver Number is not valid";
													}
												}
												else
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Receiver Number doesnot exists ! ";
												}
												#endregion


												if (IsFullValid)
												{
													Skypeexceldetail data = new Skypeexceldetail();

													data.CallerNumber = _CallerNumber;//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
													data.ReceiverNumber = _ReceiverNumber;
													CallAmountStr = RemoveSpecialChars(CallAmountStr);
													data.CallAmount = Convert.ToDecimal(CallAmountStr);
													data.CallDate = Convert.ToDateTime(CallDateStr);
													if (_CallTime != null)
													{
														data.CallTime = _CallTime;  // Call duration hh:mm:ss to long convert and stored
													}

													data.CallDuration = DuractionSeconds;

													// --> Optional Field Data--------------------
													data.Description = descriptionTextStr;
													data.CallerNumber = CallerNameStr;
													data.ReceiverNumber = _ReceiverNumber;
													data.ServiceTypeId = (long)EnumList.ServiceType.VOIP;
													data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
													data.ExcelUploadLogId = 0;
													data.IsMatched = false;
													data.AssignType = null;
													if (isBusinessOnly)
													{
														data.AssignType = (long)EnumList.AssignType.Business;
													}
													else
													{
														if (data.CallAmount <= 0)
														{
															data.AssignType = (long)EnumList.AssignType.Business;
														}
													}

													datalist.Add(data);
												}
												else
												{
													datalistInvalid.Add(new VoipExcelUploadDetailStringAC
													{
														CallerNumber = _CallerNumber,
														ReceiverNumber = _ReceiverNumber,
														Description = descriptionTextStr,
														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
													});
												}

											}
											catch (Exception e)
											{
												if (e.GetType() != typeof(System.NullReferenceException))
												{

													datalistInvalid.Add(new VoipExcelUploadDetailStringAC
													{
														CallerNumber = _CallerNumber,
														ReceiverNumber = _ReceiverNumber,
														Description = descriptionTextStr,
														CallAmount = CallAmountStr,
														CallDate = CallDateStr,
														CallTime = CallTimeStr,
														CallDuration = DuractionSecondsStr,
														ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
													});
												}

											}

										}

									}

								}
							}
							catch (Exception e)
							{
								csv.Dispose();
								if (File.Exists(Path.Combine(filepath, filename)))
									File.Delete(Path.Combine(filepath, filename));

								importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
								importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
								return importBillDetail;
							}


						}
					}
					catch (Exception e)
					{
						importBillDetail.Message = "Error during reading csv file. " + e.InnerException.Message;
						importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
						return importBillDetail;
					}

					#endregion

				}
				else if (sFileExtension == ".xls" || sFileExtension == ".xlsx")
				{
					using (var stream = new FileStream(fullPath, FileMode.Open))
					{
						int worksheetno = 0;
						int readingIndex = 0;
						if (mappingExcel != null)
						{
							worksheetno = Convert.ToInt16(mappingExcel.WorkSheetNo);
							readingIndex = (mappingExcel.HaveHeader ? 2 : 1);
							readingIndex = (!string.IsNullOrEmpty(mappingExcel.ExcelReadingColumn) ? Convert.ToInt16(mappingExcel.ExcelReadingColumn) : readingIndex);

						}

						stream.Position = 0;
						if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
						{
							HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
							sheet = hssfwb.GetSheetAt(worksheetno - 1);
						}
						else //This will read 2007 Excel format    
						{
							XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
							sheet = hssfwb.GetSheetAt(worksheetno - 1);
						}
						IRow headerRow = sheet.GetRow(0);

						if (headerRow != null)
						{
							int cellCount = headerRow.LastCellNum;
							int rowcount = sheet.LastRowNum;
							for (int j = readingIndex; j <= rowcount + 1; j++)
							{
								int intIndex = 0;
								intIndex = (j > 0 ? j - 1 : j);
								IRow row = sheet.GetRow(intIndex);

								if (row == null || row.Cells.All(d => d.CellType == CellType.Blank))
								{
									continue;
								}
								else
								{
									try
									{
										bool IsFullValid = true;
										string ErrorMessageSummary = string.Empty;

										#region --> Call Date Required and Format Validation Part

										var dynamicRef = getAddress(mappingExcel.DBFiledMappingList, "CallDate", j);
										string CallDateStr = getValueFromExcel(dynamicRef, sheet, (long)EnumList.SupportDataType.Date);
										string dateFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDate");
										if (!string.IsNullOrEmpty(CallDateStr))
										{
											bool isvalidDate = true;
											isvalidDate = CheckDate(CallDateStr);

											if (!isvalidDate)
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Date must be required in valid format ! ";
											}



											//DateTime dt;
											//string[] formats = { dateFormat, "dd-MM-yyyy hh:mm:ss", "dd-MMM-yyyy", "MM/dd/yyyy hh:mm tt", "dd-MM-yyyy hh:mm tt" };
											//if (!DateTime.TryParseExact(CallDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
											//{
											//    IsFullValid = false;
											//    ErrorMessageSummary = ErrorMessageSummary + "Date must be required in (" + dateFormat + ") format ! ";
											//}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Date doesnot exists ! ";

										}
										#endregion

										#region --> Call Time Required and Format Validation Part

										var dynamicRef1 = getAddress(mappingExcel.DBFiledMappingList, "CallTime", j);
										string CallTimeStr = getValueFromExcel(dynamicRef1, sheet, (long)EnumList.SupportDataType.Time);
										string timeFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallTime");
										if (!string.IsNullOrEmpty(CallTimeStr))
										{
											bool isvalidTime = true;
											isvalidTime = CheckDate(CallTimeStr);

											if (!isvalidTime)
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Time must be required in valid format ! ";
											}
											//DateTime dt;
											//string[] formats = { timeFormat, "dd-MM-yyyy hh:mm:ss",
											//                     "dd-MM-yyyy HH:mm:ss", "h:mm:ss", "hh:mm:ss", "HH:mm:ss","hh:mm tt","h:mm tt" };
											//if (!DateTime.TryParseExact(CallTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
											//{
											//    IsFullValid = false;
											//    ErrorMessageSummary = ErrorMessageSummary + "Time must be required in (" + timeFormat + ") format ! ";
											//}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Time doesnot exists ! ";

										}
										#endregion

										#region --> Call Duration Required and Format Validation Part
										long DuractionSeconds = 0;
										string durationFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallDuration");
										var dynamicRef2 = getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j);

										string CallDurationStr = getValueFromExcel(dynamicRef2, sheet, (durationFormat == "seconds" ? (long)EnumList.SupportDataType.Number : (long)EnumList.SupportDataType.Time));

										if (!string.IsNullOrEmpty(CallDurationStr))
										{

											if (durationFormat == "seconds")
											{
												long number;
												if (!long.TryParse(CallDurationStr, out number))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
												}
											}
											else
											{
												if (CallDurationStr.GetType() == typeof(DateTime) || CallDurationStr.GetType() == typeof(TimeSpan))
												{
													DateTime dt1 = DateTime.Parse(CallDurationStr);
													CallDurationStr = String.Format("{0:HH:mm:ss}", dt1);
												}
												//string DateFormat = Convert.ToDateTime(CallDurationStr).ToString("hh:mm:ss");

												DateTime dt;
												string[] formats = { durationFormat, "h:mm:ss", "hh:mm:ss", "HH:mm:ss" };
												if (!DateTime.TryParseExact(CallDurationStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
												{
													IsFullValid = false;
													ErrorMessageSummary = ErrorMessageSummary + "Duration must be required in (" + durationFormat + ") format ! ";
												}
												else
												{
													TimeSpan ts = TimeSpan.Parse(CallDurationStr);
													double totalSeconds = ts.TotalSeconds;
													DuractionSeconds = Convert.ToInt64(totalSeconds);
												}
											}

										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Duration doesnot exists ! ";

										}
										#endregion

										#region --> Call Amount Required and Format Validation Part

										string CallAmountStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
										//string CallAmountStr = Convert.ToString(workSheet.Cells[getAddress(mappingExcel.DBFiledMappingList, "CallAmount", i)].Value.ToString());
										if (!string.IsNullOrEmpty(CallAmountStr))
										{
											CallAmountStr = RemoveSpecialChars(CallAmountStr);
											decimal number;
											if (!decimal.TryParse(CallAmountStr, out number))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Amount format is not valid";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Amount doesnot exists ! ";
										}
										#endregion

										#region --> Call Number Required and Format Validation Part
										string CallNumberStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
										string callernumberFormat = getFormatField(mappingExcel.DBFiledMappingList, "CallerNumber");
										if (!string.IsNullOrEmpty(CallNumberStr))
										{
											if (callernumberFormat == "From: +XXXXXXXXXXXXX To: +XXXXXXXXXX")
											{
												_callDt = getCallerReceiverNumber(CallNumberStr);
												_CallerNumber = _callDt.CallerNumber;
												_ReceiverNumber = _callDt.ReceiverNumber;
											}
											else if (callernumberFormat == "onlynumber(XXXXXXXXXX)")
											{
												_CallerNumber = CallNumberStr;
											}
											else // Other pending
											{
												_CallerNumber = CallNumberStr;

											}

											if (!(_CallerNumber.Length > 5))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not valid";
											}
											//else
											//{
											//    long? EmployeeId = (await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == CallNumberStr && !x.IsDelete && x.IsActive))?.EmployeeId;

											//    if (EmployeeId == null || EmployeeId <= 0)
											//    {
											//        IsFullValid = false;
											//        ErrorMessageSummary = ErrorMessageSummary + "Caller Number is not allocated to employee!";
											//    }
											//}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Caller Number doesnot exists ! ";
										}
										#endregion

										#region --> Receiver Number Required and Format Validation Part
										string ReceivNumberStr = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ReceiverNumber", j), sheet, (long)EnumList.SupportDataType.String);
										string receivernumberFormat = getFormatField(mappingExcel.DBFiledMappingList, "ReceiverNumber");
										if (!string.IsNullOrEmpty(ReceivNumberStr))
										{
											if (receivernumberFormat == "From: +XXXXXXXXXXXXX To: +XXXXXXXXXX")
											{
												_callDt = getCallerReceiverNumber(ReceivNumberStr);
												_CallerNumber = _callDt.CallerNumber;
												_ReceiverNumber = _callDt.ReceiverNumber;
											}
											else if (receivernumberFormat == "onlynumber(XXXXXXXXXX)")
											{
												_ReceiverNumber = ReceivNumberStr;
											}
											else // Other pending
											{
												_ReceiverNumber = ReceivNumberStr;

											}
											if (!(_ReceiverNumber.Length > 5))
											{
												IsFullValid = false;
												ErrorMessageSummary = ErrorMessageSummary + "Receiver Number is not valid";
											}
										}
										else
										{
											IsFullValid = false;
											ErrorMessageSummary = ErrorMessageSummary + "Receiver Number doesnot exists ! ";
										}
										#endregion

										if (IsFullValid)
										{
											Skypeexceldetail data = new Skypeexceldetail();
											// --> Required Field Data--------------------
											data.CallerNumber = _CallerNumber;//getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet, (long)EnumList.SupportDataType.String);
											data.ReceiverNumber = _ReceiverNumber; //getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ReceiverNumber", j), sheet, (long)EnumList.SupportDataType.String);
											string CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet, (long)EnumList.SupportDataType.Number);
											CallAmount = RemoveSpecialChars(CallAmount);
											data.CallAmount = Convert.ToDecimal(CallAmount);
											data.CallDate = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date));
											data.CallTime = Convert.ToDateTime(getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time)).TimeOfDay;
											// Call duration hh:mm:ss to long convert and stored
											data.CallDuration = DuractionSeconds;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												data.Description = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet, (long)EnumList.SupportDataType.String);
											}

											data.ServiceTypeId = (long)EnumList.ServiceType.VOIP;
											data.CurrencyId = (await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == billUploadAC.ProviderId && !x.IsDelete && x.IsActive))?.CurrencyId;
											data.ExcelUploadLogId = 0;
											data.IsMatched = false;
											data.AssignType = null;
											if (isBusinessOnly)
											{
												data.AssignType = (long)EnumList.AssignType.Business;
											}
											else
											{
												if (data.CallAmount <= 0)
												{
													data.AssignType = (long)EnumList.AssignType.Business;
												}
											}

											datalist.Add(data);
										}
										else
										{
											string dDescription = string.Empty;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);
											}

											datalistInvalid.Add(new VoipExcelUploadDetailStringAC
											{
												Description = dDescription,
												ReceiverNumber = _ReceiverNumber,// getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ReceiverNumber", j), sheet),
												CallerNumber = _CallerNumber, // getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
												CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
												CallDate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
												CallTime = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
												CallDuration = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
												ErrorMessage = "At :" + j.ToString() + " " + ErrorMessageSummary
											});
										}
									}
									catch (Exception e)
									{
										if (e.GetType() != typeof(System.NullReferenceException))
										{
											string dDescription = string.Empty;
											// --> Optional Field Data--------------------
											if (!string.IsNullOrEmpty(mappingExcel.DBFiledMappingList.Where(x => x.DBColumnName == "Description").Select(x => x.ExcelcolumnName).FirstOrDefault()))
											{
												dDescription = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "Description", j), sheet);
											}

											datalistInvalid.Add(new VoipExcelUploadDetailStringAC
											{
												Description = dDescription,
												ReceiverNumber = _ReceiverNumber, // getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "ReceiverNumber", j), sheet),
												CallerNumber = _CallerNumber, // getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallerNumber", j), sheet),
												CallAmount = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallAmount", j), sheet),
												CallDate = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDate", j), sheet, (long)EnumList.SupportDataType.Date),
												CallTime = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallTime", j), sheet, (long)EnumList.SupportDataType.Time),
												CallDuration = getValueFromExcel(getAddress(mappingExcel.DBFiledMappingList, "CallDuration", j), sheet, (long)EnumList.SupportDataType.Time),
												ErrorMessage = "At :" + j.ToString() + " " + "Error :" + e.Message
											});
										}
									}
								}
							}
						}

					}
				}

				voipUploadListAC.InvalidVoipList = datalistInvalid;
				voipUploadListAC.ValidVoipList = datalist;
				ResponseDynamicDataAC<VoipUploadListAC> responseData = new ResponseDynamicDataAC<VoipUploadListAC>();
				responseData.Data = voipUploadListAC;
				importBillDetail.UploadData = responseData;

				if (datalistInvalid.Count > 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "Some data upload with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.SomeDataInvalid);

				}
				else if (datalistInvalid.Count > 0 && datalist.Count == 0)
				{
					importBillDetail.Message = "All data with error!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.DataInvalid);
				}
				if (datalistInvalid.Count == 0 && datalist.Count > 0)
				{
					importBillDetail.Message = "All data upload!";
					importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.Success);
				}

				#region --> Delete File Upload after reading Successful

				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#endregion

				return importBillDetail;

				#endregion
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				importBillDetail.Message = "Error during reading : " + e.Message;
				importBillDetail.Status = Convert.ToInt16(EnumList.ExcelUploadResponseType.ExceptionError);
				return importBillDetail;
			}
		}


		public async Task<SaveAllServiceExcelResponseAC> CheckMappingWithFileFormat(string filepath, string filename, long MaxWorkSheetNo)
		{
			SaveAllServiceExcelResponseAC responeAC = new SaveAllServiceExcelResponseAC();

			try
			{
				string sFileExtension = Path.GetExtension(filename).ToLower();
				string fullPath = Path.Combine(filepath, filename);
				long TotalNumberOfWorksheet = 0;
				using (var stream = new FileStream(fullPath, FileMode.Open))
				{
					#region --> Read  .xls or .xlsx File Using NPOI Package Class
					stream.Position = 0;
					if (sFileExtension == ".xls")//This will read the Excel 97-2000 formats    
					{
						HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
						TotalNumberOfWorksheet = hssfwb.NumberOfSheets;
					}
					else if (sFileExtension == ".xlsx") //This will read 2007 Excel format    
					{
						XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
						TotalNumberOfWorksheet = hssfwb.NumberOfSheets;
					}
					#endregion
				}

				if (TotalNumberOfWorksheet >= 0 && MaxWorkSheetNo <= TotalNumberOfWorksheet)
				{
					#region --> if mapping is missing for selected services
					responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.Success;
					responeAC.Message = "All Worksheet Found!";
					responeAC.TotalValidCount = "0";
					responeAC.TotalAmount = "0";
					return responeAC;
					#endregion
				}
				else
				{
					if (File.Exists(Path.Combine(filepath, filename)))
						File.Delete(Path.Combine(filepath, filename));

					#region --> Worksheet No .is out of range
					responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
					responeAC.Message = "Worksheet No." + Convert.ToString(MaxWorkSheetNo) + " is out of range!";
					responeAC.TotalValidCount = "0";
					responeAC.TotalAmount = "0";
					return responeAC;
					#endregion
				}
			}
			catch (Exception e)
			{
				if (File.Exists(Path.Combine(filepath, filename)))
					File.Delete(Path.Combine(filepath, filename));

				#region --> if mapping is missing for selected services
				responeAC.StatusCode = (int)EnumList.ExcelUploadResponseType.NoDataFound;
				responeAC.Message = "Worksheet No." + Convert.ToString(MaxWorkSheetNo) + " is out of range!";
				responeAC.TotalValidCount = "0";
				responeAC.TotalAmount = "0";
				return responeAC;
				#endregion
			}

		}


		#endregion


		#region Bill Allocation

		public async Task<BillAllocationListAC> GetBillAllocationList(BillAllocationAC billAllocationAC)
		{
			BillAllocationListAC billAllocationResponse = new BillAllocationListAC();
			billAllocationResponse.Month = billAllocationAC.Month;
			billAllocationResponse.ToAssigneType = billAllocationAC.ToAssigneType;
			billAllocationResponse.Year = billAllocationAC.Year;
			billAllocationResponse.ServiceTypes = billAllocationAC.ServiceTypes;
			billAllocationResponse.ProviderId = billAllocationAC.ProviderId;
			billAllocationResponse.AssignedBillList = new List<AssignedBillAC>();
			billAllocationResponse.BusinessAssignedBillList = new List<BusinessAssignedBillAC>();
			billAllocationResponse.UnAssignedBillList = new List<UnAssignedBillAC>();

			List<Exceluploadlog> excelUploadLogList = await _dbTeleBilling_V01Context.Exceluploadlog.Where(x => x.Month == billAllocationAC.Month && x.Year == billAllocationAC.Year && x.ProviderId == billAllocationAC.ProviderId && !x.IsDelete && x.MergedWithId == null && x.IsApproved == true).Include(x => x.Provider).ToListAsync();
			if (excelUploadLogList.Any())
			{
				foreach (var excelUploadLog in excelUploadLogList)
				{
					long assignTypeForEmployee = Convert.ToInt16(EnumList.AssignType.Employee);
					long assignTypeForBusiness = Convert.ToInt16(EnumList.AssignType.Business);

					List<Exceldetail> lstExcelDetailForEmployee = new List<Exceldetail>();
					List<Exceldetail> lstExcelDetailForBusiness = new List<Exceldetail>();
					List<Exceldetail> lstForUnAssigned = new List<Exceldetail>();
					foreach (var serviceType in billAllocationAC.ServiceTypes)
					{
						if (_dbTeleBilling_V01Context.ExceluploadlogServicetype.Include(x => x.ServiceType).Any(x => x.ServiceTypeId == serviceType.Id && !x.IsAllocated && !x.IsDelete && x.ExcelUploadLogId == excelUploadLog.Id && !x.ServiceType.IsBusinessOnly))
						{

							lstExcelDetailForEmployee.AddRange(await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == true && x.AssignType == assignTypeForEmployee && x.ServiceTypeId == serviceType.Id).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.ServiceType).ToListAsync());
							lstExcelDetailForBusiness.AddRange(await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == true && x.AssignType == assignTypeForBusiness && x.ServiceTypeId == serviceType.Id).Include(x => x.Currency).Include(x => x.BusinessUnitNavigation).Include(x => x.ServiceType).ToListAsync());
							lstForUnAssigned.AddRange(await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == false && x.ServiceTypeId == serviceType.Id && !x.ServiceType.IsBusinessOnly).Include(x => x.Currency).Include(x => x.ServiceType).ToListAsync());
						}
					}

					if (lstExcelDetailForEmployee.Any())
					{
						var lstofEmployee = lstExcelDetailForEmployee.GroupBy(x => x.EmployeeId).ToList();
						foreach (var item in lstofEmployee.ToList())
						{

							#region Get Service Types
							var lstOfServiceGroup = item.ToList().GroupBy(x => x.ServiceType.Name).ToList();
							string serviceTypes = string.Empty;
							foreach (var serviceItem in lstOfServiceGroup)
							{
								serviceTypes += serviceItem.Key + ",";
							}
							#endregion

							AssignedBillAC assignedBillAC = new AssignedBillAC();
							assignedBillAC.ServiceType = serviceTypes.Substring(0, serviceTypes.Length - 1);
							assignedBillAC.AssignType = EnumList.AssignType.Employee.ToString();
							assignedBillAC.ExcelUploadLogId = excelUploadLog.Id;
							assignedBillAC.Currency = lstExcelDetailForEmployee[0].Currency.Code;
							EnumList.Month monthEnum = (EnumList.Month)billAllocationAC.Month;
							assignedBillAC.BillDate = monthEnum.ToString() + " " + billAllocationAC.Year.ToString();
							assignedBillAC.Provider = excelUploadLog.Provider.Name;
							assignedBillAC.EmployeeName = item.ToList()[0].Employee.FullName;
							assignedBillAC.EmployeeId = Convert.ToInt64(item.Key);
							foreach (var subitem in item.ToList())
							{
								assignedBillAC.BillAmount += (subitem.CallAmount != null ? Convert.ToDecimal(subitem.CallAmount) : 0);
							}
							billAllocationResponse.AssignedBillList.Add(assignedBillAC);
						}
					}

					if (lstExcelDetailForBusiness.Any())
					{
						var lstOfBusinessUnit = lstExcelDetailForBusiness.GroupBy(x => x.BusinessUnitId).ToList();
						foreach (var item in lstOfBusinessUnit.ToList())
						{
							#region Get Service Types
							var lstOfServiceGroup = item.ToList().GroupBy(x => x.ServiceType.Name).ToList();
							string serviceTypes = string.Empty;
							foreach (var serviceItem in lstOfServiceGroup)
							{
								serviceTypes += serviceItem.Key + ",";
							}
							#endregion

							BusinessAssignedBillAC businessAssignedBillAC = new BusinessAssignedBillAC();
							businessAssignedBillAC.ServiceType = serviceTypes.Substring(0, serviceTypes.Length - 1);
							businessAssignedBillAC.ExcelUploadLogId = excelUploadLog.Id;
							businessAssignedBillAC.AssignType = EnumList.AssignType.Business.ToString();
							businessAssignedBillAC.Currency = lstExcelDetailForBusiness[0].Currency.Code;
							EnumList.Month monthEnum = (EnumList.Month)billAllocationAC.Month;
							businessAssignedBillAC.BillDate = monthEnum.ToString() + " " + billAllocationAC.Year.ToString();
							businessAssignedBillAC.Provider = excelUploadLog.Provider.Name;
							businessAssignedBillAC.BusinessUnit = item.ToList()[0].BusinessUnitNavigation.Name;
							businessAssignedBillAC.BusinessUnitId = Convert.ToInt64(item.Key);
							foreach (var subitem in item.ToList())
							{
								businessAssignedBillAC.BillAmount += (subitem.CallAmount != null ? Convert.ToDecimal(subitem.CallAmount) : 0);
							}
							billAllocationResponse.BusinessAssignedBillList.Add(businessAssignedBillAC);
						}
					}

					if (lstForUnAssigned.Any())
					{
						billAllocationResponse.UnAssignedBillList.AddRange(_mapper.Map<List<UnAssignedBillAC>>(lstForUnAssigned));
					}

				}
			}
			return billAllocationResponse;
		}

		public async Task<ResponseAC> AssigneBills(BillAssigneAC billAssigneAC, string userId)
		{
			ResponseAC responseAC = new ResponseAC();
			List<Exceldetail> excelDetails = new List<Exceldetail>();
			foreach (var item in billAssigneAC.UnAssignedBillList)
			{
				Exceldetail excelDetail = await _dbTeleBilling_V01Context.Exceldetail.FirstOrDefaultAsync(x => x.Id == item.ExcelDetailId && x.ExcelUploadLogId == item.ExcelUploadLogId);
				excelDetail.IsAssigned = true;
				excelDetail.AssignType = billAssigneAC.AssigneType;
				if (billAssigneAC.AssigneType == Convert.ToInt16(EnumList.AssignType.Business))
					excelDetail.BusinessUnitId = billAssigneAC.BusinessUnitId;
				else
					excelDetail.EmployeeId = billAssigneAC.EmployeeData.UserId;

				excelDetails.Add(excelDetail);
			}
			_dbTeleBilling_V01Context.UpdateRange(excelDetails);
			await _dbTeleBilling_V01Context.SaveChangesAsync();

			responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAC.Message = _iStringConstant.BillAssignedSuccesfully;
			return responseAC;
		}

		public async Task<List<UnAssignedBillAC>> GetAssignedBillList(long employeeId, long exceluploadlogid, long businessunitId)
		{
			long assignTypeForEmployee = Convert.ToInt16(EnumList.AssignType.Employee);
			long assignTypeForBusiness = Convert.ToInt16(EnumList.AssignType.Business);

			List<Exceldetail> lstForExcelDetail = new List<Exceldetail>();
			if (employeeId != 0)
				lstForExcelDetail = await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == exceluploadlogid && x.IsAssigned == true && x.AssignType == assignTypeForEmployee && x.EmployeeId == employeeId).Include(x => x.Currency).Include(x => x.ServiceType).ToListAsync();
			else if (businessunitId != 0)
				lstForExcelDetail = await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == exceluploadlogid && x.IsAssigned == true && x.AssignType == assignTypeForBusiness && x.BusinessUnitId == businessunitId).Include(x => x.Currency).Include(x => x.ServiceType).ToListAsync();

			return _mapper.Map<List<UnAssignedBillAC>>(lstForExcelDetail);
		}

		public async Task<bool> UnAssgineCallLogs(List<UnAssignedBillAC> unAssignedBillACs)
		{
			List<Exceldetail> lstExcelDetail = new List<Exceldetail>();
			foreach (var item in unAssignedBillACs)
			{
				Exceldetail excelDetail = await _dbTeleBilling_V01Context.Exceldetail.FirstAsync(x => x.Id == item.ExcelDetailId);
				excelDetail.BusinessUnitId = null;
				excelDetail.IsAssigned = false;
				excelDetail.AssignType = null;
				lstExcelDetail.Add(excelDetail);
			}
			_dbTeleBilling_V01Context.UpdateRange(lstExcelDetail);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			return true;
		}

		//This is only for employee bill
		public async Task<ResponseAC> BillAllocation(BillAllocationAC billAllocationAC, long userId, string loginUserName)
		{
			ResponseAC responseAc = new ResponseAC();
			List<Exceluploadlog> excelUploadLogList = await _dbTeleBilling_V01Context.Exceluploadlog.Where(x => x.Month == billAllocationAC.Month && x.Year == billAllocationAC.Year && x.ProviderId == billAllocationAC.ProviderId && !x.IsDelete).Include(x => x.Provider).ToListAsync();
			List<Exceldetail> lstExcelDetailForEmployee = new List<Exceldetail>();
			List<ExceluploadlogServicetype> lstExcelUploadLogServiceType = new List<ExceluploadlogServicetype>();

			foreach (var excelUploadLog in excelUploadLogList)
			{
				foreach (var serviceType in billAllocationAC.ServiceTypes)
				{
					ExceluploadlogServicetype excelUploadLogServiceType = await _dbTeleBilling_V01Context.ExceluploadlogServicetype.FirstOrDefaultAsync(x => x.ServiceTypeId == serviceType.Id && !x.IsAllocated && !x.IsDelete && x.ExcelUploadLogId == excelUploadLog.Id);
					if (excelUploadLogServiceType != null)
					{
						excelUploadLogServiceType.IsAllocated = true;
						excelUploadLogServiceType.UpdatedBy = userId;
						excelUploadLogServiceType.UpdatedDate = DateTime.Now;

						lstExcelUploadLogServiceType.Add(excelUploadLogServiceType);
						lstExcelDetailForEmployee.AddRange(await _dbTeleBilling_V01Context.Exceldetail.Where(x => x.ExcelUploadLogId == excelUploadLog.Id && x.IsAssigned == true && x.ServiceTypeId == serviceType.Id).Include(x => x.Currency).Include(x => x.Employee).Include(x => x.ServiceType).ToListAsync());
					}
				}
			}

			if (lstExcelDetailForEmployee.Any()) {

				#region Bill Master Entry
				Billmaster billMaster = new Billmaster();
				billMaster.ProviderId = Convert.ToInt64(billAllocationAC.ProviderId);
				billMaster.BillMonth = billAllocationAC.Month;
				billMaster.BillYear = billAllocationAC.Year;
				billMaster.BillStatusId = Convert.ToInt16(EnumList.BillStatus.BillAllocated);
				billMaster.BillNumber = _iLogManagement.GenerateBillNumber();
				billMaster.BillAmount = 0;//remaning;
				billMaster.BillDueDate = billAllocationAC.BillDueDate.AddDays(1);
				billMaster.BillAllocationDate = DateTime.Now;
				billMaster.BillAllocatedBy = userId;
				billMaster.CreatedBy = userId;
				billMaster.CreatedDate = DateTime.Now;
				billMaster.CurrencyId = null;

				await _dbTeleBilling_V01Context.AddAsync(billMaster);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				#endregion

				List<BillmasterServicetype> billMasterServiceTypes = new List<BillmasterServicetype>();
				foreach (var serviceDetail in lstExcelUploadLogServiceType)
				{
					if (!_dbTeleBilling_V01Context.BillmasterServicetype.Any(x => !x.IsDelete && x.BillMasterId == billMaster.Id && x.ServiceTypeId == serviceDetail.ServiceTypeId))
					{
						BillmasterServicetype billMasterServiceType = new BillmasterServicetype();
						billMasterServiceType.BillMasterId = billMaster.Id;
						billMasterServiceType.ServiceTypeId = serviceDetail.ServiceTypeId;
						billMasterServiceTypes.Add(billMasterServiceType);
					}
				}
				await _dbTeleBilling_V01Context.AddRangeAsync(billMasterServiceTypes);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				decimal totalBillAmount = 0;
				long? currencyId = null;

				var lstofEmployee = lstExcelDetailForEmployee.GroupBy(x => new { x.EmployeeId, x.CallerNumber }).ToList();
				List<Notificationlog> notificationlogs = new List<Notificationlog>();
				Billdelegate billDelegate = new Billdelegate();
				foreach (var item in lstofEmployee.ToList())
				{
					decimal totalEmployeeBillAmount = 0;
					List<Exceldetail> finalItem = item.ToList();
					if (finalItem.ToList().Any())
					{

						#region Employee Bill Master Entry
						MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstOrDefaultAsync(x => x.UserId == finalItem[0].EmployeeId && !x.IsDelete);
						Telephonenumberallocation telephoneNumberAllocation = await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.TelephoneNumber == finalItem[0].CallerNumber && x.EmployeeId == mstEmployee.UserId && !x.IsDelete);

						Employeebillmaster employeeBillMaster = new Employeebillmaster();
						employeeBillMaster.BillMasterId = billMaster.Id;

						#region It's Not Proper Way, Need to clear
						if (mstEmployee != null)
						{
							employeeBillMaster.LinemanagerId = mstEmployee.LineManagerId;
							employeeBillMaster.EmpBusinessUnitId = mstEmployee.BusinessUnitId;
							employeeBillMaster.EmployeeId = mstEmployee.UserId;

							billDelegate = await _dbTeleBilling_V01Context.Billdelegate.FirstOrDefaultAsync(x => x.EmployeeId == employeeBillMaster.EmployeeId && !x.IsDelete);
							if (billDelegate != null) { 
								employeeBillMaster.BillDelegatedEmpId = billDelegate.DelegateEmployeeId;
							}
						}
						#endregion

						employeeBillMaster.ProviderId = billAllocationAC.ProviderId;
						employeeBillMaster.TelephoneNumber = finalItem[0].CallerNumber;
						employeeBillMaster.BillMonth = billMaster.BillMonth;
						employeeBillMaster.BillNumber = billMaster.BillNumber;
						employeeBillMaster.BillYear = billMaster.BillYear;
						employeeBillMaster.CreatedBy = userId;
						employeeBillMaster.CreatedDate = DateTime.Now;
						employeeBillMaster.CurrencyId = finalItem[0].CurrencyId;
						employeeBillMaster.EmployeeBillStatus = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
						employeeBillMaster.MobileAssignType = telephoneNumberAllocation.AssignTypeId;
						employeeBillMaster.TotalBillAmount = 0;
						employeeBillMaster.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

						await _dbTeleBilling_V01Context.AddAsync(employeeBillMaster);
						await _dbTeleBilling_V01Context.SaveChangesAsync();

						#region Notification Log
						Notificationlog notificationlog = new Notificationlog();
						//check delegate user have authority to allow bill identification
						if (employeeBillMaster.BillDelegatedEmpId != null && billDelegate.AllowBillIdentification) {
							notificationlog = _iLogManagement.GenerateNotificationObject(Convert.ToInt16(employeeBillMaster.BillDelegatedEmpId),userId,Convert.ToInt64(EnumList.NotificationType.DelegateBillIdentification),employeeBillMaster.Id);
							notificationlogs.Add(notificationlog);	
						}
						notificationlog = _iLogManagement.GenerateNotificationObject(Convert.ToInt16(employeeBillMaster.EmployeeId), userId, Convert.ToInt64(EnumList.NotificationType.EmployeeBillIdentification), employeeBillMaster.Id);
						notificationlogs.Add(notificationlog);
						#endregion

						#endregion

						List<Telephonenumberallocationpackage> telePhoneNumberAllocationPackage = await _dbTeleBilling_V01Context.Telephonenumberallocationpackage.Where(x => x.TelephoneNumberAllocationId == telephoneNumberAllocation.Id && !x.IsDelete).ToListAsync();
						foreach (var itemService in telePhoneNumberAllocationPackage)
						{
							foreach (var selectedService in lstExcelUploadLogServiceType)
							{
								if (itemService.ServiceId == selectedService.ServiceTypeId)
								{
									if (!_dbTeleBilling_V01Context.Employeebillservicepackage.Any(x => x.ServiceTypeId == itemService.ServiceId && x.EmployeeBillId == employeeBillMaster.Id && !x.IsDelete))
									{
										Employeebillservicepackage employeeBillServicePackage = new Employeebillservicepackage();
										employeeBillServicePackage.PackageId = itemService.PackageId;
										employeeBillServicePackage.ServiceTypeId = itemService.ServiceId;
										employeeBillServicePackage.EmployeeBillId = employeeBillMaster.Id;
										long businessAsignType = Convert.ToInt16(EnumList.AssignType.Business);
										List<Exceldetail> lstExcelDetils = finalItem.Where(x => x.ServiceTypeId == itemService.ServiceId && x.AssignType == businessAsignType).ToList();
										decimal totalAutoBusinessAmount = 0;
										foreach (var exceldetail in lstExcelDetils)
										{
											totalAutoBusinessAmount += (exceldetail.CallAmount != null ? Convert.ToDecimal(exceldetail.CallAmount) : 0);
										}
										employeeBillServicePackage.BusinessIdentificationAmount = totalAutoBusinessAmount;
										employeeBillServicePackage.BusinessTotalAmount = totalAutoBusinessAmount;
										employeeBillServicePackage.PersonalIdentificationAmount = 0;

										await _dbTeleBilling_V01Context.AddAsync(employeeBillServicePackage);
										await _dbTeleBilling_V01Context.SaveChangesAsync();
									}
								}
							}
						}

						List<Billdetails> billDetailList = new List<Billdetails>();
						foreach (var numberEmployee in finalItem)
						{
							Billdetails billDetails = new Billdetails();
							billDetails.BillMasterId = billMaster.Id;
							billDetails.CallAmount = numberEmployee.CallAmount;
							totalEmployeeBillAmount += numberEmployee.CallAmount != null ? Convert.ToDecimal(billDetails.CallAmount) : 0;
							billDetails.CallAssignedBy = userId;
							billDetails.CallAssignedDate = DateTime.Now;
							billDetails.CallDate = numberEmployee.CallDate;
							billDetails.CallDuration = numberEmployee.CallDuration;
							billDetails.CallerName = numberEmployee.CallerName;
							billDetails.CallerNumber = numberEmployee.CallerNumber;
							billDetails.CallIwithInGroup = numberEmployee.CallWithinGroup;
							billDetails.CallTime = numberEmployee.CallTime;
							billDetails.CallTransactionTypeId = numberEmployee.CallTransactionTypeId;
							billDetails.CallIdentificationType = null;
							billDetails.CreatedBy = userId;
							billDetails.CreatedDate = DateTime.Now;
							billDetails.EmployeeBillId = employeeBillMaster.Id;
							billDetails.ReceiverName = numberEmployee.ReceiverName;
							billDetails.ReceiverNumber = numberEmployee.ReceiverNumber;
							billDetails.ServiceTypeId = numberEmployee.ServiceTypeId;
							billDetails.SubscriptionType = numberEmployee.SubscriptionType;
							billDetails.AssignTypeId = numberEmployee.AssignType;
							billDetails.Description = numberEmployee.Description;
							if (numberEmployee.AssignType == Convert.ToInt16(EnumList.AssignType.Business))
							{
								billDetails.BusinessUnitId = numberEmployee.BusinessUnitId;
								billDetails.CallIdentificationType = (int)numberEmployee.AssignType;
								billDetails.IsAutoAssigned = true;
							}
							billDetails.TransType = numberEmployee.TransType;
							billDetailList.Add(billDetails);
						}

						await _dbTeleBilling_V01Context.AddRangeAsync(billDetailList);
						await _dbTeleBilling_V01Context.SaveChangesAsync();


						#region Employee Total Amount Update
						Employeebillmaster newEmployeeBillMaster = await _dbTeleBilling_V01Context.Employeebillmaster.FirstAsync(x => x.Id == employeeBillMaster.Id);
						newEmployeeBillMaster.TotalBillAmount = totalEmployeeBillAmount;
						totalBillAmount += totalEmployeeBillAmount;
						currencyId = finalItem[0].CurrencyId;
						_dbTeleBilling_V01Context.Update(newEmployeeBillMaster);
						await _dbTeleBilling_V01Context.SaveChangesAsync();
						#endregion
					}
				}


				#region Bill Master Total Bill AMount Update
				billMaster = await _dbTeleBilling_V01Context.Billmaster.FirstAsync(x => x.Id == billMaster.Id);
				billMaster.BillAmount = totalBillAmount;
				billMaster.CurrencyId = currencyId;
				_dbTeleBilling_V01Context.Update(billMaster);
				await _dbTeleBilling_V01Context.SaveChangesAsync();
				#endregion

				#region Update Excel Upload Log Service Table
				if (lstExcelUploadLogServiceType.Any())
				{
					_dbTeleBilling_V01Context.UpdateRange(lstExcelUploadLogServiceType);
					await _dbTeleBilling_V01Context.SaveChangesAsync();
				}
				#endregion

				#region Insert Into Notification Log Table and Auodit Log Table
				if (notificationlogs.Any())
					await _iLogManagement.SaveNotificationList(notificationlogs);

				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.BillAllocation, loginUserName, userId, "Bill Allocation("+ billMaster.BillNumber+ ")", (int)EnumList.ActionTemplateTypes.BillAllcation, billMaster.Id);
				#endregion
			}
			responseAc.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
			responseAc.Message = _iStringConstant.BillAllocatedSuccesfully;
			return responseAc;
		}
		#endregion

		#endregion

		#region Private Method(s)
		private int? getAlphabatefromIndex(char alphbate)
		{
			int temp = (int)alphbate;
			int temp_integer = 96; //for lower case
			if (temp <= 122 & temp >= 97)
				return ((temp - temp_integer) - 1);
			return null;
		}


		//var formats = new[] { "M-d-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "M.d.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "M/d/yyyy", "dd/MM/yyyy", "MM/dd/yyyy" }

		//private DateTime ConvertDateSpecificFormate(string callDateStr)
		//{
		//    string newDate = callDateStr.Replace(".", "/").Replace("-","/");
		//    DateTime date;
		//    DateTime dt;
		//    if (DateTime.TryParseExact(newDate,"", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }
		//    else if (DateTime.TryParseExact(callDateStr, "", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }
		//    else if (DateTime.TryParseExact(callDateStr, "", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }
		//    else if (DateTime.TryParseExact(callDateStr, "", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }
		//    else if (DateTime.TryParseExact(callDateStr, "", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }
		//    else if (DateTime.TryParseExact(callDateStr, "", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }
		//    else if (DateTime.TryParseExact(callDateStr, "", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
		//    {
		//        date = DateTime.ParseExact(callDateStr, "yyyy-MM-dd HH:mm:ss:fff", null);
		//    }


		//    return Convert.ToDateTime(Convert.ToDateTime(date).ToString("dd/MM/yyyy HH:mm:ss"));
		//}

		#endregion

	}

}
