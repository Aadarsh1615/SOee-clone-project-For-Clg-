using SOEEApp.Helpers;
using SOEEApp.Models;
using System.Linq;
using System.Web.Mvc;

namespace SOEEApp.Controllers
{
    public class ServiceTypeSlabMapsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private static readonly string[] DefaultsServiceNames = new[]
        {
            "Turnkey",
            "Consultancy",
            "Application Software",
            "System Software",
            "Hardware",
            "FMS",
            "Retainership",
            "Training"
        };

        private static readonly decimal?[][] DefaultPercentages = new decimal?[][]
        {
            new decimal?[] { 15m, 10m, 10m, 3m, 5m, 10m, 5m, 5m }, // up to 25L
            new decimal?[] { 12m, 8m, 8m, 3m, 5m, 8m, 5m, 5m },  // 25L-1Cr
            new decimal?[] { 10m, 6m, 6m, 3m, 5m, 7m, null, 5m } // 1Cr+
        };

        public ActionResult Index()
        {
            var maps = db.ServiceTypeSlabMaps
                .Include("ServiceType")
                .Include("Slab")
                .ToList();

            var serviceTypes = ServiceTypeAllowed.ListAllowed(db);

            var matrixRows = serviceTypes.Select(st =>
            {
                var ranges = maps.Where(m => m.ServiceTypeID == st.ServiceTypeID).ToList();
                var range0To25 = ranges.FirstOrDefault(m => m.Slab.MinAmount >= 0 && m.Slab.MaxAmount.HasValue && m.Slab.MaxAmount.Value <= 2500000)?.Percentage;
                var range25To1Cr = ranges.FirstOrDefault(m => m.Slab.MinAmount >= 2500000 && m.Slab.MaxAmount.HasValue && m.Slab.MaxAmount.Value <= 10000000)?.Percentage;
                var range1CrPlus = ranges.FirstOrDefault(m => m.Slab.MinAmount >= 10000000 && !m.Slab.MaxAmount.HasValue)?.Percentage;

                return new ServiceTypeSlabMatrixRow
                {
                    ServiceType = st.ServiceName,
                    Range0To25Lac = range0To25,
                    Range25LacTo1Cr = range25To1Cr,
                    Range1CrPlus = range1CrPlus
                };
            }).ToList();

            return View(matrixRows);
        }

        [HttpPost]
        public ActionResult ApplyDefaultRates()
        {
            ApplyDefaultTableRates();
            return RedirectToAction("Index");
        }

        private void ApplyDefaultTableRates()
        {
            var slabUpTo25 = db.ServiceCostSlabs
                .FirstOrDefault(s => s.MinAmount <= 0m && s.MaxAmount.HasValue && s.MaxAmount.Value >= 2500000m);
            var slab25To100 = db.ServiceCostSlabs
                .FirstOrDefault(s => s.MinAmount <= 2500000m && s.MaxAmount.HasValue && s.MaxAmount.Value >= 10000000m);
            var slab100Plus = db.ServiceCostSlabs
                .FirstOrDefault(s => s.MinAmount >= 10000000m && !s.MaxAmount.HasValue);

            if (slabUpTo25 == null || slab25To100 == null || slab100Plus == null)
                return;

            var serviceTypes = ServiceTypeAllowed.ListAllowed(db).ToList();
            for (var i = 0; i < DefaultsServiceNames.Length; i++)
            {
                var serviceName = DefaultsServiceNames[i];
                var serviceType = serviceTypes.FirstOrDefault(s => s.ServiceName == serviceName);
                if (serviceType == null)
                    continue;

                var percentagesRow0 = DefaultPercentages[0][i];
                var percentagesRow1 = DefaultPercentages[1][i];
                var percentagesRow2 = DefaultPercentages[2][i];

                SetOrUpdateRate(serviceType.ServiceTypeID, slabUpTo25.Id, percentagesRow0);
                SetOrUpdateRate(serviceType.ServiceTypeID, slab25To100.Id, percentagesRow1);
                SetOrUpdateRate(serviceType.ServiceTypeID, slab100Plus.Id, percentagesRow2);
            }

            db.SaveChanges();
        }

        private void SetOrUpdateRate(int serviceTypeId, int slabId, decimal? percentage)
        {
            var existing = db.ServiceTypeSlabMaps
                .FirstOrDefault(m => m.ServiceTypeID == serviceTypeId && m.SlabID == slabId);

            if (!percentage.HasValue)
            {
                if (existing != null)
                {
                    db.ServiceTypeSlabMaps.Remove(existing);
                }
                return;
            }

            if (existing == null)
            {
                db.ServiceTypeSlabMaps.Add(new ServiceTypeSlabMap
                {
                    ServiceTypeID = serviceTypeId,
                    SlabID = slabId,
                    Percentage = percentage.Value
                });
            }
            else
            {
                existing.Percentage = percentage.Value;
            }
        }

        [HttpGet]
        public JsonResult GetServiceChargeBySlab(int serviceTypeId, int slabId)
        {
            var map = db.ServiceTypeSlabMaps
                .FirstOrDefault(m => m.ServiceTypeID == serviceTypeId && m.SlabID == slabId);
            if (map == null)
            {
                return Json(new { success = false, percentage = (decimal?)null }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, percentage = map.Percentage }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ViewBag.ServiceTypeID = ServiceTypeAllowed.GetAllowedSelectList(db);
            ViewBag.SlabID = db.ServiceCostSlabs
                .Select(s => new
                {
                    SlabID = s.Id,
                    Text = s.MinAmount + " - " + (s.MaxAmount.HasValue ? s.MaxAmount.ToString() : "+")
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceTypeSlabMap model)
        {
            if (ModelState.IsValid)
            {
                db.ServiceTypeSlabMaps.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ServiceTypeID = ServiceTypeAllowed.GetAllowedSelectList(db);
            ViewBag.SlabID = db.ServiceCostSlabs
                .Select(s => new
                {
                    SlabID = s.Id,
                    Text = s.MinAmount + " - " + s.MaxAmount
                })
                .ToList();

            return View(model);
        }
    }
}
