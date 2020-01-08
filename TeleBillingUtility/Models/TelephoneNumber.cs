using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Telephonenumber
    {
        public Telephonenumber()
        {
            Telephonenumberallocation = new HashSet<Telephonenumberallocation>();
        }

        public long Id { get; set; }
        public string TelephoneNumber1 { get; set; }
        public long ProviderId { get; set; }
        public string AccountNumber { get; set; }
        public int LineTypeId { get; set; }
        public string Description { get; set; }
        public bool IsDataRoaming { get; set; }
        public bool IsInternationalCalls { get; set; }
        public bool IsVoiceRoaming { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixLinetype LineType { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual ICollection<Telephonenumberallocation> Telephonenumberallocation { get; set; }
    }
}
