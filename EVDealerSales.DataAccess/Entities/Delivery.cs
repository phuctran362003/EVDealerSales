using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.DataAccess.Entities
{
    public class Delivery : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public DateTime? PlannedDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public DeliveryStatus Status { get; set; }
        public string? ShippingAddress { get; set; }  // Delivery address
        public string? Notes { get; set; }  // Customer notes
        public string? StaffNotes { get; set; }  // Staff notes
    }
}
