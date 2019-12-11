using System;
using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class MstCurrency
    {
        public MstCurrency()
        {
            Billmaster = new HashSet<Billmaster>();
            Employeebillmaster = new HashSet<Employeebillmaster>();
            Exceldetail = new HashSet<Exceldetail>();
            MstCountry = new HashSet<MstCountry>();
            Provider = new HashSet<Provider>();
            Skypeexceldetail = new HashSet<Skypeexceldetail>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Billmaster> Billmaster { get; set; }
        public virtual ICollection<Employeebillmaster> Employeebillmaster { get; set; }
        public virtual ICollection<Exceldetail> Exceldetail { get; set; }
        public virtual ICollection<MstCountry> MstCountry { get; set; }
        public virtual ICollection<Provider> Provider { get; set; }
        public virtual ICollection<Skypeexceldetail> Skypeexceldetail { get; set; }
    }
}
