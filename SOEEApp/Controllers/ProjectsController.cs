using SOEEApp.Models; // ApplicationUser, ApplicationDbContext
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework; // if available

namespace SOEEApp.Controllers
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        public ActionResult Index()
        {
            var projects = db.Projects.Include("OICUser").ToList();
            return View(projects);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            PopulateOICDropDown();
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Project project)
        {
            if (!ModelState.IsValid)
            {
                PopulateOICDropDown();
                return View(project);
            }

            db.Projects.Add(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void PopulateOICDropDown()
        {
            // Option A: if you use ASP.NET Identity (default)
            var oicRole = db.Roles.FirstOrDefault(r => r.Name == "OIC");
            if (oicRole != null)
            {
                var usersInOIC = db.Users
                    .Where(u => u.Roles.Any(ur => ur.RoleId == oicRole.Id))
                    .Select(u => new { u.Id, Name = (u.Name ?? u.UserName ?? u.Email) })
                    .ToList();

                ViewBag.OICUserId = new SelectList(usersInOIC, "Id", "Name");
                return;
            }
        }
        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            Project project = db.Projects.Find(id);
            if (project == null) return HttpNotFound();
            PopulateOICDropDown();
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateOICDropDown();
            return View(project);
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            Project project = db.Projects.Find(id);
            if (project == null) return HttpNotFound();
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Check for SOEE dependencies
            bool hasSOEEs = db.SOEEs.Any(s => s.ProjectID == id);
            if (hasSOEEs)
            {
                Project project = db.Projects.Find(id);
                ModelState.AddModelError("", "Cannot delete Project because it has associated SOEEs. Please delete the SOEEs first.");
                return View(project);
            }

            Project projectToDelete = db.Projects.Find(id);
            
            // Safe Delete: Remove service maps first
            var maps = db.ProjectServiceMaps.Where(m => m.ProjectID == id).ToList();
            db.ProjectServiceMaps.RemoveRange(maps);

            db.Projects.Remove(projectToDelete);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
