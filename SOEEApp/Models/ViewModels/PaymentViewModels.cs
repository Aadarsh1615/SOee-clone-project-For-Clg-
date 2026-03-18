using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models.ViewModels
{
    public class PaymentEntryViewModel
    {
        public int SOEEID { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public decimal SOEEGrandTotal { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string CurrentStatus { get; set; }

        [Required]
        [Display(Name = "UTR / Reference No.")]
        public string UTRNo { get; set; }

        [Required]
        [Display(Name = "Amount Received")]
        public decimal AmountReceived { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentMode Mode { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        public IList<PaymentSummaryViewModel> History { get; set; } = new List<PaymentSummaryViewModel>();
        public ReceiptVoucherViewModel ReceiptVoucher { get; set; }
    }

    public class PaymentSummaryViewModel
    {
        public int PaymentID { get; set; }
        public string UTRNo { get; set; }
        public decimal AmountReceived { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMode Mode { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime? ReconciledOn { get; set; }
        public string ConfirmedBy { get; set; }
        public string Remarks { get; set; }
    }

    public class ReceiptVoucherViewModel
    {
        public int VoucherID { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public decimal Amount { get; set; }
        public ReceiptVoucherStatus Status { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string Remarks { get; set; }
    }

    public class PaymentReconciliationViewModel
    {
        public int SOEEID { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal OutstandingAmount { get; set; }
        public IList<PaymentSummaryViewModel> Payments { get; set; } = new List<PaymentSummaryViewModel>();

        [Required]
        [Display(Name = "Voucher No.")]
        public string VoucherNumber { get; set; }

        [Required]
        [Display(Name = "Voucher Date")]
        [DataType(DataType.Date)]
        public DateTime VoucherDate { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }

    public class PaymentQueueRowViewModel
    {
        public int SOEEID { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
