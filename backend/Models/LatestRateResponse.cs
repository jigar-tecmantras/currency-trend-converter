namespace CurrencyTrendConverter.Models;

public record LatestRateResponse
{
    public required string BaseCurrency { get; init; }
    public required string TargetCurrency { get; init; }
    public decimal Rate { get; init; }
    public decimal Amount { get; init; }
    public decimal ConvertedAmount { get; init; }
    public DateTime AsOf { get; init; }
}
