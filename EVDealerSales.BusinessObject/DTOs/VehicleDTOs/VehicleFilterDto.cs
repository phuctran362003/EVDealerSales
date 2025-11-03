namespace EVDealerSales.BusinessObject.DTOs.VehicleDTOs
{
    public class VehicleFilterDto
    {
        // Search
        public string? SearchTerm { get; set; }

        // Filter by Battery Capacity
        public int? MinBatteryCapacity { get; set; }
        public int? MaxBatteryCapacity { get; set; }

        // Filter by Top Speed
        public int? MinTopSpeed { get; set; }
        public int? MaxTopSpeed { get; set; }

        // Filter by Charging Time
        public int? MinChargingTime { get; set; }
        public int? MaxChargingTime { get; set; }

        // Filter by Base Price
        public decimal? MinBasePrice { get; set; }
        public decimal? MaxBasePrice { get; set; }

        // Filter by Range
        public int? MinRangeKM { get; set; }
        public int? MaxRangeKM { get; set; }

        // Filter by Model Year
        public int? ModelYear { get; set; }

        // Sorting
        public string? SortBy { get; set; } // "price", "range", "battery", "speed", "charging"
        public bool SortDescending { get; set; } = false;
    }
}