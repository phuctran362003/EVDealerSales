using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Staff
{
    [Authorize(Roles = "DealerStaff,DealerManager")]
    public class CustomersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<CustomersModel> _logger;

        public CustomersModel(IUserService userService, ILogger<CustomersModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public Pagination<UserResponseDto> Customers { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var filter = new UserFilterDto
                {
                    SearchTerm = SearchTerm,
                    FromDate = FromDate,
                    ToDate = ToDate
                };

                Customers = await _userService.GetAllCustomersAsync(PageNumber, PageSize, filter);

                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers");
                TempData["ErrorMessage"] = "An error occurred while loading customers";
                Customers = new Pagination<UserResponseDto>(
                    new List<UserResponseDto>(), 0, PageNumber, PageSize);
                return Page();
            }
        }

        public async Task<IActionResult> OnGetDetailsAsync(Guid id)
        {
            try
            {
                var customer = await _userService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return new JsonResult(new { success = false, message = "Customer not found" }) { StatusCode = 404 };
                }

                return new JsonResult(new { success = true, customer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer details");
                return new JsonResult(new { success = false, message = "An error occurred" }) { StatusCode = 500 };
            }
        }
    }
}