using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVDealerSales.Presentation.Pages.TestDrive
{
    [Authorize(Policy = "CustomerPolicy")]
    public class RegisterModel : PageModel
    {
        private readonly ITestDriveService _testDriveService;
        private readonly IVehicleService _vehicleService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            ITestDriveService testDriveService,
            IVehicleService vehicleService,
            IClaimsService claimsService,
            ILogger<RegisterModel> logger)
        {
            _testDriveService = testDriveService;
            _vehicleService = vehicleService;
            _claimsService = claimsService;
            _logger = logger;
        }

        [BindProperty]
        public CreateTestDriveRequestDto TestDrive { get; set; } = new();

        public List<SelectListItem> VehicleList { get; set; } = new();
        public VehicleResponseDto? SelectedVehicle { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? vehicleId = null)
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

                // Pre-select vehicle if provided
                if (vehicleId.HasValue)
                {
                    TestDrive = new CreateTestDriveRequestDto { VehicleId = vehicleId.Value };
                    SelectedVehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId.Value);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading register test drive page");
                TempData["ErrorMessage"] = "Failed to load page. Please try again.";
                return RedirectToPage("/Vehicle/Index");
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
                var result = await _testDriveService.RegisterTestDriveAsync(TestDrive);
                
                TempData["SuccessMessage"] = $"Test drive registered successfully! Your test drive is scheduled for {result.ScheduledAt:MMM dd, yyyy hh:mm tt}. Please wait for staff confirmation.";
                return RedirectToPage("MyTestDrives");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering test drive");
                ModelState.AddModelError(string.Empty, ex.Message);

                // Reload vehicles list
                var vehicles = await _vehicleService.GetAllVehiclesAsync(1, 100, includeInactive: false);
                VehicleList = vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.ModelName} {v.TrimName} - ${v.BasePrice:N0}"
                }).ToList();

                if (TestDrive.VehicleId != Guid.Empty)
                {
                    SelectedVehicle = await _vehicleService.GetVehicleByIdAsync(TestDrive.VehicleId);
                }

                return Page();
            }
        }

        public async Task<JsonResult> OnGetVehicleDetailsAsync(Guid vehicleId)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return new JsonResult(new { success = false });
                }

                return new JsonResult(new
                {
                    success = true,
                    modelName = vehicle.ModelName,
                    trimName = vehicle.TrimName,
                    basePrice = vehicle.BasePrice,
                    imageUrl = vehicle.ImageUrl,
                    batteryCapacity = vehicle.BatteryCapacity,
                    rangeKM = vehicle.RangeKM
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle details");
                return new JsonResult(new { success = false });
            }
        }
    }
}
