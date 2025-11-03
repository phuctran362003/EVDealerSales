using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.DataAccess.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        public Guid CustomerId { get; set; }   // Customer now refers to User
        public User Customer { get; set; }

        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string Notes { get; set; }

        // Navigation
        public ICollection<Payment> Payments { get; set; }
    }
}
