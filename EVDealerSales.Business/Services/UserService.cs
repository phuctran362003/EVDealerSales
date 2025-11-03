using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.UserDTOs;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess.Entities;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        public UserService(
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger,
            IClaimsService claimsService,
            ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        #region Staff Management (Manager Only)

        public async Task<UserResponseDto> CreateStaffAsync(CreateStaffRequestDto request)
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
                    throw new UnauthorizedAccessException("Only managers can create staff");
                }

                _logger.LogInformation("Manager {ManagerId} creating new staff", currentUserId);

                // Check if email already exists
                var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("Email already exists");
                }

                // Hash password
                var passwordHasher = new PasswordHasher();
                var hashedPassword = passwordHasher.HashPassword(request.Password);

                // Create staff
                var staff = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = hashedPassword ?? throw new Exception("Password hashing failed"),
                    Role = RoleType.DealerStaff,
                    CreatedAt = _currentTime.GetCurrentTime(),
                    IsDeleted = false
                };

                await _unitOfWork.Users.AddAsync(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff {StaffId} created successfully", staff.Id);

                return MapToResponseDto(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                throw;
            }
        }

        public async Task<UserResponseDto?> GetStaffByIdAsync(Guid id)
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
                    throw new UnauthorizedAccessException("Only managers can view staff");
                }

                var staff = await _unitOfWork.Users.GetByIdAsync(id);
                if (staff == null || staff.IsDeleted || staff.Role != RoleType.DealerStaff)
                {
                    return null;
                }

                return MapToResponseDto(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff {StaffId}", id);
                throw;
            }
        }

        public async Task<Pagination<UserResponseDto>> GetAllStaffAsync(
            int pageNumber = 1,
            int pageSize = 10,
            UserFilterDto? filter = null)
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
                    throw new UnauthorizedAccessException("Only managers can view staff");
                }

                _logger.LogInformation("Manager {ManagerId} fetching staff list", currentUserId);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _unitOfWork.Users.GetQueryable()
                    .Where(u => u.Role == RoleType.DealerStaff && !u.IsDeleted);

                // Apply filters
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(u =>
                            u.FullName.ToLower().Contains(searchTerm) ||
                            u.Email.ToLower().Contains(searchTerm) ||
                            u.PhoneNumber.Contains(searchTerm));
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt >= filter.FromDate);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt <= filter.ToDate);
                    }
                }

                query = query.OrderByDescending(u => u.CreatedAt);

                var totalCount = await query.CountAsync();

                var staff = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = staff.Select(s => MapToResponseDto(s)).ToList();

                return new Pagination<UserResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staff list");
                throw;
            }
        }

        public async Task<UserResponseDto> UpdateStaffAsync(Guid id, UpdateStaffRequestDto request)
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
                    throw new UnauthorizedAccessException("Only managers can update staff");
                }

                _logger.LogInformation("Manager {ManagerId} updating staff {StaffId}", currentUserId, id);

                var staff = await _unitOfWork.Users.GetByIdAsync(id);
                if (staff == null || staff.IsDeleted || staff.Role != RoleType.DealerStaff)
                {
                    throw new KeyNotFoundException($"Staff with ID {id} not found");
                }

                // Update fields
                staff.FullName = request.FullName;
                staff.PhoneNumber = request.PhoneNumber;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    var passwordHasher = new PasswordHasher();
                    staff.PasswordHash = passwordHasher.HashPassword(request.NewPassword)
                        ?? throw new Exception("Password hashing failed");
                }

                staff.UpdatedAt = _currentTime.GetCurrentTime();
                staff.UpdatedBy = currentUserId;

                await _unitOfWork.Users.Update(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff {StaffId} updated successfully", id);

                return MapToResponseDto(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff {StaffId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStaffAsync(Guid id)
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
                    throw new UnauthorizedAccessException("Only managers can delete staff");
                }

                _logger.LogInformation("Manager {ManagerId} deleting staff {StaffId}", currentUserId, id);

                var staff = await _unitOfWork.Users.GetByIdAsync(id);
                if (staff == null || staff.IsDeleted || staff.Role != RoleType.DealerStaff)
                {
                    throw new KeyNotFoundException($"Staff with ID {id} not found");
                }

                staff.IsDeleted = true;
                staff.DeletedAt = _currentTime.GetCurrentTime();
                staff.DeletedBy = currentUserId;

                await _unitOfWork.Users.Update(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff {StaffId} deleted successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff {StaffId}", id);
                throw;
            }
        }

        #endregion

        #region Customer Management (Staff Only)

        public async Task<UserResponseDto?> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null ||
                    (currentUser.Role != RoleType.DealerStaff && currentUser.Role != RoleType.DealerManager))
                {
                    throw new UnauthorizedAccessException("Only staff can view customers");
                }

                var customer = await _unitOfWork.Users.GetByIdAsync(id);
                if (customer == null || customer.IsDeleted || customer.Role != RoleType.Customer)
                {
                    return null;
                }

                return MapToResponseDto(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer {CustomerId}", id);
                throw;
            }
        }

        public async Task<Pagination<UserResponseDto>> GetAllCustomersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            UserFilterDto? filter = null)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (currentUser == null ||
                    (currentUser.Role != RoleType.DealerStaff && currentUser.Role != RoleType.DealerManager))
                {
                    throw new UnauthorizedAccessException("Only staff can view customers");
                }

                _logger.LogInformation("Staff {StaffId} fetching customer list", currentUserId);

                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _unitOfWork.Users.GetQueryable()
                    .Where(u => u.Role == RoleType.Customer && !u.IsDeleted);

                // Apply filters
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.ToLower();
                        query = query.Where(u =>
                            u.FullName.ToLower().Contains(searchTerm) ||
                            u.Email.ToLower().Contains(searchTerm) ||
                            u.PhoneNumber.Contains(searchTerm));
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt >= filter.FromDate);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt <= filter.ToDate);
                    }
                }

                query = query.OrderByDescending(u => u.CreatedAt);

                var totalCount = await query.CountAsync();

                var customers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseDtos = customers.Select(c => MapToResponseDto(c)).ToList();

                return new Pagination<UserResponseDto>(responseDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer list");
                throw;
            }
        }

        #endregion

        #region Profile Management (All Users)

        public async Task<UserResponseDto?> GetMyProfileAsync()
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null || user.IsDeleted)
                {
                    return null;
                }

                return MapToResponseDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile");
                throw;
            }
        }

        public async Task<UserResponseDto> UpdateMyProfileAsync(UpdateProfileRequestDto request)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                if (currentUserId == Guid.Empty)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                _logger.LogInformation("User {UserId} updating their profile", currentUserId);

                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null || user.IsDeleted)
                {
                    throw new KeyNotFoundException("User not found");
                }

                // Update basic info
                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    // Verify current password
                    if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                    {
                        throw new InvalidOperationException("Current password is required to change password");
                    }

                    var passwordHasher = new PasswordHasher();
                    if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    {
                        throw new InvalidOperationException("Current password is incorrect");
                    }

                    user.PasswordHash = passwordHasher.HashPassword(request.NewPassword)
                        ?? throw new Exception("Password hashing failed");
                }

                user.UpdatedAt = _currentTime.GetCurrentTime();
                user.UpdatedBy = currentUserId;

                await _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} profile updated successfully", currentUserId);

                return MapToResponseDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsDeleted = user.IsDeleted
            };
        }

        #endregion
    }
}