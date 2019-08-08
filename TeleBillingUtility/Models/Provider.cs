using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class Provider
    {
        public Provider()
        {
            BillMaster = new HashSet<BillMaster>();
            EmployeeBillMaster = new HashSet<EmployeeBillMaster>();
            ExcelUploadLog = new HashSet<ExcelUploadLog>();
            MappingExcel = new HashSet<MappingExcel>();
            Memo = new HashSet<Memo>();
            OperatorCallLog = new HashSet<OperatorCallLog>();
            ProviderContactDetail = new HashSet<ProviderContactDetail>();
            ProviderPackage = new HashSet<ProviderPackage>();
            ProviderService = new HashSet<ProviderService>();
            TelephoneNumber = new HashSet<TelephoneNumber>();
            TransactionTypeSetting = new HashSet<TransactionTypeSetting>();
        }

        public long Id { get; set; }
        public string ContractNumber { get; set; }
        public string Name { get; set; }
        public long CountryId { get; set; }
        public long CurrencyId { get; set; }
        public string AccountNumber { get; set; }
        public string Bank { get; set; }
        public string Ibancode { get; set; }
        public string Swiftcode { get; set; }
        public string ProviderEmail { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual MstCountry Country { get; set; }
        public virtual ICollection<BillMaster> BillMaster { get; set; }
        public virtual ICollection<EmployeeBillMaster> EmployeeBillMaster { get; set; }
        public virtual ICollection<ExcelUploadLog> ExcelUploadLog { get; set; }
        public virtual ICollection<MappingExcel> MappingExcel { get; set; }
        public virtual ICollection<Memo> Memo { get; set; }
        public virtual ICollection<OperatorCallLog> OperatorCallLog { get; set; }
        public virtual ICollection<ProviderContactDetail> ProviderContactDetail { get; set; }
        public virtual ICollection<ProviderPackage> ProviderPackage { get; set; }
        public virtual ICollection<ProviderService> ProviderService { get; set; }
        public virtual ICollection<TelephoneNumber> TelephoneNumber { get; set; }
        public virtual ICollection<TransactionTypeSetting> TransactionTypeSetting { get; set; }
    }
}
