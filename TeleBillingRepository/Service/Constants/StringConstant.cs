namespace TeleBillingRepository.Service.Constants
{
	public class StringConstant : IStringConstant
	{
		public string LoginCredentailWrong { get { return "Unauthenticate! Try agin!"; }}
		public string EmailOrPfNumberNotValid { get { return "Email or PFNumber not valid"; }}
        public string EmailNotFound { get { return "Email not found"; }}
        public string DataFound { get { return "Data found successfully"; }}
        public string DataNotFound { get { return "Data not found !"; } }
        public string EmailSent { get { return "Email sent successfully"; }}
		public string RoleExists{ get { return "Role is already exists!"; }}
		public string RoleAddedSuccessfully { get { return "Role added successfully"; }}
		public string RoleUpdatedSuccessfully { get { return "Role updated successfully"; }}
		public string RoleRightsUpdatedSuccessfully { get { return "Role rights updated successfully"; } }
		public string ContractNumberExists { get { return "Contract number is already exists!"; }}
		public string ProviderAddedSuccessfully { get { return "Provider added successfully"; } }
		public string ProviderUpdatedSuccessfully { get { return "Provider updated successfully"; } }
        public string ExcelMappingExists { get { return "excel mapping is already exists!"; } }
        public string ExcelMappingAddedSuccessfully { get { return "Excel mapping added successfully"; } }
        public string ExcelMappingUpdatedSuccessfully { get { return "Excel mapping updated successfully"; } }
		public string PackageAddedSuccessfully { get { return "Package added successfully"; }}
		public string PackageAlreadyExists { get { return "Package is already exists!"; } }
		public string TelphoneAddedSuccessfully { get { return "Telphone added successfully"; } }
		public string TelphoneAlreadyExists { get { return "Telphone is already exists!"; } }
		public string TelphoneUpdateSuccessfully { get { return "Telphone updated successfully"; } }
		public string TemplateAddedSuccessfully { get { return "Template added successfully"; } }
		public string TemplateTypeAlreadyExists { get { return "Template type already exists!"; } }
		public string TemplateUpdateSuccessfully { get { return "Template updated successfully"; } }
		public string HandsetAlreadyExists { get { return "Handset is already exists!"; } }
		public string HandsetUpdateSuccessfully { get { return "Handset updated successfully"; } }
		public string HandsetAddedSuccessfully { get { return "Handset added successfully"; } }
		public string TelephoneAssignedSuccessfully { get { return "Telephone assinged successfully"; } }
		public string AssignedTelePhoneEditSuccessfully { get { return "Assigned telephone updated successfully"; } }
		public string PhoneNumberNotExists { get { return "Telephone number does not exists"; } }
		public string TelphoneNumberAlreadyAssigned { get { return "Telephone number was already assigned"; } }
		public string EmployeeNotExists { get { return "Employee does not exists"; } }
		public string PackageNotExists { get { return "Package does not exists"; } }
		public string StartDateIsEmpty { get { return "Start Date is empty"; } }
		public string EndDateIsEmpty { get { return "End Date is empty"; } }
		public string LineStatusNotExists { get { return "Line Status does not exists"; } }
		public string StartDateNotValid { get { return "Start Date format is not valid"; } }
		public string EndDateNotValid { get { return "End Date format is not valid"; } }
		public string OperatorCallLogUpdateSuccessfully { get { return "Operator call log updated successfully"; } }
		public string OperatorCallLogAddedSuccessfully { get { return "Operator call log added successfully"; } }
		public string CallDateNotValid { get { return "Call Date format is not valid"; } }
		public string CallDateIsEmpty { get { return "Call Date is empty"; } }
		public string CallTypeNotExists { get { return "Call Type does not exists"; } }
		public string TelePhoneNumberIsEmpty { get { return "Telephone Number is empty"; } }
		public string ProviderNotExists { get { return "Provider does not exists"; } }
		public string TelePhoneNumberMaxLength { get { return "Telephone number not allow to more than 50 length"; } }
		public string ConfigurationUpdateSuccessfully { get { return "Configuration updated successfully"; } }
		public string ConfigurationAddedSuccessfully { get { return "Configuration added successfully"; } }
		public string ProviderWiseTransactionTypeAddedSuccessfully { get { return "Provider wise transaction type added successfully"; } }
		public string ProviderWiseTransactionTypeUpdatedSuccessfully { get { return "Provider wise transaction type updated successfully"; } }
		public string ProviderWiseTransactionTypeAlreadyExists { get { return "Provider wise transaction type is already exists!"; } }
		public string TransactionTypeSettingUpdatedSuccessfully { get { return "Transaction type setting updated successfully"; } }
		public string TransactionTypeIsEmpty { get { return "Transaction type is empty"; } }
		public string InternetDeviceAlreadyExists { get { return "Internet device is already exists!"; } }
		public string InternetDeviceUpdateSuccessfully { get { return "Internet device added successfully"; } }
		public string InternetDeviceAddedSuccessfully { get { return "Internet device updated successfully"; } }
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
		public string BillAssignedSuccesfully { get { return "Bills assigned successfully"; }  }
		public string BillAllocatedSuccesfully { get { return "Bill allocated successfully"; } }
		public string TelphoneNumberPackageAlreadyAssigned { get {  return "Telephone number {{$telephonenumber$}} already exists this package"; } }
		public string IdentificationSaveChangeSuccessfully { get { return "Bill identification save change successfully"; } }
		public string BillProcessSuccessfully { get { return "Bill Process successfully"; } }
		public string LineManagerApprovalSuccessfully { get { return "Line Manager Approval successfully"; } }
		public string AtLeastSelectOneRecord { get { return "Please select at least one record"; } }

        public string DelegateAlreadyExists { get { return "Delegate is already exists!"; } }
        public string DelegateUpdateSuccessfully { get { return "Delegate updated successfully"; } }
        public string DelegateAddedSuccessfully { get { return "Delegate added successfully"; } }
		public string ReImbursementRequestAddedSuccessfully { get { return "Re-Imbursement Request Added Successfully"; } }
		public string BillReImburseApprovesuccessfully { get { return "Re-Imbursement Request Approved Successfully"; } }
		public string BillReImburseRejectsuccessfully { get { return "Re-Imbursement Request Rejected Successfully"; } }
		public string MemoAddedsuccessfully { get { return "Memo Added Successfully"; } }
		public string MemoApprovedsuccessfully { get { return "Memo Approved Successfully"; } }
		public string MemoRejectedsuccessfully { get { return "Memo Rejected Successfully"; } }
		public string BillChangeStatusSuccesfully { get { return "Bill Status Change Successfully"; } }
	}
}
