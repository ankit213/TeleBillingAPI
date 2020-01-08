namespace TeleBillingRepository.Service.Constants
{
    public class StringConstant : IStringConstant
    {
        public string LoginCredentailWrong { get { return "Wrong password, please try again!"; } }
        public string EmailOrPfNumberNotValid { get { return "Email or pfnumber not valid"; } }
        public string UserAccountDeactivated { get { return "Your account have been deactivated, please contact to admin"; } }
        public string EmailNotFound { get { return "Email not found"; } }
        public string DataFound { get { return "Data found successfully"; } }
        public string DataNotFound { get { return "Data not found"; } }
        public string EmailSent { get { return "Email sent successfully!"; } }
        public string RoleExists { get { return "Role is already exists"; } }
        public string RoleAddedSuccessfully { get { return "Role added successfully!"; } }
        public string RoleUpdatedSuccessfully { get { return "Role updated successfully!"; } }
        public string RoleRightsUpdatedSuccessfully { get { return "Role rights updated successfully!"; } }
        public string ContractNumberExists { get { return "Contract number is already exists"; } }
        public string ProviderAddedSuccessfully { get { return "Provider added successfully!"; } }
        public string ProviderUpdatedSuccessfully { get { return "Provider updated successfully!"; } }
        public string ExcelMappingExists { get { return "excel mapping is already exists"; } }
        public string ExcelMappingAddedSuccessfully { get { return "Excel mapping added successfully!"; } }
        public string ExcelMappingUpdatedSuccessfully { get { return "Excel mapping updated successfully!"; } }
        public string PackageAddedSuccessfully { get { return "Package added successfully!"; } }
        public string PackageAlreadyExists { get { return "Package is already exists"; } }
        public string TelphoneAddedSuccessfully { get { return "Telephone added successfully!"; } }
        public string TelphoneAlreadyExists { get { return "Telephone number is already exists"; } }
        public string TelphoneUpdateSuccessfully { get { return "Telephone updated successfully!"; } }
        public string TemplateAddedSuccessfully { get { return "Template added successfully!"; } }
        public string TemplateTypeAlreadyExists { get { return "Template type already exists"; } }
        public string TemplateUpdateSuccessfully { get { return "Template updated successfully!"; } }
        public string HandsetAlreadyExists { get { return "Handset is already exists"; } }
        public string HandsetUpdateSuccessfully { get { return "Handset updated successfully!"; } }
        public string HandsetAddedSuccessfully { get { return "Handset added successfully!"; } }
        public string TelephoneAssignedSuccessfully { get { return "Telephone assinged successfully!"; } }
        public string AssignedTelePhoneEditSuccessfully { get { return "Assigned telephone updated successfully!"; } }
        public string PhoneNumberNotExists { get { return "Telephone number does not exists"; } }
        public string TelphoneNumberAlreadyAssigned { get { return "Telephone number was already assigned"; } }
        public string EmployeeNotExists { get { return "Employee does not exists"; } }
        public string PackageNotExists { get { return "Package does not exists"; } }
        public string StartDateIsEmpty { get { return "Start Date is empty"; } }
        public string EndDateIsEmpty { get { return "End Date is empty"; } }
        public string LineStatusNotExists { get { return "Line Status does not exists"; } }
        public string StartDateNotValid { get { return "Start Date format is not valid"; } }
        public string EndDateNotValid { get { return "End Date format is not valid"; } }
        public string OperatorCallLogUpdateSuccessfully { get { return "Operator call log updated successfully!"; } }
        public string OperatorCallLogAddedSuccessfully { get { return "Operator call log added successfully!"; } }
        public string CallDateNotValid { get { return "Call Date format is not valid"; } }
        public string CallDateIsEmpty { get { return "Call Date is empty"; } }
        public string CallTypeNotExists { get { return "Call Type does not exists"; } }
        public string TelePhoneNumberIsEmpty { get { return "Telephone Number is empty"; } }
        public string ProviderNotExists { get { return "Provider does not exists"; } }
        public string TelePhoneNumberMaxLength { get { return "Telephone number not allow to more than 50 length"; } }
        public string ConfigurationUpdateSuccessfully { get { return "Configuration updated successfully!"; } }
        public string ConfigurationAddedSuccessfully { get { return "Configuration added successfully!"; } }
        public string ProviderWiseTransactionTypeAddedSuccessfully { get { return "Provider wise transaction type added successfully!"; } }
        public string ProviderWiseTransactionTypeUpdatedSuccessfully { get { return "Provider wise transaction type updated successfully!"; } }
        public string ProviderWiseTransactionTypeAlreadyExists { get { return "Provider wise transaction type is already exists"; } }
        public string TransactionTypeSettingUpdatedSuccessfully { get { return "Transaction type setting updated successfully!"; } }
        public string TransactionTypeIsEmpty { get { return "Transaction type is empty"; } }
        public string InternetDeviceAlreadyExists { get { return "Internet device is already exists"; } }
        public string InternetDeviceUpdateSuccessfully { get { return "Internet device added successfully!"; } }
        public string InternetDeviceAddedSuccessfully { get { return "Internet device updated successfully!"; } }
        // Service String constant
        public string Mobility { get { return "Mobility"; } }
        public string VoiceOnly { get { return "Voice Only"; } }
        public string InternetService { get { return "Internet Service"; } }
        public string ManagedHostingService { get { return "Managed Hosting Service"; } }
        public string DataCenterFacility { get { return "Data Center Facility"; } }
        public string StaticIP { get { return "Static IP"; } }
        public string VOIP { get { return "VOIP"; } }
        public string MOC { get { return "MOC"; } }
        public string GeneralService { get { return "General Service"; } }
        public string BillAssignedSuccesfully { get { return "Bills assigned successfully!"; } }
        public string BillAllocatedSuccesfully { get { return "Bill allocated successfully!"; } }
        public string TelphoneNumberPackageAlreadyAssigned { get { return "Telephone number {{$telephonenumber$}} already exists this package"; } }
        public string IdentificationSaveChangeSuccessfully { get { return "Bill identification save change successfully!"; } }
        public string BillProcessSuccessfully { get { return "Bill Process successfully!"; } }
        public string LineManagerApprovalSuccessfully { get { return "Line Manager Approval successfully!"; } }
        public string AtLeastSelectOneRecord { get { return "Please select at least one record"; } }
        public string DelegateAlreadyExists { get { return "Delegate is already exists"; } }
        public string DelegateUpdateSuccessfully { get { return "Delegate updated successfully!"; } }
        public string DelegateAddedSuccessfully { get { return "Delegate added successfully!"; } }
        public string ReImbursementRequestAddedSuccessfully { get { return "Reimbursement request added successfully!"; } }
        public string BillReImburseApprovesuccessfully { get { return "Reimbursement request approved successfully!"; } }
        public string BillReImburseRejectsuccessfully { get { return "Reimbursement request rejected successfully!"; } }
        public string MemoAddedsuccessfully { get { return "Memo added successfully!"; } }
        public string MemoApprovedsuccessfully { get { return "Memo approved successfully!"; } }
        public string MemoRejectedsuccessfully { get { return "Memo rejected successfully!"; } }
        public string BillChangeStatusSuccesfully { get { return "Bill status change successfully!"; } }
        public string AssignTypeNotExists { get { return "Assign type does not exists"; } }
        public string PackageNotMatchWithTelephoneNumber { get { return "Package not match with telephone number"; } }
        public string LineManagerApproveSuccessfully { get { return "Bill approved successfully!"; } }
        public string LineManagerRejectSuccessfully { get { return "Bill rejected successfully!"; } }
        public string BillApprovalMessageSuccessfully { get { return "({{@employee}}) bill's are not {{@currentapproval}} because it's already approved/rejected by another user!"; } }
        public string MemoApprovalMessagesuccessfully { get { return "{{@memo}}(memo's) are not {{@currentapproval}} because it's already approved/rejected by another user!"; } }

        public string EmployeeBillIdentificationNotificationMessage { get { return "New bill received for identification"; } }
        public string DelegateBillIdentificationNotificationMessage { get { return "New delegate bill received for identification"; } }
        public string LineManagerApprovalNotificationMessage { get { return "Bill received for approval"; } }
        public string DelegateBillApprovalNotificationMessage { get { return "Delegate bill received for approval"; } }
        public string LineManagerApproveNotificationMessage { get { return "Bill approved by line manager"; } }
        public string LineManagerRejectNotificationMessage { get { return "Bill rejected by line manager"; } }
        public string DelegateBillApproveNotificationMessage { get { return "Bill approved by delegate user"; } }
        public string DelegateBillRejectNotificationMessage { get { return "Bill rejected by delegate user"; } }
        public string BillReImbursementRequestNotificationMessage { get { return "New Bill received for reimbursement"; } }
        public string ReImbursementApproveNotificationMessage { get { return "Reimbursement bill approved"; } }
        public string ReImbursementRejectNotificationMessage { get { return "Reimbursement bill rejected"; } }
        public string SendMemoNotificationMessage { get { return "New memo received"; } }
        public string MemoApproveNotificationMessage { get { return "Memo approved"; } }
        public string MemoRejectNotificationMessage { get { return "Memo rejected"; } }
        public string BillChangestatusNotificaiton { get { return "Bill Change Status"; } }
    }
}
