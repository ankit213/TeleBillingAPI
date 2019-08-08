using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class OperatorCallLog
    {
        public long Id { get; set; }
        public DateTime CallDate { get; set; }
        public int? CallDateInt { get; set; }
        public long EmployeeId { get; set; }
        public string EmpPfnumber { get; set; }
        public string ExtensionNumber { get; set; }
        public string DialedNumber { get; set; }
        public long ProviderId { get; set; }
        public long CallTypeId { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public int? CreatedDateInt { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual FixCallType CallType { get; set; }
        public virtual MstEmployee Employee { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
