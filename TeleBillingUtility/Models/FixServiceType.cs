using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixServiceType
    {
        public FixServiceType()
        {
            BillDetails = new HashSet<BillDetails>();
            BillMasterServiceType = new HashSet<BillMasterServiceType>();
            EmployeeBillServicePackage = new HashSet<EmployeeBillServicePackage>();
            ExcelDetail = new HashSet<ExcelDetail>();
            ExcelUploadLogServiceType = new HashSet<ExcelUploadLogServiceType>();
            MappingExcel = new HashSet<MappingExcel>();
            MappingServiceTypeField = new HashSet<MappingServiceTypeField>();
            ProviderPackage = new HashSet<ProviderPackage>();
            ProviderService = new HashSet<ProviderService>();
            SkypeExcelDetail = new HashSet<SkypeExcelDetail>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsBusinessOnly { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public virtual ICollection<BillDetails> BillDetails { get; set; }
        public virtual ICollection<BillMasterServiceType> BillMasterServiceType { get; set; }
        public virtual ICollection<EmployeeBillServicePackage> EmployeeBillServicePackage { get; set; }
        public virtual ICollection<ExcelDetail> ExcelDetail { get; set; }
        public virtual ICollection<ExcelUploadLogServiceType> ExcelUploadLogServiceType { get; set; }
        public virtual ICollection<MappingExcel> MappingExcel { get; set; }
        public virtual ICollection<MappingServiceTypeField> MappingServiceTypeField { get; set; }
        public virtual ICollection<ProviderPackage> ProviderPackage { get; set; }
        public virtual ICollection<ProviderService> ProviderService { get; set; }
        public virtual ICollection<SkypeExcelDetail> SkypeExcelDetail { get; set; }
    }
}
