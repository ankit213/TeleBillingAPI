﻿namespace TeleBillingRepository.Service.Constants
{
    public interface IStringConstant
    {
        string LoginCredentailWrong { get; }
        string EmailOrPfNumberNotValid { get; }
        string UserAccountDeactivated { get; }
        string EmailNotFound { get; }
        string DataFound { get; }
        string DataNotFound { get; }
        string EmailSent { get; }
        string RoleExists { get; }
        string RoleAddedSuccessfully { get; }
        string RoleUpdatedSuccessfully { get; }
        string RoleRightsUpdatedSuccessfully { get; }
        string ContractNumberExists { get; }
        string ProviderAddedSuccessfully { get; }
        string ProviderUpdatedSuccessfully { get; }
        string ExcelMappingExists { get; }
        string ExcelMappingAddedSuccessfully { get; }
        string ExcelMappingUpdatedSuccessfully { get; }
        string PackageAddedSuccessfully { get; }
        string PackageAlreadyExists { get; }
        string TelphoneAddedSuccessfully { get; }
        string TelphoneAlreadyExists { get; }
        string TelphoneUpdateSuccessfully { get; }
        string TemplateAddedSuccessfully { get; }
        string TemplateTypeAlreadyExists { get; }
        string TemplateUpdateSuccessfully { get; }
        string HandsetAlreadyExists { get; }
        string HandsetUpdateSuccessfully { get; }
        string HandsetAddedSuccessfully { get; }
        string TelephoneAssignedSuccessfully { get; }
        string AssignedTelePhoneEditSuccessfully { get; }
        string PhoneNumberNotExists { get; }
        string TelphoneNumberAlreadyAssigned { get; }
        string EmployeeNotExists { get; }
        string PackageNotExists { get; }
        string StartDateIsEmpty { get; }
        string EndDateIsEmpty { get; }
        string LineStatusNotExists { get; }
        string StartDateNotValid { get; }
        string EndDateNotValid { get; }
        string OperatorCallLogUpdateSuccessfully { get; }
        string OperatorCallLogAddedSuccessfully { get; }
        string CallDateNotValid { get; }
        string CallDateIsEmpty { get; }
        string CallTypeNotExists { get; }
        string TelePhoneNumberIsEmpty { get; }
        string ProviderNotExists { get; }
        string TelePhoneNumberMaxLength { get; }
        string ConfigurationUpdateSuccessfully { get; }
        string ConfigurationAddedSuccessfully { get; }
        string ProviderWiseTransactionTypeAddedSuccessfully { get; }
        string ProviderWiseTransactionTypeUpdatedSuccessfully { get; }
        string ProviderWiseTransactionTypeAlreadyExists { get; }
        string TransactionTypeSettingUpdatedSuccessfully { get; }
        string TransactionTypeIsEmpty { get; }
        string InternetDeviceAlreadyExists { get; }
        string InternetDeviceUpdateSuccessfully { get; }
        string InternetDeviceAddedSuccessfully { get; }
        // Service String constant
        string Mobility { get; }
        string VoiceOnly { get; }
        string InternetService { get; }
        string ManagedHostingService { get; }
        string DataCenterFacility { get; }
        string StaticIP { get; }
        string VOIP { get; }
        string MOC { get; }
        string GeneralService { get; }
        // end of service Constant
        string BillAssignedSuccesfully { get; }
        string BillAllocatedSuccesfully { get; }
        string TelphoneNumberPackageAlreadyAssigned { get; }
        string IdentificationSaveChangeSuccessfully { get; }
        string BillProcessSuccessfully { get; }
        string LineManagerApprovalSuccessfully { get; }
        string AtLeastSelectOneRecord { get; }
        string DelegateAlreadyExists { get; }
        string DelegateUpdateSuccessfully { get; }
        string DelegateAddedSuccessfully { get; }
        string ReImbursementRequestAddedSuccessfully { get; }
        string BillReImburseApprovesuccessfully { get; }
        string BillReImburseRejectsuccessfully { get; }
        string MemoAddedsuccessfully { get; }
        string MemoApprovedsuccessfully { get; }
        string MemoRejectedsuccessfully { get; }
        string BillChangeStatusSuccesfully { get; }
        string AssignTypeNotExists { get; }
        string PackageNotMatchWithTelephoneNumber { get; }
        string LineManagerApproveSuccessfully { get; }
        string LineManagerRejectSuccessfully { get; }
        string BillApprovalMessageSuccessfully { get; }
        string MemoApprovalMessagesuccessfully { get; }
        string EmployeeBillIdentificationNotificationMessage { get; }
        string DelegateBillIdentificationNotificationMessage { get; }
        string LineManagerApprovalNotificationMessage { get; }
        string DelegateBillApprovalNotificationMessage { get; }
        string LineManagerApproveNotificationMessage { get; }
        string LineManagerRejectNotificationMessage { get; }
        string DelegateBillApproveNotificationMessage { get; }
        string DelegateBillRejectNotificationMessage { get; }
        string BillReImbursementRequestNotificationMessage { get; }
        string ReImbursementApproveNotificationMessage { get; }
        string ReImbursementRejectNotificationMessage { get; }
        string SendMemoNotificationMessage { get; }
        string MemoApproveNotificationMessage { get; }
        string MemoRejectNotificationMessage { get; }
        string BillChangestatusNotificaiton { get; }
    }
}
