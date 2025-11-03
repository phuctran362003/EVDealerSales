using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    [Authorize(Policy = "ManagerPolicy")]
    public class DetailsModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IVehicleService vehicleService, ILogger<DetailsModel> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public VehicleResponseDto? Vehicle { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Loading details page for vehicle ID: {VehicleId}", id);

                Vehicle = await _vehicleService.GetVehicleByIdAsync(id);

                if (Vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found", id);
                    TempData["ErrorMessage"] = "Vehicle not found.";
                    return RedirectToPage("Index");
                }

                return Page();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while loading vehicle details");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicle details for ID: {VehicleId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading vehicle details.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete vehicle with ID: {VehicleId} from details page", id);

                var deleted = await _vehicleService.DeleteVehicleAsync(id);

                if (deleted)
                {
                    TempData["SuccessMessage"] = "Vehicle deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Vehicle not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle with ID: {VehicleId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the vehicle.";
            }

            return RedirectToPage("Index");
        }
    }
}