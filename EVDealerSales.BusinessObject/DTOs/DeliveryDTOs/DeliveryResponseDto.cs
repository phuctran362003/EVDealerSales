using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.DeliveryDTOs
{
    public class DeliveryResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public DateTime? PlannedDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public DeliveryStatus Status { get; set; }
        public string? VehicleInfo { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public string? StaffNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}