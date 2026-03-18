using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SOEEApp.Models;
using SOEEApp.Models.ViewModels;

namespace SOEEApp.Controllers
{
    [Authorize(Roles = "OIC,Cashier")]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var rows = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .Where(s => s.Status == SOEEStatus.Dispatched ||
                            s.Status == SOEEStatus.PartiallyPaid ||
                            s.Status == SOEEStatus.FullyPaid ||
                            s.Status == SOEEStatus.ErrorInPayment ||
                            s.Status == SOEEStatus.Reconciled)
                .OrderByDescending(s => s.CreatedDate)
                .ToList()
                .Select(s => new PaymentQueueRowViewModel
                {
                    SOEEID = s.SOEEID,
                    ProjectName = s.Project != null ? s.Project.Name : string.Empty,
                    CustomerName = s.Customer != null ? s.Customer.CustomerName : string.Empty,
                    GrandTotal = s.GrandTotal,
                    TotalReceived = s.Payments.Sum(p => p.AmountReceived),
                    OutstandingAmount = s.GrandTotal - s.Payments.Sum(p => p.AmountReceived),
                    Status = s.Status.ToString(),
                    VoucherNumber = s.ReceiptVoucher != null ? s.ReceiptVoucher.VoucherNumber : string.Empty,
                    VoucherStatus = s.ReceiptVoucher != null ? s.ReceiptVoucher.Status.ToString() : "Not Created",
                    CreatedDate = s.CreatedDate
                })
                .ToList();

            return View(rows);
        }

        [Authorize(Roles = "OIC,Cashier")]
        public ActionResult Record(int soeeId)
        {
            var soee = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == soeeId);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status == SOEEStatus.Closed)
            {
                return new HttpStatusCodeResult(403);
            }

            if (!CanRecordPayment(soee.Status))
            {
                return new HttpStatusCodeResult(403);
            }

            return View(BuildPaymentEntryViewModel(soee));
        }

        [HttpPost]
        [Authorize(Roles = "OIC,Cashier")]
        [ValidateAntiForgeryToken]
        public ActionResult Record(PaymentEntryViewModel model)
        {
            var soee = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == model.SOEEID);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status == SOEEStatus.Closed)
            {
                return new HttpStatusCodeResult(403);
            }

            if (!CanRecordPayment(soee.Status))
            {
                return new HttpStatusCodeResult(403);
            }

            if (!ModelState.IsValid)
            {
                return View(BuildPaymentEntryViewModel(soee, model));
            }

            var payment = new Payment
            {
                SOEEID = soee.SOEEID,
                UTRNo = model.UTRNo,
                AmountReceived = model.AmountReceived,
                PaymentDate = model.PaymentDate,
                Mode = model.Mode,
                Remarks = model.Remarks,
                ConfirmedBy = User.Identity.Name,
                ConfirmedOn = DateTime.Now
            };

            db.Payments.Add(payment);

            var previousStatus = soee.Status;
            var totalReceived = soee.Payments.Sum(p => p.AmountReceived) + model.AmountReceived;
            var nextStatus = totalReceived >= soee.GrandTotal
                ? SOEEStatus.FullyPaid
                : SOEEStatus.PartiallyPaid;

            soee.Status = nextStatus;
            EnsureVoucher(soee, totalReceived, model.PaymentDate, model.Remarks);

            db.SOEEActivities.Add(new SOEEActivity
            {
                SOEEID = soee.SOEEID,
                OldStatus = previousStatus,
                NewStatus = nextStatus,
                ActionBy = User.Identity.Name,
                ActionOn = DateTime.Now,
                Remarks = "Payment recorded by OIC. UTR: " + model.UTRNo
            });

            db.SaveChanges();
            return RedirectToAction("Details", "SOEE", new { id = soee.SOEEID });
        }

        [Authorize(Roles = "Cashier,OIC")]
        public ActionResult Reconcile(int soeeId)
        {
            var soee = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == soeeId);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (!CanReconcile(soee.Status))
            {
                return new HttpStatusCodeResult(403);
            }

            return View(BuildPaymentReconciliationViewModel(soee));
        }

        [HttpPost]
        [Authorize(Roles = "Cashier,OIC")]
        [ValidateAntiForgeryToken]
        public ActionResult Reconcile(PaymentReconciliationViewModel model)
        {
            var soee = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == model.SOEEID);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (!CanReconcile(soee.Status))
            {
                return new HttpStatusCodeResult(403);
            }

            if (!ModelState.IsValid)
            {
                return View(BuildPaymentReconciliationViewModel(soee, model));
            }

            if (!soee.Payments.Any())
            {
                ModelState.AddModelError("", "At least one payment must be recorded before reconciliation.");
                return View(BuildPaymentReconciliationViewModel(soee, model));
            }

            foreach (var payment in soee.Payments.Where(p => !p.IsReconciled))
            {
                payment.IsReconciled = true;
                payment.ReconciledOn = DateTime.Now;
            }

            var totalReceived = soee.Payments.Sum(p => p.AmountReceived);
            var previousStatus = soee.Status;

            if (soee.ReceiptVoucher == null)
            {
                soee.ReceiptVoucher = new ReceiptVoucher
                {
                    SOEEID = soee.SOEEID,
                    GeneratedOn = DateTime.Now
                };
            }

            soee.ReceiptVoucher.VoucherNumber = model.VoucherNumber;
            soee.ReceiptVoucher.VoucherDate = model.VoucherDate;
            soee.ReceiptVoucher.Amount = totalReceived;
            soee.ReceiptVoucher.Status = ReceiptVoucherStatus.Reconciled;
            soee.ReceiptVoucher.GeneratedBy = User.Identity.Name;
            soee.ReceiptVoucher.GeneratedOn = DateTime.Now;
            soee.ReceiptVoucher.Remarks = model.Remarks;

            soee.Status = SOEEStatus.Reconciled;

            db.SOEEActivities.Add(new SOEEActivity
            {
                SOEEID = soee.SOEEID,
                OldStatus = previousStatus,
                NewStatus = SOEEStatus.Reconciled,
                ActionBy = User.Identity.Name,
                ActionOn = DateTime.Now,
                Remarks = "Receipt voucher generated and payment reconciled."
            });

            db.SaveChanges();
            return RedirectToAction("Details", "SOEE", new { id = soee.SOEEID });
        }

        public ActionResult Voucher(int soeeId)
        {
            var voucher = db.ReceiptVouchers
                .Include(v => v.SOEE.Project)
                .Include(v => v.SOEE.Customer)
                .FirstOrDefault(v => v.SOEEID == soeeId);

            if (voucher == null)
            {
                return HttpNotFound();
            }

            return View(voucher);
        }

        private PaymentEntryViewModel BuildPaymentEntryViewModel(SOEE soee, PaymentEntryViewModel posted = null)
        {
            var totalReceived = soee.Payments.Sum(p => p.AmountReceived);

            return new PaymentEntryViewModel
            {
                SOEEID = soee.SOEEID,
                ProjectName = soee.Project != null ? soee.Project.Name : string.Empty,
                CustomerName = soee.Customer != null ? soee.Customer.CustomerName : string.Empty,
                SOEEGrandTotal = soee.GrandTotal,
                TotalReceived = totalReceived,
                OutstandingAmount = soee.GrandTotal - totalReceived,
                CurrentStatus = soee.Status.ToString(),
                UTRNo = posted != null ? posted.UTRNo : string.Empty,
                AmountReceived = posted != null ? posted.AmountReceived : 0m,
                PaymentDate = posted != null ? posted.PaymentDate : DateTime.Today,
                Mode = posted != null ? posted.Mode : PaymentMode.BankTransfer,
                Remarks = posted != null ? posted.Remarks : string.Empty,
                History = soee.Payments
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new PaymentSummaryViewModel
                    {
                        PaymentID = p.PaymentID,
                        UTRNo = p.UTRNo,
                        AmountReceived = p.AmountReceived,
                        PaymentDate = p.PaymentDate,
                        Mode = p.Mode,
                        IsReconciled = p.IsReconciled,
                        ReconciledOn = p.ReconciledOn,
                        ConfirmedBy = p.ConfirmedBy,
                        Remarks = p.Remarks
                    }).ToList(),
                ReceiptVoucher = soee.ReceiptVoucher == null
                    ? null
                    : new ReceiptVoucherViewModel
                    {
                        VoucherID = soee.ReceiptVoucher.VoucherID,
                        VoucherNumber = soee.ReceiptVoucher.VoucherNumber,
                        VoucherDate = soee.ReceiptVoucher.VoucherDate,
                        Amount = soee.ReceiptVoucher.Amount,
                        Status = soee.ReceiptVoucher.Status,
                        GeneratedBy = soee.ReceiptVoucher.GeneratedBy,
                        GeneratedOn = soee.ReceiptVoucher.GeneratedOn,
                        Remarks = soee.ReceiptVoucher.Remarks
                    }
            };
        }

        private PaymentReconciliationViewModel BuildPaymentReconciliationViewModel(SOEE soee, PaymentReconciliationViewModel posted = null)
        {
            var totalReceived = soee.Payments.Sum(p => p.AmountReceived);

            return new PaymentReconciliationViewModel
            {
                SOEEID = soee.SOEEID,
                ProjectName = soee.Project != null ? soee.Project.Name : string.Empty,
                CustomerName = soee.Customer != null ? soee.Customer.CustomerName : string.Empty,
                GrandTotal = soee.GrandTotal,
                TotalReceived = totalReceived,
                OutstandingAmount = soee.GrandTotal - totalReceived,
                VoucherNumber = posted != null
                    ? posted.VoucherNumber
                    : soee.ReceiptVoucher != null
                        ? soee.ReceiptVoucher.VoucherNumber
                        : BuildVoucherNumber(soee.SOEEID),
                VoucherDate = posted != null
                    ? posted.VoucherDate
                    : soee.ReceiptVoucher != null && soee.ReceiptVoucher.VoucherDate != DateTime.MinValue
                        ? soee.ReceiptVoucher.VoucherDate
                        : DateTime.Today,
                Remarks = posted != null
                    ? posted.Remarks
                    : soee.ReceiptVoucher != null
                        ? soee.ReceiptVoucher.Remarks
                        : string.Empty,
                Payments = soee.Payments
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new PaymentSummaryViewModel
                    {
                        PaymentID = p.PaymentID,
                        UTRNo = p.UTRNo,
                        AmountReceived = p.AmountReceived,
                        PaymentDate = p.PaymentDate,
                        Mode = p.Mode,
                        IsReconciled = p.IsReconciled,
                        ReconciledOn = p.ReconciledOn,
                        ConfirmedBy = p.ConfirmedBy,
                        Remarks = p.Remarks
                    }).ToList()
            };
        }

        private bool CanReconcile(SOEEStatus status)
        {
            return status == SOEEStatus.Dispatched ||
                   status == SOEEStatus.PartiallyPaid ||
                   status == SOEEStatus.FullyPaid ||
                   status == SOEEStatus.ErrorInPayment;
        }

        private bool CanRecordPayment(SOEEStatus status)
        {
            return status == SOEEStatus.Dispatched ||
                   status == SOEEStatus.PartiallyPaid ||
                   status == SOEEStatus.FullyPaid ||
                   status == SOEEStatus.ErrorInPayment;
        }

        private void EnsureVoucher(SOEE soee, decimal totalReceived, DateTime paymentDate, string remarks)
        {
            if (soee.ReceiptVoucher == null)
            {
                soee.ReceiptVoucher = new ReceiptVoucher
                {
                    SOEEID = soee.SOEEID,
                    VoucherNumber = BuildVoucherNumber(soee.SOEEID),
                    VoucherDate = paymentDate,
                    GeneratedBy = User.Identity.Name,
                    GeneratedOn = DateTime.Now
                };
            }

            soee.ReceiptVoucher.Amount = totalReceived;
            soee.ReceiptVoucher.Status = ReceiptVoucherStatus.Pending;
            soee.ReceiptVoucher.Remarks = remarks;
        }

        private string BuildVoucherNumber(int soeeId)
        {
            return "RV-" + soeeId.ToString("D5");
        }
    }
}
