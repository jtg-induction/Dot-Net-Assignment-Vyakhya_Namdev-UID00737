namespace DotNetRestaurantManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UpdateEntityValidationAndModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "PhoneNumber", c => c.String(nullable: false, maxLength: 10));
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false, maxLength: 255));
        }

        public override void Down()
        {
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.Users", "PhoneNumber");
        }
    }
}
