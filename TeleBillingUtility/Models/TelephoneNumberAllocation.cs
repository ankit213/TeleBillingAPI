using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Telephonenumberallocation
    {
        public Telephonenumberallocation()
        {
            Telephonenumberallocationpackage = new HashSet<Telephonenumberallocationpackage>();
        }

        public long Id { get; set; }
        public string TelephoneNumber { get; set; }
        public long TelephoneNumberId { get; set; }
        public long AssignTypeId { get; set; }
        public long EmployeeId { get; set; }
        public string EmpPfnumber { get; set; }
        public string Reason { get; set; }
        public long? BusinessUnitId { get; set; }
        public int LineStatusId { get; set; }
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

        public virtual FixAssigntype AssignType { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual FixLinestatus LineStatus { get; set; }
        public virtual Telephonenumber TelephoneNumberNavigation { get; set; }
        public virtual ICollection<Telephonenumberallocationpackage> Telephonenumberallocationpackage { get; set; }
    }
}
