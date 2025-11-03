using EVDealerSales.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Order
{
    [Authorize(Roles = "Customer")]
    public class ConfirmPaymentModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ConfirmPaymentModel> _logger;

        public ConfirmPaymentModel(IPaymentService paymentService, ILogger<ConfirmPaymentModel> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostConfirmAsync([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.PaymentIntentId))
                {
                    return new JsonResult(new { success = false, message = "Payment Intent ID is required" })
                    {
                        StatusCode = 400
                    };
                }

                var order = await _paymentService.ConfirmPaymentAsync(request.PaymentIntentId);
                
                return new JsonResult(new 
                { 
                    success = true, 
                    message = "Payment confirmed successfully",
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for PaymentIntentId: {PaymentIntentId}", 
                    request?.PaymentIntentId);
                
                return new JsonResult(new { success = false, message = ex.Message })
                {
                    StatusCode = 400
                };
            }
        }
    }

    public class ConfirmPaymentRequest
    {
        public string? PaymentIntentId { get; set; }
    }
}
