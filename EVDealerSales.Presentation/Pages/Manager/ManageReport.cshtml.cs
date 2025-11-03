using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using EVDealerSales.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Manager
{
    [Authorize(Roles = "DealerManager,DealerStaff")]
    public class ManageReportModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<ManageReportModel> _logger;

        public ManageReportModel(
            IOrderService orderService,
            IFeedbackService feedbackService,
            ILogger<ManageReportModel> logger)
        {
            _orderService = orderService;
            _feedbackService = feedbackService;
            _logger = logger;
        }

        // Summary Metrics
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalTestDrives { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double TestDriveConversionRate { get; set; }
        public double OnTimeDeliveryRate { get; set; }

        // Counts
        public int PendingOrders { get; set; }
        public int ActiveDeliveries { get; set; }
        public int PendingFeedbacks { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int NewCustomersThisMonth { get; set; }

        // Feedback Statistics
        public int TotalFeedbacks { get; set; }
        public int ResolvedFeedbacks { get; set; }
        public double FeedbackResolutionRate { get; set; }

        // Chart Data
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new();
        public Dictionary<DeliveryStatus, int> DeliveriesByStatus { get; set; } = new();
        public List<VehicleSalesDto> TopSellingVehicles { get; set; } = new();
        public List<VehicleStockDto> LowStockVehicles { get; set; } = new();
        public List<VehicleStockDto> OutOfStockVehicles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Fetch all statistics
                TotalRevenue = await _orderService.GetTotalRevenueAsync();
                TotalOrders = await _orderService.GetTotalOrdersCountAsync();
                TotalCustomers = await _orderService.GetTotalCustomersCountAsync();
                TotalTestDrives = await _orderService.GetTotalTestDrivesCountAsync();
                AverageOrderValue = await _orderService.GetAverageOrderValueAsync();
                TestDriveConversionRate = await _orderService.GetTestDriveConversionRateAsync();
                OnTimeDeliveryRate = await _orderService.GetOnTimeDeliveryRateAsync();
                NewCustomersThisMonth = await _orderService.GetNewCustomersCountAsync(startOfMonth);

                // Feedback Statistics
                TotalFeedbacks = await _orderService.GetTotalFeedbacksCountAsync();
                PendingFeedbacks = await _orderService.GetPendingFeedbacksCountAsync();
                ResolvedFeedbacks = await _orderService.GetResolvedFeedbacksCountAsync();
                FeedbackResolutionRate = await _orderService.GetFeedbackResolutionRateAsync();

                // Chart data
                MonthlyRevenue = await _orderService.GetMonthlyRevenueAsync(6);
                OrdersByStatus = await _orderService.GetOrdersByStatusAsync();
                DeliveriesByStatus = await _orderService.GetDeliveriesByStatusAsync();
                TopSellingVehicles = await _orderService.GetTopSellingVehiclesAsync(5);
                LowStockVehicles = await _orderService.GetLowStockVehiclesAsync(5);
                OutOfStockVehicles = await _orderService.GetOutOfStockVehiclesAsync();

                // Calculate counts
                PendingOrders = OrdersByStatus.GetValueOrDefault(OrderStatus.Pending, 0);
                ActiveDeliveries = DeliveriesByStatus.GetValueOrDefault(DeliveryStatus.Scheduled, 0) +
                                   DeliveriesByStatus.GetValueOrDefault(DeliveryStatus.InTransit, 0);

                LowStockCount = LowStockVehicles.Count;
                OutOfStockCount = OutOfStockVehicles.Count;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "Error loading dashboard data. Please try again.";
                return Page();
            }
        }
    }
}
