using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public enum SOEEStatus
    {
        Draft = 0,          // OIC created / correction mode
        Submitted = 1,      // OIC submitted to FM
        NeedCorrection = 2, // FM sent back to OIC
        Approved = 3,       // FM approved
        Dispatched = 4,     // FD approved / dispatched
        PartiallyPaid = 5,
        FullyPaid = 6,
        Rejected = 7,
        ErrorInPayment = 8,
        Reconciled = 9,
        Closed = 10
    }


    public class SOEE
    {
        [Key]
        public int SOEEID { get; set; }

        [Required]
        public int ProjectID { get; set; }
        public virtual Project Project { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime SOEERaiseDate { get; set; } = DateTime.Now;
        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }
        public string MarkTo { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Reference { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalServiceCharge { get; set; }
        public decimal TotalTaxAmount { get; internal set; }
        public decimal GrandTotal { get; set; }

        public decimal PreviousSOEEBalance { get; set; }
        public int? PrevSOEEID { get; set; }

        public SOEEStatus Status { get; set; } = SOEEStatus.Draft;
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ClosedDate { get; set; }
        public bool IsESigned { get; set; }
        public string ESignedBy { get; set; }
        public DateTime? ESignedOn { get; set; }
        public string DispatchEmail { get; set; }
        public string DispatchedBy { get; set; }
        public DateTime? DispatchedOn { get; set; }
        public string ClosureRemarks { get; set; }

        public virtual ICollection<SOEEItem> Items { get; set; }
        public virtual ICollection<SOEEActivity> Activities { get; set; } = new List<SOEEActivity>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ReceiptVoucher ReceiptVoucher { get; set; }
        public string CustomerName { get; internal set; }
    }
}
