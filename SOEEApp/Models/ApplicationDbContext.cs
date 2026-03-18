using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace SOEEApp.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // ----------- YOUR CUSTOM TABLES -----------
        public DbSet<SOEE> SOEEs { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<ProjectServiceMap> ProjectServiceMaps { get; set; }
        public DbSet<ServiceCostSlab> ServiceCostSlabs { get; set; }
        public DbSet<ServiceTypeSlabMap> ServiceTypeSlabMaps { get; set; }
        public DbSet<SOEEItem> SOEEItems { get; set; }
        public DbSet<SOEEActivity> SOEEActivities { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ReceiptVoucher> ReceiptVouchers { get; set; }


        // ------------------------------------------
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // MUST CALL base FIRST
            base.OnModelCreating(modelBuilder);

            // ==============================
            // 1️⃣ RENAME ASP.NET IDENTITY TABLES
            // ==============================

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");

            // ==============================
            // 2️⃣ YOUR CUSTOM RELATIONSHIPS
            // ==============================

            // ProjectServiceMap → Project
            modelBuilder.Entity<ProjectServiceMap>()
                .HasRequired(ps => ps.Project)
                .WithMany(p => p.ProjectServiceMaps)
                .HasForeignKey(ps => ps.ProjectID)
                .WillCascadeOnDelete(false);

            // ProjectServiceMap → ServiceType
            modelBuilder.Entity<ProjectServiceMap>()
                .HasRequired(ps => ps.ServiceType)
                .WithMany(st => st.ProjectServiceMaps)
                .HasForeignKey(ps => ps.ServiceTypeID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.SOEE)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SOEEID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ReceiptVoucher>()
                .HasRequired(v => v.SOEE)
                .WithOptional(s => s.ReceiptVoucher);
        }
    }
}
