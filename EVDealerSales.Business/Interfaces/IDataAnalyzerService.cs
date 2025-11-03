using EVDealerSales.DataAccess.Entities;

namespace EVDealerSales.Business.Interfaces
{
    public interface IDataAnalyzerService
    {
        Task<IReadOnlyList<Vehicle>> AnalyzeVehiclesAsync();
        Task<IReadOnlyList<Order>> AnalyzeSalesAsync();
        Task<IReadOnlyList<Feedback>> AnalyzeFeedbacksAsync();
    }
}
