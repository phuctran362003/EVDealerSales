namespace EVDealerSales.BusinessObject.DTOs.ChatDTOs
{
    // DTO used when presenting chat requests from the UI to the manager/chatbot features.
    public class AskManagerRequestDto
    {
        // Prompt is required for a meaningful request
        public string Prompt { get; set; } = string.Empty;

        // Optional group id to which responses should be broadcast
        public string? GroupId { get; set; }
    }
}
