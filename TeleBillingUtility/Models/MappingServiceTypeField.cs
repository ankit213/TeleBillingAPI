using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MappingServiceTypeField
    {
        public MappingServiceTypeField()
        {
            MappingExcelColumn = new HashSet<MappingExcelColumn>();
        }

        public long Id { get; set; }
        public long ServiceTypeId { get; set; }
        public string DbtableName { get; set; }
        public string DbcolumnName { get; set; }
        public string DisplayFieldName { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSpecial { get; set; }

        public virtual FixServiceType ServiceType { get; set; }
        public virtual ICollection<MappingExcelColumn> MappingExcelColumn { get; set; }
    }
}
