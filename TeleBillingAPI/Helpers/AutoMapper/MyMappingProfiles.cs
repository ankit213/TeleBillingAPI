using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            CreateMap<MstHandsetDetail, DrpResponseAC>();
            CreateMap<ProviderService, DrpResponseAC>()
                    .ForMember(dest => dest.Name, source => source.MapFrom(src => src.ServiceType.Name))
                    .ForMember(dest => dest.Id, source => source.MapFrom(src => src.ServiceType.Id));

            CreateMap<Provider, DrpResponseAC>();
            CreateMap<MstCountry, DrpCountryAC>()
                    .ForMember(dest => dest.currency, source => source.MapFrom(src => src.Currency.Name));
            CreateMap<FixAssignType, DrpResponseAC>();
            CreateMap<FixLineType, DrpResponseAC>();
            CreateMap<FixEmailTemplateType, DrpResponseAC>().
                ForMember(dest => dest.Name, source => source.MapFrom(src => src.TemplateType));

            CreateMap<ProviderPackage, DrpResponseAC>();
            CreateMap<FixLineStatus, DrpResponseAC>();
            CreateMap<FixCallType, DrpResponseAC>();
            CreateMap<TransactionTypeSetting, DrpResponseAC>().
                ForMember(dest => dest.Name, source => source.MapFrom(src => src.TransactionType));

			CreateMap<FixBillEmployeeStatus,DrpResponseAC>();
			CreateMap<MstInternetDeviceDetail, DrpResponseAC>();
			
			#endregion

			CreateMap<MstRole, RoleAC>();
            CreateMap<MstRoleRight, RoleRightsAC>().ReverseMap();
            CreateMap<MstRoleRight, MenuLinkAC>();
            CreateMap<Provider, ProviderListAC>()
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Country.Currency.Name))
                .ForMember(dest => dest.Country, source => source.MapFrom(src => src.Country.Name));
            CreateMap<Provider, ProviderAC>().ReverseMap();
            CreateMap<MappingExcel, ExcelMappingListAC>()
                .ForMember(dest => dest.ServiceType, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name));
            CreateMap<MappingServiceTypeField, MappingServiceTypeFieldAC>()
               .ForMember(dest => dest.ColumnAddress, source => source.MapFrom(src => src.DbtableName))
               .ForMember(dest => dest.DisplayFieldName, source => source.MapFrom(src => src.DisplayFieldName));
            CreateMap<ProviderPackage, PackageAC>()
                .ForMember(dest => dest.ServiceType, source => source.MapFrom(src => src.ServiceType.Name));

            CreateMap<PackageDetailAC, ProviderPackage>().ReverseMap();

            CreateMap<TelephoneNumber, TelephoneAC>()
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name))
                .ForMember(dest => dest.LineType, source => source.MapFrom(src => src.LineType.Name));

            CreateMap<TelephoneAC, TelephoneNumber>();
            CreateMap<TelephoneDetailAC, TelephoneNumber>().ReverseMap();

            CreateMap<MappingServiceTypeFieldPbx, MappingServiceTypeFieldAC>()
               .ForMember(dest => dest.ColumnAddress, source => source.MapFrom(src => src.DbtableName))
               .ForMember(dest => dest.DisplayFieldName, source => source.MapFrom(src => src.DisplayFieldName));

            CreateMap<MappingExcelPbx, PbxExcelMappingListAC>()
                .ForMember(dest => dest.Device, source => source.MapFrom(src => src.Device.Name));

            CreateMap<ExcelUploadLog, BillUploadListAC>()
                .ForMember(dest => dest.BillDate, source => source.MapFrom
                (src =>
                (src.Month > 0 ? CommonFunction.GetMonth(src.Month) : " ") + "  " + src.Year))
                .ForMember(dest => dest.FileName, source => source.MapFrom(src => src.ExcelFileName))
                .ForMember(dest => dest.UploadedDate, source => source.MapFrom(src => src.UploadDate))
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name));

            CreateMap<EmailTemplate, TemplateAC>().
                    ForMember(des => des.TemplateType, source => source.MapFrom(src => src.EmailTemplateType.TemplateType));

            CreateMap<EmailTemplate, TemplateDetailAC>().ReverseMap();
            CreateMap<FixEmailTemplateTag, EmailTemplateTagAC>();
            CreateMap<MstHandsetDetail, HandsetDetailAC>().ReverseMap();
            CreateMap<MstEmployee, EmployeeDetailAC>();

            CreateMap<TelephoneNumberAllocation, AssignTelePhoneAC>()
                 .ForMember(dest => dest.AssignType, source => source.MapFrom(src => src.AssignType.Name))
                 .ForMember(dest => dest.CostCenter, source => source.MapFrom(src => src.Employee.CostCenter.Name))
                 .ForMember(dest => dest.Department, source => source.MapFrom(src => src.Employee.Department.Name))
                 .ForMember(dest => dest.Descrption, source => source.MapFrom(src => src.TelephoneNumberNavigation.Description))
                 .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
                 .ForMember(dest => dest.LineStatus, source => source.MapFrom(src => src.LineStatus.Name));

            CreateMap<TelePhoneNumberAllocationPackage, DrpResponseAC>()
                 .ForMember(dest => dest.Id, source => source.MapFrom(src => src.Package.Id))
                 .ForMember(dest => dest.Name, source => source.MapFrom(src => src.Package.Name));

            CreateMap<MstEmployee, EmployeeAC>()
                 .ForMember(dest => dest.Department, source => source.MapFrom(src => src.Department.Name));




            CreateMap<AssignTelephoneDetailAC, TelephoneNumberAllocation>()
                .ForMember(dest => dest.TelephoneNumber, source => source.MapFrom(src => src.TelephoneNumberData.TelephoneNumber1))
                .ForMember(dest => dest.TelephoneNumberId, source => source.MapFrom(src => src.TelephoneNumberData.Id))
                .ForMember(dest => dest.EmployeeId, source => source.MapFrom(src => src.EmployeeData.UserId))
                .ForMember(dest => dest.EmpPfnumber, source => source.MapFrom(src => src.EmployeeData.EmpPfnumber));

            CreateMap<TelephoneNumberAllocation, AssignTelephoneDetailAC>();

            CreateMap<MappingExcel, MappingDetailAC>()
                 .ForMember(dest => dest.Id, source => source.MapFrom(src => src.Id))
                 .ForMember(dest => dest.ProviderId, source => source.MapFrom(src => src.ProviderId))
                 .ForMember(dest => dest.ServiceTypeId, source => source.MapFrom(src => src.ServiceTypeId))
                 .ForMember(dest => dest.HaveHeader, source => source.MapFrom(src => src.HaveHeader))
                 .ForMember(dest => dest.HaveTitle, source => source.MapFrom(src => src.HaveTitle))
                 .ForMember(dest => dest.TitleName, source => source.MapFrom(src => src.TitleName))
                 .ForMember(dest => dest.WorkSheetNo, source => source.MapFrom(src => src.WorkSheetNo))
                 .ForMember(dest => dest.ExcelReadingColumn, source => source.MapFrom(src => src.ExcelReadingColumn))
                 .ForMember(dest => dest.ExcelColumnNameForTitle, source => source.MapFrom(src => src.ExcelColumnNameForTitle));

            CreateMap<OperatorCallLog, OperatorCallLogAC>()
               .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
               .ForMember(dest => dest.ProviderName, source => source.MapFrom(src => src.Provider.Name))
               .ForMember(dest => dest.CallTypeName, source => source.MapFrom(src => src.CallType.Name));

            CreateMap<OperatorCallLogDetailAC, OperatorCallLog>()
               .ForMember(dest => dest.EmpPfnumber, source => source.MapFrom(src => src.EmployeeAC.EmpPfnumber))
               .ForMember(dest => dest.EmployeeId, source => source.MapFrom(src => src.EmployeeAC.UserId))
               .ForMember(dest => dest.ExtensionNumber, source => source.MapFrom(src => src.EmployeeAC.ExtensionNumber));

            CreateMap<OperatorCallLog, OperatorCallLogDetailAC>();

            CreateMap<Configuration, Configuration>();

            CreateMap<ProviderContactDetailAC, ProviderContactDetail>().ReverseMap();

            CreateMap<TransactionTypeSetting, ProviderWiseTransactionAC>()
                 .ForMember(dest => dest.ProviderName, source => source.MapFrom(src => src.Provider.Name))
                 .ForMember(dest => dest.TypeAs, source => source.MapFrom(src => src.SetTypeAsNavigation.Name));

            CreateMap<ProviderWiseTransactionAC, TransactionTypeSetting>();

            CreateMap<ExcelDetail, UnAssignedBillAC>()
                .ForMember(dest => dest.ExcelDetailId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceTypeName, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Currency.Code));

            CreateMap<BillDetails, UnAssignedBillAC>()
                .ForMember(dest => dest.ExcelDetailId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceTypeName, source => source.MapFrom(src => src.ServiceType.Name))
				.ForMember(dest => dest.Comment, source => source.MapFrom(src => src.EmployeeComment));

            CreateMap<TelePhoneNumberAllocationPackage, TelePhonePackageDetails>();

            CreateMap<EmployeeBillServicePackage, PackageServiceAC>()
                .ForMember(dest => dest.PackageName, source => source.MapFrom(src => src.Package.Name))
                .ForMember(dest => dest.ServiceTypeName, source => source.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.PackageLimitAmount, source => source.MapFrom(src => src.Package.PackageAmount));

            CreateMap<EmployeeBillMaster, CurrentBillAC>()
                .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name))
                .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.Currency, source => source.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.AssigneType, source => source.MapFrom(src => src.MbileAssignTypeNavigation.Name))
                .ForMember(dest => dest.BillStatus, source => source.MapFrom(src => src.EmployeeBillStatusNavigation.Name))
                .ForMember(dest => dest.EmployeeBillMasterId, source => source.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.TotalBillAmount));

            CreateMap<BillDelegate, BillDelegatesListAC>()
                .ForMember(dest => dest.DelegateEmployeeName, source => source.MapFrom(src => src.DelegateEmployee.FullName))
                .ForMember(dest => dest.EmployeeName, source => source.MapFrom(src => src.Employee.FullName));
			
			CreateMap<BillReImburse,ReImburseBillsAC>()
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

			CreateMap<BillMaster,MemoBillsAC>()
				 .ForMember(dest => dest.Provider, source => source.MapFrom(src => src.Provider.Name))
				 .ForMember(dest => dest.Amount, source => source.MapFrom(src => src.BillAmount))
				 .ForMember(dest => dest.BillMasterId, source => source.MapFrom(src => src.Id));

			CreateMap<MemoAC,Memo>();

            CreateMap<MstRole, DrpResponseAC>()
                .ForMember(dest => dest.Id, source => source.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Name, source => source.MapFrom(src => src.RoleName));

        }
    }
}
