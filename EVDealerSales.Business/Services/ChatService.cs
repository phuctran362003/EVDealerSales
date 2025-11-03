using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.ChatDTOs;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Business.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChatService> _logger;
        private const string CHAT_CACHE_KEY_PREFIX = "ChatMessages_";
        private const int CACHE_DURATION_HOURS = 24;

        public ChatService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            IMemoryCache cache,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<ChatConversationDto>> GetAvailableUsersAsync()
        {
            var currentUserId = _claimsService.GetCurrentUserId;
            if (currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                throw new KeyNotFoundException("Current user not found");
            }

            _logger.LogInformation("Getting available users for {Role}", currentUser.Role);

            List<ChatConversationDto> availableUsers = new();

            // Customer can chat with Staff and Manager
            if (currentUser.Role == RoleType.Customer)
            {
                var staffAndManagers = await _unitOfWork.Users.GetQueryable()
                    .Where(u => !u.IsDeleted &&
                               (u.Role == RoleType.DealerStaff || u.Role == RoleType.DealerManager))
                    .Select(u => new { u.Id, u.FullName, u.Role })
                    .ToListAsync();

                availableUsers = staffAndManagers.Select(u => new ChatConversationDto
                {
                    UserId = u.Id,
                    UserName = u.FullName,
                    UserRole = u.Role.ToString()
                }).ToList();
            }
            // Staff can chat with Customers and Manager
            else if (currentUser.Role == RoleType.DealerStaff)
            {
                var customersAndManagers = await _unitOfWork.Users.GetQueryable()
                    .Where(u => !u.IsDeleted &&
                               (u.Role == RoleType.Customer || u.Role == RoleType.DealerManager))
                    .Select(u => new { u.Id, u.FullName, u.Role })
                    .ToListAsync();

                availableUsers = customersAndManagers.Select(u => new ChatConversationDto
                {
                    UserId = u.Id,
                    UserName = u.FullName,
                    UserRole = u.Role.ToString()
                }).ToList();
            }
            // Manager can chat with Staff
            else if (currentUser.Role == RoleType.DealerManager)
            {
                var staff = await _unitOfWork.Users.GetQueryable()
                    .Where(u => !u.IsDeleted && u.Role == RoleType.DealerStaff)
                    .Select(u => new { u.Id, u.FullName, u.Role })
                    .ToListAsync();

                availableUsers = staff.Select(u => new ChatConversationDto
                {
                    UserId = u.Id,
                    UserName = u.FullName,
                    UserRole = u.Role.ToString()
                }).ToList();
            }

            // Add last message and unread count from cache
            foreach (var user in availableUsers)
            {
                var cacheKey = GetCacheKey(currentUserId, user.UserId);
                if (_cache.TryGetValue(cacheKey, out List<ChatMessageDto>? messages) && messages != null)
                {
                    var lastMessage = messages.LastOrDefault();
                    if (lastMessage != null)
                    {
                        user.LastMessage = lastMessage.Message;
                        user.LastMessageTime = lastMessage.SentAt;
                    }

                    user.UnreadCount = messages.Count(m =>
                        m.SenderId == user.UserId &&
                        m.ReceiverId == currentUserId &&
                        !m.IsRead);
                }
            }

            return availableUsers.OrderByDescending(u => u.LastMessageTime).ToList();
        }

        public async Task<List<ChatMessageDto>> GetChatHistoryAsync(Guid otherUserId)
        {
            var currentUserId = _claimsService.GetCurrentUserId;
            if (currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var cacheKey = GetCacheKey(currentUserId, otherUserId);

            if (_cache.TryGetValue(cacheKey, out List<ChatMessageDto>? messages))
            {
                return messages ?? new List<ChatMessageDto>();
            }

            return new List<ChatMessageDto>();
        }

        public async Task<ChatMessageDto> SaveMessageAsync(SendMessageDto messageDto)
        {
            var currentUserId = _claimsService.GetCurrentUserId;
            if (currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Get sender and receiver info
            var sender = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            var receiver = await _unitOfWork.Users.GetByIdAsync(messageDto.ReceiverId);

            if (sender == null || receiver == null)
            {
                throw new KeyNotFoundException("Sender or receiver not found");
            }

            // Validate chat permissions
            if (!CanUsersChat(sender.Role, receiver.Role))
            {
                throw new UnauthorizedAccessException("You don't have permission to chat with this user");
            }

            var chatMessage = new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                SenderId = currentUserId,
                SenderName = sender.FullName,
                SenderRole = sender.Role.ToString(),
                ReceiverId = messageDto.ReceiverId,
                ReceiverName = receiver.FullName,
                Message = messageDto.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            // Store in cache for both sender and receiver
            var cacheKey = GetCacheKey(currentUserId, messageDto.ReceiverId);

            var messages = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CACHE_DURATION_HOURS);
                return new List<ChatMessageDto>();
            }) ?? new List<ChatMessageDto>();

            messages.Add(chatMessage);
            _cache.Set(cacheKey, messages, TimeSpan.FromHours(CACHE_DURATION_HOURS));

            _logger.LogInformation("Message saved from {SenderId} to {ReceiverId}", currentUserId, messageDto.ReceiverId);

            return chatMessage;
        }

        public async Task MarkMessagesAsReadAsync(Guid senderId)
        {
            var currentUserId = _claimsService.GetCurrentUserId;
            if (currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var cacheKey = GetCacheKey(currentUserId, senderId);

            if (_cache.TryGetValue(cacheKey, out List<ChatMessageDto>? messages) && messages != null)
            {
                foreach (var message in messages.Where(m => m.SenderId == senderId && !m.IsRead))
                {
                    message.IsRead = true;
                }

                _cache.Set(cacheKey, messages, TimeSpan.FromHours(CACHE_DURATION_HOURS));
            }

            await Task.CompletedTask;
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var currentUserId = _claimsService.GetCurrentUserId;
            if (currentUserId == Guid.Empty)
            {
                return 0;
            }

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return 0;
            }

            int totalUnread = 0;

            // Get all potential chat partners
            var availableUsers = await GetAvailableUsersAsync();

            foreach (var user in availableUsers)
            {
                totalUnread += user.UnreadCount;
            }

            return totalUnread;
        }

        private static string GetCacheKey(Guid userId1, Guid userId2)
        {
            // Create a consistent cache key regardless of order
            var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToList();
            return $"{CHAT_CACHE_KEY_PREFIX}{sortedIds[0]}_{sortedIds[1]}";
        }

        private static bool CanUsersChat(RoleType role1, RoleType role2)
        {
            // Customer can chat with Staff or Manager
            if (role1 == RoleType.Customer && (role2 == RoleType.DealerStaff || role2 == RoleType.DealerManager))
                return true;

            // Staff can chat with Customer or Manager
            if (role1 == RoleType.DealerStaff && (role2 == RoleType.Customer || role2 == RoleType.DealerManager))
                return true;

            // Manager can chat with Staff or Customer
            if (role1 == RoleType.DealerManager && (role2 == RoleType.DealerStaff || role2 == RoleType.Customer))
                return true;

            return false;
        }
    }
}
