using EVDealerSales.BusinessObject.DTOs.AuthDTOs;
using Microsoft.Extensions.Configuration;

namespace EVDealerSales.Business.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterUserAsync(UserRegistrationDto userRegistrationDto);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequestDto, IConfiguration configuration);
        Task<bool> LogoutAsync(Guid userId);
    }
}
