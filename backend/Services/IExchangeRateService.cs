using CurrencyTrendConverter.Models;

namespace CurrencyTrendConverter.Services;

public interface IExchangeRateService
{
    IReadOnlyCollection<string> SupportedCurrencies { get; }

    Task<LatestRateResponse> GetLatestRateAsync(string baseCurrency, string targetCurrency, decimal amount);

    Task<HistoryResponse> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, int days);
}
