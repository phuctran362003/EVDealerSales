using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.TestDriveDTOs;

namespace EVDealerSales.Business.Interfaces
{
    public interface ITestDriveService
    {
        Task<TestDriveResponseDto> RegisterTestDriveAsync(CreateTestDriveRequestDto request);

        Task<TestDriveResponseDto> RegisterTestDriveByStaffAsync(CreateTestDriveRequestDto request);

        Task<Pagination<TestDriveResponseDto>> GetAllTestDrivesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            TestDriveFilterDto? filter = null);

        Task<TestDriveResponseDto?> GetTestDriveByIdAsync(Guid id);

        Task<TestDriveResponseDto?> ConfirmTestDriveAsync(Guid testDriveId, string? notes = null);

        Task<TestDriveResponseDto?> CancelTestDriveAsync(Guid testDriveId, string? cancellationReason = null);

        Task<TestDriveResponseDto?> CompleteTestDriveAsync(Guid testDriveId, string? notes = null);

        Task<Pagination<TestDriveResponseDto>> GetMyTestDrivesAsync(
            int pageNumber = 1,
            int pageSize = 10);
    }
}
