namespace CurrencyTrendConverter.Models;

public record HistoricalRate
{
    public DateTime Date { get; init; }
    public decimal Rate { get; init; }
}
