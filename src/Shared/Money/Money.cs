using System;
using System.Globalization;

namespace MoneyDataType;

/// <summary>
/// Holds signed 128-bit (16-byte) values representing 96-bit (12-byte) integer numbers scaled
/// by a variable power of 10. The scaling factor specifies the number of digits to the right
/// of the decimal point; it ranges from 0 through 28.
///
/// Currency identity is represented by ISO currency codes such as "USD", "EUR", or "AUD".
/// RegionInfo is used only for formatting.
/// </summary>
[Serializable]
public struct Money : ICloneable, IFormattable, IMoneyConvertible, IEquatable<Money>
{
    public static readonly Money Empty = new Money(0m);

    private decimal _value;
    private string _currencyCode;
    private CultureInfo? _culture;

    public Money(decimal value)
        : this(value, GetCurrencyCodeFromCulture(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture)
    {
    }

    public Money(decimal value, string currencyCode)
        : this(value, currencyCode, CultureInfo.CurrentCulture)
    {
    }

    public Money(decimal value, string currencyCode, CultureInfo culture)
    {
        _value = value;
        _currencyCode = NormalizeCurrencyCode(currencyCode);
        _culture = NormalizeCulture(culture);
    }

    public Money(decimal value, RegionInfo region)
    {
        if (region is null)
        {
            throw new MissingCurrencyCodeException("Region is missing.");
        }

        _value = value;
        _currencyCode = NormalizeCurrencyCode(region.ISOCurrencySymbol);
        _culture = FindCultureForRegion(region);
    }

    public Money(decimal value, string currencyCode, RegionInfo region)
    {
        if (region is null)
        {
            throw new MissingCurrencyCodeException("Region is missing.");
        }

        _value = value;
        _currencyCode = NormalizeCurrencyCode(currencyCode);
        _culture = FindCultureForRegion(region);
    }

    public static Money Create(decimal value, string currencyCode)
    {
        return Create(value, currencyCode, null);
    }

    public static Money Create(decimal value, string currencyCode, string? region)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new MissingCurrencyCodeException("Currency code is missing.");
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            var regionInfo = new RegionInfo(region);
            return new Money(value, currencyCode, regionInfo);
        }

        return new Money(value, currencyCode);
    }

    public decimal Value
    {
        get => _value;
        set => _value = value;
    }

    public string CurrencyCode => _currencyCode;

    public CultureInfo? Culture => _culture;

    public RegionInfo? FormattingRegion
    {
        get
        {
            if (_culture is null)
            {
                return null;
            }

            return new RegionInfo(_culture.Name);
        }
    }

    public object Clone()
    {
        return new Money(_value, _currencyCode, _culture ?? CultureInfo.CurrentCulture);
    }

    public override bool Equals(object? obj)
    {
        return obj is Money money && Equals(money);
    }

    public bool Equals(Money other)
    {
        return _value == other._value
            && string.Equals(_currencyCode, other._currencyCode, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, StringComparer.OrdinalIgnoreCase.GetHashCode(_currencyCode));
    }

    private static bool CanDoMaths(Money[] values)
    {
        if (values is null || values.Length == 0)
        {
            return false;
        }

        string currencyCode = values[0]._currencyCode;
        for (int i = 1; i < values.Length; i++)
        {
            if (!string.Equals(values[i]._currencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public static Money operator +(Money m1, Money m2)
    {
        if (!CanDoMaths(new[] { m1, m2 }))
        {
            throw new CurrencyMismatchException();
        }

        return new Money(m1.Value + m2.Value, m1._currencyCode, m1._culture ?? CultureInfo.CurrentCulture);
    }

    public static Money operator -(Money m1, Money m2)
    {
        if (!CanDoMaths(new[] { m1, m2 }))
        {
            throw new CurrencyMismatchException();
        }

        return new Money(m1.Value - m2.Value, m1._currencyCode, m1._culture ?? CultureInfo.CurrentCulture);
    }

    public static Money operator *(Money m1, Money m2)
    {
        if (!CanDoMaths(new[] { m1, m2 }))
        {
            throw new CurrencyMismatchException();
        }

        return new Money(m1.Value * m2.Value, m1._currencyCode, m1._culture ?? CultureInfo.CurrentCulture);
    }

    public static Money operator /(Money m1, Money m2)
    {
        if (!CanDoMaths(new[] { m1, m2 }))
        {
            throw new CurrencyMismatchException();
        }

        return new Money(m1.Value / m2.Value, m1._currencyCode, m1._culture ?? CultureInfo.CurrentCulture);
    }

    public static Money operator %(Money m1, Money m2)
    {
        if (!CanDoMaths(new[] { m1, m2 }))
        {
            throw new CurrencyMismatchException();
        }

        return new Money(m1.Value % m2.Value, m1._currencyCode, m1._culture ?? CultureInfo.CurrentCulture);
    }

    public static bool operator ==(Money m1, Money m2)
    {
        return m1.Value == m2.Value
            && string.Equals(m1._currencyCode, m2._currencyCode, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(Money m1, Money m2)
    {
        return m1.Value != m2.Value
            || !string.Equals(m1._currencyCode, m2._currencyCode, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator >=(Money m1, Money m2)
    {
        return m1.Value >= m2.Value
            && string.Equals(m1._currencyCode, m2._currencyCode, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator <=(Money m1, Money m2)
    {
        return m1.Value <= m2.Value
            && string.Equals(m1._currencyCode, m2._currencyCode, StringComparison.OrdinalIgnoreCase);
    }

    public static Money Sum(Money[] values)
    {
        if (!CanDoMaths(values))
        {
            throw new CurrencyMismatchException();
        }

        Money total = new Money(0m, values[0]._currencyCode, values[0]._culture ?? CultureInfo.CurrentCulture);
        foreach (Money money in values)
        {
            total.Value += money.Value;
        }

        return total;
    }

    public static Money Parse(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        string trimmed = value.Trim();
        string? currencyCode = null;

        if (trimmed.Length >= 3)
        {
            string tail = trimmed.Substring(trimmed.Length - 3);
            if (IsAlphabetic(tail))
            {
                currencyCode = tail.ToUpperInvariant();
                trimmed = trimmed.Substring(0, trimmed.Length - 3).Trim();
            }
        }

        decimal parsed = decimal.Parse(trimmed, NumberStyles.Currency, CultureInfo.CurrentCulture);
        return currencyCode is null
            ? new Money(parsed)
            : new Money(parsed, currencyCode, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        CultureInfo culture = _culture ?? CultureInfo.CurrentCulture;
        string formatted = _value.ToString("C", culture.NumberFormat);
        string cultureCode = GetCurrencyCodeFromCulture(culture);

        if (string.Equals(cultureCode, _currencyCode, StringComparison.OrdinalIgnoreCase))
        {
            return formatted;
        }

        return $"{_value.ToString("N2", culture.NumberFormat)} {_currencyCode}";
    }

    public string ToString(IFormatProvider? formatProvider)
    {
        return string.Format(formatProvider, "{0}", _value);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return _value.ToString(format, formatProvider);
    }

    public Money ToCurrency(string currencyCode, IMoneyConversionProvider convertProvider)
    {
        if (convertProvider is null)
        {
            throw new ArgumentNullException(nameof(convertProvider));
        }

        return convertProvider.Convert(this, currencyCode);
    }

    public Money WithScale(int decimals, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        if (decimals < 0 || decimals > 28)
        {
            throw new ArgumentOutOfRangeException(nameof(decimals), "Scale must be between 0 and 28.");
        }

        decimal rounded = decimal.Round(_value, decimals, rounding);
        return new Money(rounded, _currencyCode, _culture ?? CultureInfo.CurrentCulture);
    }

    public static Money ForCardPurchase(decimal amount, string currencyCode, CultureInfo? culture = null)
    {
        decimal rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        return new Money(rounded, currencyCode, culture ?? CultureInfo.CurrentCulture);
    }

    private static string NormalizeCurrencyCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new MissingCurrencyCodeException("Currency code is missing.");
        }

        return currencyCode.Trim().ToUpperInvariant();
    }

    private static CultureInfo NormalizeCulture(CultureInfo? culture)
    {
        CultureInfo resolved = culture ?? CultureInfo.CurrentCulture;
        if (string.IsNullOrWhiteSpace(resolved.Name) || resolved.Equals(CultureInfo.InvariantCulture))
        {
            resolved = CultureInfo.CurrentCulture;
            if (string.IsNullOrWhiteSpace(resolved.Name) || resolved.Equals(CultureInfo.InvariantCulture))
            {
                resolved = CultureInfo.GetCultureInfo("en-US");
            }
        }
        if (resolved.IsNeutralCulture)
        {
            resolved = CultureInfo.CreateSpecificCulture(resolved.Name);
        }

        return resolved;
    }

    private static string GetCurrencyCodeFromCulture(CultureInfo culture)
    {
        CultureInfo resolved = NormalizeCulture(culture);
        RegionInfo region = new RegionInfo(resolved.Name);
        return region.ISOCurrencySymbol;
    }

    private static CultureInfo? FindCultureForRegion(RegionInfo region)
    {
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                RegionInfo cultureRegion = new RegionInfo(culture.Name);
                if (string.Equals(cultureRegion.Name, region.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return culture;
                }
            }
            catch
            {
            }
        }

        try
        {
            return CultureInfo.GetCultureInfo(region.Name);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsAlphabetic(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsLetter(value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
