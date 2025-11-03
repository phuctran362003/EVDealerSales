using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.Presentation.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public LoginModel(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    Response.Redirect(ReturnUrl);
                }
                else
                {
                    Response.Redirect("/Home/LandingPage");
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var loginRequest = new LoginRequestDto
                {
                    Email = Email,
                    Password = Password
                };

                var result = await _authService.LoginAsync(loginRequest, _configuration);

                if (result == null)
                {
                    ErrorMessage = "Email & Password may not correct.";
                    return Page();
                }

                // Lưu token vào session
                HttpContext.Session.SetString("AuthToken", result.Token);

                // Redirect to return URL or home page
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("/Home/LandingPage");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
        public async Task<IActionResult> OnPostLogout()
        {
            HttpContext.Session.Remove("AuthToken");
            return RedirectToPage("/Home/LandingPage");
        }
    }
}
