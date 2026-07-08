namespace CurrencyTrendConverter.Models;

public class ExchangeRateApiOptions
{
    public string BaseUrl { get; set; } = "https://api.exchangerate.host/";
    public int TimeoutSeconds { get; set; } = 12;
    public int CacheDurationSeconds { get; set; } = 300;
}
