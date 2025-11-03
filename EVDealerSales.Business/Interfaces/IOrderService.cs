using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.Business.Interfaces
{
    public interface IOrderService
    {
        // Customer operations
        Task<Guid> CreateOrderAsync(CreateOrderRequestDto request);
        Task<Pagination<OrderResponseDto>> GetMyOrdersAsync(int pageNumber = 1, int pageSize = 10);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId);
        Task<bool> CancelOrderAsync(Guid orderId, string? reason = null);

        // Staff operations
        Task<Pagination<OrderResponseDto>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10, OrderFilterDto? filter = null);
        Task<OrderResponseDto?> AssignStaffToOrderAsync(Guid orderId, Guid staffId);
        Task<OrderResponseDto?> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request);

        // ========= Statistics =========

        // General Statistics
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetTotalOrdersCountAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Order Statistics
        Task<Dictionary<OrderStatus, int>> GetOrdersByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months = 6);
        Task<List<VehicleSalesDto>> GetTopSellingVehiclesAsync(int topCount = 5, DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Delivery Statistics
        Task<Dictionary<DeliveryStatus, int>> GetDeliveriesByStatusAsync();
        Task<double> GetOnTimeDeliveryRateAsync();

        // Customer Statistics
        Task<int> GetTotalCustomersCountAsync();
        Task<int> GetNewCustomersCountAsync(DateTime? fromDate = null);

        // Test Drive Statistics
        Task<int> GetTotalTestDrivesCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<double> GetTestDriveConversionRateAsync();

        // Feedback Statistics
        Task<int> GetTotalFeedbacksCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetPendingFeedbacksCountAsync();
        Task<int> GetResolvedFeedbacksCountAsync();
        Task<double> GetFeedbackResolutionRateAsync();

        // Inventory Alerts
        Task<List<VehicleStockDto>> GetLowStockVehiclesAsync(int threshold = 5);
        Task<List<VehicleStockDto>> GetOutOfStockVehiclesAsync();
    }
}
