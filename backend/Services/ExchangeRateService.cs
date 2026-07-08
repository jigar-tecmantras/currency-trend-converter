using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CurrencyTrendConverter.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CurrencyTrendConverter.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ExchangeRateApiOptions _options;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(HttpClient httpClient, IMemoryCache cache, IOptions<ExchangeRateApiOptions> options, ILogger<ExchangeRateService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public IReadOnlyCollection<string> SupportedCurrencies => CurrencyData.SupportedCurrencies;

    public async Task<LatestRateResponse> GetLatestRateAsync(string baseCurrency, string targetCurrency, decimal amount)
    {
        var normalizedBase = CurrencyData.Normalize(baseCurrency);
        var normalizedTarget = CurrencyData.Normalize(targetCurrency);
        if (!CurrencyData.IsSupported(normalizedBase) || !CurrencyData.IsSupported(normalizedTarget))
        {
            throw new ArgumentException("Unsupported currency pair.");
        }

        var snapshot = await GetRateSnapshotAsync(normalizedBase, normalizedTarget);
        var sanitizedAmount = amount <= 0 ? 1 : amount;
        var converted = Math.Round(sanitizedAmount * snapshot.Rate, 6);

        return new LatestRateResponse
        {
            BaseCurrency = normalizedBase,
            TargetCurrency = normalizedTarget,
            Rate = Math.Round(snapshot.Rate, 6),
            Amount = sanitizedAmount,
            ConvertedAmount = converted,
            AsOf = snapshot.AsOf
        };
    }

    public async Task<HistoryResponse> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, int days)
    {
        var normalizedBase = CurrencyData.Normalize(baseCurrency);
        var normalizedTarget = CurrencyData.Normalize(targetCurrency);

        if (!CurrencyData.IsSupported(normalizedBase) || !CurrencyData.IsSupported(normalizedTarget))
        {
            throw new ArgumentException("Unsupported currency pair.");
        }

        if (days < 1 || days > 30)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 30.");
        }

        var cacheKey = $"history_{normalizedBase}_{normalizedTarget}_{days}";
        if (_cache.TryGetValue(cacheKey, out HistoryResponse? cached))
        {
            return cached;
        }

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days + 1);
        var url = $"timeseries?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&base={normalizedBase}&symbols={normalizedTarget}";

        var response = await GetFromApiAsync<ExchangeRateTimeSeriesResponse>(url);
        if (response == null || !response.Success || response.Rates == null)
        {
            throw new InvalidOperationException("Unable to retrieve historical rates.");
        }

        var ordered = response.Rates
            .Select(kvp => new
            {
                Date = DateTime.ParseExact(kvp.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Value = kvp.Value.TryGetValue(normalizedTarget, out var rate) ? rate : (decimal?)null
            })
            .Where(x => x.Value.HasValue)
            .OrderBy(x => x.Date)
            .Select(x => new HistoricalRate { Date = x.Date, Rate = Math.Round(x.Value!.Value, 6) })
            .ToList();

        var result = new HistoryResponse
        {
            BaseCurrency = normalizedBase,
            TargetCurrency = normalizedTarget,
            Rates = ordered
        };

        _cache.Set(cacheKey, result, TimeSpan.FromSeconds(_options.CacheDurationSeconds));
        return result;
    }

    private async Task<RateSnapshot> GetRateSnapshotAsync(string baseCurrency, string targetCurrency)
    {
        var cacheKey = $"latest_rate_{baseCurrency}_{targetCurrency}";
        if (_cache.TryGetValue(cacheKey, out RateSnapshot? cached))
        {
            return cached;
        }

        var url = $"latest?base={baseCurrency}&symbols={targetCurrency}";
        var response = await GetFromApiAsync<ExchangeRateLatestResponse>(url);

        if (response == null || !response.Success || response.Rates == null)
        {
            throw new InvalidOperationException("Unable to fetch the latest exchange rate.");
        }

        if (!response.Rates.TryGetValue(targetCurrency, out var rate))
        {
            throw new InvalidOperationException("The exchange rate provider did not return the requested pair.");
        }

        var date = DateTime.UtcNow.Date;
        if (!string.IsNullOrWhiteSpace(response.Date))
        {
            if (DateTime.TryParse(response.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                date = parsed.Date;
            }
        }

        var snapshot = new RateSnapshot(rate, date);
        _cache.Set(cacheKey, snapshot, TimeSpan.FromSeconds(_options.CacheDurationSeconds));
        return snapshot;
    }

    private async Task<T?> GetFromApiAsync<T>(string relativePath)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reach exchange rate provider for {Path}", relativePath);
            throw;
        }
    }

    private sealed record ExchangeRateLatestResponse
    (
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("date")] string? Date,
        [property: JsonPropertyName("rates")] Dictionary<string, decimal>? Rates
    );

    private sealed record ExchangeRateTimeSeriesResponse
    (
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("start_date")] string? StartDate,
        [property: JsonPropertyName("end_date")] string? EndDate,
        [property: JsonPropertyName("rates")] Dictionary<string, Dictionary<string, decimal>>? Rates
    );

    private sealed record RateSnapshot(decimal Rate, DateTime AsOf);
}
