using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models.ViewModels
{
    // ---------------------------------------------------
    // ITEM VIEW MODEL (aligned with SOEEItem.cs)
    // ---------------------------------------------------
    public class SOEEItemViewModel
    {
        public int SOEEItemID { get; set; }     // corresponds to SOEEItemID
        public int ServiceTypeID { get; set; }
        public string ServiceTypeName { get; set; }

        [Required]
        public string DescriptionOfWork { get; set; }

        public decimal Quantity { get; set; }
        public decimal Unit { get; set; }           // numeric unit
        public decimal UnitPrice { get; set; }      // rate

        // new computed fields
        public decimal SubTotal { get; set; }       // Quantity * Unit * UnitPrice
        public decimal ServiceChargePercent { get; set; }   // already exists
        public decimal ServiceCharge { get; set; }
        public decimal TotalAfterServiceCharge { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Total { get; set; }          // SubTotal + charges + taxes
        public bool IsDeleted { get; set; }
    }

    // ---------------------------------------------------
    // MAIN SOEE CREATE VIEWMODEL
    // ---------------------------------------------------
    public class SOEECreateViewModel
    {
        public int? SOEEID { get; set; }

        // ----------------- Header Fields ----------------
        [Required]
        public int ProjectID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        public string ReferenceNo { get; set; }
        public DateTime SOEERaiseDate { get; set; } = DateTime.Now;

        public string MarkTo { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Reference { get; set; }

        // ------------- Totals at SOEE Level -------------
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalServiceCharge { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public SOEEStatus Status { get; set; }
        public int? PrevSOEEID { get; set; }
        public decimal PreviousSOEEBalance { get; set; }

        // ---------------- Dropdown Data -----------------
        public List<Project> Projects { get; set; }
        public List<Customer> Customers { get; set; }
        public List<ServiceType> ServiceTypes { get; set; }
        
        // Navigation properties for Details view
        public Project Project { get; set; }
        public Customer Customer { get; set; }
        public bool IsESigned { get; set; }
        public string ESignedBy { get; set; }
        public DateTime? ESignedOn { get; set; }
        public string DispatchEmail { get; set; }
        public string DispatchedBy { get; set; }
        public DateTime? DispatchedOn { get; set; }
        public string ClosureRemarks { get; set; }

        // ---------------- Items (Dynamic Table) ---------
        public List<SOEEItemViewModel> Items { get; set; } = new List<SOEEItemViewModel>();
        public List<SOEEActivityViewModel> Activities { get; set; }
        public List<PaymentSummaryViewModel> Payments { get; set; } = new List<PaymentSummaryViewModel>();
        public ReceiptVoucherViewModel ReceiptVoucher { get; set; }
    }
    public class SOEEActivityViewModel
    {
        public string Action { get; set; }
        public SOEEStatus OldStatus { get; set; }
        public SOEEStatus NewStatus { get; set; }
        public string ActionBy { get; set; }
        public DateTime ActionOn { get; set; }
        public string Remarks { get; set; }
    }

}
