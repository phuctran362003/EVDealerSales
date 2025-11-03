using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.TestDriveDTOs
{
    public class TestDriveFilterDto
    {
        public string? CustomerEmail { get; set; }

        public Guid? VehicleId { get; set; }

        public TestDriveStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public Guid? StaffId { get; set; }

        public string? SearchTerm { get; set; }
    }
}
