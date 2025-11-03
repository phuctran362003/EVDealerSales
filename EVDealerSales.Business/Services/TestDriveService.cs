using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess.Entities;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Business.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TestDriveService> _logger;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        private const int TEST_DRIVE_DURATION_HOURS = 2;

        public TestDriveService(
            IUnitOfWork unitOfWork,
            ILogger<TestDriveService> logger,
            IClaimsService claimsService,
            ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        public async Task<TestDriveResponseDto> RegisterTestDriveAsync(CreateTestDriveRequestDto request)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation("User {UserId} registering test drive for vehicle {VehicleId}",
                    currentUserId, request.VehicleId);

                // Validate scheduled time
                if (request.ScheduledAt <= _currentTime.GetCurrentTime())
                {
                    throw new ArgumentException("Scheduled time must be in the future");
                }

                // Get customer by email
                var customer = await _unitOfWork.Users.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Email == request.CustomerEmail && !u.IsDeleted);

                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with email {request.CustomerEmail} not found");
                }

                // Note: Assuming all users can book, but typically should be Customer role
                // Add role check if needed based on business requirements

                // Check availability
                var (isAvailable, reason) = await CheckAvailabilityAsync(
                    request.VehicleId,
                    request.CustomerEmail,
                    request.ScheduledAt);

                if (!isAvailable)
                {
                    throw new InvalidOperationException($"Cannot book test drive: {reason}");
                }

                // Verify vehicle exists and is active
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                {
                    throw new KeyNotFoundException($"Vehicle with ID {request.VehicleId} not found");
                }

                if (!vehicle.IsActive)
                {
                    throw new InvalidOperationException("This vehicle is not available for test drives");
                }

                // Create test drive
                var testDrive = new TestDrive
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    VehicleId = request.VehicleId,
                    ScheduledAt = request.ScheduledAt,
                    Status = TestDriveStatus.Pending,
                    Notes = request.Notes,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    IsDeleted = false
                };

                await _unitOfWork.TestDrives.AddAsync(testDrive);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} registered successfully by customer {CustomerId}",
                    testDrive.Id, customer.Id);

                return await MapToResponseDto(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering test drive for vehicle {VehicleId}", request.VehicleId);
                throw;
            }
        }

        public async Task<TestDriveResponseDto> RegisterTestDriveByStaffAsync(CreateTestDriveRequestDto request)
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
                    throw new UnauthorizedAccessException("Only staff can register test drives for customers");
                }

                _logger.LogInformation("Staff {StaffId} registering test drive for customer {CustomerEmail}",
                    currentUserId, request.CustomerEmail);

                // Validate scheduled time
                if (request.ScheduledAt <= _currentTime.GetCurrentTime())
                {
                    throw new ArgumentException("Scheduled time must be in the future");
                }

                // Get customer by email
                var customer = await _unitOfWork.Users.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Email == request.CustomerEmail && !u.IsDeleted);

                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with email {request.CustomerEmail} not found");
                }

                // Note: Assuming all users can book, but typically should be Customer role

                // Check availability
                var (isAvailable, reason) = await CheckAvailabilityAsync(
                    request.VehicleId,
                    request.CustomerEmail,
                    request.ScheduledAt);

                if (!isAvailable)
                {
                    throw new InvalidOperationException($"Cannot book test drive: {reason}");
                }

                // Verify vehicle exists and is active
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                {
                    throw new KeyNotFoundException($"Vehicle with ID {request.VehicleId} not found");
                }

                if (!vehicle.IsActive)
                {
                    throw new InvalidOperationException("This vehicle is not available for test drives");
                }

                // Create test drive with Confirmed status (staff registers directly)
                var testDrive = new TestDrive
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    VehicleId = request.VehicleId,
                    ScheduledAt = request.ScheduledAt,
                    Status = TestDriveStatus.Confirmed,
                    StaffId = currentUserId,
                    ConfirmedAt = _currentTime.GetCurrentTime(),
                    Notes = request.Notes,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    IsDeleted = false
                };

                await _unitOfWork.TestDrives.AddAsync(testDrive);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} registered and confirmed by staff {StaffId}",
                    testDrive.Id, currentUserId);

                return await MapToResponseDto(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering test drive by staff for vehicle {VehicleId}", request.VehicleId);
                throw;
            }
        }

        public async Task<Pagination<TestDriveResponseDto>> GetAllTestDrivesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            TestDriveFilterDto? filter = null)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation("User {UserId} fetching test drives (Page: {PageNumber}, PageSize: {PageSize})",
                    currentUserId, pageNumber, pageSize);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // Base query
                var query = _unitOfWork.TestDrives.GetQueryable()
                    .Include(td => td.Customer)
                    .Include(td => td.Vehicle)
                    .Include(td => td.Staff)
                    .Where(td => !td.IsDeleted);

                // Apply filters
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.CustomerEmail))
                    {
                        query = query.Where(td => td.Customer.Email == filter.CustomerEmail);
                    }

                    if (filter.VehicleId.HasValue)
                    {
                        query = query.Where(td => td.VehicleId == filter.VehicleId);
                    }

                    if (filter.Status.HasValue)
                    {
                        query = query.Where(td => td.Status == filter.Status);
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(td => td.ScheduledAt >= filter.FromDate);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(td => td.ScheduledAt <= filter.ToDate);
                    }

                    if (filter.StaffId.HasValue)
                    {
                        query = query.Where(td => td.StaffId == filter.StaffId);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(td =>
                            td.Customer.FullName.ToLower().Contains(searchTerm) ||
                            td.Vehicle.ModelName.ToLower().Contains(searchTerm) ||
                            td.Vehicle.TrimName.ToLower().Contains(searchTerm));
                    }
                }

                // Order by scheduled date descending
                query = query.OrderByDescending(td => td.ScheduledAt);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var testDrives = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = new List<TestDriveResponseDto>();
                foreach (var td in testDrives)
                {
                    responseDtos.Add(await MapToResponseDto(td));
                }

                return new Pagination<TestDriveResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching test drives");
                throw;
            }
        }

        public async Task<TestDriveResponseDto?> GetTestDriveByIdAsync(Guid id)
        {
            try
            {
                var testDrive = await _unitOfWork.TestDrives.GetQueryable()
                    .Include(td => td.Customer)
                    .Include(td => td.Vehicle)
                    .Include(td => td.Staff)
                    .FirstOrDefaultAsync(td => td.Id == id && !td.IsDeleted);

                if (testDrive == null)
                {
                    return null;
                }

                return await MapToResponseDto(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching test drive {TestDriveId}", id);
                throw;
            }
        }

        public async Task<TestDriveResponseDto?> ConfirmTestDriveAsync(Guid testDriveId, string? notes = null)
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
                    throw new UnauthorizedAccessException("Only staff can confirm test drives");
                }

                var testDrive = await _unitOfWork.TestDrives.GetQueryable()
                    .Include(td => td.Customer)
                    .Include(td => td.Vehicle)
                    .FirstOrDefaultAsync(td => td.Id == testDriveId && !td.IsDeleted);

                if (testDrive == null)
                {
                    throw new KeyNotFoundException($"Test drive with ID {testDriveId} not found");
                }

                if (testDrive.Status != TestDriveStatus.Pending)
                {
                    throw new InvalidOperationException($"Can only confirm pending test drives. Current status: {testDrive.Status}");
                }

                // Check if vehicle is still available
                var (isAvailable, reason) = await CheckAvailabilityAsync(
                    testDrive.VehicleId,
                    testDrive.Customer.Email,
                    testDrive.ScheduledAt,
                    excludeTestDriveId: testDriveId);

                if (!isAvailable)
                {
                    throw new InvalidOperationException($"Cannot confirm test drive: {reason}");
                }

                testDrive.Status = TestDriveStatus.Confirmed;
                testDrive.StaffId = currentUserId;
                testDrive.ConfirmedAt = _currentTime.GetCurrentTime();
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    testDrive.Notes = notes;
                }
                testDrive.UpdatedAt = _currentTime.GetCurrentTime();
                testDrive.UpdatedBy = currentUserId;

                await _unitOfWork.TestDrives.Update(testDrive);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} confirmed by staff {StaffId}",
                    testDriveId, currentUserId);

                return await MapToResponseDto(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming test drive {TestDriveId}", testDriveId);
                throw;
            }
        }

        public async Task<TestDriveResponseDto?> CancelTestDriveAsync(Guid testDriveId, string? cancellationReason = null)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var testDrive = await _unitOfWork.TestDrives.GetQueryable()
                    .Include(td => td.Customer)
                    .Include(td => td.Vehicle)
                    .Include(td => td.Staff)
                    .FirstOrDefaultAsync(td => td.Id == testDriveId && !td.IsDeleted);

                if (testDrive == null)
                {
                    throw new KeyNotFoundException($"Test drive with ID {testDriveId} not found");
                }

                // Only customer who booked or staff can cancel
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }

                bool isOwner = testDrive.CustomerId == currentUserId;
                bool isStaff = currentUser.Role == RoleType.DealerStaff || currentUser.Role == RoleType.DealerManager;

                if (!isOwner && !isStaff)
                {
                    throw new UnauthorizedAccessException("You don't have permission to cancel this test drive");
                }

                if (testDrive.Status == TestDriveStatus.Canceled)
                {
                    throw new InvalidOperationException("Test drive is already canceled");
                }

                if (testDrive.Status == TestDriveStatus.Completed)
                {
                    throw new InvalidOperationException("Cannot cancel a completed test drive");
                }

                testDrive.Status = TestDriveStatus.Canceled;
                testDrive.CanceledAt = _currentTime.GetCurrentTime();
                testDrive.CancellationReason = cancellationReason;
                testDrive.UpdatedAt = _currentTime.GetCurrentTime();
                testDrive.UpdatedBy = currentUserId;

                await _unitOfWork.TestDrives.Update(testDrive);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} canceled by user {UserId}",
                    testDriveId, currentUserId);

                return await MapToResponseDto(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling test drive {TestDriveId}", testDriveId);
                throw;
            }
        }

        public async Task<TestDriveResponseDto?> CompleteTestDriveAsync(Guid testDriveId, string? notes = null)
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
                    throw new UnauthorizedAccessException("Only staff can complete test drives");
                }

                var testDrive = await _unitOfWork.TestDrives.GetQueryable()
                    .Include(td => td.Customer)
                    .Include(td => td.Vehicle)
                    .Include(td => td.Staff)
                    .FirstOrDefaultAsync(td => td.Id == testDriveId && !td.IsDeleted);

                if (testDrive == null)
                {
                    throw new KeyNotFoundException($"Test drive with ID {testDriveId} not found");
                }

                if (testDrive.ScheduledAt > _currentTime.GetCurrentTime())
                {
                    throw new InvalidOperationException("Cannot complete a test drive before its scheduled time");
                }

                if (testDrive.Status != TestDriveStatus.Confirmed)
                {
                    throw new InvalidOperationException($"Can only complete confirmed test drives. Current status: {testDrive.Status}");
                }

                testDrive.Status = TestDriveStatus.Completed;
                testDrive.CompletedAt = _currentTime.GetCurrentTime();
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    testDrive.Notes = string.IsNullOrWhiteSpace(testDrive.Notes)
                        ? notes
                        : $"{testDrive.Notes}\n{notes}";
                }
                testDrive.UpdatedAt = _currentTime.GetCurrentTime();
                testDrive.UpdatedBy = currentUserId;

                await _unitOfWork.TestDrives.Update(testDrive);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} completed by staff {StaffId}",
                    testDriveId, currentUserId);

                return await MapToResponseDto(testDrive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing test drive {TestDriveId}", testDriveId);
                throw;
            }
        }

        public async Task<(bool IsAvailable, string? Reason)> CheckAvailabilityAsync(
            Guid vehicleId,
            string customerEmail,
            DateTime scheduledAt,
            Guid? excludeTestDriveId = null)
        {
            try
            {
                // Check if scheduled time is in the past
                if (scheduledAt <= _currentTime.GetCurrentTime())
                {
                    return (false, "Scheduled time must be in the future");
                }

                // Get customer
                var customer = await _unitOfWork.Users.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Email == customerEmail && !u.IsDeleted);

                if (customer == null)
                {
                    return (false, "Customer not found");
                }

                var testDriveEnd = scheduledAt.AddHours(TEST_DRIVE_DURATION_HOURS);

                // Check if customer has any overlapping test drive in the same time period
                var overlappingCustomerTestDrives = await _unitOfWork.TestDrives.GetQueryable()
                    .Where(td => td.CustomerId == customer.Id
                        && !td.IsDeleted
                        && (td.Status == TestDriveStatus.Pending || td.Status == TestDriveStatus.Confirmed)
                        && (excludeTestDriveId == null || td.Id != excludeTestDriveId))
                    .ToListAsync();

                foreach (var existingTestDrive in overlappingCustomerTestDrives)
                {
                    var existingEnd = existingTestDrive.ScheduledAt.AddHours(TEST_DRIVE_DURATION_HOURS);

                    // Check for time overlap
                    if (scheduledAt < existingEnd && testDriveEnd > existingTestDrive.ScheduledAt)
                    {
                        return (false, $"Customer already has a {existingTestDrive.Status.ToString().ToLower()} test drive that overlaps with this time. Existing test drive: {existingTestDrive.ScheduledAt:yyyy-MM-dd HH:mm} - {existingEnd:yyyy-MM-dd HH:mm}");
                    }
                }

                // Check if vehicle has overlapping test drive


                var overlappingTestDrive = await _unitOfWork.TestDrives.GetQueryable()
                    .Where(td => td.VehicleId == vehicleId
                        && !td.IsDeleted
                        && (td.Status == TestDriveStatus.Pending || td.Status == TestDriveStatus.Confirmed)
                        && (excludeTestDriveId == null || td.Id != excludeTestDriveId))
                    .ToListAsync();

                foreach (var td in overlappingTestDrive)
                {
                    var existingEnd = td.ScheduledAt.AddHours(TEST_DRIVE_DURATION_HOURS);

                    // Check for overlap
                    if (scheduledAt < existingEnd && testDriveEnd > td.ScheduledAt)
                    {
                        return (false, $"Vehicle is already booked from {td.ScheduledAt:yyyy-MM-dd HH:mm} to {existingEnd:yyyy-MM-dd HH:mm}");
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for vehicle {VehicleId}", vehicleId);
                throw;
            }
        }

        public async Task<Pagination<TestDriveResponseDto>> GetMyTestDrivesAsync(
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

                _logger.LogInformation("User {UserId} fetching their test drives", currentUserId);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _unitOfWork.TestDrives.GetQueryable()
                    .Include(td => td.Customer)
                    .Include(td => td.Vehicle)
                    .Include(td => td.Staff)
                    .Where(td => td.CustomerId == currentUserId && !td.IsDeleted)
                    .OrderByDescending(td => td.ScheduledAt);

                var totalCount = await query.CountAsync();

                var testDrives = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = new List<TestDriveResponseDto>();
                foreach (var td in testDrives)
                {
                    responseDtos.Add(await MapToResponseDto(td));
                }

                return new Pagination<TestDriveResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user's test drives");
                throw;
            }
        }

        private async Task<TestDriveResponseDto> MapToResponseDto(TestDrive testDrive)
        {
            // Ensure navigation properties are loaded
            if (testDrive.Customer == null)
            {
                testDrive.Customer = await _unitOfWork.Users.GetByIdAsync(testDrive.CustomerId)
                    ?? throw new InvalidOperationException("Customer not found");
            }

            if (testDrive.Vehicle == null)
            {
                testDrive.Vehicle = await _unitOfWork.Vehicles.GetByIdAsync(testDrive.VehicleId)
                    ?? throw new InvalidOperationException("Vehicle not found");
            }

            User? staff = null;
            if (testDrive.StaffId.HasValue)
            {
                staff = testDrive.Staff ?? await _unitOfWork.Users.GetByIdAsync(testDrive.StaffId.Value);
            }

            return new TestDriveResponseDto
            {
                Id = testDrive.Id,
                CustomerId = testDrive.CustomerId,
                CustomerName = testDrive.Customer.FullName,
                CustomerEmail = testDrive.Customer.Email,
                CustomerPhone = testDrive.Customer.PhoneNumber,
                VehicleId = testDrive.VehicleId,
                VehicleModelName = testDrive.Vehicle.ModelName,
                VehicleTrimName = testDrive.Vehicle.TrimName,
                VehicleImageUrl = testDrive.Vehicle.ImageUrl,
                ScheduledAt = testDrive.ScheduledAt,
                Status = testDrive.Status,
                Notes = testDrive.Notes,
                StaffId = testDrive.StaffId,
                StaffName = staff?.FullName,
                StaffEmail = staff?.Email,
                CreatedAt = testDrive.CreatedAt,
                ConfirmedAt = testDrive.ConfirmedAt,
                CompletedAt = testDrive.CompletedAt,
                CanceledAt = testDrive.CanceledAt,
                CancellationReason = testDrive.CancellationReason
            };
        }
    }
}
