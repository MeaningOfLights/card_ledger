namespace MoneyDataType;

/// <summary>
/// Provides a mechanism for converting a Money value to another currency.
/// </summary>
public interface IMoneyConvertible
{
    /// <summary>
    /// Converts a money amount to another currency.
    /// </summary>
    /// <param name="currencyCode">The ISO currency code to convert the money to.</param>
    /// <param name="convertProvider">Conversion logic to convert the Money value.</param>
    /// <returns>Returns the new money value.</returns>
    Money ToCurrency(string currencyCode, IMoneyConversionProvider convertProvider);
}
