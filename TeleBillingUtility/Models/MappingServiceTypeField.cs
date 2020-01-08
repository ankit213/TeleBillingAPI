using System.Collections.Generic;

namespace TeleBillingUtility.Models
{
    public partial class Mappingservicetypefield
    {
        public Mappingservicetypefield()
        {
            Mappingexcelcolumn = new HashSet<Mappingexcelcolumn>();
        }

        public long Id { get; set; }
        public long ServiceTypeId { get; set; }
        public string DbtableName { get; set; }
        public string DbcolumnName { get; set; }
        public string DisplayFieldName { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSpecial { get; set; }

        public virtual FixServicetype ServiceType { get; set; }
        public virtual ICollection<Mappingexcelcolumn> Mappingexcelcolumn { get; set; }
    }
}
