using EVDealerSales.BusinessObject.DTOs.OrderDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IPaymentService
    {
        // Stripe Payment Integration
        Task<string> CreateCheckoutSessionAsync(Guid orderId);
        Task<OrderResponseDto> ConfirmPaymentAsync(string paymentIntentId);
    }
}
