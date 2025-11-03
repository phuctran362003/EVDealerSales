using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.DeliveryDTOs
{
    public class DeliveryFilterDto
    {
        public string? SearchTerm { get; set; }
        public DeliveryStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}