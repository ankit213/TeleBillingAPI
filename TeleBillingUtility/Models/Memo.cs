﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeleBillingUtility.Models
{
    public partial class Memo
    {
        public Memo()
        {
            Memobills = new HashSet<Memobills>();
        }

        public long Id { get; set; }
        public string RefrenceNo { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long ProviderId { get; set; }
        public bool IsBankTransaction { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsDelete { get; set; }
        public bool? IsApproved { get; set; }
        public string Comment { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? ApprovedDateInt { get; set; }
        public string Bank { get; set; }
        public string Ibancode { get; set; }
        public string Swiftcode { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? CreatedDateInt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long? UpdatedDateInt { get; set; }
        public long? TransactionId { get; set; }

        public virtual Provider Provider { get; set; }
        public virtual ICollection<Memobills> Memobills { get; set; }
    }
}
