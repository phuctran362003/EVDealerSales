using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    [Authorize(Policy = "ManagerPolicy")]
    public class EditModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IVehicleService vehicleService, ILogger<EditModel> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [BindProperty]
        public UpdateVehicleRequestDto Vehicle { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Loading edit page for vehicle ID: {VehicleId}", id);

                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found", id);
                    TempData["ErrorMessage"] = "Vehicle not found.";
                    return RedirectToPage("Index");
                }

                // Map to update DTO
                Vehicle = new UpdateVehicleRequestDto
                {
                    Id = vehicle.Id,
                    ModelName = vehicle.ModelName,
                    TrimName = vehicle.TrimName,
                    ModelYear = vehicle.ModelYear,
                    BasePrice = vehicle.BasePrice,
                    ImageUrl = vehicle.ImageUrl,
                    BatteryCapacity = vehicle.BatteryCapacity,
                    RangeKM = vehicle.RangeKM,
                    ChargingTime = vehicle.ChargingTime,
                    TopSpeed = vehicle.TopSpeed,
                    Stock = vehicle.Stock,
                    IsActive = vehicle.IsActive
                };

                return Page();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while loading vehicle for edit");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle for edit with ID: {VehicleId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the vehicle.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Updating vehicle with ID: {VehicleId}", Vehicle?.Id);

                var result = await _vehicleService.UpdateVehicleAsync(Vehicle);

                if (result == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for update", Vehicle?.Id);
                    TempData["ErrorMessage"] = "Vehicle not found.";
                    return RedirectToPage("Index");
                }

                TempData["SuccessMessage"] = $"Vehicle '{result.ModelName} {result.TrimName}' updated successfully!";
                _logger.LogInformation("Vehicle with ID {VehicleId} updated successfully", Vehicle.Id);

                return RedirectToPage("Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating vehicle");
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle with ID: {VehicleId}", Vehicle?.Id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the vehicle.");
                return Page();
            }
        }
    }
}