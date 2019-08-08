using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class Configuration
    {
        public long Id { get; set; }
        public int? RLinemanagerApprovalInterval { get; set; }
        public bool RLinemanagerApprovalIsActive { get; set; }
        public int? REmployeeCallIdentificationInterval { get; set; }
        public bool REmployeeCallIdentificationIsActive { get; set; }
        public bool NBillAllocationToEmployee { get; set; }
        public bool NBillDelegatesForIdentification { get; set; }
        public bool NNewBillReceiveForApproval { get; set; }
        public bool NDelegatesBillForApproval { get; set; }
        public bool NApprovedByLineManager { get; set; }
        public bool NRejectedByLineManager { get; set; }
        public bool NChargeBill { get; set; }
        public bool NSendMemo { get; set; }
        public bool NMemoApprovalRejection { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public int? CreatedDateInt { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
    }
}
