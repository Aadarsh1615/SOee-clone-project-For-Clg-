using System;
using System.Collections.Generic;

namespace SOEEApp.Models.ViewModels
{
    public class ReportsDashboardViewModel
    {
        public IList<SOEESummaryRowViewModel> SummaryRows { get; set; } = new List<SOEESummaryRowViewModel>();
        public IList<QuarterlySOEERowViewModel> QuarterlyRows { get; set; } = new List<QuarterlySOEERowViewModel>();
        public IList<PaymentReconciliationRowViewModel> ReconciliationRows { get; set; } = new List<PaymentReconciliationRowViewModel>();
        public IList<PendingClosureRowViewModel> PendingClosureRows { get; set; } = new List<PendingClosureRowViewModel>();
        public IList<AuditLogRowViewModel> AuditRows { get; set; } = new List<AuditLogRowViewModel>();
    }

    public class SOEESummaryRowViewModel
    {
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public int SOEECount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class QuarterlySOEERowViewModel
    {
        public string QuarterLabel { get; set; }
        public int SOEECount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal PendingAmount { get; set; }
    }

    public class PaymentReconciliationRowViewModel
    {
        public int SOEEID { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalReceived { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherStatus { get; set; }
        public string SOEEStatus { get; set; }
    }

    public class PendingClosureRowViewModel
    {
        public int SOEEID { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalReceived { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AuditLogRowViewModel
    {
        public int SOEEID { get; set; }
        public string ProjectName { get; set; }
        public string ActionBy { get; set; }
        public string RoleHint { get; set; }
        public string Transition { get; set; }
        public string Remarks { get; set; }
        public DateTime ActionOn { get; set; }
    }
}
