using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Provider
    {
        public Provider()
        {
            Billmaster = new HashSet<Billmaster>();
            Employeebillmaster = new HashSet<Employeebillmaster>();
            Exceluploadlog = new HashSet<Exceluploadlog>();
            Mappingexcel = new HashSet<Mappingexcel>();
            Memo = new HashSet<Memo>();
            Operatorcalllog = new HashSet<Operatorcalllog>();
            Providercontactdetail = new HashSet<Providercontactdetail>();
            Providerpackage = new HashSet<Providerpackage>();
            Providerservice = new HashSet<Providerservice>();
            Telephonenumber = new HashSet<Telephonenumber>();
            Transactiontypesetting = new HashSet<Transactiontypesetting>();
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
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? TransactionId { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }

        public virtual MstCountry Country { get; set; }
        public virtual MstCurrency Currency { get; set; }
        public virtual ICollection<Billmaster> Billmaster { get; set; }
        public virtual ICollection<Employeebillmaster> Employeebillmaster { get; set; }
        public virtual ICollection<Exceluploadlog> Exceluploadlog { get; set; }
        public virtual ICollection<Mappingexcel> Mappingexcel { get; set; }
        public virtual ICollection<Memo> Memo { get; set; }
        public virtual ICollection<Operatorcalllog> Operatorcalllog { get; set; }
        public virtual ICollection<Providercontactdetail> Providercontactdetail { get; set; }
        public virtual ICollection<Providerpackage> Providerpackage { get; set; }
        public virtual ICollection<Providerservice> Providerservice { get; set; }
        public virtual ICollection<Telephonenumber> Telephonenumber { get; set; }
        public virtual ICollection<Transactiontypesetting> Transactiontypesetting { get; set; }
    }
}
