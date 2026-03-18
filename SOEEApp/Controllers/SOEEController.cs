using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SOEEApp.Helpers;
using SOEEApp.Models;
using SOEEApp.Models.ViewModels;

namespace SOEEApp.Controllers
{
    [Authorize]
    public class SOEEController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "OIC,FinanceManager,FinanceDirector,Cashier,Client")]
        public ActionResult Index(int projectId = 0, string status = null)
        {
            var query = BuildRoleScopedQuery(projectId);
            var parsedStatus = ParseStatus(status);

            if (parsedStatus.HasValue)
            {
                query = query.Where(s => s.Status == parsedStatus.Value);
            }

            ViewBag.Projects = new SelectList(db.Projects.OrderBy(p => p.Name).ToList(), "ProjectID", "Name", projectId);
            ViewBag.SelectedStatus = status;

            return View(query.OrderByDescending(s => s.SOEEID).ToList());
        }

        [Authorize(Roles = "FinanceManager")]
        public ActionResult PendingFM()
        {
            ViewBag.Projects = new SelectList(db.Projects.OrderBy(p => p.Name).ToList(), "ProjectID", "Name");
            ViewBag.SelectedStatus = SOEEStatus.Submitted.ToString();
            return View("Index", BuildRoleScopedQuery(0)
                .Where(s => s.Status == SOEEStatus.Submitted)
                .OrderByDescending(s => s.SOEEID)
                .ToList());
        }

        [Authorize(Roles = "FinanceDirector")]
        public ActionResult PendingFD()
        {
            ViewBag.Projects = new SelectList(db.Projects.OrderBy(p => p.Name).ToList(), "ProjectID", "Name");
            ViewBag.SelectedStatus = SOEEStatus.Approved.ToString();
            return View("Index", BuildRoleScopedQuery(0)
                .Where(s => s.Status == SOEEStatus.Approved)
                .OrderByDescending(s => s.SOEEID)
                .ToList());
        }

        [Authorize(Roles = "Client")]
        public ActionResult MySOEE()
        {
            ViewBag.Projects = new SelectList(db.Projects.OrderBy(p => p.Name).ToList(), "ProjectID", "Name");
            ViewBag.SelectedStatus = string.Empty;
            return View("Index", BuildRoleScopedQuery(0)
                .Where(s => s.Status == SOEEStatus.Dispatched ||
                            s.Status == SOEEStatus.PartiallyPaid ||
                            s.Status == SOEEStatus.FullyPaid ||
                            s.Status == SOEEStatus.ErrorInPayment ||
                            s.Status == SOEEStatus.Reconciled ||
                            s.Status == SOEEStatus.Closed)
                .OrderByDescending(s => s.SOEEID)
                .ToList());
        }

        [Authorize(Roles = "OIC")]
        public ActionResult Create(int? projectId = null)
        {
            var vm = new SOEECreateViewModel();
            if (projectId.HasValue)
            {
                vm.ProjectID = projectId.Value;
                ApplyPreviousSOEEContext(vm, projectId.Value, null);
            }

            PopulateCreateDropDowns(vm.ProjectID, vm.CustomerID);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OIC")]
        public ActionResult Create(SOEECreateViewModel vm)
        {
            vm.Items = SanitizeItems(vm.Items);

            if (!vm.Items.Any())
            {
                ModelState.AddModelError("", "Please add at least one service item.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCreateDropDowns(vm.ProjectID, vm.CustomerID);
                ApplyPreviousSOEEContext(vm, vm.ProjectID, null);
                return View(vm);
            }

            var previousSOEE = GetLatestPreviousSOEE(vm.ProjectID, null);
            var soee = new SOEE
            {
                ProjectID = vm.ProjectID,
                CustomerID = vm.CustomerID,
                ReferenceNo = vm.ReferenceNo,
                SOEERaiseDate = vm.SOEERaiseDate,
                MarkTo = vm.MarkTo,
                Subject = vm.Subject,
                Content = vm.Content,
                Reference = vm.Reference,
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now,
                Status = SOEEStatus.Draft,
                PreviousSOEEBalance = previousSOEE != null ? GetOutstandingAmount(previousSOEE) : 0m,
                PrevSOEEID = previousSOEE != null ? (int?)previousSOEE.SOEEID : null,
                Items = BuildItemEntities(vm.Items)
            };

            db.SOEEs.Add(soee);
            db.SaveChanges();

            RecalculateTotals(soee);
            AddSOEEActivity(soee, SOEEStatus.Draft, SOEEStatus.Draft, "Draft saved by OIC.");

            db.SaveChanges();
            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [Authorize(Roles = "OIC")]
        public ActionResult Edit(int id)
        {
            var soee = db.SOEEs
                .Include(s => s.Items)
                .FirstOrDefault(s => s.SOEEID == id);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (!CanEdit(soee))
            {
                return new HttpStatusCodeResult(403);
            }

            var vm = new SOEECreateViewModel
            {
                SOEEID = soee.SOEEID,
                ProjectID = soee.ProjectID,
                CustomerID = soee.CustomerID,
                ReferenceNo = soee.ReferenceNo,
                SOEERaiseDate = soee.SOEERaiseDate,
                MarkTo = soee.MarkTo,
                Subject = soee.Subject,
                Reference = soee.Reference,
                Content = soee.Content,
                Status = soee.Status,
                PrevSOEEID = soee.PrevSOEEID,
                PreviousSOEEBalance = soee.PreviousSOEEBalance,
                Items = soee.Items
                    .Where(it => !it.IsDeleted)
                    .OrderBy(it => it.SOEEItemID)
                    .Select(it => new SOEEItemViewModel
                    {
                        SOEEItemID = it.SOEEItemID,
                        DescriptionOfWork = it.DescriptionOfWork,
                        ServiceTypeID = it.ServiceTypeID,
                        Unit = it.Unit,
                        Quantity = it.Quantity,
                        UnitPrice = it.UnitPrice,
                        SubTotal = it.SubTotal,
                        ServiceChargePercent = it.ServiceChargePercent,
                        ServiceCharge = it.ServiceCharge,
                        TotalAfterServiceCharge = it.SubTotal + it.ServiceCharge,
                        CGST = it.CGST,
                        SGST = it.SGST,
                        Total = it.Total,
                        IsDeleted = it.IsDeleted
                    })
                    .ToList()
            };

            PopulateCreateDropDowns(vm.ProjectID, vm.CustomerID);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OIC")]
        public ActionResult Edit(SOEECreateViewModel vm)
        {
            var soee = db.SOEEs
                .Include(s => s.Items)
                .FirstOrDefault(s => s.SOEEID == vm.SOEEID);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (!CanEdit(soee))
            {
                return new HttpStatusCodeResult(403);
            }

            vm.Items = vm.Items == null ? new List<SOEEItemViewModel>() : vm.Items;
            if (!vm.Items.Any(i => !i.IsDeleted))
            {
                ModelState.AddModelError("", "Please keep at least one active service item.");
            }

            if (!ModelState.IsValid)
            {
                PopulateCreateDropDowns(vm.ProjectID, vm.CustomerID);
                return View(vm);
            }

            soee.ProjectID = vm.ProjectID;
            soee.CustomerID = vm.CustomerID;
            soee.ReferenceNo = vm.ReferenceNo;
            soee.SOEERaiseDate = vm.SOEERaiseDate;
            soee.MarkTo = vm.MarkTo;
            soee.Subject = vm.Subject;
            soee.Content = vm.Content;
            soee.Reference = vm.Reference;
            soee.Status = SOEEStatus.Draft;

            var previousSOEE = GetLatestPreviousSOEE(vm.ProjectID, soee.SOEEID);
            soee.PreviousSOEEBalance = previousSOEE != null ? GetOutstandingAmount(previousSOEE) : 0m;
            soee.PrevSOEEID = previousSOEE != null ? (int?)previousSOEE.SOEEID : null;

            foreach (var existing in soee.Items)
            {
                existing.IsDeleted = true;
            }

            foreach (var item in vm.Items)
            {
                if (item.SOEEItemID > 0)
                {
                    var existing = soee.Items.FirstOrDefault(x => x.SOEEItemID == item.SOEEItemID);
                    if (existing == null)
                    {
                        continue;
                    }

                    if (item.IsDeleted)
                    {
                        existing.IsDeleted = true;
                        continue;
                    }

                    existing.DescriptionOfWork = item.DescriptionOfWork;
                    existing.ServiceTypeID = item.ServiceTypeID;
                    existing.Unit = item.Unit;
                    existing.Quantity = item.Quantity;
                    existing.UnitPrice = item.UnitPrice;
                    existing.ServiceChargePercent = 0m;
                    existing.IsDeleted = false;
                }
                else if (!item.IsDeleted && item.ServiceTypeID > 0)
                {
                    soee.Items.Add(new SOEEItem
                    {
                        DescriptionOfWork = item.DescriptionOfWork,
                        ServiceTypeID = item.ServiceTypeID,
                        Unit = item.Unit,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ServiceChargePercent = 0m,
                        IsDeleted = false
                    });
                }
            }

            RecalculateTotals(soee);
            AddSOEEActivity(soee, vm.Status, SOEEStatus.Draft, "SOEE updated by OIC.");

            db.SaveChanges();
            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [Authorize(Roles = "OIC,FinanceManager,FinanceDirector,Cashier,Client")]
        public ActionResult Details(int id)
        {
            var soee = db.SOEEs
                .Include(s => s.Items.Select(it => it.ServiceType))
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Activities)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == id);

            if (soee == null)
            {
                return HttpNotFound();
            }

            var vm = new SOEECreateViewModel
            {
                SOEEID = soee.SOEEID,
                ProjectID = soee.ProjectID,
                CustomerID = soee.CustomerID,
                ReferenceNo = soee.ReferenceNo,
                MarkTo = soee.MarkTo,
                Subject = soee.Subject,
                Reference = soee.Reference,
                Content = soee.Content,
                SOEERaiseDate = soee.SOEERaiseDate,
                TotalBasicAmount = soee.TotalBasicAmount,
                TotalServiceCharge = soee.TotalServiceCharge,
                TotalTaxAmount = soee.TotalTaxAmount,
                GrandTotal = soee.GrandTotal,
                Status = soee.Status,
                PrevSOEEID = soee.PrevSOEEID,
                PreviousSOEEBalance = soee.PreviousSOEEBalance,
                Project = soee.Project,
                Customer = soee.Customer,
                IsESigned = soee.IsESigned,
                ESignedBy = soee.ESignedBy,
                ESignedOn = soee.ESignedOn,
                DispatchEmail = soee.DispatchEmail,
                DispatchedBy = soee.DispatchedBy,
                DispatchedOn = soee.DispatchedOn,
                ClosureRemarks = soee.ClosureRemarks,
                Items = soee.Items
                    .Where(it => !it.IsDeleted)
                    .OrderBy(it => it.SOEEItemID)
                    .Select(i => new SOEEItemViewModel
                    {
                        SOEEItemID = i.SOEEItemID,
                        DescriptionOfWork = i.DescriptionOfWork,
                        ServiceTypeID = i.ServiceTypeID,
                        ServiceTypeName = i.ServiceType != null ? i.ServiceType.ServiceName : string.Empty,
                        Unit = i.Unit,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        SubTotal = i.SubTotal,
                        ServiceChargePercent = i.ServiceChargePercent,
                        ServiceCharge = i.ServiceCharge,
                        TotalAfterServiceCharge = i.SubTotal + i.ServiceCharge,
                        CGST = i.CGST,
                        SGST = i.SGST,
                        Total = i.Total
                    }).ToList(),
                Activities = soee.Activities
                    .OrderByDescending(a => a.ActionOn)
                    .Select(a => new SOEEActivityViewModel
                    {
                        OldStatus = a.OldStatus,
                        NewStatus = a.NewStatus,
                        ActionBy = a.ActionBy,
                        ActionOn = a.ActionOn,
                        Remarks = a.Remarks
                    }).ToList(),
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

            return View(vm);
        }

        [Authorize(Roles = "OIC,FinanceManager,FinanceDirector,Cashier,Client")]
        public ActionResult Print(int id)
        {
            var details = Details(id) as ViewResult;
            if (details == null || details.Model == null)
            {
                return HttpNotFound();
            }

            return View("Print", details.Model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OIC")]
        public ActionResult Submit(int id)
        {
            var soee = db.SOEEs.Find(id);
            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Draft && soee.Status != SOEEStatus.NeedCorrection)
            {
                return new HttpStatusCodeResult(403);
            }

            AddSOEEActivity(soee, soee.Status, SOEEStatus.Submitted, "Submitted to Finance Manager.");
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinanceManager")]
        public ActionResult ApproveByFM(int id)
        {
            var soee = db.SOEEs.Find(id);
            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Submitted)
            {
                return new HttpStatusCodeResult(403);
            }

            soee.IsESigned = true;
            soee.ESignedBy = User.Identity.Name;
            soee.ESignedOn = DateTime.Now;

            AddSOEEActivity(soee, soee.Status, SOEEStatus.Approved, "Approved and e-signed by Finance Manager.");
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinanceManager")]
        public ActionResult NeedCorrectionByFM(int id, string remarks)
        {
            var soee = db.SOEEs.Find(id);
            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Submitted)
            {
                return new HttpStatusCodeResult(403);
            }

            AddSOEEActivity(soee, soee.Status, SOEEStatus.NeedCorrection, string.IsNullOrWhiteSpace(remarks) ? "Returned for correction by Finance Manager." : remarks);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinanceDirector")]
        public ActionResult ApproveByFD(int id)
        {
            var soee = db.SOEEs
                .Include(s => s.Customer)
                .FirstOrDefault(s => s.SOEEID == id);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Approved)
            {
                return new HttpStatusCodeResult(403);
            }

            soee.DispatchedOn = DateTime.Now;
            soee.DispatchedBy = User.Identity.Name;
            soee.DispatchEmail = soee.Customer != null ? soee.Customer.Email : string.Empty;

            AddSOEEActivity(soee, soee.Status, SOEEStatus.Dispatched, "Formal dispatch completed to client" +
                (string.IsNullOrWhiteSpace(soee.DispatchEmail) ? "." : " at " + soee.DispatchEmail + "."));

            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OIC")]
        public ActionResult CloseSOEE(int id, string remarks)
        {
            var soee = db.SOEEs
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == id);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Reconciled)
            {
                return new HttpStatusCodeResult(403);
            }

            soee.ClosedDate = DateTime.Now;
            soee.ClosureRemarks = string.IsNullOrWhiteSpace(remarks)
                ? "Closed by OIC. Outstanding difference will be adjusted in the next SOEE."
                : remarks;

            if (soee.ReceiptVoucher != null)
            {
                soee.ReceiptVoucher.Status = ReceiptVoucherStatus.Closed;
            }

            AddSOEEActivity(soee, soee.Status, SOEEStatus.Closed, soee.ClosureRemarks);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult MarkPartiallyPaid(int id)
        {
            return UpdateClientPaymentConfirmation(id, SOEEStatus.PartiallyPaid, "Client confirmed partial payment. OIC to record receipt details.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult MarkFullyPaid(int id)
        {
            return UpdateClientPaymentConfirmation(id, SOEEStatus.FullyPaid, "Client confirmed full payment. OIC to record receipt details.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult RejectByClient(int id, string remarks)
        {
            var soee = db.SOEEs.Find(id);
            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Dispatched)
            {
                return new HttpStatusCodeResult(403);
            }

            AddSOEEActivity(soee, soee.Status, SOEEStatus.Rejected, string.IsNullOrWhiteSpace(remarks) ? "Client rejected the dispatched SOEE." : remarks);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cashier")]
        public ActionResult MarkErrorInPayment(int id, string remarks)
        {
            var soee = db.SOEEs.Find(id);
            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.PartiallyPaid && soee.Status != SOEEStatus.FullyPaid)
            {
                return new HttpStatusCodeResult(403);
            }

            AddSOEEActivity(soee, soee.Status, SOEEStatus.ErrorInPayment, string.IsNullOrWhiteSpace(remarks) ? "Cashier reported an error in payment." : remarks);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cashier")]
        public ActionResult ReconcilePayment(int id)
        {
            var soee = db.SOEEs
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .FirstOrDefault(s => s.SOEEID == id);

            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.PartiallyPaid &&
                soee.Status != SOEEStatus.FullyPaid &&
                soee.Status != SOEEStatus.ErrorInPayment)
            {
                return new HttpStatusCodeResult(403);
            }

            var totalReceived = soee.Payments.Sum(p => p.AmountReceived);

            foreach (var payment in soee.Payments.Where(p => !p.IsReconciled))
            {
                payment.IsReconciled = true;
                payment.ReconciledOn = DateTime.Now;
            }

            if (soee.ReceiptVoucher == null)
            {
                soee.ReceiptVoucher = new ReceiptVoucher
                {
                    SOEEID = soee.SOEEID,
                    VoucherNumber = BuildVoucherNumber(soee.SOEEID),
                    GeneratedOn = DateTime.Now
                };
            }

            soee.ReceiptVoucher.VoucherDate = DateTime.Today;
            soee.ReceiptVoucher.Amount = totalReceived;
            soee.ReceiptVoucher.Status = ReceiptVoucherStatus.Reconciled;
            soee.ReceiptVoucher.GeneratedBy = User.Identity.Name;
            soee.ReceiptVoucher.GeneratedOn = DateTime.Now;
            soee.ReceiptVoucher.Remarks = "Reconciled from quick action.";

            AddSOEEActivity(soee, soee.Status, SOEEStatus.Reconciled, "Payment reconciled and OIC notified.");
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OIC")]
        public ActionResult SubmitAjax(int id)
        {
            return Submit(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinanceManager")]
        public ActionResult ApproveByFMAjax(int id)
        {
            return ApproveByFM(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinanceManager")]
        public ActionResult NeedCorrectionByFMAjax(int id)
        {
            return NeedCorrectionByFM(id, "Need correction from Finance Manager.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinanceDirector")]
        public ActionResult ApproveByFDAjax(int id)
        {
            return ApproveByFD(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult MarkPartiallyPaidAjax(int id)
        {
            return MarkPartiallyPaid(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult MarkFullyPaidAjax(int id)
        {
            return MarkFullyPaid(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public ActionResult RejectByClientAjax(int id)
        {
            return RejectByClient(id, "Rejected by client.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cashier")]
        public ActionResult ErrorInPaymentAjax(int id)
        {
            return MarkErrorInPayment(id, "Error in payment.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cashier")]
        public ActionResult ReconcileAjax(int id)
        {
            return ReconcilePayment(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OIC")]
        public ActionResult CloseAjax(int id)
        {
            return CloseSOEE(id, null);
        }

        [Authorize(Roles = "OIC")]
        public JsonResult GetPreviousSOEEData(int projectId)
        {
            var previous = GetLatestPreviousSOEE(projectId, null);
            if (previous == null)
            {
                return Json(new { found = false }, JsonRequestBehavior.AllowGet);
            }

            var previousLoaded = db.SOEEs
                .Include(s => s.Items.Select(i => i.ServiceType))
                .Include(s => s.Payments)
                .FirstOrDefault(s => s.SOEEID == previous.SOEEID);

            return Json(new
            {
                found = true,
                soeeId = previousLoaded.SOEEID,
                referenceNo = previousLoaded.ReferenceNo,
                subject = previousLoaded.Subject,
                reference = previousLoaded.Reference,
                content = previousLoaded.Content,
                markTo = previousLoaded.MarkTo,
                previousBalance = GetOutstandingAmount(previousLoaded),
                items = previousLoaded.Items
                    .Where(i => !i.IsDeleted)
                    .OrderBy(i => i.SOEEItemID)
                    .Select(i => new
                    {
                        descriptionOfWork = i.DescriptionOfWork,
                        serviceTypeID = i.ServiceTypeID,
                        serviceTypeName = i.ServiceType != null ? i.ServiceType.ServiceName : string.Empty,
                        unit = i.Unit,
                        quantity = i.Quantity,
                        unitPrice = i.UnitPrice
                    }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "OIC,FinanceManager,FinanceDirector,Cashier")]
        public FileResult ExportCsv(int projectId = 0, string status = null)
        {
            var query = BuildRoleScopedQuery(projectId);
            var parsedStatus = ParseStatus(status);

            if (parsedStatus.HasValue)
            {
                query = query.Where(s => s.Status == parsedStatus.Value);
            }

            var rows = query.OrderByDescending(s => s.SOEEID).ToList();
            var csv = new StringBuilder();
            csv.AppendLine("SOEE ID,Project,Customer,Reference No,SOEE Date,Grand Total,Received Amount,Status");

            foreach (var row in rows)
            {
                csv.AppendLine(string.Format("{0},\"{1}\",\"{2}\",\"{3}\",{4:yyyy-MM-dd},{5},{6},{7}",
                    row.SOEEID,
                    row.Project != null ? row.Project.Name : string.Empty,
                    row.Customer != null ? row.Customer.CustomerName : string.Empty,
                    row.ReferenceNo ?? string.Empty,
                    row.SOEERaiseDate,
                    row.GrandTotal,
                    row.Payments.Sum(p => p.AmountReceived),
                    row.Status));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "soee-list.csv");
        }

        public JsonResult GetServicesForProject(int projectId)
        {
            var services = db.ProjectServiceMaps
                .Where(m => m.ProjectID == projectId && m.IsActive)
                .Select(m => new
                {
                    m.ServiceTypeID,
                    ServiceName = m.ServiceType.ServiceName
                })
                .ToList();

            if (!services.Any())
            {
                services = ServiceTypeAllowed.ListAllowed(db)
                    .Select(s => new
                    {
                        s.ServiceTypeID,
                        ServiceName = s.ServiceName
                    })
                    .ToList();
            }

            return Json(services, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetServiceChargePercent(int serviceTypeId, decimal subtotal, int projectId, int? soeeId = null)
        {
            var projectCost = db.Projects
                .Where(p => p.ProjectID == projectId)
                .Select(p => (decimal?)p.ProjectCost)
                .FirstOrDefault() ?? 0m;

            var percent = ServiceChargeResolver.ResolvePercentage(db, projectCost, serviceTypeId);

            return Json(new
            {
                percentage = percent,
                projectCost = projectCost,
                subtotal = subtotal
            });
        }

        public class ItemDto
        {
            public int serviceTypeID { get; set; }
            public decimal qty { get; set; }
            public decimal amount { get; set; }
        }

        [HttpPost]
        public JsonResult AjaxCompute(int projectID, ItemDto[] items)
        {
            var soee = new SOEE
            {
                ProjectID = projectID,
                Items = items.Select(i => new SOEEItem
                {
                    ServiceTypeID = i.serviceTypeID,
                    Quantity = i.qty,
                    Unit = 1,
                    UnitPrice = i.amount
                }).ToList()
            };

            var totals = CostCalculator.ComputeTotalsForItems(soee.Items, db, soee.ProjectID, soee.SOEEID);
            return Json(new
            {
                serviceCharge = totals.ServiceCharge,
                cgst = totals.CGST,
                sgst = totals.SGST,
                total = totals.Total
            });
        }

        private IQueryable<SOEE> BuildRoleScopedQuery(int projectId)
        {
            var query = db.SOEEs
                .Include(s => s.Project)
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.ReceiptVoucher)
                .Where(s => projectId == 0 || s.ProjectID == projectId);

            if (User.IsInRole("OIC"))
            {
                query = query.Where(s => s.CreatedBy == User.Identity.Name);
            }
            else if (User.IsInRole("FinanceManager"))
            {
                query = query.Where(s => s.Status != SOEEStatus.Draft);
            }
            else if (User.IsInRole("FinanceDirector"))
            {
                query = query.Where(s =>
                    s.Status == SOEEStatus.Approved ||
                    s.Status == SOEEStatus.Dispatched ||
                    s.Status == SOEEStatus.PartiallyPaid ||
                    s.Status == SOEEStatus.FullyPaid ||
                    s.Status == SOEEStatus.ErrorInPayment ||
                    s.Status == SOEEStatus.Reconciled ||
                    s.Status == SOEEStatus.Closed);
            }
            else if (User.IsInRole("Client"))
            {
                query = query.Where(s =>
                    s.Status == SOEEStatus.Dispatched ||
                    s.Status == SOEEStatus.PartiallyPaid ||
                    s.Status == SOEEStatus.FullyPaid ||
                    s.Status == SOEEStatus.ErrorInPayment ||
                    s.Status == SOEEStatus.Reconciled ||
                    s.Status == SOEEStatus.Closed);
            }
            else if (User.IsInRole("Cashier"))
            {
                query = query.Where(s =>
                    s.Status == SOEEStatus.PartiallyPaid ||
                    s.Status == SOEEStatus.FullyPaid ||
                    s.Status == SOEEStatus.ErrorInPayment ||
                    s.Status == SOEEStatus.Reconciled ||
                    s.Status == SOEEStatus.Closed);
            }

            return query;
        }

        private void PopulateCreateDropDowns(int projectId, int customerId)
        {
            ViewBag.Projects = new SelectList(db.Projects.OrderBy(p => p.Name).ToList(), "ProjectID", "Name", projectId == 0 ? (object)null : projectId);
            ViewBag.Customers = new SelectList(db.Customers.OrderBy(c => c.CustomerName).ToList(), "CustomerID", "CustomerName", customerId == 0 ? (object)null : customerId);
            ViewBag.ServiceTypes = GetProjectServices(projectId);
        }

        private List<ServiceType> GetProjectServices(int projectId)
        {
            if (projectId <= 0)
            {
                return ServiceTypeAllowed.ListAllowed(db);
            }

            var services = db.ProjectServiceMaps
                .Where(m => m.ProjectID == projectId && m.IsActive && ServiceTypeAllowed.AllowedServiceNames.Contains(m.ServiceType.ServiceName))
                .Select(m => m.ServiceType)
                .OrderBy(s => s.ServiceName)
                .ToList();

            return services.Any()
                ? services
                : ServiceTypeAllowed.ListAllowed(db);
        }

        private List<SOEEItemViewModel> SanitizeItems(List<SOEEItemViewModel> items)
        {
            return items == null
                ? new List<SOEEItemViewModel>()
                : items.Where(i => i != null && i.ServiceTypeID > 0 && !i.IsDeleted).ToList();
        }

        private List<SOEEItem> BuildItemEntities(IEnumerable<SOEEItemViewModel> items)
        {
            return items.Select(i => new SOEEItem
            {
                ServiceTypeID = i.ServiceTypeID,
                Quantity = i.Quantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                DescriptionOfWork = i.DescriptionOfWork,
                ServiceChargePercent = 0m,
                ServiceCharge = 0m,
                CGST = 0m,
                SGST = 0m,
                Total = 0m,
                IsDeleted = false
            }).ToList();
        }

        private void RecalculateTotals(SOEE soee)
        {
            var activeItems = soee.Items.Where(x => !x.IsDeleted).ToList();
            var totals = CostCalculator.ComputeTotalsForItems(activeItems, db, soee.ProjectID, soee.SOEEID);

            soee.TotalBasicAmount = totals.Basic;
            soee.TotalServiceCharge = totals.ServiceCharge;
            soee.TotalTaxAmount = totals.CGST + totals.SGST;
            soee.GrandTotal = totals.Total;
        }

        private bool CanEdit(SOEE soee)
        {
            return soee.Status == SOEEStatus.Draft || soee.Status == SOEEStatus.NeedCorrection;
        }

        private SOEEStatus? ParseStatus(string status)
        {
            SOEEStatus parsed;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(status, true, out parsed))
            {
                return parsed;
            }

            return null;
        }

        private SOEE GetLatestPreviousSOEE(int projectId, int? currentSoeeId)
        {
            return db.SOEEs
                .Include(s => s.Payments)
                .Where(s =>
                    s.ProjectID == projectId &&
                    (!currentSoeeId.HasValue || s.SOEEID != currentSoeeId.Value) &&
                    s.Status != SOEEStatus.Draft &&
                    s.Status != SOEEStatus.Rejected)
                .OrderByDescending(s => s.SOEERaiseDate)
                .ThenByDescending(s => s.SOEEID)
                .FirstOrDefault();
        }

        private void ApplyPreviousSOEEContext(SOEECreateViewModel vm, int projectId, int? currentSoeeId)
        {
            var previous = GetLatestPreviousSOEE(projectId, currentSoeeId);
            if (previous == null)
            {
                return;
            }

            vm.PrevSOEEID = previous.SOEEID;
            vm.PreviousSOEEBalance = GetOutstandingAmount(previous);
        }

        private decimal GetOutstandingAmount(SOEE soee)
        {
            return soee.GrandTotal - (soee.Payments != null ? soee.Payments.Sum(p => p.AmountReceived) : 0m);
        }

        private string BuildVoucherNumber(int soeeId)
        {
            return "RV-" + soeeId.ToString("D5");
        }

        private ActionResult UpdateClientPaymentConfirmation(int id, SOEEStatus newStatus, string remarks)
        {
            var soee = db.SOEEs.Find(id);
            if (soee == null)
            {
                return HttpNotFound();
            }

            if (soee.Status != SOEEStatus.Dispatched &&
                soee.Status != SOEEStatus.PartiallyPaid &&
                soee.Status != SOEEStatus.ErrorInPayment)
            {
                return new HttpStatusCodeResult(403);
            }

            AddSOEEActivity(soee, soee.Status, newStatus, remarks);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, status = soee.Status.ToString() });
            }

            return RedirectToAction("Details", new { id = soee.SOEEID });
        }

        private void AddSOEEActivity(SOEE soee, SOEEStatus oldStatus, SOEEStatus newStatus, string remarks)
        {
            soee.Status = newStatus;

            db.SOEEActivities.Add(new SOEEActivity
            {
                SOEEID = soee.SOEEID,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ActionBy = User.Identity.Name,
                ActionOn = DateTime.Now,
                Remarks = remarks
            });
        }
    }
}
