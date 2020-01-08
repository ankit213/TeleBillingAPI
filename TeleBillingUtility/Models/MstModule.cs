using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class MstModule
    {
        public MstModule()
        {
            MstLink = new HashSet<MstLink>();
        }

        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string IconName { get; set; }
        public int? ViewIndex { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }

        public virtual ICollection<MstLink> MstLink { get; set; }
    }
}
