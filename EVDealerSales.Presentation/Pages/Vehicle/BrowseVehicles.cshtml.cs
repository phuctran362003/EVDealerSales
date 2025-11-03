using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using EVDealerSales.Business.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    public class BrowseVehiclesModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        [BindProperty(SupportsGet = true)]
        public VehicleFilterDto Filter { get; set; } = new VehicleFilterDto();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public Pagination<VehicleResponseDto> Vehicles { get; set; } = new Pagination<VehicleResponseDto>(new List<VehicleResponseDto>(), 0, 1, 10);

        public async Task OnGetAsync()
        {
            var currentSortBy = Request.Query["sortBy"].ToString();
            var currentAscendingString = Request.Query["ascending"].ToString();
            var currentIsAscending = (currentAscendingString != "false");

            Filter.SortDescending = !currentIsAscending;
            Filter.SearchTerm = Request.Query["search"].ToString();

            Filter.SortBy = currentSortBy switch
            {
                "ModelName" => "name",
                "ModelYear" => "year",
                "BasePrice" => "price",
                _ => Filter.SortBy
            };

            var vehiclesResult = await _vehicleService.GetAllVehiclesAsync(
                pageNumber: PageNumber,
                pageSize: PageSize,
                includeInactive: false,
                filter: Filter);

            Vehicles = vehiclesResult;

            ViewData["Search"] = Filter.SearchTerm;
            ViewData["SortBy"] = currentSortBy;
            ViewData["Ascending"] = currentAscendingString;
        }

        public BrowseVehiclesModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }
    }
}