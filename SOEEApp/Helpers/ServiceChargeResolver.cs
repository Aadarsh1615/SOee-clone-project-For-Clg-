using System.Linq;
using SOEEApp.Models;

namespace SOEEApp.Helpers
{
    public static class ServiceChargeResolver
    {
        public static decimal ResolvePercentage(ApplicationDbContext db, int projectId, int serviceTypeId)
        {
            if (projectId <= 0 || serviceTypeId <= 0)
            {
                return 0m;
            }

            var projectCost = db.Projects
                .Where(p => p.ProjectID == projectId)
                .Select(p => (decimal?)p.ProjectCost)
                .FirstOrDefault() ?? 0m;

            return ResolvePercentage(db, projectCost, serviceTypeId);
        }

        public static decimal ResolvePercentage(ApplicationDbContext db, decimal projectCost, int serviceTypeId)
        {
            if (projectCost <= 0m || serviceTypeId <= 0)
            {
                return 0m;
            }

            return db.ServiceTypeSlabMaps
                .Where(m =>
                    m.ServiceTypeID == serviceTypeId &&
                    m.Slab.IsActive &&
                    projectCost >= m.Slab.MinAmount &&
                    (!m.Slab.MaxAmount.HasValue || projectCost <= m.Slab.MaxAmount.Value))
                .OrderByDescending(m => m.Slab.MinAmount)
                .Select(m => (decimal?)m.Percentage)
                .FirstOrDefault() ?? 0m;
        }
    }
}
