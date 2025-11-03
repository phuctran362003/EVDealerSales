using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class CreateOrderRequestDto
    {
        [Required]
        public Guid VehicleId { get; set; }

        public string? Notes { get; set; }
    }
}
