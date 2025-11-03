using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.DTOs.DeliveryDTOs;
using EVDealerSales.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVDealerSales.Presentation.Pages.Staff
{
    [Authorize(Roles = "DealerStaff,DealerManager")]
    public class ManageDeliveriesModel : PageModel
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ILogger<ManageDeliveriesModel> _logger;

        public ManageDeliveriesModel(
            IDeliveryService deliveryService,
            ILogger<ManageDeliveriesModel> logger)
        {
            _deliveryService = deliveryService;
            _logger = logger;
        }

        public Pagination<DeliveryResponseDto> Deliveries { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DeliveryStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var filter = new DeliveryFilterDto
                {
                    SearchTerm = SearchTerm,
                    Status = Status,
                    FromDate = FromDate,
                    ToDate = ToDate
                };

                Deliveries = await _deliveryService.GetAllDeliveriesAsync(PageNumber, PageSize, filter);

                return Page();
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading deliveries");
                TempData["ErrorMessage"] = "An error occurred while loading deliveries";
                Deliveries = new Pagination<DeliveryResponseDto>(
                    new List<DeliveryResponseDto>(), 0, PageNumber, PageSize);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync([FromBody] ConfirmDeliveryRequest request)
        {
            try
            {
                _logger.LogInformation("Confirming delivery {DeliveryId}", request.DeliveryId);

                var confirmDto = new ConfirmDeliveryRequestDto
                {
                    PlannedDate = request.PlannedDate,
                    StaffNotes = request.StaffNotes
                };

                var result = await _deliveryService.ConfirmDeliveryAsync(request.DeliveryId, confirmDto);

                return new JsonResult(new { 
                    success = true, 
                    message = "Delivery confirmed and scheduled successfully",
                    delivery = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delivery confirmation");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming delivery");
                return new JsonResult(new { success = false, message = "An error occurred while confirming delivery" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync([FromBody] UpdateStatusRequest request)
        {
            try
            {
                _logger.LogInformation("Updating delivery {DeliveryId} status to {Status}", request.DeliveryId, request.Status);

                var updateDto = new UpdateDeliveryStatusRequestDto
                {
                    Status = request.Status,
                    PlannedDate = request.PlannedDate,
                    ActualDate = request.ActualDate
                };

                var result = await _deliveryService.UpdateDeliveryStatusAsync(request.DeliveryId, updateDto);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Delivery status updated to {request.Status}",
                    delivery = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delivery status update");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status");
                return new JsonResult(new { success = false, message = "An error occurred while updating delivery" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnPostCancelAsync([FromBody] CancelDeliveryStaffRequest request)
        {
            try
            {
                _logger.LogInformation("Staff cancelling delivery {DeliveryId}", request.DeliveryId);

                var result = await _deliveryService.CancelDeliveryAsync(request.DeliveryId);

                if (result == null)
                {
                    return new JsonResult(new { success = false, message = "Delivery not found" }) { StatusCode = 404 };
                }

                return new JsonResult(new { 
                    success = true, 
                    message = "Delivery cancelled successfully" 
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delivery cancellation");
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling delivery");
                return new JsonResult(new { success = false, message = "An error occurred while cancelling delivery" }) { StatusCode = 500 };
            }
        }
    }

    public class ConfirmDeliveryRequest
    {
        public Guid DeliveryId { get; set; }
        public DateTime PlannedDate { get; set; }
        public string? StaffNotes { get; set; }
    }

    public class UpdateStatusRequest
    {
        public Guid DeliveryId { get; set; }
        public DeliveryStatus Status { get; set; }
        public DateTime? PlannedDate { get; set; }
        public DateTime? ActualDate { get; set; }
    }

    public class CancelDeliveryStaffRequest
    {
        public Guid DeliveryId { get; set; }
    }
}