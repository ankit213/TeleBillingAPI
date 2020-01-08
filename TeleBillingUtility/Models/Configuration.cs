using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
    }
}
