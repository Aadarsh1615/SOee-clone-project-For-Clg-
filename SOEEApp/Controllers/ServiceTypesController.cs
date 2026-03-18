using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SOEEApp.Helpers;
using SOEEApp.Models;

public class ServiceTypesController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    private SelectList GetServiceNameSelectList(string selected = null)
    {
        var existing = db.ServiceTypes.Select(s => s.ServiceName).ToList();
        var options = ServiceTypeAllowed.AllowedServiceNames
            .Where(n => !existing.Contains(n) || n == selected)
            .ToList();
        return new SelectList(options, selected);
    }

    // GET: ServiceTypes
    public ActionResult Index()
    {
        var list = ServiceTypeAllowed.ListAllowed(db);
        return View(list);
    }

    // GET: ServiceTypes/Create
    public ActionResult Create()
    {
        ViewBag.AllowedServiceNames = GetServiceNameSelectList();
        return View();
    }


    // POST: ServiceTypes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(ServiceType serviceType)
    {
        if (!ServiceTypeAllowed.AllowedServiceNames.Contains(serviceType.ServiceName))
        {
            ModelState.AddModelError("ServiceName", "Service is not allowed. Please select one of the approved service types.");
        }

        if (db.ServiceTypes.Any(s => s.ServiceName == serviceType.ServiceName))
        {
            ModelState.AddModelError("ServiceName", "This service type already exists.");
        }

        if (ModelState.IsValid)
        {
            serviceType.CreatedDate = DateTime.Now;
            db.ServiceTypes.Add(serviceType);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.AllowedServiceNames = GetServiceNameSelectList(serviceType.ServiceName);
        return View(serviceType);
    }

    // GET: ServiceTypes/Edit/5
    public ActionResult Edit(int? id)
    {
        if (id == null)
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var service = db.ServiceTypes.Find(id);
        if (service == null)
            return HttpNotFound();

        ViewBag.AllowedServiceNames = GetServiceNameSelectList(service.ServiceName);
        return View(service);
    }

    // POST: ServiceTypes/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(ServiceType serviceType)
    {
        if (!ServiceTypeAllowed.AllowedServiceNames.Contains(serviceType.ServiceName))
        {
            ModelState.AddModelError("ServiceName", "Service is not allowed. Please select one of the approved service types.");
        }

        if (ModelState.IsValid)
        {
            db.Entry(serviceType).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.AllowedServiceNames = GetServiceNameSelectList(serviceType.ServiceName);
        return View(serviceType);
    }

    // GET: ServiceTypes/Delete/5
    public ActionResult Delete(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        ServiceType serviceType = db.ServiceTypes.Find(id);
        if (serviceType == null)
        {
            return HttpNotFound();
        }
        return View(serviceType);
    }

    // POST: ServiceTypes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        ServiceType serviceType = db.ServiceTypes.Find(id);
        
        // Cascade Delete: Remove related maps first
        var relatedProjectMaps = db.ProjectServiceMaps.Where(m => m.ServiceTypeID == id).ToList();
        db.ProjectServiceMaps.RemoveRange(relatedProjectMaps);

        var relatedSlabMaps = db.ServiceTypeSlabMaps.Where(m => m.ServiceTypeID == id).ToList();
        db.ServiceTypeSlabMaps.RemoveRange(relatedSlabMaps);

        db.ServiceTypes.Remove(serviceType);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
