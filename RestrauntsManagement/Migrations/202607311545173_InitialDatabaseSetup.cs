namespace DotNetRestaurantManagement.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialDatabaseSetup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    HouseNumber = c.String(nullable: false, maxLength: 50),
                    StreetAddress = c.String(nullable: false, maxLength: 255),
                    City = c.String(nullable: false, maxLength: 100),
                    State = c.String(nullable: false, maxLength: 100),
                    PinCode = c.String(nullable: false, maxLength: 10),
                    Country = c.String(nullable: false, maxLength: 100),
                    AddressType = c.Int(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Orders",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    RestaurantId = c.Int(nullable: false),
                    CustomerId = c.Long(nullable: false),
                    DeliveryAddressId = c.Long(nullable: false),
                    TotalItems = c.Long(nullable: false),
                    TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Int(nullable: false),
                    PlacedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(nullable: false),
                    DeliveredAt = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CustomerId)
                .ForeignKey("dbo.Addresses", t => t.DeliveryAddressId)
                .ForeignKey("dbo.Restaurants", t => t.RestaurantId)
                .Index(t => t.RestaurantId)
                .Index(t => t.CustomerId)
                .Index(t => t.DeliveryAddressId);

            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    Email = c.String(nullable: false, maxLength: 255),
                    Name = c.String(nullable: false, maxLength: 100),
                    Password = c.String(nullable: false, maxLength: 255),
                    Role = c.Int(nullable: false),
                    Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                    IsActive = c.Boolean(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true, name: "Users_Email");

            CreateTable(
                "dbo.Restaurants",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OwnerId = c.Long(nullable: false),
                    Name = c.String(nullable: false, maxLength: 150),
                    AddressId = c.Long(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                    Cuisine = c.Int(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.AddressId)
                .ForeignKey("dbo.Users", t => t.OwnerId)
                .Index(t => t.OwnerId)
                .Index(t => t.AddressId);

            CreateTable(
                "dbo.MenuItems",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RestaurantId = c.Int(nullable: false),
                    Name = c.String(nullable: false, maxLength: 150),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    PreparationTime = c.Int(nullable: false),
                    Category = c.Int(nullable: false),
                    QuantityAvailable = c.Int(nullable: false),
                    CreatedAt = c.DateTime(nullable: false),
                    UpdatedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Restaurants", t => t.RestaurantId)
                .Index(t => t.RestaurantId);

            CreateTable(
                "dbo.OrderedItems",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MenuItemId = c.Int(nullable: false),
                    OrderId = c.Long(nullable: false),
                    Quantity = c.Int(nullable: false),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuItems", t => t.MenuItemId)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .Index(t => t.MenuItemId)
                .Index(t => t.OrderId);

            CreateTable(
                "dbo.UserAddress",
                c => new
                {
                    UserId = c.Long(nullable: false),
                    AddressId = c.Long(nullable: false),
                })
                .PrimaryKey(t => new { t.UserId, t.AddressId })
                .ForeignKey("dbo.Addresses", t => t.AddressId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.AddressId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Orders", "RestaurantId", "dbo.Restaurants");
            DropForeignKey("dbo.Orders", "DeliveryAddressId", "dbo.Addresses");
            DropForeignKey("dbo.Orders", "CustomerId", "dbo.Users");
            DropForeignKey("dbo.UserAddress", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserAddress", "AddressId", "dbo.Addresses");
            DropForeignKey("dbo.Restaurants", "OwnerId", "dbo.Users");
            DropForeignKey("dbo.MenuItems", "RestaurantId", "dbo.Restaurants");
            DropForeignKey("dbo.OrderedItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderedItems", "MenuItemId", "dbo.MenuItems");
            DropForeignKey("dbo.Restaurants", "AddressId", "dbo.Addresses");
            DropIndex("dbo.UserAddress", new[] { "AddressId" });
            DropIndex("dbo.UserAddress", new[] { "UserId" });
            DropIndex("dbo.OrderedItems", new[] { "OrderId" });
            DropIndex("dbo.OrderedItems", new[] { "MenuItemId" });
            DropIndex("dbo.MenuItems", new[] { "RestaurantId" });
            DropIndex("dbo.Restaurants", new[] { "AddressId" });
            DropIndex("dbo.Restaurants", new[] { "OwnerId" });
            DropIndex("dbo.Users", "Users_Email");
            DropIndex("dbo.Orders", new[] { "DeliveryAddressId" });
            DropIndex("dbo.Orders", new[] { "CustomerId" });
            DropIndex("dbo.Orders", new[] { "RestaurantId" });
            DropTable("dbo.UserAddress");
            DropTable("dbo.OrderedItems");
            DropTable("dbo.MenuItems");
            DropTable("dbo.Restaurants");
            DropTable("dbo.Users");
            DropTable("dbo.Orders");
            DropTable("dbo.Addresses");
        }
    }
}
