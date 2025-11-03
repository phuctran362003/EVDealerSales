namespace EVDealerSales.BusinessObject.DTOs.ChatDTOs
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public Guid? ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class SendMessageDto
    {
        public Guid ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ChatConversationDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
