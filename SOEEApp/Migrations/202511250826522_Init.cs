namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        ProjectID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Customer = c.String(),
                        OIC = c.String(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.ProjectID);
            
            CreateTable(
                "dbo.ProjectServiceMaps",
                c => new
                    {
                        ProjServMapID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ServiceTypeID = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        AddedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProjServMapID)
                .ForeignKey("dbo.Projects", t => t.ProjectID)
                .ForeignKey("dbo.ServiceTypes", t => t.ServiceTypeID)
                .Index(t => t.ProjectID)
                .Index(t => t.ServiceTypeID);
            
            CreateTable(
                "dbo.ServiceTypes",
                c => new
                    {
                        ServiceTypeID = c.Int(nullable: false, identity: true),
                        ServiceName = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ServiceTypeID);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.ServiceCostSlabs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MinAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxAmount = c.Decimal(precision: 18, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ServiceTypeSlabMaps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceTypeID = c.Int(nullable: false),
                        SlabID = c.Int(nullable: false),
                        Percentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceTypes", t => t.ServiceTypeID, cascadeDelete: true)
                .ForeignKey("dbo.ServiceCostSlabs", t => t.SlabID, cascadeDelete: true)
                .Index(t => t.ServiceTypeID)
                .Index(t => t.SlabID);
            
            CreateTable(
                "dbo.SOEEHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SOEEID = c.Int(nullable: false),
                        ActionBy = c.String(),
                        ActionOn = c.DateTime(nullable: false),
                        Action = c.String(),
                        Notes = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SOEEs", t => t.SOEEID, cascadeDelete: true)
                .Index(t => t.SOEEID);
            
            CreateTable(
                "dbo.SOEEs",
                c => new
                    {
                        SOEEID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        ReferenceNo = c.String(),
                        Date = c.DateTime(nullable: false),
                        ClientName = c.String(),
                        MarkTo = c.String(),
                        Subject = c.String(),
                        Description = c.String(),
                        BasicAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ServiceCharge = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CGST = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SGST = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GrandTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PreviousSOEEBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PrevSOEEID = c.Int(),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ClosedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.SOEEID)
                .ForeignKey("dbo.Projects", t => t.ProjectID, cascadeDelete: true)
                .Index(t => t.ProjectID);
            
            CreateTable(
                "dbo.SOEEItems",
                c => new
                    {
                        SOEEItemID = c.Int(nullable: false, identity: true),
                        SOEEID = c.Int(nullable: false),
                        ServiceTypeID = c.Int(nullable: false),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Unit = c.String(),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.SOEEItemID)
                .ForeignKey("dbo.ServiceTypes", t => t.ServiceTypeID, cascadeDelete: true)
                .ForeignKey("dbo.SOEEs", t => t.SOEEID, cascadeDelete: true)
                .Index(t => t.SOEEID)
                .Index(t => t.ServiceTypeID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.Users");
            DropForeignKey("dbo.SOEEs", "ProjectID", "dbo.Projects");
            DropForeignKey("dbo.SOEEItems", "SOEEID", "dbo.SOEEs");
            DropForeignKey("dbo.SOEEItems", "ServiceTypeID", "dbo.ServiceTypes");
            DropForeignKey("dbo.SOEEHistories", "SOEEID", "dbo.SOEEs");
            DropForeignKey("dbo.ServiceTypeSlabMaps", "SlabID", "dbo.ServiceCostSlabs");
            DropForeignKey("dbo.ServiceTypeSlabMaps", "ServiceTypeID", "dbo.ServiceTypes");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.ProjectServiceMaps", "ServiceTypeID", "dbo.ServiceTypes");
            DropForeignKey("dbo.ProjectServiceMaps", "ProjectID", "dbo.Projects");
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.SOEEItems", new[] { "ServiceTypeID" });
            DropIndex("dbo.SOEEItems", new[] { "SOEEID" });
            DropIndex("dbo.SOEEs", new[] { "ProjectID" });
            DropIndex("dbo.SOEEHistories", new[] { "SOEEID" });
            DropIndex("dbo.ServiceTypeSlabMaps", new[] { "SlabID" });
            DropIndex("dbo.ServiceTypeSlabMaps", new[] { "ServiceTypeID" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.ProjectServiceMaps", new[] { "ServiceTypeID" });
            DropIndex("dbo.ProjectServiceMaps", new[] { "ProjectID" });
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.SOEEItems");
            DropTable("dbo.SOEEs");
            DropTable("dbo.SOEEHistories");
            DropTable("dbo.ServiceTypeSlabMaps");
            DropTable("dbo.ServiceCostSlabs");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Roles");
            DropTable("dbo.ServiceTypes");
            DropTable("dbo.ProjectServiceMaps");
            DropTable("dbo.Projects");
        }
    }
}
