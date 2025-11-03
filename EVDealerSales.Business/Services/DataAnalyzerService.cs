using EVDealerSales.Business.Interfaces;
using EVDealerSales.DataAccess;
using EVDealerSales.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVDealerSales.Business.Services
{
    public class DataAnalyzerService : IDataAnalyzerService
    {
        private readonly EVDealerSalesDbContext _dbContext;
        public DataAnalyzerService(EVDealerSalesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Vehicle>> AnalyzeVehiclesAsync()
        {
            // Return active, non-deleted vehicles with related navigation properties loaded
            var vehicles = await _dbContext.Vehicles
                .Where(v => !v.IsDeleted)
                // include common navigation properties useful for analysis
                .Include(v => v.OrderItems).ThenInclude(oi => oi.Order)
                .Include(v => v.TestDrives)
                // use split queries to avoid cartesian product issues when including multiple collections
                .AsSplitQuery()
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return vehicles;
        }

        public async Task<IReadOnlyList<Order>> AnalyzeSalesAsync()
        {
            // Return orders with related items, invoices, payments and delivery loaded
            var orders = await _dbContext.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.Items).ThenInclude(oi => oi.Vehicle)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders;
        }

        public async Task<IReadOnlyList<Feedback>> AnalyzeFeedbacksAsync()
        {
            // Return feedbacks with customer, order and resolution info
            var feedbacks = await _dbContext.Feedbacks
                .Where(f => !f.IsDeleted)
                .Include(f => f.Customer)
                .Include(f => f.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                .Include(f => f.Creator)
                .Include(f => f.Resolver)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return feedbacks;
        }
    }
}
