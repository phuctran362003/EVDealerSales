using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.FeedbackDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackResponseDto> CreateFeedbackAsync(CreateFeedbackRequestDto request);
        Task<FeedbackResponseDto?> GetFeedbackByIdAsync(Guid id);

        Task<Pagination<FeedbackResponseDto>> GetAllFeedbacksAsync(
            int pageNumber = 1,
            int pageSize = 10,
            FeedbackFilterDto? filter = null);
        Task<Pagination<FeedbackResponseDto>> GetMyFeedbacksAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<FeedbackResponseDto> ResolveFeedbackAsync(Guid id, ResolveFeedbackRequestDto request);

        Task<bool> DeleteFeedbackAsync(Guid id);
    }
}