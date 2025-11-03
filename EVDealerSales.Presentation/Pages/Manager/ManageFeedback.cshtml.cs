using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.FeedbackDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Manager
{
    [Authorize(Roles = "DealerManager")]
    public class ManageFeedbackModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IOrderService _orderService;
        private readonly ILogger<ManageFeedbackModel> _logger;

        public ManageFeedbackModel(
            IFeedbackService feedbackService, 
            IOrderService orderService,
            ILogger<ManageFeedbackModel> logger)
        {
            _feedbackService = feedbackService;
            _orderService = orderService;
            _logger = logger;
        }

        public Pagination<FeedbackResponseDto> Feedbacks { get; set; } = null!;

        // Feedback Statistics
        public int TotalFeedbacks { get; set; }
        public int PendingFeedbacks { get; set; }
        public int ResolvedFeedbacks { get; set; }
        public double ResolutionRate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsResolved { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var filter = new FeedbackFilterDto
                {
                    SearchTerm = SearchTerm,
                    IsResolved = IsResolved,
                    FromDate = FromDate,
                    ToDate = ToDate
                };

                Feedbacks = await _feedbackService.GetAllFeedbacksAsync(PageNumber, PageSize, filter);

                // Load feedback statistics
                TotalFeedbacks = await _orderService.GetTotalFeedbacksCountAsync(FromDate, ToDate);
                PendingFeedbacks = await _orderService.GetPendingFeedbacksCountAsync();
                ResolvedFeedbacks = await _orderService.GetResolvedFeedbacksCountAsync();
                ResolutionRate = await _orderService.GetFeedbackResolutionRateAsync();

                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading feedbacks");
                TempData["ErrorMessage"] = "An error occurred while loading feedbacks";
                Feedbacks = new Pagination<FeedbackResponseDto>(
                    new List<FeedbackResponseDto>(), 0, PageNumber, PageSize);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostResolveAsync([FromBody] ResolveFeedbackRequest request)
        {
            try
            {
                _logger.LogInformation("Manager resolving feedback {FeedbackId}", request.FeedbackId);

                var resolveDto = new ResolveFeedbackRequestDto();

                var result = await _feedbackService.ResolveFeedbackAsync(request.FeedbackId, resolveDto);

                return new JsonResult(new
                {
                    success = true,
                    message = "Feedback resolved successfully",
                    feedback = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid feedback resolution");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving feedback");
                return new JsonResult(new { success = false, message = "An error occurred while resolving feedback" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteFeedbackRequest request)
        {
            try
            {
                _logger.LogInformation("Manager deleting feedback {FeedbackId}", request.FeedbackId);

                await _feedbackService.DeleteFeedbackAsync(request.FeedbackId);

                return new JsonResult(new
                {
                    success = true,
                    message = "Feedback deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback");
                return new JsonResult(new { success = false, message = "An error occurred while deleting feedback" }) { StatusCode = 500 };
            }
        }
    }

    public class ResolveFeedbackRequest
    {
        public Guid FeedbackId { get; set; }
    }

    public class DeleteFeedbackRequest
    {
        public Guid FeedbackId { get; set; }
    }
}