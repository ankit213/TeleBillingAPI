using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.StaticData
{
    public interface IStaticDataRepository
    {
        /// <summary>
        /// This method used for get provider list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpCountryAC>> CountryList();


        /// <summary>
        /// This method used for get All ServiceType list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> ServiceTypeList();

        /// <summary>
        /// This method used for get provider's Service list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> ProviderServiceTypeList(long providerid);
        /// <summary>
        /// Provider Not Mapped Service Type List
        /// </summary>
        /// <param name="providerid"></param>
        /// <returns></returns>
        List<DrpResponseAC> ProviderNotMappedServiceTypeList(long providerid);

        /// <summary>
        /// Get Provider Common Service Type & not mapped service List
        /// </summary>
        /// <param name="providerid"></param>
        /// <returns></returns>
        List<DrpResponseAC> ProviderCommonServiceTypeList(long providerid, long mappingid);


        /// <summary>
        /// This method used for get cost service center list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> CostCenterList();
        /// <summary>
        /// Cost Center List by BusinessUnit Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<DrpResponseAC>> BusinessUnitCostCenterList(long id);

        /// <summary>
        /// This method used for get business unit  list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> BusinessUnitList();

        /// <summary>
        /// This method used for get department list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> DepartmentList();


        /// <summary>
        /// This method used for get business's department list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> BusinessUnitDepartmentList(long id);

        /// <summary>
        /// RoleNameList
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> RoleNameList();


        /// <summary>
        /// This method used for get provider and Service wise get DB fileds list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<MappingServiceTypeFieldAC>> DBFieldsList(long servicetypeid);

        /// <summary>
        /// This method used for get provider and Service wise get DB fileds list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<MappingServiceTypeFieldAC>> DBFieldsListByOrder(long servicetypeid);

        /// <summary>
        /// This method used for get line type list
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetLineTypeList();

        /// <summary>
        /// This method used for get assign type list
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetAssignTypeList();


        /// <summary>
        /// This method used for get device wise get DB fileds list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<MappingServiceTypeFieldAC>> DeviceDBFieldsList(long deviceid);

        /// <summary>
        /// This method used for get device list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> DeviceList();

        /// <summary>
        /// This method used get month list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> MonthList();

        /// <summary>
        /// This method used get year list for drop down.
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> YearList();

        /// <summary>
        /// get color hashcode
        /// </summary>
        /// <returns></returns>
        List<string> ColorList();

        /// <summary>
        /// Provider Transtype List
        /// </summary>
        /// <param name="providerid"></param>
        /// <returns></returns>
        Task<List<DrpResponseAC>> ProviderTranstypeList(long providerid);

        /// <summary>
        /// This method used for get template type list
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetTemplateTypeList();

        /// <summary>
        /// This method used for get email template tag list
        /// </summary>
        /// <returns></returns>
        Task<List<EmailTemplateTagAC>> GetTemaplteTagList();

        /// <summary>
        /// This method used for get logged in user detail
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<EmployeeDetailAC> GetLoggedInUserDetail(long userId);

        /// <summary>
        /// This method used for gwet employee list which contain give name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<EmployeeAC>> GetEmployeeListByName(string name);

        /// <summary>
        /// Get Line Manager Employee List By Name where Emailid is not null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<EmployeeAC>> GetLineManagerEmployeeListByName(string name);

        /// <summary>
        /// This method used for get telephone number list
        /// </summary>
        /// <param name="number"></param>
        /// <param name="id"></param> 
        /// <returns></returns>
        Task<List<TelephoneNumberAC>> GetNumberList(string number, long id);

        /// <summary>
        /// This method used for get package list for drop down
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetPackageList();


        /// <summary>
        /// This method used for get line list for drop down
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetLineStatusList();

        /// <summary>
        /// This method used for get call type list for drop down 
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetCallTypeList();

        /// <summary>
		/// This method used for get format list for drop down 
		/// </summary>
		/// <returns></returns>
        List<DrpResponseAC> GetFormatList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetTransactionTypeByProviderId(long providerId);

        /// <summary>
        /// This method used for get service type list by providerid id.
        /// </summary>
        /// <param name="providerid"></param>
        /// <returns></returns>
        Task<List<DrpResponseAC>> ServiceTypeListByAssigneTypeId(long providerid);


        /// <summary>
        /// This method used for get package list by service id.
        /// </summary>
        /// <param name="serviceTypeId"></param>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetServicePackageListByServiceTypeId(long serviceTypeId);


        /// <summary>
        /// This method used for get account bills.
        /// </summary>
        /// <returns></returns>
        Task<List<BillAC>> GetAccountBills();

        /// <summary>
        /// This method sued for get bill status list 
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetBillStatusList();

        /// <summary>
        /// This method used for get internet devices
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetInternetDevices();

        /// <summary>
        /// This function used for get notifications
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        NotificationObjAC GetNotifications(long userId);

        ///// <summary>
        ///// This function used for get all notifications
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //Task<List<NotificationAC>> GetAllNotifications(long userId);

        /// <summary>
        /// This function used for set isreadnotification=true flag
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Task<long> ReadNotification(long id);

        /// <summary>
        /// This fucntion used for read all notification
        /// </summary>
        /// <param name="lstNotification"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> ReadAllNotification(long userId);

        /// <summary>
        /// This method use for get packages by provider
        /// </summary>
        /// <param name="serviceTypeId"></param>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<List<DrpResponseAC>> GetServicePackagesByProvider(long serviceTypeId, long providerId);

        /// <summary>
        /// ActionList of Audit activity log
        /// </summary>
        /// <returns></returns>
        Task<List<DrpResponseAC>> ActionList();

    }
}
