using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using SOEEApp.Models;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: OwinStartupAttribute(typeof(SOEEApp.Startup))]
namespace SOEEApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = false; // ensure CSS updates are reflected during development

            ConfigureAuth(app);
            InitializeAppRoles();
        }

        private void InitializeAppRoles()
        {
            using (var context = new ApplicationDbContext())
            {
                context.Database.Initialize(false);

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                string[] roles = { "OIC", "FinanceManager", "FinanceDirector", "Cashier", "Client" };

                foreach (var role in roles)
                {
                    if (!roleManager.RoleExists(role))
                    {
                        roleManager.Create(new IdentityRole(role));
                    }
                }
            }
        }
    }
}
