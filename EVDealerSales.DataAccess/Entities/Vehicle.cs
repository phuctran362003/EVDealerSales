namespace EVDealerSales.DataAccess.Entities
{
    public class Vehicle : BaseEntity
    {
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int? ModelYear { get; set; }
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; }
        public int BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public int ChargingTime { get; set; }
        public int TopSpeed { get; set; }
        public int Stock { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<TestDrive> TestDrives { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
