using System.Globalization;
using MoneyDataType;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// Formats Money values using a culture that matches the currency code when possible.
/// </summary>
public static class MoneyFormatting
{
    /// <summary>
    /// Formats money using a currency-appropriate symbol when available.
    /// </summary>
    public static string Format(Money money)
    {
        var culture = FindCultureByCurrencyCode(money.CurrencyCode);
        var value = culture is null
            ? money
            : new Money(money.Value, money.CurrencyCode, culture);

        return value.WithScale(2).ToString();
    }

    /// <summary>
    /// Finds the culture by currency code.
    /// </summary>
    /// <param name="currencyCode">The currency code.</param>
    /// <returns></returns>
    private static CultureInfo? FindCultureByCurrencyCode(string currencyCode)
    {
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            RegionInfo region;
            try
            {
                region = new RegionInfo(culture.Name);
            }
            catch
            {
                continue;
            }

            if (string.Equals(region.ISOCurrencySymbol, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return culture;
            }
        }

        return null;
    }
}
