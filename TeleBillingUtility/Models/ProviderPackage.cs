using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class ProviderPackage
    {
        public ProviderPackage()
        {
            EmployeeBillServicePackage = new HashSet<EmployeeBillServicePackage>();
            TelePhoneNumberAllocationPackage = new HashSet<TelePhoneNumberAllocationPackage>();
        }

        public long Id { get; set; }
        public long ProviderId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int? StartDateInt { get; set; }
        public string Description { get; set; }
        public long ServiceTypeId { get; set; }
        public int PackageMonth { get; set; }
        public decimal? PackageMinute { get; set; }
        public int? PackageData { get; set; }
        public decimal PackageAmount { get; set; }
        public decimal? LocalMinute { get; set; }
        public decimal? RoamingMinute { get; set; }
        public decimal? InternationalCallMinute { get; set; }
        public decimal? InGroupMinute { get; set; }
        public int? LocalInternetData { get; set; }
        public int? InternationalSharingData { get; set; }
        public int? InternationalRoamingData { get; set; }
        public int? AdditionalMonth { get; set; }
        public decimal? AdditionalMinute { get; set; }
        public int? AdditionalData { get; set; }
        public decimal? AdditionalChargeDurationAmount { get; set; }
        public decimal? AdditionalChargeMinuteAmount { get; set; }
        public decimal? AdditionalChargeDataAmount { get; set; }
        public decimal? TerminationFees { get; set; }
        public string HandsetDetailIds { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
        public decimal? DeviceAmount { get; set; }
        public long? InternetDeviceId { get; set; }
        public decimal? DevicePenaltyAmount { get; set; }

        public virtual MstInternetDeviceDetail InternetDevice { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual FixServiceType ServiceType { get; set; }
        public virtual ICollection<EmployeeBillServicePackage> EmployeeBillServicePackage { get; set; }
        public virtual ICollection<TelePhoneNumberAllocationPackage> TelePhoneNumberAllocationPackage { get; set; }
    }
}
