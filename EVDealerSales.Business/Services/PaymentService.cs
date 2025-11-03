using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.OrderDTOs;
using EVDealerSales.BusinessObject.DTOs.StripeDTOs;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess.Entities;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EVDealerSales.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretKey;
        private readonly HttpClient _httpClient;

        public PaymentService(
            IUnitOfWork unitOfWork,
            ILogger<PaymentService> logger,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _configuration = configuration;
            _stripeSecretKey = _configuration["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe:SecretKey configuration is missing");
            _httpClient = httpClientFactory.CreateClient("Stripe");
        }

        public async Task<OrderResponseDto> ConfirmPaymentAsync(string paymentIntentId)
        {
            try
            {
                _logger.LogInformation("Confirming payment for payment intent {PaymentIntentId}", paymentIntentId);

                // Retrieve payment intent from Stripe
                var paymentIntent = await RetrievePaymentIntentFromStripe(paymentIntentId);

                if (paymentIntent == null)
                {
                    throw new KeyNotFoundException($"Payment intent {paymentIntentId} not found");
                }

                _logger.LogInformation("Payment record not found for payment intent {PaymentIntentId}, creating new payment record", paymentIntentId);

                // Try to get invoice from recent unpaid invoices
                // In a better implementation, we'd get this from Stripe checkout session metadata
                var recentInvoices = await _unitOfWork.Invoices.GetQueryable()
                    .Include(i => i.Order).ThenInclude(o => o.Customer)
                    .Include(i => i.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(i => i.Payments)
                    .Where(i => i.Status != InvoiceStatus.Paid && !i.IsDeleted)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Find invoice that doesn't have a payment with this payment intent
                var invoice = recentInvoices.FirstOrDefault(i => !i.Payments.Any(p => p.PaymentIntentId == paymentIntentId));

                if (invoice == null)
                {
                    throw new InvalidOperationException($"No unpaid invoice found for payment intent {paymentIntentId}");
                }

                // Create payment record
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Amount = invoice.TotalAmount,
                    PaymentDate = _currentTime.GetCurrentTime(),
                    Status = PaymentStatus.Pending,
                    PaymentIntentId = paymentIntentId,
                    TransactionId = paymentIntentId,
                    PaymentMethod = "card",
                    CreatedAt = _currentTime.GetCurrentTime(),
                    IsDeleted = false,
                    Invoice = invoice
                };

                await _unitOfWork.Payments.AddAsync(payment);
                _logger.LogInformation("Created payment record {PaymentId} for invoice {InvoiceId}", payment.Id, invoice.Id);

                invoice = payment.Invoice;

                if (paymentIntent.Status == "succeeded")
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.PaymentDate = _currentTime.GetCurrentTime();
                    payment.TransactionId = paymentIntent.Id;

                    if (invoice != null)
                    {
                        invoice.Status = InvoiceStatus.Paid;
                        invoice.UpdatedAt = _currentTime.GetCurrentTime();
                    }
                    if (invoice?.Order != null)
                    {
                        invoice.Order.Status = OrderStatus.Confirmed;
                        invoice.Order.UpdatedAt = _currentTime.GetCurrentTime();
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Payment {PaymentId} confirmed successfully for order {OrderId}",
                        payment.Id, invoice?.OrderId);
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                    _logger.LogWarning("Payment {PaymentId} failed for order {OrderId} with status: {Status}",
                        payment.Id, invoice?.OrderId, paymentIntent.Status);
                    if (invoice != null)
                    {
                        invoice.Status = InvoiceStatus.Canceled;
                        invoice.UpdatedAt = _currentTime.GetCurrentTime();
                    }
                    if (invoice?.Order != null)
                    {
                        invoice.Order.Status = OrderStatus.Cancelled;
                        invoice.Order.UpdatedAt = _currentTime.GetCurrentTime();
                    }
                    await _unitOfWork.SaveChangesAsync();

                    throw new InvalidOperationException($"Payment failed with status: {paymentIntent.Status}");
                }

                // Return order details
                var order = invoice?.Order;
                if (order == null)
                {
                    throw new InvalidOperationException("Order not found");
                }

                return await MapOrderToResponseDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for payment intent {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        private async Task<StripePaymentIntent?> RetrievePaymentIntentFromStripe(string paymentIntentId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.stripe.com/v1/payment_intents/{paymentIntentId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _stripeSecretKey);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve payment intent from Stripe: {Error}", responseContent);
                    return null;
                }

                return JsonSerializer.Deserialize<StripePaymentIntent>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment intent {PaymentIntentId}", paymentIntentId);
                return null;
            }
        }

        private async Task<OrderResponseDto> MapOrderToResponseDto(Order order)
        {
            if (order.Customer == null)
            {
                order.Customer = await _unitOfWork.Users.GetByIdAsync(order.CustomerId)
                    ?? throw new InvalidOperationException("Customer not found");
            }

            User? staff = null;
            if (order.StaffId.HasValue)
            {
                staff = order.Staff ?? await _unitOfWork.Users.GetByIdAsync(order.StaffId.Value);
            }

            if (order.Items == null || !order.Items.Any())
            {
                var items = await _unitOfWork.OrderItems.GetQueryable()
                    .Include(oi => oi.Vehicle)
                    .Where(oi => oi.OrderId == order.Id && !oi.IsDeleted)
                    .ToListAsync();
                order.Items = items;
            }

            if (order.Invoices == null || !order.Invoices.Any())
            {
                var invoices = await _unitOfWork.Invoices.GetQueryable()
                    .Include(i => i.Payments)
                    .Where(i => i.OrderId == order.Id && !i.IsDeleted)
                    .ToListAsync();
                order.Invoices = invoices;
            }

            if (order.Delivery == null)
            {
                var deliveryEntity = await _unitOfWork.Deliveries.GetQueryable()
                    .FirstOrDefaultAsync(d => d.OrderId == order.Id && !d.IsDeleted);
                order.Delivery = deliveryEntity;
            }

            var invoice = order.Invoices.FirstOrDefault();
            var payment = invoice?.Payments.FirstOrDefault();
            var delivery = order.Delivery;

            return new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.FullName,
                CustomerEmail = order.Customer.Email,
                CustomerPhone = order.Customer.PhoneNumber,
                StaffId = order.StaffId,
                StaffName = staff?.FullName,
                StaffEmail = staff?.Email,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    VehicleId = oi.VehicleId,
                    VehicleModelName = oi.Vehicle?.ModelName ?? "Unknown",
                    VehicleTrimName = oi.Vehicle?.TrimName ?? "Unknown",
                    VehicleImageUrl = oi.Vehicle?.ImageUrl,
                    UnitPrice = oi.UnitPrice,
                    Year = oi.Vehicle?.ModelYear ?? 0,
                }).ToList(),
                PaymentStatus = payment?.Status,
                PaymentDate = payment?.PaymentDate,
                PaymentIntentId = payment?.PaymentIntentId,
                DeliveryId = delivery?.Id,
                DeliveryStatus = delivery?.Status,
                DeliveryDate = delivery?.ActualDate,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        public async Task<string> CreateCheckoutSessionAsync(Guid orderId)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                _logger.LogInformation("User {UserId} creating checkout session for order {OrderId}",
                    currentUserId, orderId);

                // Get order with details
                var order = await _unitOfWork.Orders.GetQueryable()
                    .Include(o => o.Customer)
                    .Include(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(o => o.Invoices).ThenInclude(i => i.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found");
                }

                // Check permission
                if (order.CustomerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You don't have permission to pay for this order");
                }

                if (order.Status == OrderStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot create payment for cancelled order");
                }

                var invoice = order.Invoices.FirstOrDefault();
                if (invoice == null)
                {
                    throw new InvalidOperationException("Invoice not found for this order");
                }

                // Check if already paid
                var existingPaidPayment = invoice.Payments.FirstOrDefault(p => p.Status == PaymentStatus.Paid);
                if (existingPaidPayment != null)
                {
                    throw new InvalidOperationException("This order has already been paid");
                }

                // Create checkout session using form-urlencoded
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7067";
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("mode", "payment"),
                    new KeyValuePair<string, string>("payment_method_types[0]", "card"),
                    new KeyValuePair<string, string>("success_url", $"{baseUrl}/Order/PaymentSuccess?session_id={{CHECKOUT_SESSION_ID}}"),
                    new KeyValuePair<string, string>("cancel_url", $"{baseUrl}/Order/OrderDetail?id={orderId}"),
                    new KeyValuePair<string, string>("client_reference_id", orderId.ToString()),
                    new KeyValuePair<string, string>("metadata[order_id]", orderId.ToString()),
                    new KeyValuePair<string, string>("metadata[invoice_id]", invoice.Id.ToString())
                };

                if (!string.IsNullOrEmpty(order.Customer?.Email))
                {
                    formData.Add(new KeyValuePair<string, string>("customer_email", order.Customer.Email));
                }

                // Add line items
                int itemIndex = 0;
                foreach (var item in order.Items)
                {
                    var vehicleName = $"{item.Vehicle?.ModelName ?? "Vehicle"} - {item.Vehicle?.TrimName ?? ""}";
                    formData.Add(new KeyValuePair<string, string>($"line_items[{itemIndex}][price_data][currency]", "usd"));
                    formData.Add(new KeyValuePair<string, string>($"line_items[{itemIndex}][price_data][unit_amount]", ((long)(item.UnitPrice * 100)).ToString()));
                    formData.Add(new KeyValuePair<string, string>($"line_items[{itemIndex}][price_data][product_data][name]", vehicleName));

                    if (item.Vehicle?.ModelYear.HasValue == true)
                    {
                        formData.Add(new KeyValuePair<string, string>($"line_items[{itemIndex}][price_data][product_data][description]", $"{item.Vehicle.ModelYear} Model"));
                    }

                    // Only add image if URL is valid and not too long (Stripe limit is 2048 chars)
                    if (!string.IsNullOrEmpty(item.Vehicle?.ImageUrl) &&
                        item.Vehicle.ImageUrl.Length <= 2000 &&
                        (item.Vehicle.ImageUrl.StartsWith("http://") || item.Vehicle.ImageUrl.StartsWith("https://")))
                    {
                        formData.Add(new KeyValuePair<string, string>($"line_items[{itemIndex}][price_data][product_data][images][0]", item.Vehicle.ImageUrl));
                    }

                    formData.Add(new KeyValuePair<string, string>($"line_items[{itemIndex}][quantity]", "1"));
                    itemIndex++;
                }

                var content = new FormUrlEncodedContent(formData);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
                request.Headers.Add("Authorization", $"Bearer {_stripeSecretKey}");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Stripe API error: {StatusCode}, {Response}",
                        response.StatusCode, responseContent);
                    throw new Exception($"Failed to create checkout session: {responseContent}");
                }

                var session = JsonSerializer.Deserialize<StripeCheckoutSession>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (session == null || string.IsNullOrEmpty(session.Url))
                {
                    throw new Exception("Failed to parse checkout session response");
                }

                // Don't create payment record yet - will be created when payment is confirmed
                _logger.LogInformation("Checkout session created successfully for order {OrderId}, session: {SessionId}",
                    orderId, session.Id);

                return session.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout session for order {OrderId}", orderId);
                throw;
            }
        }
    }
}
