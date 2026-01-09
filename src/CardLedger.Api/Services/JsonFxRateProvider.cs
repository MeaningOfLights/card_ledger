using CardLedger.Api.Domain;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CardLedger.Api.Services;

/// <summary>
/// Represents a row of FX rate data.
/// </summary>
/// <param name="Currency"></param>
/// <param name="RateDate"></param>
/// <param name="UsdToCurrency"></param>
public sealed record FxRateRow(string Currency, DateOnly RateDate, decimal UsdToCurrency);

/// <summary>
/// FX rate provider that reads rates from a JSON file.
/// </summary>
public sealed class JsonFxRateProvider : IFxRateProvider
{
    private readonly IReadOnlyList<FxRateRow> _rows;
    private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFxRateProvider"/> class.
    /// </summary>
    /// <param name="env"></param>
    /// <exception cref="Exception"></exception>
    public JsonFxRateProvider(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "fx_rates.json");
        var json = File.ReadAllText(path);

        var doc = JsonSerializer.Deserialize<List<JsonFxRateRow>>(json, options)
                  ?? [];

        _rows = doc.Select(r => new FxRateRow(
                Currency: (r.Currency ?? "").Trim().ToUpperInvariant(),
                RateDate: DateOnly.Parse(r.RateDate ?? throw new Exception("Missing rateDate")),
                UsdToCurrency: r.UsdToCurrency
            ))
            .ToList();
    }

    /// <summary>
    /// Returns all FX rate rows.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<FxRateRow> GetAll() => _rows;

    /// <summary>
    /// Returns the latest FX rate such that rateDate less than or equal to purchaseDate and within last 6 months.
    /// </summary>
    /// <param name="currency"></param>
    /// <param name="purchaseDate"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ValidationException"></exception>
    public decimal GetUsdToCurrencyRate(string currency, DateOnly purchaseDate)
    {
        currency = (currency ?? "").Trim().ToUpperInvariant();
        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }
        if (currency == "USD")
        {
            return 1m;
        }

        var minDate = purchaseDate.AddMonths(-6);

        var candidate = _rows
            .Where(r => r.Currency == currency && r.RateDate <= purchaseDate && r.RateDate >= minDate)
            .OrderByDescending(r => r.RateDate)
            .FirstOrDefault() ?? throw new ValidationException($"No FX rate available for {currency} on or before {purchaseDate} within the last 6 months.");
        return candidate.UsdToCurrency;
    }
}
