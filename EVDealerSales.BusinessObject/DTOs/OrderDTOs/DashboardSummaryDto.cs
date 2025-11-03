namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveDeliveries { get; set; }
        public int PendingFeedbacks { get; set; }
        public int LowStockVehicles { get; set; }
        public int OutOfStockVehicles { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
    }
}
