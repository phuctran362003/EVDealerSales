using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Order
{
    [Authorize(Roles = "Customer")]
    public class CreateOrderModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IVehicleService _vehicleService;
        private readonly IPaymentService _paymentService;

        public CreateOrderModel(IOrderService orderService, IVehicleService vehicleService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _vehicleService = vehicleService;
            _paymentService = paymentService;
        }

        public VehicleResponseDto Vehicle { get; set; } = null!;

        [BindProperty]
        public string? Notes { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid vehicleId)
        {
            try
            {
                Vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);

                if (Vehicle == null)
                {
                    ErrorMessage = "Vehicle not found";
                    return RedirectToPage("/Vehicle/BrowseVehicles");
                }

                if (!Vehicle.IsActive)
                {
                    ErrorMessage = "This vehicle is currently unavailable";
                    return RedirectToPage("/Vehicle/DetailVehicles", new { id = vehicleId });
                }

                if (Vehicle.Stock <= 0)
                {
                    TempData["ErrorMessage"] = "This vehicle is out of stock and cannot be purchased at this time";
                    return RedirectToPage("/Vehicle/DetailVehicles", new { id = vehicleId });
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading vehicle information";
                return RedirectToPage("/Vehicle/BrowseVehicles");
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid vehicleId)
        {
            try
            {
                Vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);

                if (Vehicle == null || !Vehicle.IsActive)
                {
                    ErrorMessage = "Vehicle is not available for purchase";
                    return Page();
                }

                if (Vehicle.Stock <= 0)
                {
                    ErrorMessage = "This vehicle is out of stock and cannot be purchased at this time";
                    return Page();
                }

                var createOrderDto = new CreateOrderRequestDto
                {
                    VehicleId = vehicleId,
                    Notes = Notes
                };

                var order = await _orderService.CreateOrderAsync(createOrderDto);

                // Create Stripe Checkout Session and redirect
                var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(order);
                return Redirect(checkoutUrl);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                return Page();
            }
        }
    }
}
