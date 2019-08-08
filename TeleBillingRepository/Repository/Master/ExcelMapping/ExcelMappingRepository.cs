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

namespace TeleBillingRepository.Repository.Master.ExcelMapping
{
   public class ExcelMappingRepository : IExcelMappingRepository { 
   
        #region "Private Variable(s)"
        private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
        #endregion

        #region "Constructor"
 
        public ExcelMappingRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant
            , ILogManagement ilogManagement)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _mapper = mapper;
            _iLogManagement = ilogManagement;
        }
        #endregion

        #region Public Method(s)
       

        public async Task<List<ExcelMappingListAC>> GetExcelMappingList()
        {
            List<ExcelMappingListAC> listOfExcelMappingsAC = new List<ExcelMappingListAC>();
            List<MappingExcel> mappingExcelList = await _dbTeleBilling_V01Context.MappingExcel.Where(x => !x.IsDelete).Include(x => x.Provider).Include(x => x.ServiceType).OrderByDescending(x => x.CreatedDate).ToListAsync();
            foreach (var item in mappingExcelList)
            {
                ExcelMappingListAC excelMappingListAC = new ExcelMappingListAC();
                excelMappingListAC = _mapper.Map<ExcelMappingListAC>(item);
                listOfExcelMappingsAC.Add(excelMappingListAC);
            }
            return listOfExcelMappingsAC;
        }

        public async Task<bool> DeleteExcelMapping(long userId,long id)
        {
            MappingExcel mappingExcel = await _dbTeleBilling_V01Context.MappingExcel.FirstOrDefaultAsync(x => x.Id == id);
            if (mappingExcel != null)
            {
                mappingExcel.IsDelete = true;
                mappingExcel.UpdatedBy = userId;
                mappingExcel.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(mappingExcel);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                List<MappingExcelColumn> excelcolumnlst = new List<MappingExcelColumn>();
                excelcolumnlst = await _dbTeleBilling_V01Context.MappingExcelColumn.Where(x => x.MappingExcelId == mappingExcel.Id).ToListAsync();
                if (excelcolumnlst != null)
                {
                    _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                }
                return true;
            }
            return false;

        }

        public async Task<ResponseAC> AddExcelMapping(ExcelMappingAC excelMappingAC, long userId)
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
            
            if (!await _dbTeleBilling_V01Context.MappingExcel.AnyAsync(x => x.ProviderId==excelMappingAC.ProviderId && x.ServiceTypeId==excelMappingAC.ServiceTypeId && !x.IsDelete))
            {
                var providerData = await _dbTeleBilling_V01Context.Provider.FirstOrDefaultAsync(x => x.Id == excelMappingAC.ProviderId);
                MappingExcel mappingexcel = new MappingExcel();
                mappingexcel.Id = 0;
                mappingexcel.IsActive = true;
                mappingexcel.ProviderId = excelMappingAC.ProviderId;
                mappingexcel.CurrencyId = providerData.CurrencyId;
                mappingexcel.ServiceTypeId = excelMappingAC.ServiceTypeId;
                mappingexcel.HaveHeader = excelMappingAC.HaveHeader;
                mappingexcel.HaveTitle = excelMappingAC.HaveTitle;
                mappingexcel.TitleName = excelMappingAC.TitleName;
                mappingexcel.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);
                mappingexcel.ExcelColumnNameForTitle = excelMappingAC.ExcelColumnNameForTitle;
                mappingexcel.ExcelReadingColumn = excelMappingAC.ExcelReadingColumn;
                mappingexcel.CreatedBy = userId;
                mappingexcel.CreatedDate = DateTime.Now;
                mappingexcel.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                await _dbTeleBilling_V01Context.AddAsync(mappingexcel);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
                responeAC.Message = _iStringConstant.ExcelMappingAddedSuccessfully;
                responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);


                if (mappingexcel.Id > 0)
                {
                    #region --> ADD Mapping column Details
                    if (excelMappingAC.dbfieldList.Count() > 0)
                    {
                        List<MappingExcelColumn> mappingExcelColumnslst = new List<MappingExcelColumn>();
                  
                        foreach(var item in excelMappingAC.dbfieldList)
                        {
                            MappingExcelColumn excelColumn = new MappingExcelColumn();
                            excelColumn.MappingExcelId = mappingexcel.Id;
                            excelColumn.MappingServiceTypeFieldId = item.Id;
                            excelColumn.ExcelcolumnName = item.ColumnAddress;
                            excelColumn.FormatField = item.FormatField;
                            mappingExcelColumnslst.Add(excelColumn);
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


        public async Task<ExcelMappingAC> GetExcelMappingById(long excelMappingId)
        {
            try
            {
                ExcelMappingAC excelMapping = new ExcelMappingAC();
                MappingExcel mapExcel = new MappingExcel();
                mapExcel = await _dbTeleBilling_V01Context.MappingExcel.Include(x => x.Provider).Include(x => x.ServiceType).FirstOrDefaultAsync(x=>x.Id==excelMappingId);
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

                    List<MappingExcelColumn> mapexcelcolumn = new List<MappingExcelColumn>();
                    mapexcelcolumn = await _dbTeleBilling_V01Context.MappingExcelColumn.Include(x=>x.MappingServiceTypeField).Where(x => x.MappingExcelId == mapExcel.Id).OrderBy(x=>x.MappingServiceTypeField.DisplayOrder).ToListAsync();
                    List<MappingServiceTypeFieldAC> dbfieldlst = new List<MappingServiceTypeFieldAC>();
                    foreach (var item in mapexcelcolumn)
                    {
                        MappingServiceTypeFieldAC dbfield = new MappingServiceTypeFieldAC();
                        dbfield.Id = item.MappingServiceTypeFieldId;
                        dbfield.ColumnAddress = item.ExcelcolumnName;
                        dbfield.DisplayFieldName = item.MappingServiceTypeField.DisplayFieldName;
                        dbfield.IsRequired = item.MappingServiceTypeField.IsRequired;
                        dbfield.IsSpecial = item.MappingServiceTypeField.IsSpecial;
                        dbfield.FormatField = item.FormatField;
                        dbfieldlst.Add(dbfield);
                    }

                    excelMapping.dbfieldList = dbfieldlst;
                    return excelMapping;
                }
            }
            catch (Exception)
            {
                return new ExcelMappingAC();
            }
            return new ExcelMappingAC();
        }


        public async Task<ResponseAC> EditExcelMapping(ExcelMappingAC excelMappingAC, long userId)
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
                MappingExcel mappingexcel = new MappingExcel();
                mappingexcel = await _dbTeleBilling_V01Context.MappingExcel.FirstOrDefaultAsync(x => x.ProviderId == excelMappingAC.ProviderId && x.ServiceTypeId == excelMappingAC.ServiceTypeId && !x.IsDelete && x.Id == excelMappingAC.Id);

                if (mappingexcel != null && mappingexcel.Id>0)
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
                    mappingexcel.ExcelColumnNameForTitle = excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = excelMappingAC.ExcelReadingColumn;
                    mappingexcel.UpdatedBy = userId;
                    mappingexcel.UpdatedDate = DateTime.Now;

                     _dbTeleBilling_V01Context.Update(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                    responeAC.Message = _iStringConstant.ExcelMappingUpdatedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

                    if (mappingexcel.Id > 0)
                    {
                        #region --> Update Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            #region --> remove old Mapping
                            List<MappingExcelColumn> excelcolumnlst = new List<MappingExcelColumn>();
                            excelcolumnlst = await _dbTeleBilling_V01Context.MappingExcelColumn.Where(x => x.MappingExcelId == mappingexcel.Id).ToListAsync();
                            if (excelcolumnlst != null)
                            {
                                _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                                await _dbTeleBilling_V01Context.SaveChangesAsync();
                            }
                            #endregion

                            List<MappingExcelColumn> mappingExcelColumnslst = new List<MappingExcelColumn>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                MappingExcelColumn excelColumn = new MappingExcelColumn();
                                excelColumn.MappingExcelId = mappingexcel.Id;
                                excelColumn.MappingServiceTypeFieldId = item.Id;
                                excelColumn.ExcelcolumnName = item.ColumnAddress;
                                excelColumn.FormatField = item.FormatField;
                                mappingExcelColumnslst.Add(excelColumn);
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
            List<MappingExcelPbx> mappingExcelList = await _dbTeleBilling_V01Context.MappingExcelPbx.Where(x => !x.IsDelete).Include(x => x.Device).OrderByDescending(x => x.CreatedDate).ToListAsync();
            foreach (var item in mappingExcelList)
            {
                PbxExcelMappingListAC excelMappingListAC = new PbxExcelMappingListAC();
                excelMappingListAC = _mapper.Map<PbxExcelMappingListAC>(item);
                listOfExcelMappingsAC.Add(excelMappingListAC);
            }
            return listOfExcelMappingsAC;
        }

        public async Task<bool> DeletePbxExcelMapping(long userId, long id)
        {
            MappingExcelPbx mappingExcel = await _dbTeleBilling_V01Context.MappingExcelPbx.FirstOrDefaultAsync(x => x.Id == id);
            if (mappingExcel != null)
            {
                mappingExcel.IsDelete = true;
                mappingExcel.UpdatedBy = userId;
                mappingExcel.UpdatedDate = DateTime.Now;
                _dbTeleBilling_V01Context.Update(mappingExcel);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                List<MappingExcelColumnPbx> excelcolumnlst = new List<MappingExcelColumnPbx>();
                excelcolumnlst = await _dbTeleBilling_V01Context.MappingExcelColumnPbx.Where(x => x.MappingExcelId == mappingExcel.Id).ToListAsync();
                if (excelcolumnlst != null)
                {
                    _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                }
                return true;
            }
            return false;

        }

        public async Task<ResponseAC> AddPbxExcelMapping(PbxExcelMappingAC excelMappingAC, long userId)
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

                if (!await _dbTeleBilling_V01Context.MappingExcelPbx.AnyAsync(x => x.DeviceId == excelMappingAC.DeviceId  && !x.IsDelete))
                {
                    var deviceData = await _dbTeleBilling_V01Context.FixDevice.FirstOrDefaultAsync(x => x.Id == excelMappingAC.DeviceId);
                    MappingExcelPbx mappingexcel = new MappingExcelPbx();
                    mappingexcel.Id = 0;
                    mappingexcel.IsActive = true;
                    mappingexcel.DeviceId = excelMappingAC.DeviceId;
                    mappingexcel.CurrencyId =Convert.ToInt32(EnumList.CurrencyType.USD); // as per current files its given in $
                    mappingexcel.HaveHeader = excelMappingAC.HaveHeader;
                    mappingexcel.HaveTitle = excelMappingAC.HaveTitle;
                    mappingexcel.TitleName = excelMappingAC.TitleName;
                    mappingexcel.WorkSheetNo = Convert.ToInt64(excelMappingAC.WorkSheetNo);
                    mappingexcel.ExcelColumnNameForTitle = excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = excelMappingAC.ExcelReadingColumn;
                    mappingexcel.CreatedBy = userId;
                    mappingexcel.CreatedDate = DateTime.Now;
                    mappingexcel.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

                    await _dbTeleBilling_V01Context.AddAsync(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();
                    responeAC.Message = _iStringConstant.ExcelMappingAddedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);


                    if (mappingexcel.Id > 0)
                    {
                        #region --> ADD Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            List<MappingExcelColumnPbx> mappingExcelColumnslst = new List<MappingExcelColumnPbx>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                MappingExcelColumnPbx excelColumn = new MappingExcelColumnPbx();
                                excelColumn.MappingExcelId = mappingexcel.Id;
                                excelColumn.MappingServiceTypeFieldId = item.Id;
                                excelColumn.ExcelcolumnName = item.ColumnAddress;
                                excelColumn.FormatField = item.FormatField;
                                mappingExcelColumnslst.Add(excelColumn);
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
                MappingExcelPbx mapExcel = new MappingExcelPbx();
                mapExcel = await _dbTeleBilling_V01Context.MappingExcelPbx.Include(x => x.Device).FirstOrDefaultAsync(x => x.Id == excelMappingId);
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

                    List<MappingExcelColumnPbx> mapexcelcolumn = new List<MappingExcelColumnPbx>();
                    mapexcelcolumn = await _dbTeleBilling_V01Context.MappingExcelColumnPbx.Include(x => x.MappingServiceTypeField).Where(x => x.MappingExcelId == mapExcel.Id).OrderBy(x => x.MappingServiceTypeField.DisplayOrder).ToListAsync();
                    List<MappingServiceTypeFieldAC> dbfieldlst = new List<MappingServiceTypeFieldAC>();
                    foreach (var item in mapexcelcolumn)
                    {
                        MappingServiceTypeFieldAC dbfield = new MappingServiceTypeFieldAC();
                        dbfield.Id = item.MappingServiceTypeFieldId;
                        dbfield.ColumnAddress = item.ExcelcolumnName;
                        dbfield.DisplayFieldName = item.MappingServiceTypeField.DisplayFieldName;
                        dbfield.IsRequired = item.MappingServiceTypeField.IsRequired;
                        dbfield.IsSpecial = item.MappingServiceTypeField.IsSpecial;
                        dbfield.FormatField = item.FormatField;
                        dbfieldlst.Add(dbfield);
                    }

                    excelMapping.dbfieldList = dbfieldlst;
                    return excelMapping;
                }
            }
            catch (Exception e)
            {
                return new PbxExcelMappingAC();
            }
            return new PbxExcelMappingAC();
        }


        public async Task<ResponseAC> EditPbxExcelMapping(PbxExcelMappingAC excelMappingAC, long userId)
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
                MappingExcelPbx mappingexcel = new MappingExcelPbx();
                mappingexcel = await _dbTeleBilling_V01Context.MappingExcelPbx.FirstOrDefaultAsync(x => x.DeviceId == excelMappingAC.DeviceId && !x.IsDelete && x.Id == excelMappingAC.Id);

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
                    mappingexcel.ExcelColumnNameForTitle = excelMappingAC.ExcelColumnNameForTitle;
                    mappingexcel.ExcelReadingColumn = excelMappingAC.ExcelReadingColumn;
                    mappingexcel.UpdatedBy = userId;
                    mappingexcel.UpdatedDate = DateTime.Now;

                    _dbTeleBilling_V01Context.Update(mappingexcel);
                    await _dbTeleBilling_V01Context.SaveChangesAsync();

                    responeAC.Message = _iStringConstant.ExcelMappingUpdatedSuccessfully;
                    responeAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);

                    if (mappingexcel.Id > 0)
                    {
                        #region --> Update Mapping column Details
                        if (excelMappingAC.dbfieldList.Count() > 0)
                        {
                            #region --> remove old Mapping
                            List<MappingExcelColumnPbx> excelcolumnlst = new List<MappingExcelColumnPbx>();
                            excelcolumnlst = await _dbTeleBilling_V01Context.MappingExcelColumnPbx.Where(x => x.MappingExcelId == mappingexcel.Id).ToListAsync();
                            if (excelcolumnlst != null)
                            {
                                _dbTeleBilling_V01Context.RemoveRange(excelcolumnlst);
                                await _dbTeleBilling_V01Context.SaveChangesAsync();
                            }
                            #endregion

                            List<MappingExcelColumnPbx> mappingExcelColumnslst = new List<MappingExcelColumnPbx>();

                            foreach (var item in excelMappingAC.dbfieldList)
                            {
                                MappingExcelColumnPbx excelColumn = new MappingExcelColumnPbx();
                                excelColumn.MappingExcelId = mappingexcel.Id;
                                excelColumn.MappingServiceTypeFieldId = item.Id;
                                excelColumn.ExcelcolumnName = item.ColumnAddress;
                                excelColumn.FormatField = item.FormatField;
                                mappingExcelColumnslst.Add(excelColumn);
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
