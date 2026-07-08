using System.Collections.Generic;

namespace CurrencyTrendConverter.Models;

public static class CurrencyData
{
    private static readonly HashSet<string> _supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "NZD", "INR", "CNY",
        "SGD", "SEK", "NOK", "DKK", "BRL", "MXN"
    };

    public static IReadOnlyCollection<string> SupportedCurrencies => _supported;

    public static bool IsSupported(string? symbol) => !string.IsNullOrWhiteSpace(symbol) && _supported.Contains(symbol.Trim().ToUpperInvariant());

    public static string Normalize(string? symbol) => (symbol ?? string.Empty).Trim().ToUpperInvariant();
}
