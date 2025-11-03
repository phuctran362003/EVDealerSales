using System.ComponentModel.DataAnnotations;

namespace EVDealerSales.BusinessObject.DTOs.FeedbackDTOs
{
    // DTO for customer to create feedback
    public class CreateFeedbackRequestDto
    {
        public Guid? OrderId { get; set; }

        [Required(ErrorMessage = "Feedback content is required")]
        [MinLength(10, ErrorMessage = "Feedback must be at least 10 characters")]
        [MaxLength(2000, ErrorMessage = "Feedback cannot exceed 2000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}