using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.BusinessObject.DTOs.DeliveryDTOs;

namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }

        // Customer info
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }

        // Staff info
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? StaffEmail { get; set; }

        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        // Order items
        public List<OrderItemDto> Items { get; set; } = new();

        // Payment info (flattened for backward compatibility)
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentIntentId { get; set; }

        // Delivery info (flattened for backward compatibility)
        public Guid? DeliveryId { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Nested objects
        public InvoiceResponseDto? Invoice { get; set; }
        public PaymentResponseDto? Payment { get; set; }
        public DeliveryResponseDto? Delivery { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleModelName { get; set; } = string.Empty;
        public string VehicleTrimName { get; set; } = string.Empty;
        public string? VehicleImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Year { get; set; }
    }
}
