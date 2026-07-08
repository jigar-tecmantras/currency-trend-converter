using System.Collections.Generic;

namespace CurrencyTrendConverter.Models;

public record HistoryResponse
{
    public required string BaseCurrency { get; init; }
    public required string TargetCurrency { get; init; }
    public required IReadOnlyList<HistoricalRate> Rates { get; init; }
}
