using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    [Authorize(Policy = "ManagerPolicy")]
    public class CreateModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IVehicleService vehicleService, ILogger<CreateModel> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [BindProperty]
        public CreateVehicleRequestDto Vehicle { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Creating new vehicle: {ModelName} {TrimName}",
                    Vehicle?.ModelName, Vehicle?.TrimName);

                var result = await _vehicleService.CreateVehicleAsync(Vehicle);

                TempData["SuccessMessage"] = $"Vehicle '{result.ModelName} {result.TrimName}' created successfully!";
                _logger.LogInformation("Vehicle created successfully with ID: {VehicleId}", result.Id);

                return RedirectToPage("Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating vehicle");
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized vehicle creation attempt");
                ModelState.AddModelError(string.Empty, "You must be logged in to create vehicles.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the vehicle.");
                return Page();
            }
        }
    }
}