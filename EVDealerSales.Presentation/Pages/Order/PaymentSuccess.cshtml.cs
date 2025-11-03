using EVDealerSales.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EVDealerSales.Presentation.Pages.Order
{
    [Authorize(Roles = "Customer")]
    public class PaymentSuccessModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentSuccessModel> _logger;

        public PaymentSuccessModel(
            IPaymentService paymentService, 
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            ILogger<PaymentSuccessModel> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Stripe");
            _logger = logger;
        }

        [BindProperty(SupportsGet = true, Name = "session_id")]
        public string? SessionId { get; set; }

        public Guid OrderId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(SessionId))
            {
                return RedirectToPage("/Order/MyOrders");
            }

            try
            {
                _logger.LogInformation("Processing payment success for session: {SessionId}", SessionId);

                // Retrieve session from Stripe with expanded payment_intent
                var stripeSecretKey = _configuration["Stripe:SecretKey"];
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.stripe.com/v1/checkout/sessions/{SessionId}?expand[]=payment_intent");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stripeSecretKey);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Stripe session response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve Stripe session: {StatusCode}", response.StatusCode);
                    ErrorMessage = "Failed to verify payment";
                    return Page();
                }

                var session = JsonSerializer.Deserialize<StripeCheckoutSession>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (session == null)
                {
                    _logger.LogError("Failed to deserialize Stripe session");
                    ErrorMessage = "Invalid payment session";
                    return Page();
                }

                _logger.LogInformation("Session status: {Status}, Payment intent: {PaymentIntent}, Payment status: {PaymentStatus}",
                    session.Status, session.PaymentIntent?.Id ?? "null", session.PaymentStatus ?? "null");

                // Extract order ID from metadata first
                if (session.Metadata != null && session.Metadata.ContainsKey("order_id"))
                {
                    OrderId = Guid.Parse(session.Metadata["order_id"]);
                    _logger.LogInformation("Found order ID: {OrderId}", OrderId);
                }
                else
                {
                    _logger.LogError("Order ID not found in session metadata");
                    ErrorMessage = "Order information not found";
                    return Page();
                }

                // Check if payment_intent exists and payment is complete
                if (session.PaymentIntent == null || string.IsNullOrEmpty(session.PaymentIntent.Id))
                {
                    _logger.LogWarning("Payment intent not yet created for session {SessionId}. Session status: {Status}", 
                        SessionId, session.Status);
                    
                    // If payment not complete, redirect to order detail
                    if (session.Status == "open" || session.Status == "expired")
                    {
                        ErrorMessage = "Payment not completed. Please complete the payment to confirm your order.";
                        return Page();
                    }
                    
                    ErrorMessage = "Invalid payment session - payment intent not found";
                    return Page();
                }

                // Confirm payment with our backend
                _logger.LogInformation("Confirming payment for payment intent: {PaymentIntent}", session.PaymentIntent.Id);
                await _paymentService.ConfirmPaymentAsync(session.PaymentIntent.Id);

                // Redirect to order detail
                _logger.LogInformation("Payment confirmed successfully, redirecting to order detail");
                return RedirectToPage("/Order/OrderDetail", new { id = OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment success for session {SessionId}", SessionId);
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        private class StripeCheckoutSession
        {
            public string Id { get; set; } = string.Empty;
            
            [JsonPropertyName("payment_intent")]
            public StripePaymentIntent? PaymentIntent { get; set; }
            
            public string? Status { get; set; }
            
            [JsonPropertyName("payment_status")]
            public string? PaymentStatus { get; set; }
            
            public Dictionary<string, string>? Metadata { get; set; }
        }

        private class StripePaymentIntent
        {
            public string Id { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }
    }
}
