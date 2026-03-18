namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSOEEForCustomer1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SOEEs", "SOEERaiseDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.SOEEs", "Date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SOEEs", "Date", c => c.DateTime(nullable: false));
            DropColumn("dbo.SOEEs", "SOEERaiseDate");
        }
    }
}
