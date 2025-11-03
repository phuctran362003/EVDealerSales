using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.Presentation.Pages.TestDrive
{
    [Authorize(Policy = "StaffPolicy")]
    public class RegisterForCustomerModel : PageModel
    {
        private readonly ITestDriveService _testDriveService;
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<RegisterForCustomerModel> _logger;

        public RegisterForCustomerModel(
            ITestDriveService testDriveService,
            IVehicleService vehicleService,
            ILogger<RegisterForCustomerModel> logger)
        {
            _testDriveService = testDriveService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [BindProperty]
        public CreateTestDriveRequestDto TestDrive { get; set; } = new();

        public List<SelectListItem> VehicleList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load active vehicles
                var vehicles = await _vehicleService.GetAllVehiclesAsync(1, 100, includeInactive: false);
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.ModelName} {v.TrimName} - ${v.BasePrice:N0}"
                }).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading register for customer page");
                TempData["ErrorMessage"] = "Failed to load page. Please try again.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload vehicles list
                var vehicles = await _vehicleService.GetAllVehiclesAsync(1, 100, includeInactive: false);
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.ModelName} {v.TrimName} - ${v.BasePrice:N0}"
                }).ToList();

                return Page();
            }

            try
            {
                var result = await _testDriveService.RegisterTestDriveByStaffAsync(TestDrive);
                
                TempData["SuccessMessage"] = $"Test drive registered and confirmed successfully for {result.CustomerName}! Scheduled for {result.ScheduledAt:MMM dd, yyyy hh:mm tt}.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering test drive for customer");
                ModelState.AddModelError(string.Empty, ex.Message);

                // Reload vehicles list
                var vehicles = await _vehicleService.GetAllVehiclesAsync(1, 100, includeInactive: false);
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.ModelName} {v.TrimName} - ${v.BasePrice:N0}"
                }).ToList();

                return Page();
            }
        }
    }
}
