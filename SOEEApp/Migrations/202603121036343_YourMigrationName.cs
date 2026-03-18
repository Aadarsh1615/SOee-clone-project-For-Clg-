namespace SOEEApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YourMigrationName : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentID = c.Int(nullable: false, identity: true),
                        SOEEID = c.Int(nullable: false),
                        UTRNo = c.String(nullable: false),
                        AmountReceived = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentDate = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        ConfirmedBy = c.String(),
                        ConfirmedOn = c.DateTime(nullable: false),
                        IsReconciled = c.Boolean(nullable: false),
                        ReconciledOn = c.DateTime(),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.PaymentID)
                .ForeignKey("dbo.SOEEs", t => t.SOEEID, cascadeDelete: true)
                .Index(t => t.SOEEID);
            
            CreateTable(
                "dbo.ReceiptVouchers",
                c => new
                    {
                        VoucherID = c.Int(nullable: false),
                        SOEEID = c.Int(nullable: false),
                        VoucherNumber = c.String(nullable: false),
                        VoucherDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        GeneratedBy = c.String(),
                        GeneratedOn = c.DateTime(nullable: false),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.VoucherID)
                .ForeignKey("dbo.SOEEs", t => t.VoucherID)
                .Index(t => t.VoucherID)
                .Index(t => t.SOEEID, unique: true, name: "IX_ReceiptVoucher_SOEEID");
            
            AddColumn("dbo.Customers", "Email", c => c.String());
            AddColumn("dbo.Customers", "ContactPerson", c => c.String());
            AddColumn("dbo.Customers", "PhoneNumber", c => c.String());
            AddColumn("dbo.SOEEs", "IsESigned", c => c.Boolean(nullable: false));
            AddColumn("dbo.SOEEs", "ESignedBy", c => c.String());
            AddColumn("dbo.SOEEs", "ESignedOn", c => c.DateTime());
            AddColumn("dbo.SOEEs", "DispatchEmail", c => c.String());
            AddColumn("dbo.SOEEs", "DispatchedBy", c => c.String());
            AddColumn("dbo.SOEEs", "DispatchedOn", c => c.DateTime());
            AddColumn("dbo.SOEEs", "ClosureRemarks", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "SOEEID", "dbo.SOEEs");
            DropForeignKey("dbo.ReceiptVouchers", "VoucherID", "dbo.SOEEs");
            DropIndex("dbo.ReceiptVouchers", "IX_ReceiptVoucher_SOEEID");
            DropIndex("dbo.ReceiptVouchers", new[] { "VoucherID" });
            DropIndex("dbo.Payments", new[] { "SOEEID" });
            DropColumn("dbo.SOEEs", "ClosureRemarks");
            DropColumn("dbo.SOEEs", "DispatchedOn");
            DropColumn("dbo.SOEEs", "DispatchedBy");
            DropColumn("dbo.SOEEs", "DispatchEmail");
            DropColumn("dbo.SOEEs", "ESignedOn");
            DropColumn("dbo.SOEEs", "ESignedBy");
            DropColumn("dbo.SOEEs", "IsESigned");
            DropColumn("dbo.Customers", "PhoneNumber");
            DropColumn("dbo.Customers", "ContactPerson");
            DropColumn("dbo.Customers", "Email");
            DropTable("dbo.ReceiptVouchers");
            DropTable("dbo.Payments");
        }
    }
}
