using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.TestDrive
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ITestDriveService _testDriveService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ITestDriveService testDriveService, ILogger<DetailsModel> logger)
        {
            _testDriveService = testDriveService;
            _logger = logger;
        }

        public TestDriveResponseDto? TestDrive { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                TestDrive = await _testDriveService.GetTestDriveByIdAsync(id);
                
                if (TestDrive == null)
                {
                    TempData["ErrorMessage"] = "Test drive not found.";
                    return RedirectToPage("Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test drive details");
                TempData["ErrorMessage"] = "Failed to load test drive details.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync(Guid id, string? notes)
        {
            try
            {
                await _testDriveService.ConfirmTestDriveAsync(id, notes);
                TempData["SuccessMessage"] = "Test drive confirmed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming test drive");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { id });
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
                _logger.LogError(ex, "Error canceling test drive");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCompleteAsync(Guid id, string? notes)
        {
            try
            {
                await _testDriveService.CompleteTestDriveAsync(id, notes);
                TempData["SuccessMessage"] = "Test drive completed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing test drive");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }
    }
}
