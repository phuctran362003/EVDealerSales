namespace EVDealerSales.BusinessObject.Enums
{
    public enum DeliveryStatus
    {
        Pending = 0,      // Customer requested delivery, waiting for staff confirmation
        Scheduled = 1,    // Staff confirmed and scheduled delivery
        InTransit = 2,    // Delivery in progress
        Delivered = 3,    // Delivery completed
        Cancelled = 4     // Delivery cancelled
    }
}
