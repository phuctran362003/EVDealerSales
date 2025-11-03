using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class CreatePaymentIntentRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }
    }

    public class PaymentIntentResponseDto
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class ConfirmPaymentRequestDto
    {
        [Required]
        public required string PaymentIntentId { get; set; }
    }
}
