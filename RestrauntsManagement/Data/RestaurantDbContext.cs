using System.Data.Entity;
using DotNetRestaurantManagement.Models.Entities;

namespace DotNetRestaurantManagement.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext() : base("name=RestaurantDbContext")
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderedItems { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAddress>()
                .HasKey(x => new
                {
                    x.UserId,
                    x.AddressId
                });

            modelBuilder.Entity<UserAddress>()
                .HasRequired(x => x.User)
                .WithMany(x => x.UserAddresses)
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserAddress>()
                .HasRequired(x => x.Address)
                .WithMany(x => x.UserAddresses)
                .HasForeignKey(x => x.AddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Restaurant>()
                .HasRequired(x => x.Owner)
                .WithMany(x => x.Restaurants)
                .HasForeignKey(x => x.OwnerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Restaurant>()
                .HasRequired(x => x.Address)
                .WithMany(x => x.Restaurants)
                .HasForeignKey(x => x.AddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MenuItem>()
                .HasRequired(x => x.Restaurant)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.RestaurantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(x => x.Restaurant)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.RestaurantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(x => x.DeliveryAddress)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.DeliveryAddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(x => x.Order)
                .WithMany(x => x.OrderedItems)
                .HasForeignKey(x => x.OrderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(x => x.MenuItem)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.MenuItemId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
