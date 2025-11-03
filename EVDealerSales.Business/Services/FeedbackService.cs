using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.FeedbackDTOs;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess.Entities;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Business.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FeedbackService> _logger;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        public FeedbackService(
            IUnitOfWork unitOfWork,
            ILogger<FeedbackService> logger,
            IClaimsService claimsService,
            ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        public async Task<FeedbackResponseDto> CreateFeedbackAsync(CreateFeedbackRequestDto request)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null || currentUser.Role != RoleType.Customer)
                {
                    throw new UnauthorizedAccessException("Only customers can create feedback");
                }

                _logger.LogInformation("Customer {CustomerId} creating feedback", currentUserId);

                // Validate order if provided
                Order? order = null;
                if (request.OrderId.HasValue)
                {
                    order = await _unitOfWork.Orders.GetQueryable()
                        .Include(o => o.Customer)
                        .Include(o => o.Items).ThenInclude(oi => oi.Vehicle)
                        .Include(o => o.Delivery)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId.Value && !o.IsDeleted);

                    if (order == null)
                    {
                        throw new KeyNotFoundException($"Order with ID {request.OrderId.Value} not found");
                    }

                    if (order.CustomerId != currentUserId)
                    {
                        throw new UnauthorizedAccessException("You can only give feedback for your own orders");
                    }

                    // Check if order is confirmed (completed)
                    if (order.Status != OrderStatus.Confirmed)
                    {
                        throw new InvalidOperationException("You can only give feedback for confirmed orders");
                    }
                }

                // Create feedback
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid(),
                    CustomerId = currentUserId,
                    OrderId = request.OrderId,
                    Content = request.Content,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                await _unitOfWork.Feedbacks.AddAsync(feedback);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Feedback {FeedbackId} created successfully", feedback.Id);

                return await MapToResponseDto(feedback, currentUser, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback");
                throw;
            }
        }

        public async Task<FeedbackResponseDto?> GetFeedbackByIdAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var feedback = await _unitOfWork.Feedbacks.GetQueryable()
                    .Include(f => f.Customer)
                    .Include(f => f.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(f => f.Creator)
                    .Include(f => f.Resolver)
                    .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

                if (feedback == null)
                {
                    return null;
                }

                // Check authorization
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }

                // Customer can only view their own feedback
                if (currentUser.Role == RoleType.Customer && feedback.CustomerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You can only view your own feedback");
                }

                return await MapToResponseDto(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching feedback {FeedbackId}", id);
                throw;
            }
        }

        public async Task<Pagination<FeedbackResponseDto>> GetAllFeedbacksAsync(
            int pageNumber = 1,
            int pageSize = 10,
            FeedbackFilterDto? filter = null)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null || (currentUser.Role != RoleType.DealerStaff && currentUser.Role != RoleType.DealerManager))
                {
                    throw new UnauthorizedAccessException("Only staff can view all feedbacks");
                }

                _logger.LogInformation("Staff {StaffId} fetching feedbacks (Page: {PageNumber}, PageSize: {PageSize})",
                    currentUserId, pageNumber, pageSize);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _unitOfWork.Feedbacks.GetQueryable()
                    .Include(f => f.Customer)
                    .Include(f => f.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(f => f.Creator)
                    .Include(f => f.Resolver)
                    .Where(f => !f.IsDeleted);

                // Apply filters
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(f =>
                            f.Content.ToLower().Contains(searchTerm) ||
                            f.Customer.FullName.ToLower().Contains(searchTerm) ||
                            f.Customer.Email.ToLower().Contains(searchTerm) ||
                            (f.Order != null && f.Order.OrderNumber.ToLower().Contains(searchTerm)));
                    }

                    if (filter.IsResolved.HasValue)
                    {
                        query = query.Where(f => (f.ResolvedBy != null) == filter.IsResolved.Value);
                    }

                    if (filter.CustomerId.HasValue)
                    {
                        query = query.Where(f => f.CustomerId == filter.CustomerId.Value);
                    }

                    if (filter.OrderId.HasValue)
                    {
                        query = query.Where(f => f.OrderId == filter.OrderId.Value);
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(f => f.CreatedAt >= filter.FromDate.Value);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(f => f.CreatedAt <= filter.ToDate.Value);
                    }
                }

                // Order by creation date descending
                query = query.OrderByDescending(f => f.CreatedAt);

                var totalCount = await query.CountAsync();

                var feedbacks = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = new List<FeedbackResponseDto>();
                foreach (var feedback in feedbacks)
                {
                    responseDtos.Add(await MapToResponseDto(feedback));
                }

                return new Pagination<FeedbackResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching feedbacks");
                throw;
            }
        }

        public async Task<Pagination<FeedbackResponseDto>> GetMyFeedbacksAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null || currentUser.Role != RoleType.Customer)
                {
                    throw new UnauthorizedAccessException("Only customers can view their own feedbacks");
                }

                _logger.LogInformation("Customer {CustomerId} fetching their feedbacks", currentUserId);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _unitOfWork.Feedbacks.GetQueryable()
                    .Include(f => f.Customer)
                    .Include(f => f.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(f => f.Creator)
                    .Include(f => f.Resolver)
                    .Where(f => !f.IsDeleted && f.CustomerId == currentUserId)
                    .OrderByDescending(f => f.CreatedAt);

                var totalCount = await query.CountAsync();

                var feedbacks = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = new List<FeedbackResponseDto>();
                foreach (var feedback in feedbacks)
                {
                    responseDtos.Add(await MapToResponseDto(feedback));
                }

                return new Pagination<FeedbackResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer feedbacks");
                throw;
            }
        }

        public async Task<FeedbackResponseDto> ResolveFeedbackAsync(Guid id, ResolveFeedbackRequestDto request)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null || currentUser.Role != RoleType.DealerManager)
                {
                    throw new UnauthorizedAccessException("Only managers can resolve feedback");
                }

                _logger.LogInformation("Manager {ManagerId} resolving feedback {FeedbackId}", currentUserId, id);

                var feedback = await _unitOfWork.Feedbacks.GetQueryable()
                    .Include(f => f.Customer)
                    .Include(f => f.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(f => f.Creator)
                    .Include(f => f.Resolver)
                    .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

                if (feedback == null)
                {
                    throw new KeyNotFoundException($"Feedback with ID {id} not found");
                }

                if (feedback.ResolvedBy.HasValue)
                {
                    throw new InvalidOperationException("This feedback has already been resolved");
                }

                // Update feedback with resolution
                feedback.ResolvedBy = currentUserId;
                feedback.UpdatedAt = _currentTime.GetCurrentTime();
                feedback.UpdatedBy = currentUserId;

                await _unitOfWork.Feedbacks.Update(feedback);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Feedback {FeedbackId} resolved successfully", id);

                return await MapToResponseDto(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving feedback {FeedbackId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }

                var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
                if (feedback == null || feedback.IsDeleted)
                {
                    throw new KeyNotFoundException($"Feedback with ID {id} not found");
                }

                // Customer can delete their own feedback, Manager can delete any
                if (currentUser.Role == RoleType.Customer && feedback.CustomerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You can only delete your own feedback");
                }

                if (currentUser.Role != RoleType.Customer && currentUser.Role != RoleType.DealerManager)
                {
                    throw new UnauthorizedAccessException("Unauthorized to delete feedback");
                }

                _logger.LogInformation("User {UserId} deleting feedback {FeedbackId}", currentUserId, id);

                feedback.IsDeleted = true;
                feedback.DeletedAt = _currentTime.GetCurrentTime();
                feedback.DeletedBy = currentUserId;

                await _unitOfWork.Feedbacks.Update(feedback);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Feedback {FeedbackId} deleted successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback {FeedbackId}", id);
                throw;
            }
        }

        private async Task<FeedbackResponseDto> MapToResponseDto(Feedback feedback, User? customer = null, Order? order = null)
        {
            // Load navigation properties if not already loaded
            if (feedback.Customer == null && customer == null)
            {
                customer = await _unitOfWork.Users.GetByIdAsync(feedback.CustomerId);
            }
            else if (customer == null)
            {
                customer = feedback.Customer;
            }

            if (feedback.OrderId.HasValue && feedback.Order == null && order == null)
            {
                order = await _unitOfWork.Orders.GetQueryable()
                    .Include(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(o => o.Id == feedback.OrderId.Value);
            }
            else if (order == null)
            {
                order = feedback.Order;
            }

            User? resolver = null;
            if (feedback.ResolvedBy.HasValue)
            {
                resolver = feedback.Resolver ?? await _unitOfWork.Users.GetByIdAsync(feedback.ResolvedBy.Value);
            }

            User? creator = null;
            if (feedback.CreatedBy.HasValue && feedback.CreatedBy != feedback.CustomerId)
            {
                creator = feedback.Creator ?? await _unitOfWork.Users.GetByIdAsync(feedback.CreatedBy.Value);
            }

            var vehicleInfo = order?.Items != null && order.Items.Any()
                ? string.Join(", ", order.Items.Select(oi => $"{oi.Vehicle?.ModelName} {oi.Vehicle?.TrimName}"))
                : null;

            return new FeedbackResponseDto
            {
                Id = feedback.Id,
                CustomerId = feedback.CustomerId,
                CustomerName = customer?.FullName ?? "Unknown",
                CustomerEmail = customer?.Email ?? "Unknown",
                OrderId = feedback.OrderId,
                OrderNumber = order?.OrderNumber,
                VehicleInfo = vehicleInfo,
                Content = feedback.Content,
                IsResolved = feedback.ResolvedBy.HasValue,
                ResolvedBy = feedback.ResolvedBy,
                ResolverName = resolver?.FullName,
                ResolvedAt = feedback.UpdatedAt,
                CreatedByUserId = feedback.CreatedBy,
                CreatorName = creator?.FullName,
                CreatedAt = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt
            };
        }
    }
}