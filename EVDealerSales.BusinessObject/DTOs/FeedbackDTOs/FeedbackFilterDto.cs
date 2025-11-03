namespace EVDealerSales.BusinessObject.DTOs.FeedbackDTOs
{
    public class FeedbackFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsResolved { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}