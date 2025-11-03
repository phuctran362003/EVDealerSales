namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class VehicleSalesDto
    {
        public Guid VehicleId { get; set; }
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

}
