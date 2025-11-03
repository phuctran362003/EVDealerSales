using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;
using EVDealerSales.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.TestDrive
{
    [Authorize(Policy = "StaffPolicy")]
    public class IndexModel : PageModel
    {
        private readonly ITestDriveService _testDriveService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ITestDriveService testDriveService, ILogger<IndexModel> logger)
        {
            _testDriveService = testDriveService;
            _logger = logger;
        }

        public Pagination<TestDriveResponseDto> TestDrives { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        // Search and Filter Properties
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CustomerEmail { get; set; }

        [BindProperty(SupportsGet = true)]
        public TestDriveStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? VehicleId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading test drives list page (Page: {PageNumber}, Size: {PageSize})",
                    PageNumber, PageSize);

                // Build filter DTO
                var filter = new TestDriveFilterDto
                {
                    SearchTerm = SearchTerm,
                    CustomerEmail = CustomerEmail,
                    Status = Status,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    VehicleId = VehicleId
                };

                TestDrives = await _testDriveService.GetAllTestDrivesAsync(
                    pageNumber: PageNumber,
                    pageSize: PageSize,
                    filter: filter
                );

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test drives list");
                TempData["ErrorMessage"] = "Failed to load test drives list. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync(Guid id)
        {
            try
            {
                await _testDriveService.ConfirmTestDriveAsync(id);
                TempData["SuccessMessage"] = "Test drive confirmed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming test drive {TestDriveId}", id);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, CustomerEmail, Status, FromDate, ToDate });
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id, string? reason)
        {
            try
            {
                await _testDriveService.CancelTestDriveAsync(id, reason);
                TempData["SuccessMessage"] = "Test drive canceled successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling test drive {TestDriveId}", id);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, CustomerEmail, Status, FromDate, ToDate });
        }

        public async Task<IActionResult> OnPostCompleteAsync(Guid id)
        {
            try
            {
                await _testDriveService.CompleteTestDriveAsync(id);
                TempData["SuccessMessage"] = "Test drive completed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing test drive {TestDriveId}", id);
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, CustomerEmail, Status, FromDate, ToDate });
        }
    }
}
