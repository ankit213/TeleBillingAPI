using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class FixServicetype
    {
        public FixServicetype()
        {
            Billdetails = new HashSet<Billdetails>();
            BillmasterServicetype = new HashSet<BillmasterServicetype>();
            Employeebillservicepackage = new HashSet<Employeebillservicepackage>();
            Exceldetail = new HashSet<Exceldetail>();
            ExceluploadlogServicetype = new HashSet<ExceluploadlogServicetype>();
            Mappingexcel = new HashSet<Mappingexcel>();
            Mappingservicetypefield = new HashSet<Mappingservicetypefield>();
            Providerpackage = new HashSet<Providerpackage>();
            Providerservice = new HashSet<Providerservice>();
            Skypeexceldetail = new HashSet<Skypeexceldetail>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsBusinessOnly { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Billdetails> Billdetails { get; set; }
        public virtual ICollection<BillmasterServicetype> BillmasterServicetype { get; set; }
        public virtual ICollection<Employeebillservicepackage> Employeebillservicepackage { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
        public virtual ICollection<ExceluploadlogServicetype> ExceluploadlogServicetype { get; set; }
        public virtual ICollection<Mappingexcel> Mappingexcel { get; set; }
        public virtual ICollection<Mappingservicetypefield> Mappingservicetypefield { get; set; }
        public virtual ICollection<Providerpackage> Providerpackage { get; set; }
        public virtual ICollection<Providerservice> Providerservice { get; set; }
        public virtual ICollection<Skypeexceldetail> Skypeexceldetail { get; set; }
    }
}
