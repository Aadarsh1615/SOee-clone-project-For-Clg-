using System;
using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public enum PaymentMode
    {
        BankTransfer = 0,
        NEFT = 1,
        RTGS = 2,
        UPI = 3,
        Cheque = 4,
        Cash = 5,
        Other = 6
    }

    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int SOEEID { get; set; }
        public virtual SOEE SOEE { get; set; }

        [Required]
        [Display(Name = "UTR / Reference No.")]
        public string UTRNo { get; set; }

        [Required]
        [Display(Name = "Amount Received")]
        public decimal AmountReceived { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentMode Mode { get; set; }

        public string ConfirmedBy { get; set; }
        public DateTime ConfirmedOn { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime? ReconciledOn { get; set; }
        public string Remarks { get; set; }
    }
}
