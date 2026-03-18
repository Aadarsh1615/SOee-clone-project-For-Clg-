namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeletedToSOEEItems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SOEEItems", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SOEEItems", "IsDeleted");
        }
    }
}
