using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.TestDrive
{
    [Authorize(Policy = "CustomerPolicy")]
    public class MyTestDrivesModel : PageModel
    {
        private readonly ITestDriveService _testDriveService;
        private readonly ILogger<MyTestDrivesModel> _logger;

        public MyTestDrivesModel(ITestDriveService testDriveService, ILogger<MyTestDrivesModel> logger)
        {
            _testDriveService = testDriveService;
            _logger = logger;
        }

        public Pagination<TestDriveResponseDto> TestDrives { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Customer loading their test drives (Page: {PageNumber}, Size: {PageSize})",
                    PageNumber, PageSize);

                TestDrives = await _testDriveService.GetMyTestDrivesAsync(
                    pageNumber: PageNumber,
                    pageSize: PageSize
                );

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer's test drives");
                TempData["ErrorMessage"] = "Failed to load your test drives. Please try again.";
                return Page();
            }
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

            return RedirectToPage(new { PageNumber, PageSize });
        }
    }
}
