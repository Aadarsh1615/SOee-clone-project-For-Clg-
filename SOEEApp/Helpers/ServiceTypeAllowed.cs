using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SOEEApp.Models;

namespace SOEEApp.Helpers
{
    public static class ServiceTypeAllowed
    {
        public static readonly string[] AllowedServiceNames = new[]
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

        public static IQueryable<ServiceType> QueryAllowed(ApplicationDbContext db)
        {
            return db.ServiceTypes
                .Where(s => AllowedServiceNames.Contains(s.ServiceName))
                .OrderBy(s => s.ServiceName);
        }

        public static List<ServiceType> ListAllowed(ApplicationDbContext db)
        {
            return QueryAllowed(db)
                .ToList()
                .OrderBy(s => Array.IndexOf(AllowedServiceNames, s.ServiceName))
                .ToList();
        }

        public static SelectList GetAllowedSelectList(ApplicationDbContext db, object selectedValue = null)
        {
            return new SelectList(ListAllowed(db), "ServiceTypeID", "ServiceName", selectedValue);
        }

        public static bool IsAllowed(ApplicationDbContext db, int serviceTypeId)
        {
            var st = db.ServiceTypes.Find(serviceTypeId);
            return st != null && AllowedServiceNames.Contains(st.ServiceName);
        }
    }
}
