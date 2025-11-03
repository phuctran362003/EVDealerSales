using EVDealerSales.BusinessObject.Enums;

namespace EVDealerSales.BusinessObject.DTOs.UserDTOs
{
    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public RoleType? Role { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}