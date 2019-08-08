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
using TeleBillingUtility.Helpers.CommonFunction;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;
namespace TeleBillingRepository.Repository.StaticData
{
    public class StaticDataRepository : IStaticDataRepository 
    {

        #region "Private Variable(s)"
        private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
        #endregion

        #region "Constructor"
        public StaticDataRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
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
            List<MstCountry> lst = await _dbTeleBilling_V01Context.MstCountry.Where(x => x.IsActive).Include(x=>x.Currency).OrderBy(x => x.Id).ToListAsync();
			return _mapper.Map<List<DrpCountryAC>>(lst);
        }
        

        public async Task<List<DrpResponseAC>> ServiceTypeList()
        {
            List<FixServiceType> lst = new List<FixServiceType>();
            lst = await _dbTeleBilling_V01Context.FixServiceType.Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }


        public async Task<List<DrpResponseAC>> ProviderServiceTypeList(long providerid)
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            var lstsevice= await _dbTeleBilling_V01Context.ProviderService.Where(x => !x.IsDelete && x.ProviderId==providerid).Include(x => x.ServiceType).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lstsevice);
        }

        public async Task<List<DrpResponseAC>> CostCenterList()
        {
            List<MstCostCenter> lst = new List<MstCostCenter>();
            lst = await _dbTeleBilling_V01Context.MstCostCenter.Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        
        public async Task<List<DrpResponseAC>> BusinessUnitCostCenterList(long id)
        {
            List<MstCostCenter> lst = new List<MstCostCenter>();
            lst = await _dbTeleBilling_V01Context.MstCostCenter.Where(x => x.IsActive && x.BusinessUnitid==id).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> BusinessUnitList()
        {
            List<MstBusinessUnit> lst = new List<MstBusinessUnit>();
            lst = await _dbTeleBilling_V01Context.MstBusinessUnit.Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> RoleNameList()
        {
            List<MstRole> lst = new List<MstRole>();
            lst = await _dbTeleBilling_V01Context.MstRole.Where(x => x.IsActive).OrderBy(x => x.RoleName).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }



        public async Task<List<DrpResponseAC>> DepartmentList()
        {
            List<MstDepartment> lst = new List<MstDepartment>();
            lst = await _dbTeleBilling_V01Context.MstDepartment.Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<MappingServiceTypeFieldAC>> DBFieldsList( long servicetypeid)
        {
            List<MappingServiceTypeFieldAC> lst = new List<MappingServiceTypeFieldAC>();
            var lstsevice = await _dbTeleBilling_V01Context.MappingServiceTypeField.Where(x => x.ServiceTypeId==servicetypeid ).OrderBy(x => x.DisplayOrder).ToListAsync();
            return _mapper.Map<List<MappingServiceTypeFieldAC>>(lstsevice);
        }

        public async Task<List<MappingServiceTypeFieldAC>> DBFieldsListByOrder(long servicetypeid)
        {
            List<MappingServiceTypeFieldAC> lst = new List<MappingServiceTypeFieldAC>();
            var lstsevice = await _dbTeleBilling_V01Context.MappingServiceTypeField.Where(x => x.ServiceTypeId == servicetypeid).OrderBy(x => x.DbcolumnName).ToListAsync();
            return _mapper.Map<List<MappingServiceTypeFieldAC>>(lstsevice);
        }

        public async Task<List<DrpResponseAC>> GetLineTypeList() {
			List<FixLineType> lstLineType = await _dbTeleBilling_V01Context.FixLineType.Where(x=>x.IsActive).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lstLineType);
		}


		public async Task<List<DrpResponseAC>> GetAssignTypeList() {
			List<FixAssignType> lstAssignType = await _dbTeleBilling_V01Context.FixAssignType.Where(x => x.IsActive).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lstAssignType);
		}
		


        public async Task<List<MappingServiceTypeFieldAC>> DeviceDBFieldsList(long deviceid)
        {
            List<MappingServiceTypeFieldAC> lst = new List<MappingServiceTypeFieldAC>();
            var lstsevice = await _dbTeleBilling_V01Context.MappingServiceTypeFieldPbx.Where(x => x.DeviceId==deviceid).OrderBy(x => x.DisplayOrder).ToListAsync();
            return _mapper.Map<List<MappingServiceTypeFieldAC>>(lstsevice);
        }


        public async Task<List<DrpResponseAC>> DeviceList()
        {
            List<FixDevice> lst = new List<FixDevice>();
            lst = await _dbTeleBilling_V01Context.FixDevice.Where(x => x.IsActive == true).OrderBy(x => x.Id).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> MonthList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.JAN), Name = EnumList.Month.JAN.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.FEB), Name = EnumList.Month.FEB.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.MAR), Name = EnumList.Month.MAR.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.APR), Name = EnumList.Month.APR.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.MAY), Name = EnumList.Month.MAY.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.JUN), Name = EnumList.Month.JUN.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.JUL), Name = EnumList.Month.JUL.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.AUG), Name = EnumList.Month.AUG.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.SEP), Name = EnumList.Month.SEP.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.OCT), Name = EnumList.Month.OCT.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.NOV), Name = EnumList.Month.NOV.ToString()});
            lst.Add(new DrpResponseAC { Id = Convert.ToInt16(EnumList.Month.DEC), Name = EnumList.Month.DEC.ToString()});
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

        public async Task<List<DrpResponseAC>> YearList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();            
            int currentyear= Convert.ToInt16(DateTime.Now.Year.ToString());            
            lst.Add(new DrpResponseAC { Id = currentyear-1, Name = (currentyear - 1).ToString() });
            lst.Add(new DrpResponseAC { Id = currentyear, Name = (currentyear).ToString() });
            //lst.Add(new DrpResponseAC { Id = currentyear + 1, Name = (currentyear + 1).ToString() });

            return _mapper.Map<List<DrpResponseAC>>(lst);
        }

		public async Task<List<DrpResponseAC>> GetTemplateTypeList()
		{
			List<FixEmailTemplateType> lstForTemplateType = await _dbTeleBilling_V01Context.FixEmailTemplateType.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lstForTemplateType);
		}

		public async Task<List<EmailTemplateTagAC>> GetTemaplteTagList()
		{
			List<FixEmailTemplateTag> lstForTemplateTag = await _dbTeleBilling_V01Context.FixEmailTemplateTag.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).ToListAsync();
			return _mapper.Map<List<EmailTemplateTagAC>>(lstForTemplateTag);
		}

		public async Task<EmployeeDetailAC> GetLoggedInUserDetail(long userId) {
			MstEmployee mstEmployee = await _dbTeleBilling_V01Context.MstEmployee.FirstAsync(x=>x.UserId == userId);
			return _mapper.Map<EmployeeDetailAC>(mstEmployee);
		}

		public async Task<List<EmployeeAC>> GetEmployeeListByName(string name) {
			List<MstEmployee> mstEmployeeList = await _dbTeleBilling_V01Context.MstEmployee.Where(x => x.FullName.Contains(name) && !x.IsDelete && x.IsActive).ToListAsync();
			return _mapper.Map<List<EmployeeAC>>(mstEmployeeList);
		}

		public async Task<List<TelephoneNumberAC>> GetNumberList(string number, long id)
		{
			List<TelephoneNumberAC> lstTelePhoneNmber =  await (from tn in _dbTeleBilling_V01Context.TelephoneNumber
						 join tnx in _dbTeleBilling_V01Context.TelephoneNumberAllocation on
						 tn.TelephoneNumber1 equals tnx.TelephoneNumber into numberInfo
						 from numbers in numberInfo.DefaultIfEmpty()
						 where tn.TelephoneNumber1.Contains(number) && (numbers.TelephoneNumber == null) && !tn.IsDelete && tn.IsActive
						 orderby tn.CreatedDate descending
						 select new TelephoneNumberAC
					     { 
								 Id = tn.Id,
								 TelephoneNumber1 = tn.TelephoneNumber1
						 }).Take(10).ToListAsync();

			if(id != 0 && lstTelePhoneNmber.Any()) {//When call on edit mode at that time already assigned telephone number is added on list
			   TelephoneNumberAllocation telephoneAllocation = 	await _dbTeleBilling_V01Context.TelephoneNumberAllocation.FirstOrDefaultAsync(x=>x.Id == id);
				TelephoneNumberAC telNumber = new TelephoneNumberAC();
			    telNumber.Id = telephoneAllocation.TelephoneNumberId;
			    telNumber.TelephoneNumber1 = telephoneAllocation.TelephoneNumber;
			   lstTelePhoneNmber.Add(telNumber);
			}
			return lstTelePhoneNmber;
		}


		public async Task<List<DrpResponseAC>> GetPackageList() {
			List<ProviderPackage> providerPackages =  await _dbTeleBilling_V01Context.ProviderPackage.Where(x=> !x.IsDelete && x.IsActive).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(providerPackages);
		}

		public async Task<List<DrpResponseAC>> GetLineStatusList() {
			List<FixLineStatus> fixLineStatuses = await _dbTeleBilling_V01Context.FixLineStatus.Where(x => x.IsActive).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(fixLineStatuses);
		}

		public async Task<List<DrpResponseAC>> GetCallTypeList(){
			List<FixCallType> fixCallTypes = await _dbTeleBilling_V01Context.FixCallType.Where(x => x.IsActive && x.Id != 0).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(fixCallTypes);
		}

        public List<DrpResponseAC> GetFormatList()
        {
            List<DrpResponseAC> lst = new List<DrpResponseAC>();
            lst.Add(new DrpResponseAC { Id = 1, Name = "dd-MM-yyyy" });
            lst.Add(new DrpResponseAC { Id = 2, Name = "dd/MM/yyyy" });
            lst.Add(new DrpResponseAC { Id = 3, Name = "yyyy/MM/dd" });
            lst.Add(new DrpResponseAC { Id = 4, Name = "dd-MM-yyyy hh:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 5, Name = "dd-MM-yyyy HH:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 6, Name = "dd/MM/yyyy hh:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 7, Name = "dd/MM/yyyy HH:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 8, Name = "hh:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 9, Name = "HH:mm:ss" });
            lst.Add(new DrpResponseAC { Id = 10, Name = "hh:mm" });
            lst.Add(new DrpResponseAC { Id = 11, Name = "HH:mm" });
            lst.Add(new DrpResponseAC { Id = 12, Name = "seconds" });
            return _mapper.Map<List<DrpResponseAC>>(lst);
        }
		
		public async Task<List<DrpResponseAC>> GetTransactionTypeByProviderId(long providerId) {
			List<TransactionTypeSetting> transactionTypeSettings = await _dbTeleBilling_V01Context.TransactionTypeSetting.Where(x=>!x.IsDelete && x.ProviderId == providerId).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(transactionTypeSettings);
		}

		public async Task<List<DrpResponseAC>> ServiceTypeListByAssigneTypeId(long providerid) {
			//bool isBusinessOnly = true;
			//if(toAssigneTypeId == Convert.ToInt16(EnumList.ChargeAssigneType.Both))
			//	isBusinessOnly = false;

			List<ProviderService> lst = await _dbTeleBilling_V01Context.ProviderService.Where(x=>x.ProviderId == providerid && !x.IsDelete).Include(x=>x.ServiceType).OrderBy(x => x.Id).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(lst);
		}

	
		public async Task<List<DrpResponseAC>> GetServicePackageListByServiceTypeId(long serviceTypeId) {
			List<ProviderPackage> providerPackages = await _dbTeleBilling_V01Context.ProviderPackage.Where(x=>x.ServiceTypeId == serviceTypeId && !x.IsDelete && x.IsActive).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(providerPackages);
		}

		public async Task<List<BillAC>> GetAccountBills() {
			List<BillAC> billACs = new List<BillAC>();
			List<EmployeeBillMaster> employeeBillMasters = await _dbTeleBilling_V01Context.EmployeeBillMaster.Where(x=> !x.IsDelete).Include(x=>x.Currency).Include(x=>x.Provider).Include(x=>x.Employee).Include(x=>x.EmpBusinessUnit).Include(x => x.Employee.CostCenter).Include(x=>x.MbileAssignTypeNavigation).ToListAsync();
			if (employeeBillMasters.Any()) {
				foreach(var item in employeeBillMasters)
				{
					BillAC billAC = new BillAC();
					billAC.Amount = item.TotalBillAmount;
					billAC.AssignedType = item.MbileAssignTypeNavigation.Name;
					EnumList.Month monthEnum = (EnumList.Month)item.BillMonth;
					billAC.BillDate = monthEnum.ToString() + " " + item.BillYear.ToString();
					if(item.EmpBusinessUnitId != null)
					{
						billAC.BusinessUnit = item.EmpBusinessUnit.Name;
						billAC.BusinessUnitId = Convert.ToInt64(item.EmpBusinessUnitId);
					}
					billAC.Currency = item.Currency.Code;
					billAC.Description = item.Description;
					if(item.EmployeeId != null)
					{
						billAC.EmployeeId = Convert.ToInt64(item.EmployeeId);
						billAC.EmployeeName = item.Employee.FullName;
						billAC.CostCenter = item.Employee.CostCenter.Name;
						billAC.CostCenterId = item.Employee.CostCenterId;
					}
					billAC.MobileNumber = item.TelephoneNumber;
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

		public async Task<List<DrpResponseAC>> GetBillStatusList() {
			int billWaitingForLineMangerApprovalStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForLineManagerApproval);
			int billWaitingForIdentificationStatusId = Convert.ToInt16(EnumList.EmployeeBillStatus.WaitingForIdentification);
			List<FixBillEmployeeStatus> fixBillEmployeeStatuses = await _dbTeleBilling_V01Context.FixBillEmployeeStatus.Where(x=> (x.Id == billWaitingForIdentificationStatusId || x.Id == billWaitingForLineMangerApprovalStatusId) && x.IsActive).OrderBy(x=>x.Id).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(fixBillEmployeeStatuses);
		}

		public async Task<List<DrpResponseAC>> GetInternetDevices()
		{
			List<MstInternetDeviceDetail> mstInternetDeviceDetails = await _dbTeleBilling_V01Context.MstInternetDeviceDetail.Where(x => !x.IsDelete).OrderBy(x => x.CreatedDate).ToListAsync();
			return _mapper.Map<List<DrpResponseAC>>(mstInternetDeviceDetails);
		}
		
        public async Task<List<DrpResponseAC>> BusinessUnitDepartmentList(long id)
        {
            List<MstDepartment> departmentlist = await _dbTeleBilling_V01Context.MstDepartment.Where(x => x.BusinessUnitId == id &&  x.IsActive).ToListAsync();
            return _mapper.Map<List<DrpResponseAC>>(departmentlist);
        }
		
        #endregion
    }
}
