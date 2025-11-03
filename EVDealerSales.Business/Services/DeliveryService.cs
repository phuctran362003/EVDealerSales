using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.DeliveryDTOs;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess.Entities;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Business.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeliveryService> _logger;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        public DeliveryService(
            IUnitOfWork unitOfWork,
            ILogger<DeliveryService> logger,
            IClaimsService claimsService,
            ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        public async Task<DeliveryResponseDto> RequestDeliveryAsync(CreateDeliveryRequestDto request)
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
                    throw new UnauthorizedAccessException("Only customers can request deliveries");
                }

                _logger.LogInformation("Customer {CustomerId} requesting delivery for order {OrderId}",
                    currentUserId, request.OrderId);

                // Validate order exists and belongs to the customer
                var order = await _unitOfWork.Orders.GetQueryable()
                    .Include(o => o.Customer)
                    .Include(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Include(o => o.Invoices).ThenInclude(i => i.Payments)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {request.OrderId} not found");
                }

                if (order.CustomerId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You can only request delivery for your own orders");
                }

                // Check if order is paid
                var hasPaidPayment = order.Invoices
                    .SelectMany(i => i.Payments)
                    .Any(p => p.Status == PaymentStatus.Paid);

                if (!hasPaidPayment)
                {
                    throw new InvalidOperationException("Cannot request delivery for unpaid order");
                }

                // Check if order is confirmed
                if (order.Status != OrderStatus.Confirmed)
                {
                    throw new InvalidOperationException("Can only request delivery for confirmed orders");
                }

                // Check if request is within 24 hours of order confirmation
                if (order.UpdatedAt.HasValue)
                {
                    var hoursSinceConfirmation = (_currentTime.GetCurrentTime() - order.UpdatedAt.Value).TotalHours;
                    if (hoursSinceConfirmation > 24)
                    {
                        throw new InvalidOperationException("Delivery request must be made within 24 hours after order confirmation");
                    }
                }

                // Check if delivery already exists
                var existingDelivery = await _unitOfWork.Deliveries.GetQueryable()
                    .FirstOrDefaultAsync(d => d.OrderId == request.OrderId && !d.IsDeleted);

                if (existingDelivery != null)
                {
                    throw new InvalidOperationException("Delivery request already exists for this order");
                }

                // Create delivery with Pending status
                var delivery = new Delivery
                {
                    Id = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    Status = DeliveryStatus.Pending,
                    ShippingAddress = request.ShippingAddress,
                    Notes = request.Notes,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                await _unitOfWork.Deliveries.AddAsync(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Delivery request {DeliveryId} created for order {OrderId} with Pending status",
                    delivery.Id, request.OrderId);

                return await MapToResponseDto(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting delivery for order {OrderId}", request.OrderId);
                throw;
            }
        }

        public async Task<DeliveryResponseDto> ConfirmDeliveryAsync(Guid deliveryId, ConfirmDeliveryRequestDto request)
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
                    throw new UnauthorizedAccessException("Only staff can confirm deliveries");
                }

                _logger.LogInformation("Staff {StaffId} confirming delivery {DeliveryId}",
                    currentUserId, deliveryId);

                var delivery = await _unitOfWork.Deliveries.GetQueryable()
                    .Include(d => d.Order).ThenInclude(o => o.Customer)
                    .Include(d => d.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);

                if (delivery == null)
                {
                    throw new KeyNotFoundException($"Delivery with ID {deliveryId} not found");
                }

                if (delivery.Status != DeliveryStatus.Pending)
                {
                    throw new InvalidOperationException($"Cannot confirm delivery with status {delivery.Status}");
                }

                // Update delivery to Scheduled status
                delivery.Status = DeliveryStatus.Scheduled;
                delivery.PlannedDate = request.PlannedDate;
                delivery.StaffNotes = request.StaffNotes;
                delivery.UpdatedAt = _currentTime.GetCurrentTime();
                delivery.UpdatedBy = currentUserId;

                await _unitOfWork.Deliveries.Update(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Delivery {DeliveryId} confirmed and scheduled for {PlannedDate}",
                    deliveryId, request.PlannedDate);

                return await MapToResponseDto(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming delivery {DeliveryId}", deliveryId);
                throw;
            }
        }

        public async Task<DeliveryResponseDto?> GetDeliveryByIdAsync(Guid id)
        {
            try
            {
                var delivery = await _unitOfWork.Deliveries.GetQueryable()
                    .Include(d => d.Order).ThenInclude(o => o.Customer)
                    .Include(d => d.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (delivery == null)
                {
                    return null;
                }

                return await MapToResponseDto(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching delivery {DeliveryId}", id);
                throw;
            }
        }

        public async Task<DeliveryResponseDto?> GetDeliveryByOrderIdAsync(Guid orderId)
        {
            try
            {
                var delivery = await _unitOfWork.Deliveries.GetQueryable()
                    .Include(d => d.Order).ThenInclude(o => o.Customer)
                    .Include(d => d.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(d => d.OrderId == orderId && !d.IsDeleted);

                if (delivery == null)
                {
                    return null;
                }

                return await MapToResponseDto(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching delivery for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<Pagination<DeliveryResponseDto>> GetAllDeliveriesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            DeliveryFilterDto? filter = null)
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
                    throw new UnauthorizedAccessException("Only staff can view all deliveries");
                }

                _logger.LogInformation("Staff {StaffId} fetching deliveries (Page: {PageNumber}, PageSize: {PageSize})",
                    currentUserId, pageNumber, pageSize);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _unitOfWork.Deliveries.GetQueryable()
                    .Include(d => d.Order).ThenInclude(o => o.Customer)
                    .Include(d => d.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .Where(d => !d.IsDeleted);

                // Apply filters
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(d =>
                            d.Order.OrderNumber.ToLower().Contains(searchTerm) ||
                            (d.Order.Customer != null && d.Order.Customer.FullName.ToLower().Contains(searchTerm)) ||
                            (d.Order.Customer != null && d.Order.Customer.Email.ToLower().Contains(searchTerm)));
                    }

                    if (filter.Status.HasValue)
                    {
                        query = query.Where(d => d.Status == filter.Status.Value);
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(d => d.PlannedDate >= filter.FromDate.Value || d.ActualDate >= filter.FromDate.Value);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(d => d.PlannedDate <= filter.ToDate.Value || d.ActualDate <= filter.ToDate.Value);
                    }
                }

                // Order by creation date descending
                query = query.OrderByDescending(d => d.CreatedAt);

                var totalCount = await query.CountAsync();

                var deliveries = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = new List<DeliveryResponseDto>();
                foreach (var delivery in deliveries)
                {
                    responseDtos.Add(await MapToResponseDto(delivery));
                }

                return new Pagination<DeliveryResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching deliveries");
                throw;
            }
        }

        public async Task<DeliveryResponseDto?> UpdateDeliveryStatusAsync(Guid id, UpdateDeliveryStatusRequestDto request)
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
                    throw new UnauthorizedAccessException("Only staff can update delivery status");
                }

                var delivery = await _unitOfWork.Deliveries.GetQueryable()
                    .Include(d => d.Order).ThenInclude(o => o.Customer)
                    .Include(d => d.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (delivery == null)
                {
                    throw new KeyNotFoundException($"Delivery with ID {id} not found");
                }

                _logger.LogInformation("Staff {StaffId} updating delivery {DeliveryId} status from {OldStatus} to {NewStatus}",
                    currentUserId, id, delivery.Status, request.Status);

                // Validate status transition
                if (delivery.Status == DeliveryStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot update cancelled delivery");
                }

                if (delivery.Status == DeliveryStatus.Delivered)
                {
                    throw new InvalidOperationException("Cannot update delivered delivery");
                }

                // Validate status flow: Pending -> Scheduled -> InTransit -> Delivered
                if (request.Status == DeliveryStatus.InTransit && delivery.Status != DeliveryStatus.Scheduled)
                {
                    throw new InvalidOperationException("Delivery must be Scheduled before setting to InTransit");
                }

                if (request.Status == DeliveryStatus.Delivered && delivery.Status != DeliveryStatus.InTransit)
                {
                    throw new InvalidOperationException("Delivery must be InTransit before setting to Delivered");
                }

                // Update status
                delivery.Status = request.Status;

                // Update planned date if provided
                if (request.PlannedDate.HasValue)
                {
                    delivery.PlannedDate = request.PlannedDate.Value;
                }

                // If status is Delivered, set actual date
                if (request.Status == DeliveryStatus.Delivered)
                {
                    delivery.ActualDate = request.ActualDate ?? _currentTime.GetCurrentTime();
                }

                delivery.UpdatedAt = _currentTime.GetCurrentTime();
                delivery.UpdatedBy = currentUserId;

                await _unitOfWork.Deliveries.Update(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Delivery {DeliveryId} status updated to {Status}",
                    id, request.Status);

                return await MapToResponseDto(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery {DeliveryId}", id);
                throw;
            }
        }

        public async Task<DeliveryResponseDto?> CancelDeliveryAsync(Guid id)
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

                var delivery = await _unitOfWork.Deliveries.GetQueryable()
                    .Include(d => d.Order).ThenInclude(o => o.Customer)
                    .Include(d => d.Order).ThenInclude(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (delivery == null)
                {
                    throw new KeyNotFoundException($"Delivery with ID {id} not found");
                }

                // Customer can only cancel their own pending deliveries
                if (currentUser.Role == RoleType.Customer)
                {
                    if (delivery.Order.CustomerId != currentUserId)
                    {
                        throw new UnauthorizedAccessException("You can only cancel your own deliveries");
                    }

                    if (delivery.Status != DeliveryStatus.Pending)
                    {
                        throw new InvalidOperationException("Customer can only cancel pending delivery requests");
                    }
                }
                // Staff can cancel Pending or Scheduled deliveries
                else if (currentUser.Role == RoleType.DealerStaff || currentUser.Role == RoleType.DealerManager)
                {
                    if (delivery.Status != DeliveryStatus.Pending && delivery.Status != DeliveryStatus.Scheduled)
                    {
                        throw new InvalidOperationException("Cannot cancel delivery in progress or completed");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("Unauthorized to cancel delivery");
                }

                _logger.LogInformation("User {UserId} cancelling delivery {DeliveryId}",
                    currentUserId, id);

                delivery.Status = DeliveryStatus.Cancelled;
                delivery.UpdatedAt = _currentTime.GetCurrentTime();
                delivery.UpdatedBy = currentUserId;

                await _unitOfWork.Deliveries.Update(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Delivery {DeliveryId} cancelled", id);

                return await MapToResponseDto(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling delivery {DeliveryId}", id);
                throw;
            }
        }

        private async Task<DeliveryResponseDto> MapToResponseDto(Delivery delivery)
        {
            // Ensure navigation properties are loaded
            if (delivery.Order == null)
            {
                delivery.Order = await _unitOfWork.Orders.GetQueryable()
                    .Include(o => o.Customer)
                    .Include(o => o.Items).ThenInclude(oi => oi.Vehicle)
                    .FirstOrDefaultAsync(o => o.Id == delivery.OrderId)
                    ?? throw new InvalidOperationException("Order not found");
            }

            var vehicleInfo = delivery.Order.Items != null && delivery.Order.Items.Any()
                ? string.Join(", ", delivery.Order.Items.Select(oi => $"{oi.Vehicle?.ModelName} {oi.Vehicle?.TrimName}"))
                : "N/A";

            return new DeliveryResponseDto
            {
                Id = delivery.Id,
                OrderId = delivery.OrderId,
                OrderNumber = delivery.Order.OrderNumber,
                CustomerId = delivery.Order.CustomerId,
                CustomerName = delivery.Order.Customer?.FullName ?? "Unknown",
                CustomerEmail = delivery.Order.Customer?.Email ?? "Unknown",
                CustomerPhone = delivery.Order.Customer?.PhoneNumber,
                PlannedDate = delivery.PlannedDate,
                ActualDate = delivery.ActualDate,
                Status = delivery.Status,
                VehicleInfo = vehicleInfo,
                ShippingAddress = delivery.ShippingAddress,
                Notes = delivery.Notes,
                StaffNotes = delivery.StaffNotes,
                CreatedAt = delivery.CreatedAt,
                UpdatedAt = delivery.UpdatedAt
            };
        }
    }
}