using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TeleBillingUtility.Helpers.Enums
{
    public class EnumList
    {
        public enum ResponseType
        {
            Error = 0,
            Success = 1,
            NotFound = 2
        }

        public enum EmailTemplateType
        {
            ForgotPassword = 1,
            NewRegistrationConfirmation = 16,
            NewRegistrationInYourTeam = 17
        }

        public enum TransactionTraseLog
        {
            UpdateRecord = 1,
            ChangeStatus = 2,
            RequestApprove = 3,
            RequestReject = 4,
            ForgotPassword = 5,
			BillStatusChange=6
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
			UploadCallLog =2
		}

        public enum ExcelUploadResponseType
        {
            NoStatus=0,
            ExceptionError = -1,
            Success = 1,
            FileNotFound = 2,
            DataInvalid=3,
            SomeDataInvalid=4,
            NoDataFound=5
        }
       
        public enum ServiceType
        {
            Mobility=1,
            VoiceOnly=2,
            InternetService = 3,
            DataCenterFacility = 4,
            ManagedHostingService=5,
            StaticIP=6,
            VOIP=7,
            MOC=8,
            GeneralServiceMada = 9,
            GeneralserviceKems=10
        }

        public enum AssignType
        {
            Business=1,
            Employee=2
        }

        public enum SupportDataType
        {
            String = 1,
            Number = 2,
            Date=3,
            Time=4,
            Boolean=5            
        }

		public enum Month {
			JAN=1,
			FEB=2,
			MAR=3,
			APR=4,
			MAY=5,
			JUN=6,
			JUL=7,
			AUG=8,
			SEP=9,
			OCT=10,
			NOV=11,
			DEC=12
		}

		public enum BillStatus {
			BillAllocated=1,
			BillClosed =2,
			MemoCreated=3
		}

		public enum ChargeAssigneType {
			BusinessOnly = 1,
			Both=2
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
            Employee = 2
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

	}
}
