namespace EVDealerSales.BusinessObject.Enums
{
    public enum TestDriveStatus
    {
        Pending = 0,        // Đăng ký mới, chờ staff xác nhận
        Confirmed = 1,      // Staff đã xác nhận lịch hẹn
        Completed = 2,      // Đã hoàn thành buổi lái thử
        Canceled = 3,       // Đã hủy
    }
}
