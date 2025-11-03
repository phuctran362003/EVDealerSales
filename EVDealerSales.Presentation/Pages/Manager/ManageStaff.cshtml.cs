using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Manager
{
    [Authorize(Roles = "DealerManager")]
    public class ManageStaffModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<ManageStaffModel> _logger;

        public ManageStaffModel(IUserService userService, ILogger<ManageStaffModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public Pagination<UserResponseDto> Staff { get; set; } = null!;

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

                Staff = await _userService.GetAllStaffAsync(PageNumber, PageSize, filter);

                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff");
                TempData["ErrorMessage"] = "An error occurred while loading staff";
                Staff = new Pagination<UserResponseDto>(
                    new List<UserResponseDto>(), 0, PageNumber, PageSize);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync([FromBody] CreateStaffRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new staff");

                var createDto = new CreateStaffRequestDto
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password
                };

                var result = await _userService.CreateStaffAsync(createDto);

                return new JsonResult(new
                {
                    success = true,
                    message = "Staff created successfully",
                    staff = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid staff creation");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                return new JsonResult(new { success = false, message = "An error occurred while creating staff" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync([FromBody] UpdateStaffRequest request)
        {
            try
            {
                _logger.LogInformation("Updating staff {StaffId}", request.StaffId);

                var updateDto = new UpdateStaffRequestDto
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    NewPassword = request.NewPassword
                };

                var result = await _userService.UpdateStaffAsync(request.StaffId, updateDto);

                return new JsonResult(new
                {
                    success = true,
                    message = "Staff updated successfully",
                    staff = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid staff update");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff");
                return new JsonResult(new { success = false, message = "An error occurred while updating staff" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteStaffRequest request)
        {
            try
            {
                _logger.LogInformation("Deleting staff {StaffId}", request.StaffId);

                await _userService.DeleteStaffAsync(request.StaffId);

                return new JsonResult(new
                {
                    success = true,
                    message = "Staff deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff");
                return new JsonResult(new { success = false, message = "An error occurred while deleting staff" }) { StatusCode = 500 };
            }
        }
    }

    public class CreateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateStaffRequest
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
    }

    public class DeleteStaffRequest
    {
        public Guid StaffId { get; set; }
    }
}