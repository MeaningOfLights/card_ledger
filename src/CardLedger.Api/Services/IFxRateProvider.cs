using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CardLedger.Api.Services;

/// <summary>
/// Interface for FX rate providers.
/// </summary>
public interface IFxRateProvider
{
    /// <summary>
    /// Returns the latest FX rate such that rateDate equal or less than purchaseDate and within last 6 months.
    /// </summary>
    decimal GetUsdToCurrencyRate(string currency, DateOnly purchaseDate);

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<FxRateRow> GetAll();
}
