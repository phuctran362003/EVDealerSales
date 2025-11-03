using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class OrderFilterDto
    {
        public Guid? CustomerId { get; set; }
        public Guid? StaffId { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class UpdateOrderStatusRequestDto
    {
        public required OrderStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class AssignStaffRequestDto
    {
        public required Guid StaffId { get; set; }
    }
}
