using EVDealerSales.DataAccess.Entities;

namespace EVDealerSales.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Vehicle> Vehicles { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<Delivery> Deliveries { get; }
        IGenericRepository<TestDrive> TestDrives { get; }
        IGenericRepository<Feedback> Feedbacks { get; }
        Task<int> SaveChangesAsync();
    }
}
