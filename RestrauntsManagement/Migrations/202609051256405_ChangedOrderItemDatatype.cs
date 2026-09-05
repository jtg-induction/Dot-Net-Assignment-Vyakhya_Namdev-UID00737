namespace DotNetRestaurantManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedOrderItemDatatype : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RefreshTokens", "UserId", "dbo.Users");
            DropIndex("dbo.Users", "Users_Phonenumber");
            DropIndex("dbo.RefreshTokens", new[] { "UserId" });
            DropColumn("dbo.Users", "TokenVersion");
            DropTable("dbo.RefreshTokens");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RefreshTokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        Token = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        ExpiresAt = c.DateTime(nullable: false),
                        RevokedAt = c.DateTime(),
                        IsRevoked = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "TokenVersion", c => c.Long(nullable: false));
            CreateIndex("dbo.RefreshTokens", "UserId");
            CreateIndex("dbo.Users", "PhoneNumber", unique: true, name: "Users_Phonenumber");
            AddForeignKey("dbo.RefreshTokens", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
