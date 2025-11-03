using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.UserDTOs
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters")]
        public string? NewPassword { get; set; }
    }
}