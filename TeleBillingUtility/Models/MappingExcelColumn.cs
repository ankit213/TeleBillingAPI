using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MappingExcelColumn
    {
        public long Id { get; set; }
        public long MappingExcelId { get; set; }
        public long MappingServiceTypeFieldId { get; set; }
        public string ExcelcolumnName { get; set; }
        public string FormatField { get; set; }

        public virtual MappingExcel MappingExcel { get; set; }
        public virtual MappingServiceTypeField MappingServiceTypeField { get; set; }
    }
}
