using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.DataAccess.Entities
{
    public class Payment : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        
        // Stripe Payment Information
        public string? PaymentIntentId { get; set; }
        public string? PaymentMethod { get; set; } // card, bank_transfer, etc.
        public string? TransactionId { get; set; }
    }
}
