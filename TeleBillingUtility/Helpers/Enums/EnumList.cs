using System.ComponentModel;

namespace TeleBillingUtility.Helpers.Enums
{
    public class EnumList
    {
        public enum ResponseType
        {
            Error = 0,
            Success = 1,
            NotFound = 2,
            Warning = 3,
            UserAsLineManager = -1
        }

        public enum EmailTemplateType
        {
            ForgotPassword = 1,
            BillAllocationForIdentificationNotification = 2,
            ReceiveNewBillApproval = 3,
            DelegateBillApproval = 4,
            BillDelegatesForIdentification = 5,
            ApprovedByLineManager = 6,
            RejectedByLineManager = 7,
            ChargeBillNotification = 8,
            SendMemo = 9,
            MemoApproval = 10,
            LineManagerApprovalRemider = 11,
            EmployeeCallIdentificationRemider = 12,
            NewRegistrationConfirmation = 13,
            NewRegistrationInYourTeam = 14,
            LineManagerApprovalNotification = 15,
            EmployeeCallIdentificationNotification = 16,
            SendEmailNotificationToEmployeForAmountToBeDeducted = 17
        }


        public enum NotificationType
        {
            EmployeeBillIdentification = 1,
            DelegateBillIdentification = 2,
            LineManagerApproval = 3,
            DelegateBillApproval = 4,
            LineManagerApprove = 5,
            LineManagerReject = 6,
            DelegateBillApprove = 7,
            DelegateBillReject = 8,
            BillClosed = 9,
            BillReIdentifiction = 10,
            BillReImbursementRequest = 11,
            ReImbursementApprove = 12,
            ReImbursementReject = 13,
            SendMemo = 14,
            MemoApprove = 15,
            MemoReject = 16,
            ChangeBillStatus = 17
        }


        public enum TransactionTraseLog
        {
            UpdateRecord = 1,
            ChangeStatus = 2,
            RequestApprove = 3,
            RequestReject = 4,
            ForgotPassword = 5,
            BillStatusChange = 6,
            BillClosed = 7,
            ExcelFileUpload = 8
        }

        public enum CurrencyType
        {
            KD = 1,
            SAR = 2,
            AED = 3,
            USD = 4
        }

        public enum FileType
        {
            BulkAssignTelePhone = 1,
            UploadCallLog = 2
        }

        public enum ExcelUploadResponseType
        {
            NoStatus = 0,
            ExceptionError = -1,
            Success = 1,
            FileNotFound = 2,
            DataInvalid = 3,
            SomeDataInvalid = 4,
            NoDataFound = 5,
            MultipleServiceFound = 6

        }

        public enum ServiceType
        {
            Mobility = 1,
            VoiceOnly = 2,
            InternetService = 3,
            DataCenterFacility = 4,
            ManagedHostingService = 5,
            StaticIP = 6,
            VOIP = 7,
            MOC = 8,
            GeneralServiceMada = 9,
            GeneralserviceKems = 10,
            LandLine = 11,
            InternetPlanDeviceOffer = 12
        }


        public enum AssignType
        {
            Business = 1,
            Employee = 2
        }

        public enum SupportDataType
        {
            String = 1,
            Number = 2,
            Date = 3,
            Time = 4,
            Boolean = 5
        }

        public enum Month
        {
            JAN = 1,
            FEB = 2,
            MAR = 3,
            APR = 4,
            MAY = 5,
            JUN = 6,
            JUL = 7,
            AUG = 8,
            SEP = 9,
            OCT = 10,
            NOV = 11,
            DEC = 12
        }

        public enum BillStatus
        {
            BillAllocated = 1,
            BillClosed = 2,
            MemoCreated = 3
        }

        public enum ChargeAssigneType
        {
            BusinessOnly = 1,
            Both = 2
        }

        public enum EmployeeBillStatus
        {
            [Description("Waiting For Identification")]
            WaitingForIdentification = 1,
            [Description("Waiting For Line Manager Approval")]
            WaitingForLineManagerApproval = 2,
            [Description("Line Manager Approved")]
            LineManagerApproved = 3,
            [Description("Bill Reject")]
            BillReject = 4,
            [Description("Close Bill")]
            CloseBill = 5,
            [Description("Auto Close Bill")]
            AutoCloseBill = 6
        }

        public enum CallType
        {
            UnIdentified = 0,
            Business = 1,
            Personal = 2
        }

        public enum RoleType
        {
            SuperAdmin = 1,
            Employee = 2,
            ITVM = 3,
            CallOperator = 4,
            Finance = 5,
            Accountant = 6,
            ITSD = 7
        }

        public enum MemoStatusType
        {
            [Description("Pending Apprvoal")]
            Pending = 1,
            [Description("Approved")]
            Approved = 2,
            [Description("Rejected")]
            Rejected = 3,
        }

        public enum DeviceType
        {
            Cisco = 1,
            Avaya = 2
        }

        public enum ValidYear
        {
            MinYear = 2000
        }

        public enum ActionTemplateTypes
        {
            Login = 1,
            LogOut = 2,
            ForgotPassword = 3,
            ResetPassword = 4,
            Add = 5,
            Edit = 6,
            Active = 7,
            Deactive = 8,
            Delete = 9,
            Upload = 10,
            BillAllcation = 11,
            BilIdentiSaveChanges = 12,
            BillProcess = 13,
            Approve = 14,
            Reject = 15,
            ReIdentification = 16,
            ReimbursementRequest = 17,
            ChangeBillStatus = 18,
            Print = 19,
            ReminderNotificaiton = 20,
            SetTransactionType = 21,
            CompareBill = 22,
            MergeBill = 23,
            Assign = 24
        }

        public enum AuditLogActionType
        {
            Login = 1,
            LogOut = 2,
            ForgotPassword = 3,
            ResetPassword = 4,
            AddRole = 5,
            EditRole = 6,
            ActiveRole = 7,
            DeactiveRole = 8,
            ChangeRoleRights = 9,
            AddExcelMapping = 10,
            EditExcelMapping = 11,
            DeleteExcelMapping = 12,
            AddPbxExcelMapping = 13,
            EditPbxExcelMapping = 14,
            DeletePbxExcelMapping = 15,
            AddHandset = 16,
            DeleteHandset = 17,
            AddInternetDevice = 18,
            DeleteInternetDevice = 19,
            AddEmployee = 20,
            EditEmployee = 21,
            ActiveEmployee = 22,
            DeactiveEmployee = 23,
            DeleteEmployee = 24,
            AddProvider = 25,
            EditProvider = 26,
            ActiveProvider = 27,
            DeactiveProvider = 28,
            DeleteProvider = 29,
            AddPackage = 30,
            ActivePackage = 31,
            DeactivePackage = 32,
            DeletePackage = 33,
            AddTelephone = 34,
            EditTelephone = 35,
            ActiveTelephone = 36,
            DeactiveTelephone = 37,
            DeleteTelephone = 38,
            AssignTelephone = 39,
            AssignBulkTelephone = 40,
            EditAssignTelephone = 41,
            DeleteAssignTelephone = 42,
            AddOperatorCallLog = 43,
            EditOperatorCallLog = 44,
            BulkUploadOperatorCallLog = 45,
            DeleteOperatorCallLog = 46,
            UploadNewBill = 47,
            MergeBill = 48,
            ApproveUploadedBIll = 49,
            DeleteBill = 50,
            UploadNewPBXBill = 51,
            DeletePBXBill = 52,
            ApproveUploadedPBXBill = 53,
            ComaprePBXBill = 54,
            BillAllocation = 55,
            BillIdentificationSaveChanges = 56,
            BillProceess = 57,
            LineManagerApprove = 58,
            LineManagerReject = 59,
            ReIdentificaiton = 60,
            ReimbursementRequest = 61,
            ReimbursementBillApprove = 62,
            ReimbursementBillReject = 63,
            AddDelegate = 64,
            EditDelegate = 65,
            DeleteDelegate = 66,
            ChangeBillStatus = 67,
            AddEmailTemplate = 68,
            EditEmailTemplate = 69,
            AddMemo = 70,
            PrintMemo = 71,
            DeleteMemo = 72,
            MemoApprove = 73,
            MemoReject = 74,
            UpdateReminderNotificaiton = 75,
            AddProviderWiseTransactionType = 76,
            BulkUploadTransactionType = 77,
            ActiveProviderWiseTransactionType = 78,
            DeactiveProviderWiseTransactinType = 79,
            SetTypeOfTransactionTypeSetting = 80,
            DeleteRole = 81
        }

    }
}
