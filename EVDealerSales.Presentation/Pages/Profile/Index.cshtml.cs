using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IUserService userService, ILogger<IndexModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public UserResponseDto UserProfile { get; set; } = null!;

        [BindProperty]
        public UpdateProfileRequestDto UpdateProfileRequest { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var profile = await _userService.GetMyProfileAsync();
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Profile not found";
                    return RedirectToPage("/Index");
                }

                UserProfile = profile;
                UpdateProfileRequest = new UpdateProfileRequestDto
                {
                    FullName = profile.FullName,
                    PhoneNumber = profile.PhoneNumber
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
                TempData["ErrorMessage"] = "An error occurred while loading your profile";
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload profile data
                var profile = await _userService.GetMyProfileAsync();
                if (profile != null)
                {
                    UserProfile = profile;
                }
                return Page();
            }

            try
            {
                await _userService.UpdateMyProfileAsync(UpdateProfileRequest);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid profile update");
                ModelState.AddModelError(string.Empty, ex.Message);

                // Reload profile data
                var profile = await _userService.GetMyProfileAsync();
                if (profile != null)
                {
                    UserProfile = profile;
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile");

                // Reload profile data
                var profile = await _userService.GetMyProfileAsync();
                if (profile != null)
                {
                    UserProfile = profile;
                }
                return Page();
            }
        }
    }
}