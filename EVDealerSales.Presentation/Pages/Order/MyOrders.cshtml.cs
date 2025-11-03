using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Order
{
    [Authorize]
    public class MyOrdersModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<MyOrdersModel> _logger;

        public MyOrdersModel(IOrderService orderService, ILogger<MyOrdersModel> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public Pagination<OrderResponseDto> Orders { get; set; } = new Pagination<OrderResponseDto>(new List<OrderResponseDto>(), 0, 1, 10);
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Orders = await _orderService.GetMyOrdersAsync(PageNumber, PageSize);

                // Check for success message from TempData
                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
                }

                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to orders");
                return RedirectToPage("/Auth/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user's orders");
                ErrorMessage = "An error occurred while loading your orders. Please try again.";
                return Page();
            }
        }
    }
}
