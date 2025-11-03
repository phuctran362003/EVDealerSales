using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.DataAccess.Entities
{
    public class Order : BaseEntity
    {
        // Customer who made the order
        public Guid CustomerId { get; set; }
        public User? Customer { get; set; }

        // Dealer staff who processes it
        public Guid? StaffId { get; set; }
        public User? Staff { get; set; }

        public string OrderNumber { get; set; } = string.Empty; // e.g., ORD-20251016-0001
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public Delivery? Delivery { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
