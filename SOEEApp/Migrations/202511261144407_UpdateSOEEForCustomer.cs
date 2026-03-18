namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSOEEForCustomer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustomerID = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.CustomerID);
            
            AddColumn("dbo.SOEEs", "CustomerID", c => c.Int(nullable: false));
            AddColumn("dbo.SOEEs", "Content", c => c.String());
            AddColumn("dbo.SOEEs", "Reference", c => c.String());
            AddColumn("dbo.SOEEs", "CustomerName", c => c.String());
            AddColumn("dbo.SOEEItems", "DescriptionOfWork", c => c.String(nullable: false));
            CreateIndex("dbo.SOEEs", "CustomerID");
            AddForeignKey("dbo.SOEEs", "CustomerID", "dbo.Customers", "CustomerID", cascadeDelete: true);
            DropColumn("dbo.Projects", "Customer");
            DropColumn("dbo.SOEEs", "ClientName");
            DropColumn("dbo.SOEEs", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SOEEs", "Description", c => c.String());
            AddColumn("dbo.SOEEs", "ClientName", c => c.String());
            AddColumn("dbo.Projects", "Customer", c => c.String());
            DropForeignKey("dbo.SOEEs", "CustomerID", "dbo.Customers");
            DropIndex("dbo.SOEEs", new[] { "CustomerID" });
            DropColumn("dbo.SOEEItems", "DescriptionOfWork");
            DropColumn("dbo.SOEEs", "CustomerName");
            DropColumn("dbo.SOEEs", "Reference");
            DropColumn("dbo.SOEEs", "Content");
            DropColumn("dbo.SOEEs", "CustomerID");
            DropTable("dbo.Customers");
        }
    }
}
