namespace EVDealerSales.BusinessObject.DTOs.VehicleDTOs
{
    public class UpdateVehicleRequestDto
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int? ModelYear { get; set; }
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; }
        public int BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public int ChargingTime { get; set; }
        public int TopSpeed { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}