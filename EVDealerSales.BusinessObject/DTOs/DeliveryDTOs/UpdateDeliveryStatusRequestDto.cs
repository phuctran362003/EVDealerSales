using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.DeliveryDTOs
{
    public class UpdateDeliveryStatusRequestDto
    {
        public DeliveryStatus Status { get; set; }
        public DateTime? PlannedDate { get; set; }
        public DateTime? ActualDate { get; set; }
    }
}