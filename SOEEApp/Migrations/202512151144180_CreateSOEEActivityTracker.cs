namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateSOEEActivityTracker : DbMigration
    {
        public override void Up()
        {
            // Remove all DropForeignKey / DropIndex lines related to SOEEHistories
            // They are not needed since the table is gone

            CreateTable(
                "dbo.SOEEActivities",
                c => new
                {
                    SOEEActivityID = c.Int(nullable: false, identity: true),
                    SOEEID = c.Int(nullable: false),
                    OldStatus = c.Int(nullable: false),
                    NewStatus = c.Int(nullable: false),
                    ActionBy = c.String(),
                    ActionOn = c.DateTime(nullable: false),
                    Remarks = c.String(),
                })
                .PrimaryKey(t => t.SOEEActivityID)
                .ForeignKey("dbo.SOEEs", t => t.SOEEID, cascadeDelete: true)
                .Index(t => t.SOEEID);

            // Remove this line:
            // DropTable("dbo.SOEEHistories");
        }


        public override void Down()
        {
            CreateTable(
                "dbo.SOEEHistories",
                c => new
                    {
                        SOEEHistoryID = c.Int(nullable: false, identity: true),
                        SOEEID = c.Int(nullable: false),
                        OldStatus = c.Int(nullable: false),
                        NewStatus = c.Int(nullable: false),
                        ActionBy = c.String(),
                        ActionOn = c.DateTime(nullable: false),
                        Remarks = c.String(),
                        SOEE_SOEEID = c.Int(),
                    })
                .PrimaryKey(t => t.SOEEHistoryID);
            
            DropForeignKey("dbo.SOEEActivities", "SOEEID", "dbo.SOEEs");
            DropIndex("dbo.SOEEActivities", new[] { "SOEEID" });
            DropTable("dbo.SOEEActivities");
            CreateIndex("dbo.SOEEHistories", "SOEE_SOEEID");
            AddForeignKey("dbo.SOEEHistories", "SOEE_SOEEID", "dbo.SOEEs", "SOEEID");
        }
    }
}
