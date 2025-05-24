using AgriBuy.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.EntityFramework
{
    public class DefaultDbContext : DbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
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

            // Table mappings
            ConfigureTableMappings(modelBuilder);

            // Unique index on User Email
            ConfigureUserEmailIndex(modelBuilder);

            // Configure relationships
            ConfigureRelationships(modelBuilder);

            // Configure decimal precision for monetary fields
            ConfigureDecimalPrecision(modelBuilder);

            // Configure string length and required fields
            ConfigureStringLengthAndRequiredFields(modelBuilder);
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
            // Order - OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Store - Products
            modelBuilder.Entity<Store>()
                .HasMany(s => s.Products)
                .WithOne(p => p.Store)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Store - User (Seller)
            modelBuilder.Entity<Store>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShoppingCart - User
            modelBuilder.Entity<ShoppingCart>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShoppingCart - Product
            modelBuilder.Entity<ShoppingCart>()
                .HasOne(sc => sc.Product)
                .WithMany()
                .HasForeignKey(sc => sc.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoginInfo - User
            modelBuilder.Entity<LoginInfo>()
                .HasOne(li => li.User)
                .WithMany()
                .HasForeignKey(li => li.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order - User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - Store
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
