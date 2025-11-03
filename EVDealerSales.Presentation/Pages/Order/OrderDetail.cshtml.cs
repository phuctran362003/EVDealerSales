using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using EVDealerSales.BusinessObject.DTOs.DeliveryDTOs;
using EVDealerSales.BusinessObject.DTOs.FeedbackDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Order
{
    [Authorize(Roles = "Customer,DealerStaff")]
    public class OrderDetailModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IDeliveryService _deliveryService;
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<OrderDetailModel> _logger;

        public OrderDetailModel(
            IOrderService orderService,
            IDeliveryService deliveryService,
            IFeedbackService feedbackService,
            ILogger<OrderDetailModel> logger)
        {
            _orderService = orderService;
            _deliveryService = deliveryService;
            _feedbackService = feedbackService;
            _logger = logger;
        }

        public OrderResponseDto Order { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Order = await _orderService.GetOrderByIdAsync(id);

                if (Order == null)
                {
                    ErrorMessage = "Order not found";
                    return Page();
                }

                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = ex.Message;
                return RedirectToPage("/Order/MyOrders");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading order details";
                return RedirectToPage("/Order/MyOrders");
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                await _orderService.CancelOrderAsync(id, request?.Reason);
                return new JsonResult(new { success = true, message = "Order cancelled successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 403 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostRequestDeliveryAsync([FromBody] RequestDeliveryRequest request)
        {
            try
            {
                _logger.LogInformation("Receiving delivery request for order {OrderId}", request.OrderId);

                if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                {
                    return new JsonResult(new { success = false, message = "Shipping address is required" }) { StatusCode = 400 };
                }

                var createDeliveryDto = new CreateDeliveryRequestDto
                {
                    OrderId = request.OrderId,
                    ShippingAddress = request.ShippingAddress,
                    Notes = request.Notes
                };

                var delivery = await _deliveryService.RequestDeliveryAsync(createDeliveryDto);

                _logger.LogInformation("Delivery request created successfully: {DeliveryId}", delivery.Id);

                return new JsonResult(new
                {
                    success = true,
                    message = "Delivery request submitted successfully",
                    deliveryId = delivery.Id
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delivery request attempt");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 403 };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delivery request");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery request");
                return new JsonResult(new { success = false, message = "An error occurred while processing your request" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostCancelDeliveryAsync([FromBody] CancelDeliveryRequest request)
        {
            try
            {
                _logger.LogInformation("Cancelling delivery {DeliveryId}", request.DeliveryId);

                var result = await _deliveryService.CancelDeliveryAsync(request.DeliveryId);

                if (result == null)
                {
                    return new JsonResult(new { success = false, message = "Delivery not found" }) { StatusCode = 404 };
                }

                _logger.LogInformation("Delivery cancelled successfully: {DeliveryId}", request.DeliveryId);

                return new JsonResult(new
                {
                    success = true,
                    message = "Delivery request cancelled successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delivery cancellation attempt");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 403 };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delivery cancellation");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling delivery");
                return new JsonResult(new { success = false, message = "An error occurred while cancelling the delivery" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostSubmitFeedbackAsync([FromBody] SubmitFeedbackRequest request)
        {
            try
            {
                _logger.LogInformation("Receiving feedback for order {OrderId}", request.OrderId);

                if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10)
                {
                    return new JsonResult(new { success = false, message = "Feedback content must be at least 10 characters" }) { StatusCode = 400 };
                }

                var createFeedbackDto = new CreateFeedbackRequestDto
                {
                    OrderId = request.OrderId,
                    Content = request.Content
                };

                var feedback = await _feedbackService.CreateFeedbackAsync(createFeedbackDto);

                _logger.LogInformation("Feedback created successfully: {FeedbackId}", feedback.Id);

                return new JsonResult(new
                {
                    success = true,
                    message = "Thank you for your feedback!",
                    feedbackId = feedback.Id
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized feedback submission attempt");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 403 };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid feedback submission");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback");
                return new JsonResult(new { success = false, message = "An error occurred while submitting your feedback" }) { StatusCode = 500 };
            }
        }
    }

    public class CancelOrderRequest
    {
        public string? Reason { get; set; }
    }

    public class RequestDeliveryRequest
    {
        public Guid OrderId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CancelDeliveryRequest
    {
        public Guid DeliveryId { get; set; }
    }

    public class SubmitFeedbackRequest
    {
        public Guid OrderId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}