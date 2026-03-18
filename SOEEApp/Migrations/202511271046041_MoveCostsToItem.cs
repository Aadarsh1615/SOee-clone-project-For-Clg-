namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveCostsToItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SOEEs", "TotalBasicAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEs", "TotalServiceCharge", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEs", "TotalTaxAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEItems", "ServiceCharge", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEItems", "CGST", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEItems", "SGST", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.SOEEItems", "Unit", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.SOEEs", "BasicAmount");
            DropColumn("dbo.SOEEs", "ServiceCharge");
            DropColumn("dbo.SOEEs", "CGST");
            DropColumn("dbo.SOEEs", "SGST");
            DropColumn("dbo.SOEEs", "TaxAmount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SOEEs", "TaxAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEs", "SGST", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEs", "CGST", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEs", "ServiceCharge", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEs", "BasicAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.SOEEItems", "Unit", c => c.String());
            DropColumn("dbo.SOEEItems", "SGST");
            DropColumn("dbo.SOEEItems", "CGST");
            DropColumn("dbo.SOEEItems", "ServiceCharge");
            DropColumn("dbo.SOEEs", "TotalTaxAmount");
            DropColumn("dbo.SOEEs", "TotalServiceCharge");
            DropColumn("dbo.SOEEs", "TotalBasicAmount");
        }
    }
}
