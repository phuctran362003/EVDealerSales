using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.ChatDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace EVDealerSales.Presentation.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;
        
        // Store connected users and their connection IDs
        private static readonly ConcurrentDictionary<Guid, string> ConnectedUsers = new();

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                ConnectedUsers[userId] = Context.ConnectionId;
                _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
                
                // Notify user about their unread count
                var unreadCount = await _chatService.GetUnreadCountAsync();
                await Clients.Caller.SendAsync("ReceiveUnreadCount", unreadCount);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                ConnectedUsers.TryRemove(userId, out _);
                _logger.LogInformation("User {UserId} disconnected", userId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto messageDto)
        {
            try
            {
                _logger.LogInformation("SendMessage called: ReceiverId={ReceiverId}, Message={Message}", 
                    messageDto.ReceiverId, messageDto.Message);

                // Save message to cache
                var chatMessage = await _chatService.SaveMessageAsync(messageDto);

                // Send to receiver if online
                if (ConnectedUsers.TryGetValue(messageDto.ReceiverId, out var receiverConnectionId))
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", chatMessage);
                    _logger.LogInformation("Message sent to receiver {ReceiverId}", messageDto.ReceiverId);
                }

                // Send confirmation back to sender
                await Clients.Caller.SendAsync("MessageSent", chatMessage);

                // Update unread count for receiver
                if (ConnectedUsers.TryGetValue(messageDto.ReceiverId, out receiverConnectionId))
                {
                    // Get unread count for receiver
                    await Clients.Client(receiverConnectionId).SendAsync("UpdateUnreadCount", chatMessage.SenderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task MarkAsRead(Guid senderId)
        {
            try
            {
                await _chatService.MarkMessagesAsReadAsync(senderId);
                
                // Notify sender that messages were read
                if (ConnectedUsers.TryGetValue(senderId, out var senderConnectionId))
                {
                    await Clients.Client(senderConnectionId).SendAsync("MessagesMarkedAsRead", GetCurrentUserId());
                }

                _logger.LogInformation("Messages marked as read from {SenderId}", senderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task GetChatHistory(Guid otherUserId)
        {
            try
            {
                var messages = await _chatService.GetChatHistoryAsync(otherUserId);
                await Clients.Caller.SendAsync("ReceiveChatHistory", messages);
                
                _logger.LogInformation("Chat history retrieved for user {OtherUserId}", otherUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task GetAvailableUsers()
        {
            try
            {
                var users = await _chatService.GetAvailableUsersAsync();
                await Clients.Caller.SendAsync("ReceiveAvailableUsers", users);
                
                _logger.LogInformation("Available users retrieved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available users");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task NotifyTyping(Guid receiverId)
        {
            try
            {
                if (ConnectedUsers.TryGetValue(receiverId, out var receiverConnectionId))
                {
                    var userId = GetCurrentUserId();
                    await Clients.Client(receiverConnectionId).SendAsync("UserTyping", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying typing");
            }
        }

        public async Task StopTyping(Guid receiverId)
        {
            try
            {
                if (ConnectedUsers.TryGetValue(receiverId, out var receiverConnectionId))
                {
                    var userId = GetCurrentUserId();
                    await Clients.Client(receiverConnectionId).SendAsync("UserStoppedTyping", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping typing notification");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        public static bool IsUserOnline(Guid userId)
        {
            return ConnectedUsers.ContainsKey(userId);
        }
    }
}
