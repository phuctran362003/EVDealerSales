using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.DataAccess.Entities
{
    public class TestDrive : BaseEntity
    {
        // The customer (user) who books the test drive
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }

        // The vehicle being test-driven
        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public DateTime ScheduledAt { get; set; }
        public TestDriveStatus Status { get; set; }
        public string? Notes { get; set; }

        // The staff member handling the test drive
        public Guid? StaffId { get; set; }
        public User? Staff { get; set; }

        // Tracking timeline
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}
