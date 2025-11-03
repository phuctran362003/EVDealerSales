using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Vehicle
{
    [Authorize(Policy = "ManagerPolicy")]
    public class IndexModel : PageModel
    {
        private readonly IVehicleService _vehicleService;
        private readonly IChatbotService? _chatbotService;
        private readonly ILogger<IndexModel> _logger;

        // Single constructor: IChatbotService is optional (nullable) so DI can still construct this page model
        public IndexModel(IVehicleService vehicleService, ILogger<IndexModel> logger, IChatbotService? chatbotService = null)
        {
            _vehicleService = vehicleService;
            _logger = logger;
            _chatbotService = chatbotService;
        }

    public Pagination<VehicleResponseDto> Vehicles { get; set; } = new Pagination<VehicleResponseDto>(new List<VehicleResponseDto>(), 0, 1, 10);

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        // Search and Filter Properties
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinRange { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MaxRange { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinBattery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MaxBattery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinSpeed { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MaxSpeed { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinCharging { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MaxCharging { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ModelYear { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool SortDesc { get; set; }

    // Chat properties for manager chat with chatbot
    [BindProperty]
    public string ChatPrompt { get; set; } = string.Empty;

    [TempData]
    public string? ChatResponse { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading vehicles list page (Page: {PageNumber}, Size: {PageSize})",
                    PageNumber, PageSize);

                // Build filter DTO
                var filter = new VehicleFilterDto
                {
                    SearchTerm = SearchTerm,
                    MinBasePrice = MinPrice,
                    MaxBasePrice = MaxPrice,
                    MinRangeKM = MinRange,
                    MaxRangeKM = MaxRange,
                    MinBatteryCapacity = MinBattery,
                    MaxBatteryCapacity = MaxBattery,
                    MinTopSpeed = MinSpeed,
                    MaxTopSpeed = MaxSpeed,
                    MinChargingTime = MinCharging,
                    MaxChargingTime = MaxCharging,
                    ModelYear = ModelYear,
                    SortBy = SortBy,
                    SortDescending = SortDesc
                };

                Vehicles = await _vehicleService.GetAllVehiclesAsync(
                    pageNumber: PageNumber,
                    pageSize: PageSize,
                    includeInactive: true,
                    filter: filter
                );

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicles list");
                TempData["ErrorMessage"] = "An error occurred while loading vehicles.";
                Vehicles = new Pagination<VehicleResponseDto>(new List<VehicleResponseDto>(), 0, 1, PageSize);
                return Page();
            }
        }

        // POST handler for asking the chatbot from the manager UI
        public async Task<IActionResult> OnPostAskChatbotAsync()
        {
            if (string.IsNullOrWhiteSpace(ChatPrompt))
            {
                TempData["ErrorMessage"] = "Please enter a question for the chatbot.";
                return RedirectToPage(new { PageNumber, PageSize, SearchTerm, SortBy, SortDesc });
            }

            try
            {
                _logger.LogInformation("Manager asked chatbot: {Prompt}", ChatPrompt);

                if (_chatbotService == null)
                {
                    _logger.LogWarning("IChatbotService not available via DI.");
                    TempData["ErrorMessage"] = "Chat service is not available.";
                    return RedirectToPage(new { PageNumber, PageSize, SearchTerm, SortBy, SortDesc });
                }

                var response = await _chatbotService.FreestyleAskAsync(ChatPrompt, groupId: Guid.NewGuid().ToString());
                ChatResponse = response;
                TempData["SuccessMessage"] = "Chatbot replied successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while asking chatbot");
                TempData["ErrorMessage"] = "An error occurred while contacting the chatbot.";
            }

            return RedirectToPage(new { PageNumber, PageSize, SearchTerm, SortBy, SortDesc });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete vehicle with ID: {VehicleId}", id);

                var result = await _vehicleService.DeleteVehicleAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Vehicle deleted successfully.";
                    _logger.LogInformation("Vehicle with ID {VehicleId} deleted successfully", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Vehicle not found.";
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for deletion", id);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete vehicle with ID: {VehicleId}", id);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for delete vehicle with ID: {VehicleId}", id);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle with ID: {VehicleId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the vehicle.";
            }

            return RedirectToPage(new
            {
                PageNumber,
                PageSize,
                SearchTerm,
                MinPrice,
                MaxPrice,
                MinRange,
                MaxRange,
                MinBattery,
                MaxBattery,
                MinSpeed,
                MaxSpeed,
                MinCharging,
                MaxCharging,
                ModelYear,
                SortBy,
                SortDesc
            });
        }

        public IActionResult OnPostClearFilters()
        {
            return RedirectToPage(new { PageNumber = 1, PageSize });
        }
    }
}