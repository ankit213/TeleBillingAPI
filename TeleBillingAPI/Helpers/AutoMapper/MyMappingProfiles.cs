using AutoMapper;
using System;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Models;

namespace TeleBillingAPI.Helpers.AutoMapper
{
    public class MyMappingProfiles : Profile
    {
        public MyMappingProfiles()
        {
            #region Drop Down Mapping
            CreateMap<Provider, DrpResponseAC>();
            CreateMap<MstHandsetdetail, DrpResponseAC>();
            CreateMap<Providerservice, DrpResponseAC>()
                    .ForMember(dest => dest.Name, source => source.MapFrom(src => src.ServiceType.Name))
                    .ForMember(dest => dest.Id, source => source.MapFrom(src => src.ServiceType.Id));

            CreateMap<Provider, DrpResponseAC>()
                .ForMember(dest => dest.Name, source => source.MapFrom(src => src.Name + " (" + src.ContractNumber + ")"));

            CreateMap<MstCountry, DrpCountryAC>()
                    .ForMember(dest => dest.currency, source => source.MapFrom(src => src.Currency.Name));
            CreateMap<FixAssigntype, DrpResponseAC>();
            CreateMap<FixLinetype, DrpResponseAC>();
            CreateMap<FixEmailtemplatetype, DrpResponseAC>().
                ForMember(dest => dest.Name, source => source.MapFrom(src => src.TemplateType));

            CreateMap<Providerpackage, DrpResponseAC>();
            CreateMap<FixLinestatus, DrpResponseAC>();
            CreateMap<FixCalltype, DrpResponseAC>();
            CreateMap<Transactiontypesetting, DrpResponseAC>().
                ForMember(dest => dest.Name, source => source.MapFrom(src => src.TransactionType));

            CreateMap<FixBillemployeestatus, DrpResponseAC>();
            CreateMap<MstInternetdevicedetail, DrpResponseAC>();

            #endregion

            CreateMap<MstRole, RoleAC>();
            CreateMap<MstRolerights, RoleRightsAC>().ReverseMap();
            CreateMap<MstRolerights, MenuLinkAC>();
            CreateMap<Provider, ProviderListAC>()
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Country.Currency.Name))
                .ForMember(dest => dest.Country, source => source.MapFrom(src => src.Country.Name));
            CreateMap<Provider, ProviderAC>().ReverseMap();
            CreateMap<Mappingexcel, ExcelMappingListAC>()
                .ForMember(dest => dest.ServiceType, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name));
            CreateMap<Mappingservicetypefield, MappingServiceTypeFieldAC>()
               .ForMember(dest => dest.ColumnAddress, source => source.MapFrom(src => src.DbtableName))
               .ForMember(dest => dest.DisplayFieldName, source => source.MapFrom(src => src.DisplayFieldName));
            CreateMap<Providerpackage, PackageAC>()
                .ForMember(dest => dest.ServiceType, source => source.MapFrom(src => src.ServiceType.Name));

            CreateMap<PackageDetailAC, Providerpackage>().ReverseMap();

            CreateMap<Telephonenumber, TelephoneAC>()
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name))
                .ForMember(dest => dest.LineType, source => source.MapFrom(src => src.LineType.Name));


            CreateMap<TelephoneAC, Telephonenumber>();
            CreateMap<TelephoneDetailAC, Telephonenumber>().ReverseMap();

            CreateMap<MappingservicetypefieldPbx, MappingServiceTypeFieldAC>()
               .ForMember(dest => dest.ColumnAddress, source => source.MapFrom(src => src.DbtableName))
               .ForMember(dest => dest.DisplayFieldName, source => source.MapFrom(src => src.DisplayFieldName));

            CreateMap<MappingexcelPbx, PbxExcelMappingListAC>()
                .ForMember(dest => dest.Device, source => source.MapFrom(src => src.Device.Name));

            CreateMap<Exceluploadlog, BillUploadListAC>()
                .ForMember(dest => dest.BillDate, source => source.MapFrom
                (src =>
                (src.Month > 0 ? CommonFunction.GetMonth(src.Month) : " ") + "  " + src.Year))
                .ForMember(dest => dest.FileName, source => source.MapFrom(src => src.ExcelFileName))
                .ForMember(dest => dest.UploadedDate, source => source.MapFrom(src => src.UploadDate))
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name));


            CreateMap<Exceluploadlogpbx, PbxBillUploadListAC>()
             .ForMember(dest => dest.BillDate, source => source.MapFrom
             (src =>
             (src.Month > 0 ? CommonFunction.GetMonth(src.Month) : " ") + "  " + src.Year))
             .ForMember(dest => dest.FileName, source => source.MapFrom(src => src.ExcelFileName))
             .ForMember(dest => dest.UploadedDate, source => source.MapFrom(src => src.UploadDate))
            .ForMember(dest => dest.Device, source => source.MapFrom(src => src.Device.Name));



            CreateMap<Emailtemplate, TemplateAC>().
                    ForMember(des => des.TemplateType, source => source.MapFrom(src => src.EmailTemplateType.TemplateType));

            CreateMap<Emailtemplate, TemplateDetailAC>().ReverseMap();
            CreateMap<FixEmailtemplatetag, EmailTemplateTagAC>();
            CreateMap<MstHandsetdetail, HandsetDetailAC>().ReverseMap();
            CreateMap<MstEmployee, EmployeeDetailAC>()
                   .ForMember(dest => dest.RoleName, source => source.MapFrom(src => src.Role.RoleName));


            CreateMap<Telephonenumberallocation, AssignTelePhoneAC>()
                 .ForMember(dest => dest.AssignType, source => source.MapFrom(src => src.AssignType.Name))
                 .ForMember(dest => dest.CostCenter, source => source.MapFrom(src => src.Employee.CostCenter.Name))
                 .ForMember(dest => dest.Department, source => source.MapFrom(src => src.Employee.Department.Name))
                 .ForMember(dest => dest.Description, source => source.MapFrom(src => src.TelephoneNumberNavigation.Description))
                 .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
                 .ForMember(dest => dest.AdditionalInfo, source => source.MapFrom(src => (src.Employee.IsPresidentOffice) ? "President Office" : ""))
                 .ForMember(dest => dest.LineStatus, source => source.MapFrom(src => src.LineStatus.Name));

            CreateMap<Telephonenumberallocationpackage, DrpResponseAC>()
                 .ForMember(dest => dest.Id, source => source.MapFrom(src => src.Package.Id))
                 .ForMember(dest => dest.Name, source => source.MapFrom(src => src.Package.Name));

            CreateMap<MstEmployee, EmployeeAC>()
                 .ForMember(dest => dest.Department, source => source.MapFrom(src => src.Department.Name));




            CreateMap<AssignTelephoneDetailAC, Telephonenumberallocation>()
                .ForMember(dest => dest.TelephoneNumber, source => source.MapFrom(src => src.TelephoneNumberData.TelephoneNumber1))
                .ForMember(dest => dest.TelephoneNumberId, source => source.MapFrom(src => src.TelephoneNumberData.Id))
                .ForMember(dest => dest.EmployeeId, source => source.MapFrom(src => src.EmployeeData.UserId))
                .ForMember(dest => dest.EmpPfnumber, source => source.MapFrom(src => src.EmployeeData.EmpPfnumber));

            CreateMap<Telephonenumberallocation, AssignTelephoneDetailAC>();

            CreateMap<Mappingexcel, MappingDetailAC>()
                 .ForMember(dest => dest.Id, source => source.MapFrom(src => src.Id))
                 .ForMember(dest => dest.ProviderId, source => source.MapFrom(src => src.ProviderId))
                 .ForMember(dest => dest.ServiceTypeId, source => source.MapFrom(src => src.ServiceTypeId))
                 .ForMember(dest => dest.HaveHeader, source => source.MapFrom(src => src.HaveHeader))
                 .ForMember(dest => dest.HaveTitle, source => source.MapFrom(src => src.HaveTitle))
                 .ForMember(dest => dest.TitleName, source => source.MapFrom(src => src.TitleName))
                 .ForMember(dest => dest.WorkSheetNo, source => source.MapFrom(src => src.WorkSheetNo))
                 .ForMember(dest => dest.ExcelReadingColumn, source => source.MapFrom(src => src.ExcelReadingColumn))
                 .ForMember(dest => dest.ExcelColumnNameForTitle, source => source.MapFrom(src => src.ExcelColumnNameForTitle));


            CreateMap<MappingexcelPbx, MappingDetailPbxAC>()
                .ForMember(dest => dest.Id, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceId, source => source.MapFrom(src => src.DeviceId))
                .ForMember(dest => dest.HaveHeader, source => source.MapFrom(src => src.HaveHeader))
                .ForMember(dest => dest.HaveTitle, source => source.MapFrom(src => src.HaveTitle))
                .ForMember(dest => dest.TitleName, source => source.MapFrom(src => src.TitleName))
                .ForMember(dest => dest.WorkSheetNo, source => source.MapFrom(src => src.WorkSheetNo))
                .ForMember(dest => dest.ExcelReadingColumn, source => source.MapFrom(src => src.ExcelReadingColumn))
                .ForMember(dest => dest.ExcelColumnNameForTitle, source => source.MapFrom(src => src.ExcelColumnNameForTitle));

            CreateMap<Operatorcalllog, OperatorCallLogAC>()
               .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
               .ForMember(dest => dest.ProviderName, source => source.MapFrom(src => src.Provider.Name))
               .ForMember(dest => dest.CallTypeName, source => source.MapFrom(src => src.CallType.Name))
               .ForMember(dest => dest.ExtensionNumber, source => source.MapFrom(src => src.Employee.ExtensionNumber));

            CreateMap<OperatorCallLogDetailAC, Operatorcalllog>()
               .ForMember(dest => dest.EmpPfnumber, source => source.MapFrom(src => src.EmployeeAC.EmpPfnumber))
               .ForMember(dest => dest.EmployeeId, source => source.MapFrom(src => src.EmployeeAC.UserId))
               .ForMember(dest => dest.ExtensionNumber, source => source.MapFrom(src => src.EmployeeAC.ExtensionNumber));

            CreateMap<Operatorcalllog, OperatorCallLogDetailAC>();

            CreateMap<Configuration, Configuration>();

            CreateMap<ProviderContactDetailAC, Providercontactdetail>().ReverseMap();

            CreateMap<Transactiontypesetting, ProviderWiseTransactionAC>()
                 .ForMember(dest => dest.ProviderName, source => source.MapFrom(src => src.Provider.Name))
                 .ForMember(dest => dest.TypeAs, source => source.MapFrom(src => src.SetTypeAsNavigation.Name));

            CreateMap<ProviderWiseTransactionAC, Transactiontypesetting>();

            CreateMap<Exceldetail, UnAssignedBillAC>()
                .ForMember(dest => dest.ExcelDetailId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceTypeName, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Currency.Code));

            CreateMap<Billdetails, UnAssignedBillAC>()
                .ForMember(dest => dest.ExcelDetailId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceTypeName, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.Comment, source => source.MapFrom(src => src.EmployeeComment));

            CreateMap<Telephonenumberallocationpackage, TelePhonePackageDetails>();

            CreateMap<Employeebillservicepackage, PackageServiceAC>()
                .ForMember(dest => dest.PackageName, source => source.MapFrom(src => src.Package != null ? src.Package.Name : string.Empty))
                .ForMember(dest => dest.ServiceTypeName, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.DeductionAmount, source => source.MapFrom(src => src.DeductionAmount == null ? 0 : src.DeductionAmount))
                .ForMember(dest => dest.PackageLimitAmount, source => source.MapFrom(src => src.Package != null ? src.Package.PackageAmount : 0));

            CreateMap<Employeebillmaster, CurrentBillAC>()
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name))
                .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.AssigneType, source => source.MapFrom(src => src.MobileAssignTypeNavigation.Name))
                .ForMember(dest => dest.BillStatus, source => source.MapFrom(src => src.EmployeeBillStatusNavigation.Name))
                .ForMember(dest => dest.EmployeeBillMasterId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.TotalBillAmount));

            CreateMap<Employeebillmaster, ExportMyStaffBillsAC>()
                .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.MobileAssignedType, source => source.MapFrom(src => src.MobileAssignTypeNavigation.Name))
                .ForMember(dest => dest.MobileNumber, source => source.MapFrom(src => src.TelephoneNumber))
                .ForMember(dest => dest.BillStatus, source => source.MapFrom(src => src.EmployeeBillStatusNavigation.Name))
                .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.TotalBillAmount))
                .ForMember(dest => dest.IsAlreadyReImbursement, source => source.MapFrom(src => src.IsReImbursementRequest == true ? "Yes" : "No"));


            CreateMap<Employeebillmaster, ExportPreviousPeriodBillsAC>()
                .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.BillStatus, source => source.MapFrom(src => src.EmployeeBillStatusNavigation.Name))
                .ForMember(dest => dest.ManagerName, source => source.MapFrom(src => src.Linemanager.FullName))
                .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.TotalBillAmount))
                .ForMember(dest => dest.MobileNumber, source => source.MapFrom(src => src.TelephoneNumber))
                .ForMember(dest => dest.IsAlreadyReImbursement, source => source.MapFrom(src => src.IsReImbursementRequest == true ? "Yes" : "No"));

            CreateMap<Billdelegate, BillDelegatesListAC>()
                .ForMember(dest => dest.DelegateEmployeeName, source => source.MapFrom(src => src.DelegateEmployee.FullName))
                .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName));

            CreateMap<Billreimburse, ReImburseBillsAC>()
               .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.EmployeeBill.TotalBillAmount))
               .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.EmployeeBill.Employee.FullName))
               .ForMember(dest => dest.BillNumber, source => source.MapFrom(src => src.EmployeeBill.BillNumber))
               .ForMember(dest => dest.TelephoneNumber, source => source.MapFrom(src => src.EmployeeBill.TelephoneNumber))
               .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.EmployeeBill.Provider.Name))
               .ForMember(dest => dest.Providerid, source => source.MapFrom(src => src.EmployeeBill.ProviderId))
               .ForMember(dest => dest.EmployeeId, source => source.MapFrom(src => src.EmployeeBill.EmployeeId))
               .ForMember(dest => dest.BillMonth, source => source.MapFrom(src => src.EmployeeBill.BillMonth))
               .ForMember(dest => dest.BillYear, source => source.MapFrom(src => src.EmployeeBill.BillYear));

            CreateMap<Memo, BillMemoAC>()
                  .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name));

            CreateMap<Billmaster, MemoBillsAC>()
                 .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name))
                 .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.BillAmount))
                 .ForMember(dest => dest.BillMasterId, source => source.MapFrom(src => src.Id));

            CreateMap<MemoAC, Memo>();

            CreateMap<MstRole, DrpResponseAC>()
                .ForMember(dest => dest.Id, source => source.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Name, source => source.MapFrom(src => src.RoleName));

            CreateMap<Transactiontypesetting, DrpResponseAC>()
           .ForMember(dest => dest.Id, source => source.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, source => source.MapFrom(src => src.TransactionType));


            CreateMap<FixServicetype, ServiceTypeAC>();

            CreateMap<EmployeeProfileDetailSP, EmployeeProfileDetailAC>()
                 .ForMember(dest => dest.IsActive, source => source.MapFrom(src => Convert.ToBoolean(src.IsActive)))
                 .ForMember(dest => dest.IsDelete, source => source.MapFrom(src => Convert.ToBoolean(src.IsDelete)))
                 .ForMember(dest => dest.IsPresidentOffice, source => source.MapFrom(src => Convert.ToBoolean(src.IsPresidentOffice)))
                 .ForMember(dest => dest.IsSystemUser, source => source.MapFrom(src => Convert.ToBoolean(src.IsSystemUser)));


            CreateMap<MstEmployeeSP, MstEmployeeAC>()
                 .ForMember(dest => dest.IsActive, source => source.MapFrom(src => Convert.ToBoolean(src.IsActive)))
                 .ForMember(dest => dest.IsDelete, source => source.MapFrom(src => Convert.ToBoolean(src.IsDelete)))
                 .ForMember(dest => dest.IsPresidentOffice, source => source.MapFrom(src => Convert.ToBoolean(src.IsSystemUser)))
                 .ForMember(dest => dest.IsSystemUser, source => source.MapFrom(src => Convert.ToBoolean(src.IsPresidentOffice)));

            CreateMap<Notificationlog, NotificationAC>()
                .ForMember(dest => dest.ActionUserName, source => source.MapFrom(src => src.ActionUser.FullName));

            CreateMap<ExceldetailError, MobilityExcelUploadDetailStringAC>()
                //.ForMember(dest => dest.CallerName, source => source.MapFrom(src => src.CallerName))                
                //.ForMember(dest => dest.Description, source => source.MapFrom(src => src.Description))
                //.ForMember(dest => dest.CallerNumber, source => source.MapFrom(src => src.CallerNumber))
                //.ForMember(dest => dest.CallAmount, source => source.MapFrom(src => src.CallAmount))
                //.ForMember(dest => dest.CallDate, source => source.MapFrom(src => src.CallDate))
                //.ForMember(dest => dest.CallTime, source => source.MapFrom(src => src.CallTime))
                //.ForMember(dest => dest.CallDuration, source => source.MapFrom(src => src.CallDuration))
                //.ForMember(dest => dest.MessageCount, source => source.MapFrom(src => src.MessageCount))
                .ForMember(dest => dest.CallDataKB, source => source.MapFrom(src => src.CallDataKb))
                .ForMember(dest => dest.CallType, source => source.MapFrom(src => src.TransType))
                .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            CreateMap<MobilityExcelUploadDetailStringAC, ExceldetailError>()
               .ForMember(dest => dest.CallDataKb, source => source.MapFrom(src => src.CallDataKB))
               .ForMember(dest => dest.TransType, source => source.MapFrom(src => src.CallType))
               .ForMember(dest => dest.ErrorSummary, source => source.MapFrom(src => src.ErrorMessage));

            CreateMap<VoipExcelUploadDetailStringAC, SkypeexceldetailError>()
              .ForMember(dest => dest.ErrorSummary, source => source.MapFrom(src => src.ErrorMessage));

            CreateMap<SkypeexceldetailError, VoipExcelUploadDetailStringAC>()
              .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            //CreateMap<ExceldetailError, MadaExcelUploadDetailStringAC>()
            // .ForMember(dest => dest.ServiceDetail, source => source.MapFrom
            // (src => (src.ServiceTypeId > 0 ? CommonFunction.GetServiceName(src.ServiceTypeId) : "")))
            // .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            CreateMap<ExceldetailError, DataCenterFacilityExcelUploadDetailStringAC>()
            .ForMember(dest => dest.ServiceName, source => source.MapFrom
            (src => (src.ServiceTypeId > 0 ? CommonFunction.GetServiceName(src.ServiceTypeId) : "")))
            .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            CreateMap<ExceldetailError, InternetServiceExcelUploadDetailStringAC>()
            .ForMember(dest => dest.ServiceName, source => source.MapFrom
            (src => (src.ServiceTypeId > 0 ? CommonFunction.GetServiceName(src.ServiceTypeId) : "")))
            .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            CreateMap<ExceldetailError, ManagedHostingServiceExcelUploadDetailStringAC>()
           .ForMember(dest => dest.ServiceName, source => source.MapFrom
           (src => (src.ServiceTypeId > 0 ? CommonFunction.GetServiceName(src.ServiceTypeId) : "")))
           .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            CreateMap<ExceldetailpbxError, PbxExcelUploadDetailStringAC>()
             .ForMember(dest => dest.ErrorMessage, source => source.MapFrom(src => src.ErrorSummary));

            CreateMap<PbxExcelUploadDetailStringAC, ExceldetailpbxError>()
            .ForMember(dest => dest.ErrorSummary, source => source.MapFrom(src => src.ErrorMessage));

        }
    }
}
