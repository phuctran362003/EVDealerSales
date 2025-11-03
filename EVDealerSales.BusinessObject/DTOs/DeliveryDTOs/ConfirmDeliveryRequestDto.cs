namespace EVDealerSales.BusinessObject.DTOs.DeliveryDTOs
{
    // DTO for staff to confirm and schedule delivery
    public class ConfirmDeliveryRequestDto
    {
        public DateTime PlannedDate { get; set; }
        public string? StaffNotes { get; set; }
    }
}
