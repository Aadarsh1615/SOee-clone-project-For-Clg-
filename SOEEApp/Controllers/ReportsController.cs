using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity;
using SOEEApp.Models;
using SOEEApp.Models.ViewModels;

namespace SOEEApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(BuildDashboard());
        }

        public FileResult Export(string type)
        {
            var dashboard = BuildDashboard();
            var csv = new StringBuilder();

            switch ((type ?? string.Empty).ToLowerInvariant())
            {
                case "quarterly":
                    csv.AppendLine("Quarter,SOEE Count,Total Amount,Total Received,Pending Amount");
                    foreach (var row in dashboard.QuarterlyRows)
                    {
                        csv.AppendLine(string.Format("{0},{1},{2},{3},{4}",
                            Escape(row.QuarterLabel),
                            row.SOEECount,
                            row.TotalAmount,
                            row.TotalReceived,
                            row.PendingAmount));
                    }
                    return CsvResult(csv.ToString(), "quarterly-soee-report.csv");

                case "reconciliation":
                    csv.AppendLine("SOEE ID,Project,Client,Grand Total,Total Received,Voucher Number,Voucher Status,SOEE Status");
                    foreach (var row in dashboard.ReconciliationRows)
                    {
                        csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                            row.SOEEID,
                            Escape(row.ProjectName),
                            Escape(row.CustomerName),
                            row.GrandTotal,
                            row.TotalReceived,
                            Escape(row.VoucherNumber),
                            Escape(row.VoucherStatus),
                            Escape(row.SOEEStatus)));
                    }
                    return CsvResult(csv.ToString(), "payment-reconciliation-report.csv");

                case "pending":
                    csv.AppendLine("SOEE ID,Project,Client,Grand Total,Total Received,Status,Created Date");
                    foreach (var row in dashboard.PendingClosureRows)
                    {
                        csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6:yyyy-MM-dd}",
                            row.SOEEID,
                            Escape(row.ProjectName),
                            Escape(row.CustomerName),
                            row.GrandTotal,
                            row.TotalReceived,
                            Escape(row.Status),
                            row.CreatedDate));
                    }
                    return CsvResult(csv.ToString(), "pending-closure-report.csv");

                case "audit":
                    csv.AppendLine("SOEE ID,Project,Action By,Role Hint,Transition,Remarks,Action On");
                    foreach (var row in dashboard.AuditRows)
                    {
                        csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6:yyyy-MM-dd HH:mm}",
                            row.SOEEID,
                            Escape(row.ProjectName),
                            Escape(row.ActionBy),
                            Escape(row.RoleHint),
                            Escape(row.Transition),
                            Escape(row.Remarks),
                            row.ActionOn));
                    }
                    return CsvResult(csv.ToString(), "audit-log-report.csv");

                default:
                    csv.AppendLine("Project,Client,SOEE Count,Total Amount,Total Received,Outstanding");
                    foreach (var row in dashboard.SummaryRows)
                    {
                        csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                            Escape(row.ProjectName),
                            Escape(row.ClientName),
                            row.SOEECount,
                            row.TotalAmount,
                            row.TotalReceived,
                            row.OutstandingAmount));
                    }
                    return CsvResult(csv.ToString(), "soee-summary-report.csv");
            }
        }

        private ReportsDashboardViewModel BuildDashboard()
        {
            var soeEs = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.Activities)
                .Include(s => s.ReceiptVoucher)
                .Where(s => s.Status != SOEEStatus.Draft)
                .ToList();

            var dashboard = new ReportsDashboardViewModel();

            dashboard.SummaryRows = soeEs
                .GroupBy(s => new
                {
                    ProjectName = s.Project != null ? s.Project.Name : string.Empty,
                    ClientName = s.Customer != null ? s.Customer.CustomerName : string.Empty
                })
                .Select(g => new SOEESummaryRowViewModel
                {
                    ProjectName = g.Key.ProjectName,
                    ClientName = g.Key.ClientName,
                    SOEECount = g.Count(),
                    TotalAmount = g.Sum(x => x.GrandTotal),
                    TotalReceived = g.Sum(x => x.Payments.Sum(p => p.AmountReceived)),
                    OutstandingAmount = g.Sum(x => x.GrandTotal) - g.Sum(x => x.Payments.Sum(p => p.AmountReceived))
                })
                .OrderBy(x => x.ProjectName)
                .ToList();

            dashboard.QuarterlyRows = soeEs
                .GroupBy(s => new
                {
                    Year = s.CreatedDate.Year,
                    Quarter = ((s.CreatedDate.Month - 1) / 3) + 1
                })
                .Select(g => new QuarterlySOEERowViewModel
                {
                    QuarterLabel = "Q" + g.Key.Quarter + " " + g.Key.Year,
                    SOEECount = g.Count(),
                    TotalAmount = g.Sum(x => x.GrandTotal),
                    TotalReceived = g.Sum(x => x.Payments.Sum(p => p.AmountReceived)),
                    PendingAmount = g.Sum(x => x.GrandTotal) - g.Sum(x => x.Payments.Sum(p => p.AmountReceived))
                })
                .OrderBy(x => x.QuarterLabel)
                .ToList();

            dashboard.ReconciliationRows = soeEs
                .Where(s => s.Status == SOEEStatus.PartiallyPaid ||
                            s.Status == SOEEStatus.FullyPaid ||
                            s.Status == SOEEStatus.Reconciled ||
                            s.Status == SOEEStatus.Closed)
                .Select(s => new PaymentReconciliationRowViewModel
                {
                    SOEEID = s.SOEEID,
                    ProjectName = s.Project != null ? s.Project.Name : string.Empty,
                    CustomerName = s.Customer != null ? s.Customer.CustomerName : string.Empty,
                    GrandTotal = s.GrandTotal,
                    TotalReceived = s.Payments.Sum(p => p.AmountReceived),
                    VoucherNumber = s.ReceiptVoucher != null ? s.ReceiptVoucher.VoucherNumber : string.Empty,
                    VoucherStatus = s.ReceiptVoucher != null ? s.ReceiptVoucher.Status.ToString() : "Not Created",
                    SOEEStatus = s.Status.ToString()
                })
                .OrderByDescending(x => x.SOEEID)
                .ToList();

            dashboard.PendingClosureRows = soeEs
                .Where(s => s.Status == SOEEStatus.Reconciled ||
                            s.Status == SOEEStatus.PartiallyPaid ||
                            s.Status == SOEEStatus.FullyPaid)
                .Select(s => new PendingClosureRowViewModel
                {
                    SOEEID = s.SOEEID,
                    ProjectName = s.Project != null ? s.Project.Name : string.Empty,
                    CustomerName = s.Customer != null ? s.Customer.CustomerName : string.Empty,
                    GrandTotal = s.GrandTotal,
                    TotalReceived = s.Payments.Sum(p => p.AmountReceived),
                    Status = s.Status.ToString(),
                    CreatedDate = s.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            dashboard.AuditRows = soeEs
                .SelectMany(s => s.Activities.Select(a => new AuditLogRowViewModel
                {
                    SOEEID = s.SOEEID,
                    ProjectName = s.Project != null ? s.Project.Name : string.Empty,
                    ActionBy = a.ActionBy,
                    RoleHint = GuessRole(a.NewStatus),
                    Transition = a.OldStatus + " -> " + a.NewStatus,
                    Remarks = a.Remarks,
                    ActionOn = a.ActionOn
                }))
                .OrderByDescending(x => x.ActionOn)
                .ToList();

            return dashboard;
        }

        private FileResult CsvResult(string content, string fileName)
        {
            return File(Encoding.UTF8.GetBytes(content), "text/csv", fileName);
        }

        private string Escape(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private string GuessRole(SOEEStatus status)
        {
            switch (status)
            {
                case SOEEStatus.Submitted:
                case SOEEStatus.NeedCorrection:
                    return "OIC / Finance Manager";
                case SOEEStatus.Approved:
                    return "Finance Manager";
                case SOEEStatus.Dispatched:
                    return "Finance Director";
                case SOEEStatus.PartiallyPaid:
                case SOEEStatus.FullyPaid:
                    return "OIC / Client";
                case SOEEStatus.Reconciled:
                    return "Cashier";
                case SOEEStatus.Closed:
                    return "OIC";
                default:
                    return "System";
            }
        }
    }
}
