using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.DeliveryDTOs
{
    // DTO for customer to request delivery
    public class CreateDeliveryRequestDto
    {
        public Guid OrderId { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
    }
}