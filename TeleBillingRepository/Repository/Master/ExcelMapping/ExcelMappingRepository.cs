using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.StaticData;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Master.ExcelMapping
{
    public class ExcelMappingRepository : IExcelMappingRepository
    {

        #region "Private Variable(s)"
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private readonly IStaticDataRepository _iStaticRepository;
        private IMapper _mapper;
        private readonly DAL _objDal = new DAL();
        private readonly DALMySql _objDalmysql = new DALMySql();
        #endregion

        #region "Constructor"

        public ExcelMappingRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
            , ILogManagement ilogManagement, IStaticDataRepository iStaticRepository)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _mapper = mapper;
            _iLogManagement = ilogManagement;
            _iStaticRepository = iStaticRepository;
        }
        #endregion

        #region Public Method(s)

        public async Task<List<ExcelMappingListAC>> GetExcelMappingList()
        {
            List<ExcelMappingListAC> listOfExcelMappingsAC = new List<ExcelMappingListAC>();
            try
            {

                DataSet ds = _objDalmysql.GetDataSet("usp_GetMappingList");
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        listOfExcelMappingsAC = _objDal.ConvertDataTableToGenericList<ExcelMappingListAC>(ds.Tables[0]).ToList();
                        return listOfExcelMappingsAC;
                    }
                }
                return new List<ExcelMappingListAC>();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> DeleteExcelMapping(long userId, long id, string loginUserName)
        {
            Mappingexcel mappingExcel = await _dbTeleBilling_V01Context.Mappingexcel.FirstOrDefaultAsync(x => x.Id == id);

            List<Mappingexcel> mappingExcelMerge = new List<Mappingexcel>();
            mappingExcelMerge = await _dbTeleBilling_V01Context.Mappingexcel.Where(x=>x.MappedMappingId ==id && x.IsCommonMapped == true).ToListAsync();
            if (mappingExcelMerge != null)
            {
                _dbTeleBilling_V01Context.RemoveRange(mappingExcelMerge);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
            }


            if (mappingExcel != null)
            {
                mappingExcel.IsDelete = true;
                mappingExcel.UpdatedBy = userId;
                mappingExcel.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(mappingExcel);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                List<Mappingexcelcolumn> excelcolumnlst = new List<Mappingexcelcolumn>();
                excelcolumnlst = await _dbTeleBilling_V01Context.Mappingexcelcolumn.Where(x => x.MappingExcelId == mappingExcel.Id).ToListAsync();
                if (excelcolumnlst != null)
                {
                    _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                }
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeleteExcelMapping, loginUserName, userId, "Excel mapping", (int)EnumList.ActionTemplateTypes.Delete, mappingExcel.Id);
				return true;
            }
            return false;

        }

        public async Task<bool> checkExcelMappingExistsForServices(ExcelMappingAC excelMappingAC)
        {
            bool IsExists = false;
            try
            {
                
                if (excelMappingAC != null)
                {
                    if (excelMappingAC.ProviderId > 0 && excelMappingAC.ServiceTypeId > 0)
                    {
                        Mappingexcel mapingData = await _dbTeleBilling_V01Context.Mappingexcel.Where(x => x.ServiceTypeId == excelMappingAC.ServiceTypeId && x.ProviderId == excelMappingAC.ProviderId && x.IsActive && !x.IsDelete).FirstOrDefaultAsync();
                        if (mapingData != null)
                        {
                            if (mapingData.Id > 0)
                            {
                                IsExists = true;
                            }
                        }


                        if (excelMappingAC.ServiceTypeIdInline != null)
                        {
                            if (excelMappingAC.ServiceTypeIdInline.Count() > 0)
                            {
                                //List<Mappingexcel> mapingDatalist = new List<Mappingexcel>();
                              var mapingDatalist = await _dbTeleBilling_V01Context.Mappingexcel
                                        .Where(x=>!x.IsDelete && x.ProviderId == excelMappingAC.ProviderId 
                                                         && excelMappingAC.ServiceTypeIdInline.Select(s => s.Id).Contains(x.ServiceTypeId)
                                                                                                         
                                                        ).ToListAsync();
                                   
                                if (mapingDatalist != null)
                                {
                                    if (mapingDatalist.Count > 0)
                                    {
                                        IsExists = true;
                                    }
                                }

                            }
                        }
                    }
                }

                return IsExists;

            }
            catch(Exception e)
            {
                return IsExists;
            }
         

        }

        public async Task<ResponseAC> AddExcelMapping(ExcelMappingAC excelMappingAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();
            try
            {
                if (excelMappingAC.dbfieldList.Count() == 0)
                {
                    responeAC.Message = "Mapping Column Is Missing";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }

                if (excelMappingAC != null)
                {
                    bool IsExistsServices = false;
                    IsExistsServices = await checkExcelMappingExistsForServices(excelMappingAC);
                    if (IsExistsServices)
                    {
                        responeAC.Message = "excel mapping is already exists";
                        responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                        return responeAC;
                    }
                }

                if ((await _dbTeleBilling_V01Context.Mappingexcel.FirstOrDefaultAsync(x => x.ProviderId == excelMappingAC.ProviderId && x.ServiceTypeId == excelMappingAC.ServiceTypeId && !x.IsDelete)) == null)
                {
                    var providerData = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == excelMappingAC.ProviderId);
                    Mappingexcel mappingexcel = new Mappingexcel();
                    mappingexcel.Id = 0;
                    mappingexcel.IsActive = true;
                    mappingexcel.ProviderId = excelMappingAC.ProviderId;
                    mappingexcel.CurrencyId = providerData.CurrencyId;
                    mappingexcel.ServiceTypeId = excelMappingAC.ServiceTypeId;
                    mappingexcel.HaveHeader = excelMappingAC.HaveHeader;
                    mappingexcel.HaveTitle = excelMappingAC.HaveTitle;
                    mappingexcel.TitleName = excelMappingAC.TitleName;
                    mappingexcel.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);
                    mappingexcel.ExcelColumnNameForTitle = string.IsNullOrEmpty(excelMappingAC.ExcelColumnNameForTitle) ? "" : excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = string.IsNullOrEmpty(excelMappingAC.ExcelReadingColumn) ? "0" : excelMappingAC.ExcelReadingColumn;
                    mappingexcel.CreatedBy = userId;
                    mappingexcel.CreatedDate = DateTime.Now;
                    mappingexcel.IsCommonMapped = false;
                    mappingexcel.MappedMappingId = 0;
                    mappingexcel.MappedServiceTypeId = 0;
                    mappingexcel.IsDelete = false;
                   
                    
                    mappingexcel.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                    await _dbTeleBilling_V01Context.AddAsync(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                    responeAC.Message = _iStringConstant.ExcelMappingAddedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddExcelMapping, loginUserName, userId, "Excel mapping", (int)EnumList.ActionTemplateTypes.Add, mappingexcel.Id);

					if (mappingexcel.Id > 0)
                    {
                        #region ---> Add Common Servive if exists
                        foreach (var serviceType in excelMappingAC.ServiceTypeIdInline)
                        {
                            Mappingexcel mappingexcelCommon = new Mappingexcel();
                            mappingexcelCommon.Id = 0;
                            mappingexcelCommon.IsActive = true;
                            mappingexcelCommon.ProviderId = excelMappingAC.ProviderId;
                            mappingexcelCommon.CurrencyId = providerData.CurrencyId;
                            mappingexcelCommon.ServiceTypeId = serviceType.Id;
                            mappingexcelCommon.HaveHeader = excelMappingAC.HaveHeader;
                            mappingexcelCommon.HaveTitle = excelMappingAC.HaveTitle;
                            mappingexcelCommon.TitleName = excelMappingAC.TitleName;
                            mappingexcelCommon.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);
                            mappingexcelCommon.ExcelColumnNameForTitle = excelMappingAC.ExcelColumnNameForTitle;
                            mappingexcelCommon.ExcelReadingColumn = string.IsNullOrEmpty(excelMappingAC.ExcelReadingColumn) ? "" : excelMappingAC.ExcelReadingColumn;
                            mappingexcelCommon.CreatedBy = userId;
                            mappingexcelCommon.CreatedDate = DateTime.Now;
                            mappingexcelCommon.TransactionId = mappingexcel.TransactionId;
                            mappingexcelCommon.IsCommonMapped = true;
                            mappingexcelCommon.MappedMappingId = mappingexcel.Id;
                            mappingexcelCommon.MappedServiceTypeId = mappingexcel.ServiceTypeId;

                            await _dbTeleBilling_V01Context.AddAsync(mappingexcelCommon);
                            await _dbTeleBilling_V01Context.SaveChangesAsync();
                        }

                        #endregion


                        #region --> ADD Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            List<Mappingexcelcolumn> mappingExcelColumnslst = new List<Mappingexcelcolumn>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                if (!string.IsNullOrEmpty(item.ColumnAddress))
                                {
                                    Mappingexcelcolumn excelColumn = new Mappingexcelcolumn();
                                    excelColumn.MappingExcelId = mappingexcel.Id;
                                    excelColumn.MappingServiceTypeFieldId = item.Id;
                                    excelColumn.ExcelcolumnName = item.ColumnAddress;
                                    excelColumn.FormatField = item.FormatField;
                                    mappingExcelColumnslst.Add(excelColumn);
                                }
                            }
                            await _dbTeleBilling_V01Context.AddRangeAsync(mappingExcelColumnslst);
                            await _dbTeleBilling_V01Context.SaveChangesAsync();
                            responeAC.Message = _iStringConstant.ExcelMappingAddedSuccessfully;
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        }
                        #endregion
                    }
                }
                else
                {
                    responeAC.Message = _iStringConstant.ExcelMappingExists;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                }
                return responeAC;
            }
            catch (Exception e)
            {
                responeAC.Message = e.Message.ToString();
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }
        }

        public async Task<bool> checkExcelMappingExists(long providerid, long servicetypeid)
        {
            bool IsExists = false;
            try
            {
                if (providerid > 0 && servicetypeid > 0)
                {
                    Mappingexcel mapingData = await _dbTeleBilling_V01Context.Mappingexcel.FirstOrDefaultAsync(x => x.ServiceTypeId == servicetypeid && x.ProviderId == providerid && x.IsActive == true && x.IsDelete == false);
                    if (mapingData != null)
                    {
                        if (mapingData.Id > 0)
                        {
                            IsExists = true;
                        }
                    }
                    else
                    {
                        IsExists = false;
                    }
                }
            }
            catch (Exception e)
            {
                return IsExists;
            }

            return IsExists;

        }

        public async Task<ExcelMappingAC> GetExcelMappingById(long excelMappingId)
        {
            try
            {
                ExcelMappingAC excelMapping = new ExcelMappingAC();
                Mappingexcel mapExcels = new Mappingexcel();

                var mappingData = await _dbTeleBilling_V01Context.Mappingexcel.FirstOrDefaultAsync(x => x.Id == excelMappingId);

               var  mapExcel = await _dbTeleBilling_V01Context.Mappingexcel.Include(x => x.Provider).Include(x => x.ServiceType).FirstOrDefaultAsync(x => x.Id == excelMappingId);
                if (mapExcel != null)
                {
                    excelMapping.Id = mapExcel.Id;
                    excelMapping.Provider = mapExcel.Provider.Name;
                    excelMapping.ServiceType = mapExcel.ServiceType.Name;
                    excelMapping.ProviderId = mapExcel.ProviderId;
                    excelMapping.ServiceTypeId = mapExcel.ServiceTypeId;
                    excelMapping.HaveHeader = mapExcel.HaveHeader;
                    excelMapping.HaveTitle = mapExcel.HaveTitle;
                    excelMapping.TitleName = mapExcel.TitleName;
                    excelMapping.WorkSheetNo = mapExcel.WorkSheetNo;
                    excelMapping.ExcelColumnNameForTitle = mapExcel.ExcelColumnNameForTitle;
                    excelMapping.ExcelReadingColumn = mapExcel.ExcelReadingColumn;

                    List<Mappingexcel> CommonMappingData = new List<Mappingexcel>();
                     CommonMappingData = await _dbTeleBilling_V01Context.Mappingexcel.Include(x => x.ServiceType).Where(x => x.MappedMappingId == excelMappingId && x.MappedServiceTypeId == mapExcel.ServiceTypeId && x.IsCommonMapped == true).ToListAsync();

                    if (CommonMappingData != null)
                    {
                        if (CommonMappingData.Count() > 0)
                        {
                            string joined = string.Join(",", CommonMappingData.Select(x => x.ServiceType.Name).ToArray());

                          var  ServiceTypeIdInline = from pro in CommonMappingData
                                        select new DrpResponseAC() { Name = pro.ServiceType.Name, Id = pro.ServiceType.Id };
                            excelMapping.ServiceTypeIdInline = ServiceTypeIdInline.ToList();
                            excelMapping.ServiceTypesInline = joined;
                        }
                    }

                    List<MappingServiceTypeFieldAC> dbfieldlst = new List<MappingServiceTypeFieldAC>();
                    List<Mappingservicetypefield> lstofMappingServiceType = await _dbTeleBilling_V01Context.Mappingservicetypefield.Where(x => x.ServiceTypeId == mapExcel.ServiceTypeId).ToListAsync();
                    List<Mappingexcelcolumn> mapexcelcolumn = await _dbTeleBilling_V01Context.Mappingexcelcolumn.Where(x => x.MappingExcelId == excelMappingId).ToListAsync();

                    foreach (var item in lstofMappingServiceType)
                    {
                        MappingServiceTypeFieldAC dbfield = new MappingServiceTypeFieldAC();
                        var objMapexcelcolumn = mapexcelcolumn.FirstOrDefault(x => x.MappingServiceTypeFieldId == item.Id);
                        if (objMapexcelcolumn != null)
                        {
                            dbfield.ColumnAddress = objMapexcelcolumn.ExcelcolumnName;
                            dbfield.FormatField = objMapexcelcolumn.FormatField;
                        }
                        dbfield.Id = item.Id;
                        dbfield.DisplayFieldName = item.DisplayFieldName;
                        dbfield.IsRequired = item.IsRequired;
                        dbfield.IsSpecial = item.IsSpecial;
                        dbfieldlst.Add(dbfield);
                    }
                    excelMapping.dbfieldList = dbfieldlst;
                    return excelMapping;
                }
            }
            catch (Exception e)
            {
                return new ExcelMappingAC();
            }
            return new ExcelMappingAC();
        }

        public async Task<ResponseAC> EditExcelMapping(ExcelMappingAC excelMappingAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();
            try
            {
                if (excelMappingAC.dbfieldList.Count() == 0)
                {
                    responeAC.Message = "Mapping Column Is Missing";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }

                Mappingexcel mappingexcel = new Mappingexcel();
                mappingexcel = await _dbTeleBilling_V01Context.Mappingexcel.FirstOrDefaultAsync(x => x.ProviderId == excelMappingAC.ProviderId && x.ServiceTypeId == excelMappingAC.ServiceTypeId && !x.IsDelete && x.Id == excelMappingAC.Id);

                if (mappingexcel != null && mappingexcel.Id > 0)
                {
                    #region Check For Common Mapped service  not already exists
                    bool IsValidServices = true;
                    if (excelMappingAC.ServiceTypeIdInline != null)
                    {
                        if (excelMappingAC.ServiceTypeIdInline.Count() > 0)
                        {
                            List<DrpResponseAC> ValidServiceTypeIdInline = _iStaticRepository.ProviderCommonServiceTypeList(mappingexcel.ProviderId, mappingexcel.Id);

                            if (ValidServiceTypeIdInline != null)
                            {
                                if (ValidServiceTypeIdInline.Count() > 0)
                                {
                                    var invalidService = excelMappingAC.ServiceTypeIdInline.Where(x => !ValidServiceTypeIdInline.Select(s=>s.Id).Contains(x.Id)).ToList();
                                    if (invalidService != null)
                                    {
                                        if(invalidService.Count()>0)
                                            IsValidServices = false;
                                    }
                                }
                                else
                                {
                                    IsValidServices = false;
                                }
                            }
                            else
                            {
                                IsValidServices = false;
                            }
                            
                            if (!IsValidServices)
                            {
                                responeAC.Message = "selected services for provider mapping is invalid.";
                                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                                return responeAC;
                            }
                        }
                    }
                  
                    #endregion

                    #region Transaction Log Entry
                    if (mappingexcel.TransactionId == null)
                        mappingexcel.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                    var jsonSerailzeObj = JsonConvert.SerializeObject(mappingexcel);
                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mappingexcel.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
                    #endregion

                    mappingexcel.HaveHeader = excelMappingAC.HaveHeader;
                    mappingexcel.HaveTitle = excelMappingAC.HaveTitle;
                    mappingexcel.TitleName = excelMappingAC.TitleName;
                    mappingexcel.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);

                    mappingexcel.ExcelColumnNameForTitle = string.IsNullOrEmpty(excelMappingAC.ExcelColumnNameForTitle) ? "" : excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = string.IsNullOrEmpty(excelMappingAC.ExcelReadingColumn) ? "0" : excelMappingAC.ExcelReadingColumn;

                    mappingexcel.UpdatedBy = userId;
                    mappingexcel.UpdatedDate = DateTime.Now;

                    _dbTeleBilling_V01Context.Update(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                    responeAC.Message = _iStringConstant.ExcelMappingUpdatedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditExcelMapping, loginUserName, userId, "Excel mapping", (int)EnumList.ActionTemplateTypes.Edit, mappingexcel.Id);

					if (mappingexcel.Id > 0)
                    {
                        #region --> Remove Old Common Mapping
                        List<Mappingexcel> mappingCommonList = new List<Mappingexcel>();
                        mappingCommonList = await _dbTeleBilling_V01Context.Mappingexcel.Where(x => x.MappedMappingId == mappingexcel.Id).ToListAsync();
                        if (mappingCommonList != null)
                        {
                            if (mappingCommonList.Count() > 0)
                            {
                                _dbTeleBilling_V01Context.RemoveRange(mappingCommonList);
                                await _dbTeleBilling_V01Context.SaveChangesAsync();
                            }
                               
                        }
                        #endregion
                        if (excelMappingAC.ServiceTypeIdInline != null)
                        {
                            if (excelMappingAC.ServiceTypeIdInline.Count() > 0)
                            {
                                #region ---> Add Common Servive if exists
                                foreach (var serviceType in excelMappingAC.ServiceTypeIdInline)
                                {
                                    Mappingexcel mappingexcelCommon = new Mappingexcel();
                                    mappingexcelCommon.Id = 0;
                                    mappingexcelCommon.IsActive = true;
                                    mappingexcelCommon.ProviderId = excelMappingAC.ProviderId;
                                    mappingexcelCommon.CurrencyId = mappingexcel.CurrencyId;
                                    mappingexcelCommon.ServiceTypeId = serviceType.Id;
                                    mappingexcelCommon.HaveHeader = excelMappingAC.HaveHeader;
                                    mappingexcelCommon.HaveTitle = excelMappingAC.HaveTitle;
                                    mappingexcelCommon.TitleName = excelMappingAC.TitleName;
                                    mappingexcelCommon.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);

                                    mappingexcelCommon.ExcelColumnNameForTitle = string.IsNullOrEmpty(excelMappingAC.ExcelColumnNameForTitle) ? "" : excelMappingAC.ExcelColumnNameForTitle;
                                    mappingexcelCommon.ExcelReadingColumn = string.IsNullOrEmpty(excelMappingAC.ExcelReadingColumn) ? "0" : excelMappingAC.ExcelReadingColumn;
                                    mappingexcelCommon.CreatedBy = userId;
                                    mappingexcelCommon.CreatedDate = DateTime.Now;
                                    mappingexcelCommon.TransactionId = mappingexcel.TransactionId;
                                    mappingexcelCommon.IsCommonMapped = true;
                                    mappingexcelCommon.MappedMappingId = mappingexcel.Id;
                                    mappingexcelCommon.MappedServiceTypeId = mappingexcel.ServiceTypeId;

                                    await _dbTeleBilling_V01Context.AddAsync(mappingexcelCommon);
                                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                                }

                                #endregion
                            }
                        }
                       

                        #region --> Update Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            #region --> remove old Mapping
                            List<Mappingexcelcolumn> excelcolumnlst = new List<Mappingexcelcolumn>();
                            excelcolumnlst = await _dbTeleBilling_V01Context.Mappingexcelcolumn.Where(x => x.MappingExcelId == mappingexcel.Id).ToListAsync();
                            if (excelcolumnlst != null)
                            {
                                _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                                await _dbTeleBilling_V01Context.SaveChangesAsync();
                            }
                            #endregion

                            List<Mappingexcelcolumn> mappingExcelColumnslst = new List<Mappingexcelcolumn>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                if (!string.IsNullOrEmpty(item.ColumnAddress))
                                {
                                    Mappingexcelcolumn excelColumn = new Mappingexcelcolumn();
                                    excelColumn.MappingExcelId = mappingexcel.Id;
                                    excelColumn.MappingServiceTypeFieldId = item.Id;
                                    excelColumn.ExcelcolumnName = item.ColumnAddress;
                                    excelColumn.FormatField = item.FormatField;
                                    mappingExcelColumnslst.Add(excelColumn);
                                }
                            }
                            await _dbTeleBilling_V01Context.AddRangeAsync(mappingExcelColumnslst);
                            await _dbTeleBilling_V01Context.SaveChangesAsync();
                            responeAC.Message = _iStringConstant.ExcelMappingUpdatedSuccessfully;
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        }
                        #endregion
                    }
                }
                else
                {
                    responeAC.Message = _iStringConstant.DataFound;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                }
                return responeAC;
            }
            catch (Exception e)
            {
                responeAC.Message = e.Message.ToString();
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }
        }

        public async Task<List<PbxExcelMappingListAC>> GetPbxExcelMappingList()
        {
            List<PbxExcelMappingListAC> listOfExcelMappingsAC = new List<PbxExcelMappingListAC>();
            List<MappingexcelPbx> mappingExcelList = await _dbTeleBilling_V01Context.MappingexcelPbx.Where(x => !x.IsDelete).Include(x => x.Device).OrderByDescending(x => x.Id).ToListAsync();
            foreach (var item in mappingExcelList)
            {
                PbxExcelMappingListAC excelMappingListAC = new PbxExcelMappingListAC();
                excelMappingListAC = _mapper.Map<PbxExcelMappingListAC>(item);
                listOfExcelMappingsAC.Add(excelMappingListAC);
            }
            return listOfExcelMappingsAC;
        }

        public async Task<bool> DeletePbxExcelMapping(long userId, long id, string loginUserName)
        {
            MappingexcelPbx mappingExcel = await _dbTeleBilling_V01Context.MappingexcelPbx.FirstOrDefaultAsync(x => x.Id == id);
            if (mappingExcel != null)
            {
                mappingExcel.IsDelete = true;
                mappingExcel.UpdatedBy = userId;
                mappingExcel.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(mappingExcel);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                List<MappingexcelcolumnPbx> excelcolumnlst = new List<MappingexcelcolumnPbx>();
                excelcolumnlst = await _dbTeleBilling_V01Context.MappingexcelcolumnPbx.Where(x => x.MappingExcelId == mappingExcel.Id).ToListAsync();
                if (excelcolumnlst != null)
                {
                    _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                }
				await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.DeletePbxExcelMapping, loginUserName, userId, "PBX Excel mapping", (int)EnumList.ActionTemplateTypes.Delete, mappingExcel.Id);
				return true;
            }
            return false;

        }

        public async Task<ResponseAC> AddPbxExcelMapping(PbxExcelMappingAC excelMappingAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();
            try
            {
                if (excelMappingAC.dbfieldList.Count() == 0)
                {
                    responeAC.Message = "Mapping Column Is Missing";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }

                if (!await _dbTeleBilling_V01Context.MappingexcelPbx.AnyAsync(x => x.DeviceId == excelMappingAC.DeviceId && !x.IsDelete))
                {
                    var deviceData = await _dbTeleBilling_V01Context.FixDevice.FirstOrDefaultAsync(x => x.Id == excelMappingAC.DeviceId);
                    MappingexcelPbx mappingexcel = new MappingexcelPbx();
                    mappingexcel.Id = 0;
                    mappingexcel.IsActive = true;
                    mappingexcel.DeviceId = excelMappingAC.DeviceId;
                    mappingexcel.CurrencyId = Convert.ToInt32(EnumList.CurrencyType.USD); // as per current files its given in $
                    mappingexcel.HaveHeader = excelMappingAC.HaveHeader;
                    mappingexcel.HaveTitle = excelMappingAC.HaveTitle;
                    mappingexcel.TitleName = excelMappingAC.TitleName;
                    mappingexcel.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);
                    mappingexcel.ExcelColumnNameForTitle = string.IsNullOrEmpty(excelMappingAC.ExcelColumnNameForTitle) ? "" : excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = string.IsNullOrEmpty(excelMappingAC.ExcelReadingColumn) ? "0" : excelMappingAC.ExcelReadingColumn;
                    
                    mappingexcel.CreatedBy = userId;
                    mappingexcel.CreatedDate = DateTime.Now;
                    mappingexcel.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                    await _dbTeleBilling_V01Context.AddAsync(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                    responeAC.Message = _iStringConstant.ExcelMappingAddedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddPbxExcelMapping, loginUserName, userId, "PBX Excel mapping", (int)EnumList.ActionTemplateTypes.Add, mappingexcel.Id);

					if (mappingexcel.Id > 0)
                    {
                        #region --> ADD Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            List<MappingexcelcolumnPbx> mappingExcelColumnslst = new List<MappingexcelcolumnPbx>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                if (!string.IsNullOrEmpty(item.ColumnAddress))
                                {
                                    MappingexcelcolumnPbx excelColumn = new MappingexcelcolumnPbx();
                                    excelColumn.MappingExcelId = mappingexcel.Id;
                                    excelColumn.MappingServiceTypeFieldId = item.Id;
                                    excelColumn.ExcelcolumnName = item.ColumnAddress;
                                    excelColumn.FormatField = item.FormatField;
                                    mappingExcelColumnslst.Add(excelColumn);
                                }
                            }
                            await _dbTeleBilling_V01Context.AddRangeAsync(mappingExcelColumnslst);
                            await _dbTeleBilling_V01Context.SaveChangesAsync();
                            responeAC.Message = _iStringConstant.ExcelMappingAddedSuccessfully;
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        }
                        #endregion
                    }
                }
                else
                {
                    responeAC.Message = _iStringConstant.ExcelMappingExists;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                }
                return responeAC;
            }
            catch (Exception e)
            {
                responeAC.Message = e.Message.ToString();
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }
        }

        public async Task<PbxExcelMappingAC> GetPbxExcelMappingById(long excelMappingId)
        {
            try
            {
                PbxExcelMappingAC excelMapping = new PbxExcelMappingAC();
                MappingexcelPbx mapExcel = new MappingexcelPbx();
                mapExcel = await _dbTeleBilling_V01Context.MappingexcelPbx.Include(x => x.Device).FirstOrDefaultAsync(x => x.Id == excelMappingId);
                if (mapExcel != null)
                {
                    excelMapping.Id = mapExcel.Id;
                    excelMapping.Device = mapExcel.Device.Name;
                    excelMapping.DeviceId = mapExcel.Device.Id;
                    excelMapping.HaveHeader = mapExcel.HaveHeader;
                    excelMapping.HaveTitle = mapExcel.HaveTitle;
                    excelMapping.TitleName = mapExcel.TitleName;
                    excelMapping.WorkSheetNo = mapExcel.WorkSheetNo;
                    excelMapping.ExcelColumnNameForTitle = mapExcel.ExcelColumnNameForTitle;
                    excelMapping.ExcelReadingColumn = mapExcel.ExcelReadingColumn;

                    // sweta 
                    List<MappingServiceTypeFieldAC> dbfieldlst = new List<MappingServiceTypeFieldAC>();
                    List<MappingservicetypefieldPbx> lstofMappingColmns = new List<MappingservicetypefieldPbx>();
                    lstofMappingColmns = await  _dbTeleBilling_V01Context.MappingservicetypefieldPbx.Where(x => x.DeviceId == mapExcel.DeviceId).ToListAsync();
                     List<MappingexcelcolumnPbx> mapexcelcolumn = await _dbTeleBilling_V01Context.MappingexcelcolumnPbx.Where(x => x.MappingExcelId == excelMappingId).ToListAsync();

                    foreach (var item in lstofMappingColmns)
                    {
                        MappingServiceTypeFieldAC dbfield = new MappingServiceTypeFieldAC();
                        var objMapexcelcolumn = mapexcelcolumn.FirstOrDefault(x => x.MappingServiceTypeFieldId == item.Id);
                        if (objMapexcelcolumn != null)
                        {
                            dbfield.ColumnAddress = objMapexcelcolumn.ExcelcolumnName;
                            dbfield.FormatField = objMapexcelcolumn.FormatField;
                        }
                        dbfield.Id = item.Id;
                        dbfield.DisplayFieldName = item.DisplayFieldName;
                        dbfield.IsRequired = item.IsRequired;
                        dbfield.IsSpecial = item.IsSpecial;
                        dbfieldlst.Add(dbfield);
                    }
                    excelMapping.dbfieldList = dbfieldlst;

                    // ................................
                    //List<MappingexcelcolumnPbx> mapexcelcolumn = new List<MappingexcelcolumnPbx>();
                    //mapexcelcolumn = await _dbTeleBilling_V01Context.MappingexcelcolumnPbx.Include(x => x.MappingServiceTypeField).Where(x => x.MappingExcelId == mapExcel.Id).OrderBy(x => x.MappingServiceTypeField.DisplayOrder).ToListAsync();
                    //List<MappingServiceTypeFieldAC> dbfieldlst = new List<MappingServiceTypeFieldAC>();
                    //foreach (var item in mapexcelcolumn)
                    //{
                    //    MappingServiceTypeFieldAC dbfield = new MappingServiceTypeFieldAC();
                    //    dbfield.Id = item.MappingServiceTypeFieldId;
                    //    dbfield.ColumnAddress = item.ExcelcolumnName;
                    //    dbfield.DisplayFieldName = item.MappingServiceTypeField.DisplayFieldName;
                    //    dbfield.IsRequired = item.MappingServiceTypeField.IsRequired;
                    //    dbfield.IsSpecial = item.MappingServiceTypeField.IsSpecial;
                    //    dbfield.FormatField = item.FormatField;
                    //    dbfieldlst.Add(dbfield);
                    //}

                    //excelMapping.dbfieldList = dbfieldlst;
                    return excelMapping;
                }
            }
            catch (Exception e)
            {
                return new PbxExcelMappingAC();
            }
            return new PbxExcelMappingAC();
        }

        public async Task<ResponseAC> EditPbxExcelMapping(PbxExcelMappingAC excelMappingAC, long userId, string loginUserName)
        {
            ResponseAC responeAC = new ResponseAC();
            try
            {
                if (excelMappingAC.dbfieldList.Count() == 0)
                {
                    responeAC.Message = "Mapping Column Is Missing";
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                    return responeAC;
                }
                MappingexcelPbx mappingexcel = new MappingexcelPbx();
                mappingexcel = await _dbTeleBilling_V01Context.MappingexcelPbx.FirstOrDefaultAsync(x => x.DeviceId == excelMappingAC.DeviceId && !x.IsDelete && x.Id == excelMappingAC.Id);

                if (mappingexcel != null && mappingexcel.Id > 0)
                {
                    #region Transaction Log Entry
                    if (mappingexcel.TransactionId == null)
                        mappingexcel.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                    var jsonSerailzeObj = JsonConvert.SerializeObject(mappingexcel);
                    await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(mappingexcel.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
                    #endregion

                    mappingexcel.HaveHeader = excelMappingAC.HaveHeader;
                    mappingexcel.HaveTitle = excelMappingAC.HaveTitle;
                    mappingexcel.TitleName = excelMappingAC.TitleName;
                    mappingexcel.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);                   

                    mappingexcel.ExcelColumnNameForTitle = string.IsNullOrEmpty(excelMappingAC.ExcelColumnNameForTitle) ? "" : excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = string.IsNullOrEmpty(excelMappingAC.ExcelReadingColumn) ? "0" : excelMappingAC.ExcelReadingColumn;


                    mappingexcel.UpdatedBy = userId;
                    mappingexcel.UpdatedDate = DateTime.Now;

                    _dbTeleBilling_V01Context.Update(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                    responeAC.Message = _iStringConstant.ExcelMappingUpdatedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
					await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditPbxExcelMapping, loginUserName, userId, "PBX Excel mapping", (int)EnumList.ActionTemplateTypes.Edit, mappingexcel.Id);
					if (mappingexcel.Id > 0)
                    {
                        #region --> Update Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            #region --> remove old Mapping
                            List<MappingexcelcolumnPbx> excelcolumnlst = new List<MappingexcelcolumnPbx>();
                            excelcolumnlst = await _dbTeleBilling_V01Context.MappingexcelcolumnPbx.Where(x => x.MappingExcelId == mappingexcel.Id).ToListAsync();
                            if (excelcolumnlst != null)
                            {
                                if (excelcolumnlst.Count() > 0)
                                {
                                    _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                                }
                              
                            }
                            #endregion

                            List<MappingexcelcolumnPbx> mappingExcelColumnslst = new List<MappingexcelcolumnPbx>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                if (!string.IsNullOrEmpty(item.ColumnAddress))
                                {
                                    MappingexcelcolumnPbx excelColumn = new MappingexcelcolumnPbx();
                                    excelColumn.MappingExcelId = mappingexcel.Id;
                                    excelColumn.MappingServiceTypeFieldId = item.Id;
                                    excelColumn.ExcelcolumnName = item.ColumnAddress;
                                    excelColumn.FormatField = item.FormatField;
                                    mappingExcelColumnslst.Add(excelColumn);
                                }
                            }
                            await _dbTeleBilling_V01Context.AddRangeAsync(mappingExcelColumnslst);
                            await _dbTeleBilling_V01Context.SaveChangesAsync();
                            responeAC.Message = _iStringConstant.ExcelMappingUpdatedSuccessfully;
                            responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                        }
                        #endregion
                    }
                }
                else
                {
                    responeAC.Message = _iStringConstant.DataFound;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                }
                return responeAC;
            }
            catch (Exception e)
            {
                responeAC.Message = e.Message.ToString();
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                return responeAC;
            }
        }

        #endregion

    }
}
