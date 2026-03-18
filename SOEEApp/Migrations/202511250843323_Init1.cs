namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "OICUserId", c => c.String(maxLength: 128));
            AddColumn("dbo.Users", "Name", c => c.String());
            CreateIndex("dbo.Projects", "OICUserId");
            AddForeignKey("dbo.Projects", "OICUserId", "dbo.Users", "Id");
            DropColumn("dbo.Projects", "OIC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "OIC", c => c.String());
            DropForeignKey("dbo.Projects", "OICUserId", "dbo.Users");
            DropIndex("dbo.Projects", new[] { "OICUserId" });
            DropColumn("dbo.Users", "Name");
            DropColumn("dbo.Projects", "OICUserId");
        }
    }
}
