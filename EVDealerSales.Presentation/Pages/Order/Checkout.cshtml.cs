using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Order
{
    [Authorize(Roles = "Customer")]
    public class CheckoutModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public CheckoutModel(IOrderService orderService, IPaymentService paymentService, IConfiguration configuration)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _configuration = configuration;
        }

        public BusinessObject.DTOs.OrderDTOs.OrderResponseDto Order { get; set; } = null!;
        public string StripePublishableKey { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid orderId)
        {
            try
            {
                Order = await _orderService.GetOrderByIdAsync(orderId);
                
                if (Order == null)
                {
                    ErrorMessage = "Order not found";
                    return RedirectToPage("/Order/MyOrders");
                }

                if (Order.PaymentStatus == BusinessObject.Enums.PaymentStatus.Paid)
                {
                    return RedirectToPage("/Order/OrderDetail", new { id = orderId });
                }

                var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(orderId);
                return Redirect(checkoutUrl);
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = ex.Message;
                return RedirectToPage("/Order/MyOrders");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while preparing checkout";
                return RedirectToPage("/Order/OrderDetail", new { id = orderId });
            }
        }
    }
}
