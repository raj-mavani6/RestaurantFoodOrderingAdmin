using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingAdmin.Models;

namespace RestaurantFoodOrderingAdmin.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CustomerCoupon> CustomerCoupons { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<FoodItem>()
                .HasOne(f => f.Category)
                .WithMany()
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany()
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.FoodItem)
                .WithMany()
                .HasForeignKey(oi => oi.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.FoodItem)
                .WithMany()
                .HasForeignKey(r => r.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Coupon relationships
            modelBuilder.Entity<CustomerCoupon>()
                .HasOne(cc => cc.Customer)
                .WithMany()
                .HasForeignKey(cc => cc.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerCoupon>()
                .HasOne(cc => cc.Coupon)
                .WithMany(c => c.CustomerCoupons)
                .HasForeignKey(cc => cc.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerCoupon>()
                .HasOne(cc => cc.Order)
                .WithMany()
                .HasForeignKey(cc => cc.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Combo relationships
            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Combo)
                .WithMany(c => c.ComboItems)
                .HasForeignKey(ci => ci.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.FoodItem)
                .WithMany()
                .HasForeignKey(ci => ci.FoodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed default admin
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    AdminId = 1,
                    Username = "admin",
                    Password = "admin123",
                    FullName = "System Administrator",
                    Email = "admin@tastybites.com",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                }
            );
        }
    }
}
