using SOEEApp.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

public class CustomersController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    // GET: Customers
    public ActionResult Index()
    {
        return View(db.Customers.ToList());
    }

    // GET: Customers/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Customer customer)
    {
        if (ModelState.IsValid)
        {
            db.Customers.Add(customer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(customer);
    }

    // GET: Customers/Edit/5
    public ActionResult Edit(int? id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        Customer customer = db.Customers.Find(id);
        if (customer == null) return HttpNotFound();

        return View(customer);
    }

    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Customer customer)
    {
        if (ModelState.IsValid)
        {
            db.Entry(customer).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(customer);
    }

    // GET: Customers/Delete/5
    public ActionResult Delete(int? id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        Customer customer = db.Customers.Find(id);
        if (customer == null) return HttpNotFound();

        return View(customer);
    }

    // POST: Customers/Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        Customer customer = db.Customers.Find(id);
        db.Customers.Remove(customer);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
