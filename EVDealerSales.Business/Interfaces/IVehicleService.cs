using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IVehicleService
    {
        Task<Pagination<VehicleResponseDto>> GetAllVehiclesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            bool includeInactive = false,
            VehicleFilterDto? filter = null);

        Task<VehicleResponseDto?> GetVehicleByIdAsync(Guid id);
        Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleRequestDto request);
        Task<VehicleResponseDto?> UpdateVehicleAsync(UpdateVehicleRequestDto request);
        Task<bool> DeleteVehicleAsync(Guid id);
        Task<bool> ToggleVehicleStatusAsync(Guid id);
    }
}