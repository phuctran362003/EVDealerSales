namespace EVDealerSales.BusinessObject.DTOs.OrderDTOs
{
    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

}
