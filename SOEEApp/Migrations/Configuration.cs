namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SOEEApp.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(SOEEApp.Models.ApplicationDbContext context)
        {
            SeedServiceTypes(context);
            SeedServiceCostSlabs(context);
            SeedServiceTypeSlabMaps(context);
        }

        private static void SeedServiceTypes(SOEEApp.Models.ApplicationDbContext context)
        {
            var now = DateTime.Now;
            var serviceNames = new[]
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

            foreach (var serviceName in serviceNames)
            {
                if (!context.ServiceTypes.Any(s => s.ServiceName == serviceName))
                {
                    context.ServiceTypes.Add(new SOEEApp.Models.ServiceType
                    {
                        ServiceName = serviceName,
                        CreatedDate = now
                    });
                }
            }

            context.SaveChanges();
        }

        private static void SeedServiceCostSlabs(SOEEApp.Models.ApplicationDbContext context)
        {
            context.ServiceCostSlabs.AddOrUpdate(
                s => new { s.MinAmount, s.MaxAmount },
                new SOEEApp.Models.ServiceCostSlab
                {
                    MinAmount = 0m,
                    MaxAmount = 2500000m,
                    IsActive = true
                },
                new SOEEApp.Models.ServiceCostSlab
                {
                    MinAmount = 2500000.01m,
                    MaxAmount = 10000000m,
                    IsActive = true
                },
                new SOEEApp.Models.ServiceCostSlab
                {
                    MinAmount = 10000000.01m,
                    MaxAmount = null,
                    IsActive = true
                });

            context.SaveChanges();
        }

        private static void SeedServiceTypeSlabMaps(SOEEApp.Models.ApplicationDbContext context)
        {
            var services = context.ServiceTypes.ToDictionary(s => s.ServiceName, s => s.ServiceTypeID);
            var slabs = context.ServiceCostSlabs.ToDictionary(s => s.MinAmount, s => s.Id);

            var mappings = new[]
            {
                CreateMap(services, slabs, "Turnkey", 0m, 15m),
                CreateMap(services, slabs, "Turnkey", 2500000.01m, 12m),
                CreateMap(services, slabs, "Turnkey", 10000000.01m, 10m),
                CreateMap(services, slabs, "Consultancy", 0m, 10m),
                CreateMap(services, slabs, "Consultancy", 2500000.01m, 8m),
                CreateMap(services, slabs, "Consultancy", 10000000.01m, 6m),
                CreateMap(services, slabs, "Application Software", 0m, 10m),
                CreateMap(services, slabs, "Application Software", 2500000.01m, 8m),
                CreateMap(services, slabs, "Application Software", 10000000.01m, 6m),
                CreateMap(services, slabs, "System Software", 0m, 3m),
                CreateMap(services, slabs, "System Software", 2500000.01m, 3m),
                CreateMap(services, slabs, "System Software", 10000000.01m, 3m),
                CreateMap(services, slabs, "Hardware", 0m, 5m),
                CreateMap(services, slabs, "Hardware", 2500000.01m, 5m),
                CreateMap(services, slabs, "Hardware", 10000000.01m, 5m),
                CreateMap(services, slabs, "FMS", 0m, 10m),
                CreateMap(services, slabs, "FMS", 2500000.01m, 8m),
                CreateMap(services, slabs, "FMS", 10000000.01m, 7m),
                CreateMap(services, slabs, "Retainership", 0m, 5m),
                CreateMap(services, slabs, "Retainership", 2500000.01m, 5m),
                CreateMap(services, slabs, "Training", 0m, 5m),
                CreateMap(services, slabs, "Training", 2500000.01m, 5m),
                CreateMap(services, slabs, "Training", 10000000.01m, 5m)
            };

            foreach (var mapping in mappings)
            {
                context.ServiceTypeSlabMaps.AddOrUpdate(
                    m => new { m.ServiceTypeID, m.SlabID },
                    mapping);
            }

            if (services.ContainsKey("Retainership") && slabs.ContainsKey(10000000.01m))
            {
                var retainershipId = services["Retainership"];
                var slab3Id = slabs[10000000.01m];

                var retainershipThirdSlab = context.ServiceTypeSlabMaps.FirstOrDefault(m =>
                    m.ServiceTypeID == retainershipId &&
                    m.SlabID == slab3Id);

                if (retainershipThirdSlab != null)
                {
                    context.ServiceTypeSlabMaps.Remove(retainershipThirdSlab);
                }
            }

            context.SaveChanges();
        }

        private static SOEEApp.Models.ServiceTypeSlabMap CreateMap(
            IDictionary<string, int> services,
            IDictionary<decimal, int> slabs,
            string serviceName,
            decimal slabMinAmount,
            decimal percentage)
        {
            return new SOEEApp.Models.ServiceTypeSlabMap
            {
                ServiceTypeID = services[serviceName],
                SlabID = slabs[slabMinAmount],
                Percentage = percentage
            };
        }

    }
}
