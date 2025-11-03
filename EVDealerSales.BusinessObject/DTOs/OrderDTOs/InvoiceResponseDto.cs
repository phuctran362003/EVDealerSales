using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
