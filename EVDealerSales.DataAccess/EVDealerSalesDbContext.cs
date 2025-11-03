using EVDealerSales.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVDealerSales.DataAccess
{
    public class EVDealerSalesDbContext : DbContext
    {
        public EVDealerSalesDbContext() { }

        public EVDealerSalesDbContext(DbContextOptions<EVDealerSalesDbContext> options)
            : base(options)
        {
        }

        // -------------------- DbSets --------------------
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<TestDrive> TestDrives { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------- RELATIONSHIPS --------------------

            // -------------------- TestDrive --------------------
            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Customer)
                .WithMany(u => u.TestDrivesAsCustomer)
                .HasForeignKey(td => td.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Vehicle)
                .WithMany(v => v.TestDrives)
                .HasForeignKey(td => td.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Staff)
                .WithMany(u => u.TestDrivesAsStaff)
                .HasForeignKey(td => td.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // -------------------- Order --------------------
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.OrdersAsCustomer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Staff)
                .WithMany(u => u.OrdersAsStaff)
                .HasForeignKey(o => o.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Vehicle)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------- Invoice --------------------
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Invoices)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(u => u.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------- Payment --------------------
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------- Delivery --------------------
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithOne(o => o.Delivery)
                .HasForeignKey<Delivery>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------- Feedback --------------------
            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Customer)
                .WithMany(u => u.FeedbacksGiven)
                .HasForeignKey(fb => fb.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Order)
                .WithMany(o => o.Feedbacks)
                .HasForeignKey(fb => fb.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Creator)
                .WithMany(u => u.CreatedFeedbacks)
                .HasForeignKey(fb => fb.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Resolver)
                .WithMany(u => u.ResolvedFeedbacks)
                .HasForeignKey(fb => fb.ResolvedBy)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
