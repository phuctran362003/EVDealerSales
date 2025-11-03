namespace EVDealerSales.DataAccess.Interfaces
{
    public interface IClaimsService
    {
        public Guid GetCurrentUserId { get; }

        public string? IpAddress { get; }
    }
}
