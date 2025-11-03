using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using EVDealerSales.DataAccess.Entities;
using EVDealerSales.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Business.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleService> _logger;
        private readonly IClaimsService _claimsService;

        public VehicleService(IUnitOfWork unitOfWork, ILogger<VehicleService> logger, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
        }

        public async Task<Pagination<VehicleResponseDto>> GetAllVehiclesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            bool includeInactive = false,
            VehicleFilterDto? filter = null)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                var ipAddress = _claimsService.IpAddress;

                _logger.LogInformation("User {UserId} from {IpAddress} fetching vehicles (Page: {PageNumber}, PageSize: {PageSize}, IncludeInactive: {IncludeInactive}, HasFilter: {HasFilter})",
                    currentUserId, ipAddress, pageNumber, pageSize, includeInactive, filter != null);

                // Validate pagination parameters
                if (pageNumber < 1)
                {
                    _logger.LogWarning("Invalid page number: {PageNumber}. Setting to 1", pageNumber);
                    pageNumber = 1;
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    _logger.LogWarning("Invalid page size: {PageSize}. Setting to 10", pageSize);
                    pageSize = 10;
                }

                // Start with base query
                var query = _unitOfWork.Vehicles.GetQueryable()
                    .Where(v => !v.IsDeleted && (includeInactive || v.IsActive));

                // Apply filters if provided
                if (filter != null)
                {
                    // Search by name (ModelName or TrimName)
                    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    {
                        var searchTerm = filter.SearchTerm.Trim().ToLower();
                        query = query.Where(v =>
                            v.ModelName.ToLower().Contains(searchTerm) ||
                            v.TrimName.ToLower().Contains(searchTerm));

                        _logger.LogDebug("Applied search filter: {SearchTerm}", filter.SearchTerm);
                    }

                    // Filter by Battery Capacity
                    if (filter.MinBatteryCapacity.HasValue)
                    {
                        query = query.Where(v => v.BatteryCapacity >= filter.MinBatteryCapacity.Value);
                        _logger.LogDebug("Applied MinBatteryCapacity filter: {Value}", filter.MinBatteryCapacity);
                    }
                    if (filter.MaxBatteryCapacity.HasValue)
                    {
                        query = query.Where(v => v.BatteryCapacity <= filter.MaxBatteryCapacity.Value);
                        _logger.LogDebug("Applied MaxBatteryCapacity filter: {Value}", filter.MaxBatteryCapacity);
                    }

                    // Filter by Top Speed
                    if (filter.MinTopSpeed.HasValue)
                    {
                        query = query.Where(v => v.TopSpeed >= filter.MinTopSpeed.Value);
                        _logger.LogDebug("Applied MinTopSpeed filter: {Value}", filter.MinTopSpeed);
                    }
                    if (filter.MaxTopSpeed.HasValue)
                    {
                        query = query.Where(v => v.TopSpeed <= filter.MaxTopSpeed.Value);
                        _logger.LogDebug("Applied MaxTopSpeed filter: {Value}", filter.MaxTopSpeed);
                    }

                    // Filter by Charging Time
                    if (filter.MinChargingTime.HasValue)
                    {
                        query = query.Where(v => v.ChargingTime >= filter.MinChargingTime.Value);
                        _logger.LogDebug("Applied MinChargingTime filter: {Value}", filter.MinChargingTime);
                    }
                    if (filter.MaxChargingTime.HasValue)
                    {
                        query = query.Where(v => v.ChargingTime <= filter.MaxChargingTime.Value);
                        _logger.LogDebug("Applied MaxChargingTime filter: {Value}", filter.MaxChargingTime);
                    }

                    // Filter by Base Price
                    if (filter.MinBasePrice.HasValue)
                    {
                        query = query.Where(v => v.BasePrice >= filter.MinBasePrice.Value);
                        _logger.LogDebug("Applied MinBasePrice filter: {Value}", filter.MinBasePrice);
                    }
                    if (filter.MaxBasePrice.HasValue)
                    {
                        query = query.Where(v => v.BasePrice <= filter.MaxBasePrice.Value);
                        _logger.LogDebug("Applied MaxBasePrice filter: {Value}", filter.MaxBasePrice);
                    }

                    // Filter by Range
                    if (filter.MinRangeKM.HasValue)
                    {
                        query = query.Where(v => v.RangeKM >= filter.MinRangeKM.Value);
                        _logger.LogDebug("Applied MinRangeKM filter: {Value}", filter.MinRangeKM);
                    }
                    if (filter.MaxRangeKM.HasValue)
                    {
                        query = query.Where(v => v.RangeKM <= filter.MaxRangeKM.Value);
                        _logger.LogDebug("Applied MaxRangeKM filter: {Value}", filter.MaxRangeKM);
                    }

                    // Filter by Model Year
                    if (filter.ModelYear.HasValue)
                    {
                        query = query.Where(v => v.ModelYear == filter.ModelYear.Value);
                        _logger.LogDebug("Applied ModelYear filter: {Value}", filter.ModelYear);
                    }

                    // Apply sorting
                    if (!string.IsNullOrWhiteSpace(filter.SortBy))
                    {
                        query = filter.SortBy.ToLower() switch
                        {
                            "price" => filter.SortDescending
                                ? query.OrderByDescending(v => v.BasePrice)
                                : query.OrderBy(v => v.BasePrice),

                            "range" => filter.SortDescending
                                ? query.OrderByDescending(v => v.RangeKM)
                                : query.OrderBy(v => v.RangeKM),

                            "battery" => filter.SortDescending
                                ? query.OrderByDescending(v => v.BatteryCapacity)
                                : query.OrderBy(v => v.BatteryCapacity),

                            "speed" => filter.SortDescending
                                ? query.OrderByDescending(v => v.TopSpeed)
                                : query.OrderBy(v => v.TopSpeed),

                            "charging" => filter.SortDescending
                                ? query.OrderByDescending(v => v.ChargingTime)
                                : query.OrderBy(v => v.ChargingTime),

                            "name" => filter.SortDescending
                                ? query.OrderByDescending(v => v.ModelName)
                                : query.OrderBy(v => v.ModelName),

                            "year" => filter.SortDescending
                                ? query.OrderByDescending(v => v.ModelYear)
                                : query.OrderBy(v => v.ModelYear),

                            _ => query.OrderByDescending(v => v.CreatedAt)
                        };

                        _logger.LogDebug("Applied sorting: {SortBy} {Direction}",
                            filter.SortBy, filter.SortDescending ? "DESC" : "ASC");
                    }
                    else
                    {
                        // Default sort by creation date
                        query = query.OrderByDescending(v => v.CreatedAt);
                    }
                }
                else
                {
                    // Default sort by creation date when no filter
                    query = query.OrderByDescending(v => v.CreatedAt);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Get paginated items
                var vehicles = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(v => MapToResponseDto(v)).ToList();

                var paginatedResult = new Pagination<VehicleResponseDto>(
                    vehicleDtos,
                    totalCount,
                    pageNumber,
                    pageSize
                );

                _logger.LogInformation("Successfully fetched {Count} vehicles out of {TotalCount} (Page {PageNumber}/{TotalPages})",
                    vehicleDtos.Count, totalCount, pageNumber, paginatedResult.TotalPages);

                return paginatedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching vehicles");
                throw;
            }
        }

        public async Task<VehicleResponseDto?> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching vehicle with ID: {VehicleId}", id);

                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid vehicle ID provided");
                    throw new ArgumentException("Vehicle ID cannot be empty");
                }

                var vehicle = await _unitOfWork.Vehicles.FirstOrDefaultAsync(
                    predicate: v => v.Id == id && !v.IsDeleted
                );

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found", id);
                    return null;
                }

                _logger.LogInformation("Successfully fetched vehicle with ID: {VehicleId}", id);
                return MapToResponseDto(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching vehicle with ID: {VehicleId}", id);
                throw;
            }
        }

        public async Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleRequestDto request)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation("User {UserId} creating new vehicle: {ModelName} {TrimName}",
                    currentUserId, request?.ModelName, request?.TrimName);

                // Check if user is authenticated
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempting to create vehicle");
                    throw new UnauthorizedAccessException("User must be authenticated to create vehicles");
                }

                // Validate request
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "Vehicle request cannot be null");
                }

                // Validate Model Name
                if (string.IsNullOrWhiteSpace(request.ModelName))
                {
                    throw new ArgumentException("Model name is required");
                }
                if (request.ModelName.Length > 100)
                {
                    throw new ArgumentException("Model name cannot exceed 100 characters");
                }

                // Validate Trim Name
                if (string.IsNullOrWhiteSpace(request.TrimName))
                {
                    throw new ArgumentException("Trim name is required");
                }
                if (request.TrimName.Length > 100)
                {
                    throw new ArgumentException("Trim name cannot exceed 100 characters");
                }

                // Validate Model Year
                if (request.ModelYear.HasValue && (request.ModelYear < 2000 || request.ModelYear > 2100))
                {
                    throw new ArgumentException("Model year must be between 2000 and 2100");
                }

                // Validate Base Price
                if (request.BasePrice <= 0)
                {
                    throw new ArgumentException("Base price must be greater than 0");
                }

                // Validate Image URL
                if (string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    throw new ArgumentException("Image URL is required");
                }
                if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out _))
                {
                    throw new ArgumentException("Invalid URL format for image");
                }

                // Validate Battery Capacity
                if (request.BatteryCapacity < 1 || request.BatteryCapacity > 1000)
                {
                    throw new ArgumentException("Battery capacity must be between 1 and 1000 kWh");
                }

                // Validate Range
                if (request.RangeKM < 1 || request.RangeKM > 10000)
                {
                    throw new ArgumentException("Range must be between 1 and 10000 km");
                }

                // Validate Charging Time
                if (request.ChargingTime < 1 || request.ChargingTime > 1440)
                {
                    throw new ArgumentException("Charging time must be between 1 and 1440 minutes");
                }

                // Validate Top Speed
                if (request.TopSpeed < 1 || request.TopSpeed > 500)
                {
                    throw new ArgumentException("Top speed must be between 1 and 500 km/h");
                }

                // Validate Stock
                if (request.Stock < 0)
                {
                    throw new ArgumentException("Stock cannot be negative");
                }

                _logger.LogDebug("Vehicle request validation passed");

                // Create vehicle entity
                var vehicle = new Vehicle
                {
                    ModelName = request.ModelName.Trim(),
                    TrimName = request.TrimName.Trim(),
                    ModelYear = request.ModelYear,
                    BasePrice = request.BasePrice,
                    ImageUrl = request.ImageUrl.Trim(),
                    BatteryCapacity = request.BatteryCapacity,
                    RangeKM = request.RangeKM,
                    ChargingTime = request.ChargingTime,
                    TopSpeed = request.TopSpeed,
                    Stock = request.Stock,
                    IsActive = request.IsActive,
                    CreatedBy = currentUserId
                };

                // The GenericRepository will automatically set CreatedBy and CreatedAt
                await _unitOfWork.Vehicles.AddAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} successfully created vehicle with ID: {VehicleId}",
                    currentUserId, vehicle.Id);

                return MapToResponseDto(vehicle);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating vehicle");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized vehicle creation attempt");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating vehicle: {ModelName} {TrimName}",
                    request?.ModelName, request?.TrimName);
                throw;
            }
        }

        public async Task<VehicleResponseDto?> UpdateVehicleAsync(UpdateVehicleRequestDto request)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                _logger.LogInformation("User {UserId} updating vehicle with ID: {VehicleId}",
                    currentUserId, request?.Id);

                // Validate request
                if (request == null)
                {
                    _logger.LogWarning("Update request is null");
                    throw new ArgumentNullException(nameof(request), "Update request cannot be null");
                }

                if (request.Id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid vehicle ID for update");
                    throw new ArgumentException("Vehicle ID cannot be empty");
                }

                // Check if user is authenticated
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempting to update vehicle {VehicleId}", request.Id);
                    throw new UnauthorizedAccessException("User must be authenticated to update vehicles");
                }

                var vehicle = await _unitOfWork.Vehicles.FirstOrDefaultAsync(
                    predicate: v => v.Id == request.Id && !v.IsDeleted
                );

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for update", request.Id);
                    return null;
                }

                // Store old values for audit logging
                var hasChanges = false;
                var changes = new List<string>();

                // Update ModelName only if provided and different
                if (!string.IsNullOrWhiteSpace(request.ModelName) && request.ModelName != vehicle.ModelName)
                {
                    if (request.ModelName.Length > 100)
                    {
                        throw new ArgumentException("Model name cannot exceed 100 characters");
                    }
                    changes.Add($"ModelName: '{vehicle.ModelName}' → '{request.ModelName}'");
                    vehicle.ModelName = request.ModelName.Trim();
                    hasChanges = true;
                }

                // Update TrimName only if provided and different
                if (!string.IsNullOrWhiteSpace(request.TrimName) && request.TrimName != vehicle.TrimName)
                {
                    if (request.TrimName.Length > 100)
                    {
                        throw new ArgumentException("Trim name cannot exceed 100 characters");
                    }
                    changes.Add($"TrimName: '{vehicle.TrimName}' → '{request.TrimName}'");
                    vehicle.TrimName = request.TrimName.Trim();
                    hasChanges = true;
                }

                // Update ModelYear only if provided and different
                if (request.ModelYear.HasValue && request.ModelYear != vehicle.ModelYear)
                {
                    if (request.ModelYear < 2000 || request.ModelYear > 2100)
                    {
                        throw new ArgumentException("Model year must be between 2000 and 2100");
                    }
                    changes.Add($"ModelYear: '{vehicle.ModelYear}' → '{request.ModelYear}'");
                    vehicle.ModelYear = request.ModelYear;
                    hasChanges = true;
                }

                // Update BasePrice only if provided and different
                if (request.BasePrice > 0 && request.BasePrice != vehicle.BasePrice)
                {
                    changes.Add($"BasePrice: ${vehicle.BasePrice:N2} → ${request.BasePrice:N2}");
                    vehicle.BasePrice = request.BasePrice;
                    hasChanges = true;
                }

                // Update ImageUrl only if provided and different
                if (!string.IsNullOrWhiteSpace(request.ImageUrl) && request.ImageUrl != vehicle.ImageUrl)
                {
                    if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out _))
                    {
                        throw new ArgumentException("Invalid URL format for image");
                    }
                    changes.Add("ImageUrl updated");
                    vehicle.ImageUrl = request.ImageUrl.Trim();
                    hasChanges = true;
                }

                // Update BatteryCapacity only if provided and different
                if (request.BatteryCapacity > 0 && request.BatteryCapacity != vehicle.BatteryCapacity)
                {
                    changes.Add($"BatteryCapacity: {vehicle.BatteryCapacity} kWh → {request.BatteryCapacity} kWh");
                    vehicle.BatteryCapacity = request.BatteryCapacity;
                    hasChanges = true;
                }

                // Update RangeKM only if provided and different
                if (request.RangeKM > 0 && request.RangeKM != vehicle.RangeKM)
                {
                    if (request.RangeKM < 1 || request.RangeKM > 10000)
                    {
                        throw new ArgumentException("Range must be between 1 and 10000 km");
                    }
                    changes.Add($"RangeKM: {vehicle.RangeKM} km → {request.RangeKM} km");
                    vehicle.RangeKM = request.RangeKM;
                    hasChanges = true;
                }

                // Update ChargingTime only if provided and different
                if (request.ChargingTime > 0 && request.ChargingTime != vehicle.ChargingTime)
                {
                    if (request.ChargingTime < 1 || request.ChargingTime > 1440)
                    {
                        throw new ArgumentException("Charging time must be between 1 and 1440 minutes");
                    }
                    changes.Add($"ChargingTime: {vehicle.ChargingTime} min → {request.ChargingTime} min");
                    vehicle.ChargingTime = request.ChargingTime;
                    hasChanges = true;
                }

                // Update TopSpeed only if provided and different
                if (request.TopSpeed > 0 && request.TopSpeed != vehicle.TopSpeed)
                {
                    if (request.TopSpeed < 1 || request.TopSpeed > 500)
                    {
                        throw new ArgumentException("Top speed must be between 1 and 500 km/h");
                    }
                    changes.Add($"TopSpeed: {vehicle.TopSpeed} km/h → {request.TopSpeed} km/h");
                    vehicle.TopSpeed = request.TopSpeed;
                    hasChanges = true;
                }

                // Update Stock only if provided and different
                if (request.Stock >= 0 && request.Stock != vehicle.Stock)
                {
                    changes.Add($"Stock: {vehicle.Stock} → {request.Stock}");
                    vehicle.Stock = request.Stock;
                    hasChanges = true;
                }

                // Update IsActive (always allow this change as it's a boolean)
                if (request.IsActive != vehicle.IsActive)
                {
                    changes.Add($"IsActive: {vehicle.IsActive} → {request.IsActive}");
                    vehicle.IsActive = request.IsActive;
                    hasChanges = true;
                }

                // Only update if there are actual changes
                if (hasChanges)
                {
                    // The GenericRepository will automatically set UpdatedBy and UpdatedAt
                    await _unitOfWork.Vehicles.Update(vehicle);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("User {UserId} successfully updated vehicle {VehicleId}. Changes: {Changes}",
                        currentUserId, request.Id, string.Join(", ", changes));
                }
                else
                {
                    _logger.LogInformation("User {UserId} - No changes detected for vehicle with ID: {VehicleId}",
                        currentUserId, request.Id);
                }

                return MapToResponseDto(vehicle);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating vehicle");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized vehicle update attempt");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating vehicle with ID: {VehicleId}", request?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteVehicleAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                _logger.LogInformation("User {UserId} attempting to delete vehicle with ID: {VehicleId}",
                    currentUserId, id);

                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid vehicle ID for deletion");
                    throw new ArgumentException("Vehicle ID cannot be empty");
                }

                // Check if user is authenticated
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempting to delete vehicle {VehicleId}", id);
                    throw new UnauthorizedAccessException("User must be authenticated to delete vehicles");
                }

                var vehicle = await _unitOfWork.Vehicles.FirstOrDefaultAsync(
                    predicate: v => v.Id == id && !v.IsDeleted
                );

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for deletion", id);
                    return false;
                }

                // Check if vehicle has related orders
                var hasOrders = vehicle.OrderItems?.Any() == true;
                if (hasOrders)
                {
                    _logger.LogWarning("User {UserId} attempted to delete vehicle {VehicleId} which has related orders",
                        currentUserId, id);
                    throw new InvalidOperationException("Cannot delete vehicle with existing orders");
                }

                // The GenericRepository will automatically set DeletedBy and DeletedAt
                await _unitOfWork.Vehicles.SoftRemove(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} successfully soft-deleted vehicle with ID: {VehicleId} (Model: {ModelName} {TrimName})",
                    currentUserId, id, vehicle.ModelName, vehicle.TrimName);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized vehicle deletion attempt");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting vehicle with ID: {VehicleId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleVehicleStatusAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                _logger.LogInformation("User {UserId} toggling status for vehicle with ID: {VehicleId}",
                    currentUserId, id);

                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid vehicle ID for status toggle");
                    throw new ArgumentException("Vehicle ID cannot be empty");
                }

                // Check if user is authenticated
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempting to toggle vehicle status {VehicleId}", id);
                    throw new UnauthorizedAccessException("User must beAuthenticated to toggle vehicle status");
                }

                var vehicle = await _unitOfWork.Vehicles.FirstOrDefaultAsync(
                    predicate: v => v.Id == id && !v.IsDeleted
                );

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for status toggle", id);
                    return false;
                }

                var oldStatus = vehicle.IsActive;
                vehicle.IsActive = !vehicle.IsActive;

                await _unitOfWork.Vehicles.Update(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} successfully toggled status for vehicle {VehicleId}: {OldStatus} → {NewStatus}",
                    currentUserId, id, oldStatus, vehicle.IsActive);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized vehicle status toggle attempt");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling status for vehicle with ID: {VehicleId}", id);
                throw;
            }
        }

        #region Private Helper Methods

        private static VehicleResponseDto MapToResponseDto(Vehicle vehicle)
        {
            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                TrimName = vehicle.TrimName,
                ModelYear = vehicle.ModelYear,
                BasePrice = vehicle.BasePrice,
                ImageUrl = vehicle.ImageUrl,
                BatteryCapacity = vehicle.BatteryCapacity,
                RangeKM = vehicle.RangeKM,
                ChargingTime = vehicle.ChargingTime,
                TopSpeed = vehicle.TopSpeed,
                Stock = vehicle.Stock,
                IsActive = vehicle.IsActive,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

        #endregion
    }
}