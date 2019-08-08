using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstLink
    {
        public long LinkId { get; set; }
        public long ModuleId { get; set; }
        public string Title { get; set; }
        public string RouteLink { get; set; }
        public long ViewIndex { get; set; }
        public long ParentId { get; set; }
        public bool IsSinglePage { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }

        public virtual MstModule Module { get; set; }
    }
}
