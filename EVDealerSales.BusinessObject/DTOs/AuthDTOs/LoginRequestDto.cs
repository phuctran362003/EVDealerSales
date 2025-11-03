namespace EVDealerSales.BusinessObject.DTOs.AuthDTOs
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
