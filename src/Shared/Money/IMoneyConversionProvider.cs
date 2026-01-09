namespace MoneyDataType;

/// <summary>
/// Provides a mechanism for converting a Money currency to another currency.
/// </summary>
public interface IMoneyConversionProvider
{
    /// <summary>
    /// Converts a money amount to another currency.
    /// </summary>
    /// <param name="moneyFrom">Money to convert.</param>
    /// <param name="toCurrencyCode">Target ISO currency code.</param>
    /// <returns>Converted money value.</returns>
    Money Convert(Money moneyFrom, string toCurrencyCode);
}
