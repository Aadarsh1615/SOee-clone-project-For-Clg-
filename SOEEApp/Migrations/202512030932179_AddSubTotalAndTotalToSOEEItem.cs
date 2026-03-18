namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubTotalAndTotalToSOEEItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SOEEItems", "SubTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SOEEItems", "Total", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SOEEItems", "Total");
            DropColumn("dbo.SOEEItems", "SubTotal");
        }
    }
}
