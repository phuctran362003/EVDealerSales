namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class VehicleStockDto
    {
        public Guid VehicleId { get; set; }
        public string ModelName { get; set; }
        public string TrimName { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
    }
}
