using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.CommonFunction;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;
namespace TeleBillingRepository.Repository.StaticData
{
    public class StaticDataRepository : IStaticDataRepository
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
        public StaticDataRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
            ILogManagement iLogManagement)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _iLogManagement = iLogManagement;
            _mapper = mapper;
        }
        #endregion

        #region Public Method(s)


        public async Task<List<DrpCountryAC>> CountryList()
        {
            List<DrpCountryAC> drpCountryACList = new List<DrpCountryAC>();
            List<MstCountry> lst = await _dbTeleBilling_V01Context.MstCountry.Where(x => x.IsActive).Include(x => x.Currency).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpCountryAC>>(lst);
        }


        public async Task<List<DrpResponseAC>> ServiceTypeList()
        {
            List<FixServicetype> lst = new List<FixServicetype>();
            lst = await _dbTeleBilling_V01Context.FixServicetype.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> ProviderServiceTypeList(long providerid)
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            var lstsevice = await _dbTeleBilling_V01Context.Providerservice.Where(x => !x.IsDelete && x.ProviderId == providerid).Include(x => x.ServiceType).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lstsevice);


        }
        public List<DrpResponseAC> ProviderNotMappedServiceTypeList(long providerid)
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            try
            {
                SortedList sl = new SortedList();
                sl.Add("ProviderId", providerid);

                DataSet ds = _objDalmysql.GetDataSet("usp_GetProviderNotMappedServiceList", sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        lst = _objDal.ConvertDataTableToGenericList<DrpResponseAC>(ds.Tables[0]).ToList();
                    }
                }

                return lst;

            }
            catch (Exception e)
            {
                var message = e.Message;
                return new List<DrpResponseAC>();
            }

        }

        public List<DrpResponseAC> ProviderCommonServiceTypeList(long providerid, long mappingid)
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            try
            {
                SortedList sl = new SortedList();
                sl.Add("ProviderId", providerid);
                sl.Add("MappingId", mappingid);

                DataSet ds = _objDalmysql.GetDataSet("usp_GetProviderCommonAndNotMappedServiceList", sl);
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                    {
                        lst = _objDal.ConvertDataTableToGenericList<DrpResponseAC>(ds.Tables[0]).ToList();
                    }
                }
                return lst;
            }
            catch (Exception e)
            {
                var message = e.Message;
                return new List<DrpResponseAC>();
            }
        }

        public async Task<List<DrpResponseAC>> CostCenterList()
        {
            List<MstCostcenter> lst = new List<MstCostcenter>();
            lst = await _dbTeleBilling_V01Context.MstCostcenter.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }


        public async Task<List<DrpResponseAC>> BusinessUnitCostCenterList(long id)
        {
            List<MstCostcenter> lst = new List<MstCostcenter>();
            lst = await _dbTeleBilling_V01Context.MstCostcenter.Where(x => x.IsActive && x.BusinessUnitid == id).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> BusinessUnitList()
        {
            List<MstBusinessunit> lst = new List<MstBusinessunit>();
            lst = await _dbTeleBilling_V01Context.MstBusinessunit.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> RoleNameList()
        {
            List<MstRole> lst = new List<MstRole>();
            lst = await _dbTeleBilling_V01Context.MstRole.Where(x => x.IsActive && !x.IsDelete).OrderByDescending(x => x.RoleName).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }



        public async Task<List<DrpResponseAC>> DepartmentList()
        {
            List<MstDepartment> lst = new List<MstDepartment>();
            lst = await _dbTeleBilling_V01Context.MstDepartment.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<MappingServiceTypeFieldAC>> DBFieldsList(long servicetypeid)
        {
            List<MappingServiceTypeFieldAC> lst = new List<MappingServiceTypeFieldAC>();
            var lstsevice = await _dbTeleBilling_V01Context.Mappingservicetypefield.Where(x => x.ServiceTypeId == servicetypeid).OrderBy(x => x.DisplayOrder).ToListAsync();
            return _mapper.Map<List<MappingServiceTypeFieldAC>>(lstsevice);
        }

        public async Task<List<MappingServiceTypeFieldAC>> DBFieldsListByOrder(long servicetypeid)
        {
            List<MappingServiceTypeFieldAC> lst = new List<MappingServiceTypeFieldAC>();
            var lstsevice = await _dbTeleBilling_V01Context.Mappingservicetypefield.Where(x => x.ServiceTypeId == servicetypeid).OrderByDescending(x => x.DbcolumnName).ToListAsync();
            return _mapper.Map<List<MappingServiceTypeFieldAC>>(lstsevice);
        }

        public async Task<List<DrpResponseAC>> GetLineTypeList()
        {
            List<FixLinetype> lstLineType = await _dbTeleBilling_V01Context.FixLinetype.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lstLineType);
        }


        public async Task<List<DrpResponseAC>> GetAssignTypeList()
        {
            List<FixAssigntype> lstAssignType = await _dbTeleBilling_V01Context.FixAssigntype.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lstAssignType);
        }



        public async Task<List<MappingServiceTypeFieldAC>> DeviceDBFieldsList(long deviceid)
        {
            List<MappingServiceTypeFieldAC> lst = new List<MappingServiceTypeFieldAC>();
            var lstsevice = await _dbTeleBilling_V01Context.MappingservicetypefieldPbx.Where(x => x.DeviceId == deviceid).OrderBy(x => x.DisplayOrder).ToListAsync();
            return _mapper.Map<List<MappingServiceTypeFieldAC>>(lstsevice);
        }


        public async Task<List<DrpResponseAC>> DeviceList()
        {
            List<FixDevice> lst = new List<FixDevice>();
            lst = await _dbTeleBilling_V01Context.FixDevice.Where(x => x.IsActive == true).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> MonthList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.JAN), Name = EnumList.Month.JAN.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.FEB), Name = EnumList.Month.FEB.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.MAR), Name = EnumList.Month.MAR.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.APR), Name = EnumList.Month.APR.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.MAY), Name = EnumList.Month.MAY.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.JUN), Name = EnumList.Month.JUN.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.JUL), Name = EnumList.Month.JUL.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.AUG), Name = EnumList.Month.AUG.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.SEP), Name = EnumList.Month.SEP.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.OCT), Name = EnumList.Month.OCT.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.NOV), Name = EnumList.Month.NOV.ToString() });
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.DEC), Name = EnumList.Month.DEC.ToString() });
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> YearList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            int currentyear = Convert.ToInt16(DateTime.Now.Year.ToString());
            int startyear = 2018;
            for(int yr = startyear; yr <= currentyear; yr++)
            {
                lst.Add(new DrpResponseAC { Id = yr, Name = (yr).ToString() });
            }
         //   lst.Add(new DrpResponseAC { Id = currentyear - 1, Name = (currentyear - 1).ToString() });
         // lst.Add(new DrpResponseAC { Id = currentyear, Name = (currentyear).ToString() });
           
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public List<string> ColorList()
        {
            List<string> lst = new List<string>();     

            lst.Add("#ffb99a");
            lst.Add("#ff6464");
            lst.Add("#db3056");
            lst.Add("#851d41");
            lst.Add("#ffba5a");
            lst.Add("#c0ffb3");
            lst.Add("#52de97");
            lst.Add("#2c7873");
            lst.Add("#7fa998");
            lst.Add("#df8543");
            lst.Add("#9d2503");
            lst.Add("#5d5b6a");
            lst.Add("#758184");
            lst.Add("#cfb495");
            lst.Add("#32afa9");
            lst.Add("#f67280");
            lst.Add("#6c5b7b");
            lst.Add("#35477d");
            lst.Add("#f65c78");
            lst.Add("#32407b");
            lst.Add("#ccda46");
            lst.Add("#7e0cf5");

            return lst;
        }
        public async Task<List<DrpResponseAC>> ProviderTranstypeList(long providerid)
        {
            List<Transactiontypesetting> lst = new List<Transactiontypesetting>();
            lst = await _dbTeleBilling_V01Context.Transactiontypesetting.Where(x => x.IsActive == true && !x.IsDelete && x.ProviderId == providerid).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> GetTemplateTypeList()
        {
            List<FixEmailtemplatetype> lstForTemplateType = await _dbTeleBilling_V01Context.FixEmailtemplatetype.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lstForTemplateType);
        }

        public async Task<List<EmailTemplateTagAC>> GetTemaplteTagList()
        {
            List<FixEmailtemplatetag> lstForTemplateTag = await _dbTeleBilling_V01Context.FixEmailtemplatetag.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).ToListAsync();
            return _mapper.Map<List<EmailTemplateTagAC>>(lstForTemplateTag);
        }

        public async Task<EmployeeDetailAC> GetLoggedInUserDetail(long userId)
        {
            MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.UserId == userId && !x.IsDelete).Include(x => x.Role).FirstOrDefaultAsync();
            return _mapper.Map<EmployeeDetailAC>(mstEmployee);
        }

        public async Task<List<EmployeeAC>> GetEmployeeListByName(string name)
        {
            List<EmployeeAC> mstEmployeeList = new List<EmployeeAC>();
            if (!string.IsNullOrEmpty(name))
            {
                List<MstEmployee> mstEmployees = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.FullName.Contains(name) && !x.IsDelete && x.IsActive).ToListAsync();
                mstEmployeeList = _mapper.Map<List<EmployeeAC>>(mstEmployees);
            }
            return mstEmployeeList;
        }


        public async Task<List<EmployeeAC>> GetLineManagerEmployeeListByName(string name)
        {
            List<EmployeeAC> mstEmployeeList = new List<EmployeeAC>();
            if (!string.IsNullOrEmpty(name))
            {
                List<MstEmployee> mstEmployees = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.FullName.Contains(name) && !string.IsNullOrEmpty(x.EmailId) && !x.IsDelete && x.IsActive).ToListAsync();
                mstEmployeeList = _mapper.Map<List<EmployeeAC>>(mstEmployees);
            }
            return mstEmployeeList;
        }

        public async Task<List<TelephoneNumberAC>> GetNumberList(string number, long id)
        {
            List<TelephoneNumberAC> lstTelePhoneNmber = new List<TelephoneNumberAC>();
            if (!string.IsNullOrEmpty(number))
            {
                lstTelePhoneNmber = await (from tn in _dbTeleBilling_V01Context.Telephonenumber.Include(x => x.Provider)
                                           join tnx in _dbTeleBilling_V01Context.Telephonenumberallocation.Where(x => !x.IsDelete).ToList() on
                                           tn.TelephoneNumber1 equals tnx.TelephoneNumber into numberInfo
                                           from numbers in numberInfo.DefaultIfEmpty()
                                           where tn.TelephoneNumber1.Contains(number) && (numbers.TelephoneNumber == null) && !tn.IsDelete && tn.IsActive
                                           orderby tn.CreatedDate descending
                                           select new TelephoneNumberAC
                                           {
                                               Id = tn.Id,
                                               TelephoneNumber1 = tn.TelephoneNumber1,
                                               IsDelete = numbers.IsDelete,
                                               ProviderName = tn.Provider.Name,
                                               ProviderId = tn.ProviderId

                                           }).Take(10).ToListAsync();


                if (id != 0 && lstTelePhoneNmber.Any())
                {//When call on edit mode at that time already assigned telephone number is added on list
                    Telephonenumberallocation telephoneAllocation = await _dbTeleBilling_V01Context.Telephonenumberallocation.FirstOrDefaultAsync(x => x.Id == id);
                    TelephoneNumberAC telNumber = new TelephoneNumberAC();
                    telNumber.Id = telephoneAllocation.TelephoneNumberId;
                    telNumber.TelephoneNumber1 = telephoneAllocation.TelephoneNumber;
                    lstTelePhoneNmber.Add(telNumber);
                }
            }
            return lstTelePhoneNmber.OrderByDescending(x => x.Id).ToList();
        }


        public async Task<List<DrpResponseAC>> GetPackageList()
        {
            List<Providerpackage> providerPackages = await _dbTeleBilling_V01Context.Providerpackage.Where(x => !x.IsDelete && x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(providerPackages);
        }

        public async Task<List<DrpResponseAC>> GetLineStatusList()
        {
            List<FixLinestatus> fixLineStatuses = await _dbTeleBilling_V01Context.FixLinestatus.Where(x => x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(fixLineStatuses);
        }

        public async Task<List<DrpResponseAC>> GetCallTypeList()
        {
            List<FixCalltype> fixCallTypes = await _dbTeleBilling_V01Context.FixCalltype.Where(x => x.IsActive && x.Id != 0).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(fixCallTypes);
        }

        public List<DrpResponseAC> GetFormatList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            lst.Add(new DrpResponseAC { Id = 1, Name = "dd-MM-yyyy" });
            lst.Add(new DrpResponseAC { Id = 2, Name = "dd/MM/yyyy" });
            lst.Add(new DrpResponseAC { Id = 3, Name = "yyyy/MM/dd" });
            lst.Add(new DrpResponseAC { Id = 13, Name = "MM/dd/yyyy" });
            lst.Add(new DrpResponseAC { Id = 14, Name = "MM-dd-yyyy" });
            lst.Add(new DrpResponseAC { Id = 4, Name = "dd-MM-yyyy hh:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 5, Name = "dd-MM-yyyy HH:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 6, Name = "dd/MM/yyyy hh:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 7, Name = "dd/MM/yyyy HH:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 8, Name = "hh:mm tt" });
            lst.Add(new DrpResponseAC { Id = 9, Name = "HH:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 10, Name = "hh:mm" });
            lst.Add(new DrpResponseAC { Id = 11, Name = "HH:mm" });
            lst.Add(new DrpResponseAC { Id = 12, Name = "seconds" });
            lst.Add(new DrpResponseAC { Id = 18, Name = "h:mm tt" });

            // for From To type Data
            lst.Add(new DrpResponseAC { Id = 15, Name = "onlynumber(XXXXXXXXXX)" });
            lst.Add(new DrpResponseAC { Id = 16, Name = "From: +XXXXXXXXXXXXX To: +XXXXXXXXXX" });
            //lst.Add(new DrpResponseAC { Id = 17, Name = "other" });
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> GetTransactionTypeByProviderId(long providerId)
        {
            if (providerId > 0)
            {
                List<Transactiontypesetting> transactionTypeSettings = await _dbTeleBilling_V01Context.Transactiontypesetting.Where(x => !x.IsDelete && x.ProviderId == providerId && x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
                return _mapper.Map<List<DrpResponseAC>>(transactionTypeSettings);
            }
            return new List<DrpResponseAC>();
        }

        public async Task<List<DrpResponseAC>> ServiceTypeListByAssigneTypeId(long providerid)
        {
            //bool isBusinessOnly = true;
            //if(toAssigneTypeId == Convert.ToInt16(EnumList.ChargeAssigneType.Both))
            //	isBusinessOnly = false;

            List<Providerservice> lst = await _dbTeleBilling_V01Context.Providerservice.Where(x => x.ProviderId == providerid && !x.IsDelete).Include(x => x.ServiceType).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }


        public async Task<List<DrpResponseAC>> GetServicePackageListByServiceTypeId(long serviceTypeId)
        {
            List<Providerpackage> providerPackages = await _dbTeleBilling_V01Context.Providerpackage.Where(x => x.ServiceTypeId == serviceTypeId && !x.IsDelete && x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(providerPackages);
        }

        public async Task<List<BillAC>> GetAccountBills()
        {
            List<BillAC> billACs = new List<BillAC>();
            List<Employeebillmaster> employeeBillMasters = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete).Include(x => x.Currency).Include(x => x.Provider).Include(x => x.Employee).Include(x => x.EmpBusinessUnit).Include(x => x.Employee.CostCenter).Include(x => x.MobileAssignTypeNavigation).ToListAsync();
            if (employeeBillMasters.Any())
            {
                foreach (var item in employeeBillMasters)
                {
                    BillAC billAC = new BillAC();
                    billAC.Amount = item.TotalBillAmount;
					
					if(item.MobileAssignTypeNavigation != null)
						billAC.AssignedType = item.MobileAssignTypeNavigation.Name;
                    
					EnumList.Month monthEnum = (EnumList.Month)item.BillMonth;
                    billAC.BillDate = monthEnum.ToString() + " " + item.BillYear.ToString();
                    if (item.EmpBusinessUnitId != null)
                    {
                        billAC.BusinessUnit = item.EmpBusinessUnit.Name;
                        billAC.BusinessUnitId = Convert.ToInt64(item.EmpBusinessUnitId);
                    }
                    billAC.Currency = item.Currency.Code;
                    billAC.Description = item.Description;
                    if (item.EmployeeId != null)
                    {
                        billAC.EmployeeId = Convert.ToInt64(item.EmployeeId);
                        billAC.EmployeeName = item.Employee.FullName;
                        billAC.CostCenter = item.Employee.CostCenter.Name;
                        billAC.CostCenterId = item.Employee.CostCenterId;
                    }
                    billAC.MobileNumber = item.TelephoneNumber;
                    billAC.BillNumber = item.BillNumber;
                    billAC.Month = item.BillMonth;
                    billAC.Provider = item.Provider.Name;
                    billAC.ProviderId = item.ProviderId;
                    billAC.Year = item.BillYear;
                    billAC.Status = CommonFunction.GetDescriptionFromEnumValue(((EnumList.EmployeeBillStatus)item.EmployeeBillStatus));
                    billACs.Add(billAC);
                }
            }
            return billACs;
        }

        public async Task<List<DrpResponseAC>> GetBillStatusList()
        {
            int billWaitingForLineMangerApprovalStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
            int billWaitingForIdentificationStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
            List<FixBillemployeestatus> fixBillEmployeeStatuses = await _dbTeleBilling_V01Context.FixBillemployeestatus.Where(x => (x.Id == billWaitingForIdentificationStatusId || x.Id == billWaitingForLineMangerApprovalStatusId) && x.IsActive).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(fixBillEmployeeStatuses);
        }

        public async Task<List<DrpResponseAC>> GetInternetDevices()
        {
            List<MstInternetdevicedetail> mstInternetDeviceDetails = await _dbTeleBilling_V01Context.MstInternetdevicedetail.Where(x => !x.IsDelete).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(mstInternetDeviceDetails);
        }

        public async Task<List<DrpResponseAC>> BusinessUnitDepartmentList(long id)
        {
            List<MstDepartment> departmentlist = await _dbTeleBilling_V01Context.MstDepartment.Where(x => x.BusinessUnitId == id && x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(departmentlist);
        }

        public NotificationObjAC GetNotifications(long userId)
        {
            NotificationObjAC notificationObjAC = new NotificationObjAC();
            SortedList sl = new SortedList();
            sl.Add("LoginUserId", userId);
            DataSet ds = _objDalmysql.GetDataSet("ups_GetNotificationList", sl);
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                {
                    notificationObjAC.listOfNotification = _objDal.ConvertDataTableToGenericList<NotificationAC>(ds.Tables[0]).ToList();
                }
                if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
                {
                    notificationObjAC.RemainingNotificationCount = Convert.ToInt64(ds.Tables[1].Rows[0]["RemainingNotificationCount"]);
                }
            }
            return notificationObjAC;
        }

        //public async Task<List<NotificationAC>> GetAllNotifications(long userId)
        //{
        //	List<NotificationAC> notificationACs = new List<NotificationAC>();
        //	MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstAsync(x => x.UserId == userId);
        //	if (!string.IsNullOrEmpty(mstEmployee.EmailId))
        //	{
        //		List<Emailreminderlog> emailreminderlogs = await _dbTeleBilling_V01Context.Emailreminderlog.Where(x => !x.IsReadNotification && x.EmailTo.Trim().ToLower() == mstEmployee.EmailId.ToLower().Trim()).Include(x => x.Template).OrderBy(x => x.CreatedDateInt).ToListAsync();
        //		if (emailreminderlogs.Any()) { 
        //			foreach(var item in emailreminderlogs)
        //			{
        //				item.IsReadNotification = true;
        //			}
        //			_dbTeleBilling_V01Context.UpdateRange(emailreminderlogs);
        //			await _dbTeleBilling_V01Context.SaveChangesAsync();

        //			notificationACs = _mapper.Map<List<NotificationAC>>(emailreminderlogs);
        //		}
        //	}
        //	return notificationACs;
        //}

        //public async Task<long> ReadNotification(long id) {
        //	Emailreminderlog emailreminderlog = await _dbTeleBilling_V01Context.Emailreminderlog.Where(x=>x.Id == id).Include(x=>x.EmployeeBill).Include(x=>x.EmployeeBill.Employee).FirstAsync();
        //	emailreminderlog.IsReadNotification = true;
        //	_dbTeleBilling_V01Context.Update(emailreminderlog);
        //	await _dbTeleBilling_V01Context.SaveChangesAsync();
        //	return 0;
        //}

        public async Task<bool> ReadAllNotification(long userId)
        {
            List<Notificationlog> notificationlogs = await _dbTeleBilling_V01Context.Notificationlog.Where(x => x.UserId == userId && x.IsReadNotification == false).ToListAsync();
            if (notificationlogs.Any())
            {
                foreach (var item in notificationlogs)
                {
                    item.IsReadNotification = true;
                }
                _dbTeleBilling_V01Context.UpdateRange(notificationlogs);
                await _dbTeleBilling_V01Context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<List<DrpResponseAC>> GetServicePackagesByProvider(long serviceTypeId, long providerId)
        {
            List<Providerpackage> providerPackages = await _dbTeleBilling_V01Context.Providerpackage.Where(x => x.ServiceTypeId == serviceTypeId && x.ProviderId == providerId && !x.IsDelete && x.IsActive).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(providerPackages);
        }


        public async Task<List<DrpResponseAC>> ActionList()
        {
            List<DrpCountryAC> drpActionList = new List<DrpCountryAC>();
            List<FixAuditlogactiontype> lst = await _dbTeleBilling_V01Context.FixAuditlogactiontype.Where(x => !x.IsDeleted).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        #endregion
    }
}
