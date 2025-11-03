using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.TestDriveDTOs
{
    public class TestDriveResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        public Guid VehicleId { get; set; }
        public string VehicleModelName { get; set; } = string.Empty;
        public string VehicleTrimName { get; set; } = string.Empty;
        public string? VehicleImageUrl { get; set; }

        public DateTime ScheduledAt { get; set; }
        public TestDriveStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string? Notes { get; set; }

        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? StaffEmail { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}
