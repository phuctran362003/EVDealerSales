using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.FeedbackDTOs
{
    public class ResolveFeedbackRequestDto
    {
        [Required(ErrorMessage = "Resolution is required")]
        [MinLength(10, ErrorMessage = "Resolution must be at least 10 characters")]
        public string Resolution { get; set; } = string.Empty;
    }
}