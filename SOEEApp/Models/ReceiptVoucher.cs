using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOEEApp.Models
{
    public enum ReceiptVoucherStatus
    {
        Pending = 0,
        Reconciled = 1,
        Closed = 2
    }

    public class ReceiptVoucher
    {
        [Key]
        public int VoucherID { get; set; }

        [Required]
        [Index("IX_ReceiptVoucher_SOEEID", IsUnique = true)]
        public int SOEEID { get; set; }
        public virtual SOEE SOEE { get; set; }

        [Required]
        [Display(Name = "Voucher No.")]
        public string VoucherNumber { get; set; }

        [Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; }

        public decimal Amount { get; set; }
        public ReceiptVoucherStatus Status { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string Remarks { get; set; }
    }
}
