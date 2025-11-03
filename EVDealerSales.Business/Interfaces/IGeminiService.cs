namespace EVDealerSales.Business.Interfaces
{
    public interface IGeminiService
    {
        Task<string> GetGeminiResponseAsync(string prompt);
    }
}
