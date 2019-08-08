using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MappingExcelColumnPbx
    {
        public long Id { get; set; }
        public long MappingExcelId { get; set; }
        public long MappingServiceTypeFieldId { get; set; }
        public string ExcelcolumnName { get; set; }
        public string FormatField { get; set; }

        public virtual MappingExcelPbx MappingExcel { get; set; }
        public virtual MappingServiceTypeFieldPbx MappingServiceTypeField { get; set; }
    }
}
