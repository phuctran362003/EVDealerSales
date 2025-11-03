using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.DeliveryDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IDeliveryService
    {
        // Customer request delivery
        Task<DeliveryResponseDto> RequestDeliveryAsync(CreateDeliveryRequestDto request);
        
        // Staff confirm and schedule delivery
        Task<DeliveryResponseDto> ConfirmDeliveryAsync(Guid deliveryId, ConfirmDeliveryRequestDto request);
        
        Task<DeliveryResponseDto?> GetDeliveryByIdAsync(Guid id);
        Task<DeliveryResponseDto?> GetDeliveryByOrderIdAsync(Guid orderId);
        Task<Pagination<DeliveryResponseDto>> GetAllDeliveriesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            DeliveryFilterDto? filter = null);
        Task<DeliveryResponseDto?> UpdateDeliveryStatusAsync(Guid id, UpdateDeliveryStatusRequestDto request);
        Task<DeliveryResponseDto?> CancelDeliveryAsync(Guid id);
    }
}
