using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.UserDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IUserService
    {
        // Staff Management (Manager only)
        Task<UserResponseDto> CreateStaffAsync(CreateStaffRequestDto request);
        Task<UserResponseDto?> GetStaffByIdAsync(Guid id);
        Task<Pagination<UserResponseDto>> GetAllStaffAsync(
            int pageNumber = 1,
            int pageSize = 10,
            UserFilterDto? filter = null);
        Task<UserResponseDto> UpdateStaffAsync(Guid id, UpdateStaffRequestDto request);
        Task<bool> DeleteStaffAsync(Guid id);

        // Customer Management (Staff only)
        Task<UserResponseDto?> GetCustomerByIdAsync(Guid id);
        Task<Pagination<UserResponseDto>> GetAllCustomersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            UserFilterDto? filter = null);

        // Profile Management (All users)
        Task<UserResponseDto?> GetMyProfileAsync();
        Task<UserResponseDto> UpdateMyProfileAsync(UpdateProfileRequestDto request);
    }
}