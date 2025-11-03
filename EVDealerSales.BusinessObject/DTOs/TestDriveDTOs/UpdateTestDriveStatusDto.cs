using EVDealerSales.BusinessObject.Enums;
using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.TestDriveDTOs
{
    public class UpdateTestDriveStatusDto
    {
        [Required(ErrorMessage = "Test Drive ID is required")]
        public Guid TestDriveId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public TestDriveStatus Status { get; set; }

        public string? Notes { get; set; }
        
        public string? CancellationReason { get; set; }
    }
}
