namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddServiceChargePercentToSOEEItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SOEEItems", "ServiceChargePercent", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SOEEItems", "ServiceChargePercent");
        }
    }
}
