using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    public class DetailVehiclesModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        public DetailVehiclesModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public VehicleResponseDto? Vehicle { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                Vehicle = await _vehicleService.GetVehicleByIdAsync(id);

                if (Vehicle == null)
                {
                    // Nếu không tìm thấy xe, trả về trang 404
                    return NotFound();
                }

                // Trả về Page() để hiển thị View
                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error");
            }
        }
    }
}
