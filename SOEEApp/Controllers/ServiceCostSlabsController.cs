using SOEEApp.Models;
using System.Linq;
using System.Web.Mvc;

namespace SOEEApp.Controllers
{
    public class ServiceCostSlabsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(db.ServiceCostSlabs.ToList());
        }

        public ActionResult Create()
        {
            return View(new ServiceCostSlab());
        }

        [HttpPost]
        public ActionResult Create(ServiceCostSlab model)
        {
            if (ModelState.IsValid)
            {
                db.ServiceCostSlabs.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var slab = db.ServiceCostSlabs.Find(id);
            if (slab == null)
            {
                return HttpNotFound();
            }
            return View(slab);
        }

        [HttpPost]
        public ActionResult Edit(ServiceCostSlab model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var slab = db.ServiceCostSlabs.Find(id);
            if (slab == null)
            {
                return HttpNotFound();
            }
            return View(slab);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var slab = db.ServiceCostSlabs.Find(id);
            db.ServiceCostSlabs.Remove(slab);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
