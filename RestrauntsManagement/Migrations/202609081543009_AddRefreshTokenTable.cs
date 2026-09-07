namespace DotNetRestaurantManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRefreshTokenTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RefreshTokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        Token = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RefreshTokens", "UserId", "dbo.Users");
            DropIndex("dbo.RefreshTokens", new[] { "UserId" });
            DropTable("dbo.RefreshTokens");
        }
    }
}
