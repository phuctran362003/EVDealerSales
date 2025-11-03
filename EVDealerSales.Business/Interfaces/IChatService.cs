using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVDealerSales.BusinessObject.DTOs.ChatDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface IChatService
    {
        // Get list of users available for chat based on role
        Task<List<ChatConversationDto>> GetAvailableUsersAsync();
        
        // Get chat history for a conversation (from in-memory store)
        Task<List<ChatMessageDto>> GetChatHistoryAsync(Guid otherUserId);
        
        // Store message in session/memory
        Task<ChatMessageDto> SaveMessageAsync(SendMessageDto messageDto);
        
        // Mark messages as read
        Task MarkMessagesAsReadAsync(Guid senderId);
        
        // Get unread message count
        Task<int> GetUnreadCountAsync();
    }
}
