using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Mappingexcel
    {
        public Mappingexcel()
        {
            Mappingexcelcolumn = new HashSet<Mappingexcelcolumn>();
        }

        public long Id { get; set; }
        public long ProviderId { get; set; }
        public long ServiceTypeId { get; set; }
        public long WorkSheetNo { get; set; }
        public bool HaveHeader { get; set; }
        public bool HaveTitle { get; set; }
        public string TitleName { get; set; }
        public string ExcelColumnNameForTitle { get; set; }
        public string ExcelReadingColumn { get; set; }
        public long CurrencyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }
        // Added ON 2019/10/04
        public bool? IsCommonMapped { get; set; }
        public long? MappedMappingId { get; set; }
        public long? MappedServiceTypeId { get; set; }          

        public virtual Provider Provider { get; set; }
        public virtual FixServicetype ServiceType { get; set; }
        public virtual ICollection<Mappingexcelcolumn> Mappingexcelcolumn { get; set; }
    }
}
