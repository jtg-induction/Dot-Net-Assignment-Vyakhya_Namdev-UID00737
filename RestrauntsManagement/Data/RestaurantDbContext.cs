using DotNetRestaurantManagement.Models.Entities;
using System.Data.Common;
using System.Data.Entity;

namespace DotNetRestaurantManagement.Data
{
    /// Represents the Entity Framework database context for the restaurant management system.
    public class RestaurantDbContext : DbContext
    {
        /// Initializes the database context using the configured connection string.
        public RestaurantDbContext()
            : base("name=RestaurantDbContext")
        {
        }

        /// <summary>
        /// Initializes the database context using a provided database connection.
        /// This constructor is mainly used for testing with a separate test database.
        /// </summary>
        /// <param name="connection">The database connection to use.</param>
        public RestaurantDbContext(DbConnection connection)
            : base(connection, true)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderedItems { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        /// <summary>
        /// Configures entity relationships and database constraints.
        /// </summary>
        /// <param name="modelBuilder">Builder used to configure the entity model.</param>
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