using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    public class CompareModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        public CompareModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public VehicleResponseDto? Vehicle1 { get; set; }
        public VehicleResponseDto? Vehicle2 { get; set; }
        
        public async Task<IActionResult> OnGetAsync(Guid? id1, Guid? id2)
        {
            if (!id1.HasValue || !id2.HasValue)
            {
                TempData["ErrorMessage"] = "Both vehicle IDs must be provided for comparison.";
                return RedirectToPage("./BrowseVehicles");

            }

            Vehicle1 = await _vehicleService.GetVehicleByIdAsync(id1.Value);
            Vehicle2 = await _vehicleService.GetVehicleByIdAsync(id2.Value);
            if (Vehicle1 == null || Vehicle2 == null)
            {
                TempData["ErrorMessage"] = "One or both vehicles not found.";
                return RedirectToPage("./BrowseVehicles");
            }

            return Page();
        }
    }
}
