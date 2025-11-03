using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IChatbotService
    {
        Task<string> FreestyleAskAsync(string prompt, string? groupId = null);
        Task<VehicleResponseDto> AutomateAddVehicleAsync(string instruction);
        Task<string> GenerateVehicleSpecAsync(string instruction);
        Task<VehicleResponseDto> CreateVehicleFromSpecAsync(string spec);
        Task<VehicleResponseDto> GenerateAndCreateVehicleAsync(string instruction);
    }
}
