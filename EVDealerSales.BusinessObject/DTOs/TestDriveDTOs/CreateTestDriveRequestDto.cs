using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.TestDriveDTOs
{
    public class CreateTestDriveRequestDto
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        public Guid VehicleId { get; set; }

        [Required(ErrorMessage = "Customer email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Scheduled date time is required")]
        public DateTime ScheduledAt { get; set; }

        public string? Notes { get; set; }
    }
}
