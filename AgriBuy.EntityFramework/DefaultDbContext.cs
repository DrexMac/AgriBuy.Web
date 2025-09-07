using AgriBuy.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.EntityFramework
{
    public class DefaultDbContext : DbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<LoginInfo> LoginInfos { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTableMappings(modelBuilder);
            ConfigureUserEmailIndex(modelBuilder);
            ConfigureRelationships(modelBuilder);
            ConfigureDecimalPrecision(modelBuilder);
            ConfigureStringLengthAndRequiredFields(modelBuilder);

            // --- Fixed GUIDs for seed data ---
            var userId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var storeId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var productId1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var productId2 = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // ✅ Seed User to satisfy Store FK constraint
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = userId1,
                FirstName = "Agri",
                LastName = "Owner",
                EmailAddress = "owner@agribuy.com"
            });

            modelBuilder.Entity<Store>().HasData(new Store
            {
                Id = storeId1,
                Name = "AgriBuy Store",
                UserId = userId1 // ✅ Correct FK
            });

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = productId1,
                    Name = "Organic Apples",
                    UnitPrice = 5.50m,
                    UnitOfMeasure = "Kg",
                    StoreId = storeId1
                },
                new Product
                {
                    Id = productId2,
                    Name = "Fresh Milk",
                    UnitPrice = 2.20m,
                    UnitOfMeasure = "L",
                    StoreId = storeId1
                }
            );
        }

        private void ConfigureTableMappings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<LoginInfo>().ToTable("LoginInfos");
            modelBuilder.Entity<Store>().ToTable("Stores");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<ShoppingCart>().ToTable("ShoppingCarts");
        }

        private void ConfigureUserEmailIndex(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailAddress)
                .IsUnique();
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Products)
                .WithOne(p => p.Store)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Explicit Store ↔ User relationship
            modelBuilder.Entity<Store>()
                .HasOne(s => s.User)
                .WithMany(u => u.Stores)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShoppingCart>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShoppingCart>()
                .HasOne(sc => sc.Product)
                .WithMany()
                .HasForeignKey(sc => sc.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoginInfo>()
                .HasOne(li => li.User)
                .WithMany(u => u.LoginInfos)
                .HasForeignKey(li => li.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany()
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.ItemPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");
        }

        private void ConfigureStringLengthAndRequiredFields(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.EmailAddress)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Store>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(255);
        }
    }
}
