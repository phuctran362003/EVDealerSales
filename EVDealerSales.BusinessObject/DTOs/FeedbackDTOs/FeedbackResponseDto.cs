namespace EVDealerSales.BusinessObject.DTOs.FeedbackDTOs
{
    public class FeedbackResponseDto
    {
        public Guid Id { get; set; }

        // Customer info
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Order info (optional)
        public Guid? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string? VehicleInfo { get; set; }

        public string Content { get; set; } = string.Empty;

        // Resolution info
        public bool IsResolved { get; set; }
        public Guid? ResolvedBy { get; set; }
        public string? ResolverName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }

        // Creator info (staff who created record)
        public Guid? CreatedByUserId { get; set; }
        public string? CreatorName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}