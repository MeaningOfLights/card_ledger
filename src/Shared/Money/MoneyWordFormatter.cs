using System;
using System.Globalization;
using System.Text;

namespace MoneyDataType;

/// <summary>
/// A FormatProvider specific to the Money structure, which will format the
/// Money value as a word representation.
/// </summary>
public class MoneyWordFormatter : ICustomFormatter, IFormatProvider
{
    private StringBuilder _result = new StringBuilder();

    public string Format(string? formatString, object? arg, IFormatProvider? formatProvider)
    {
        if (arg is Money money)
        {
            return ValueToWords(money);
        }

        if (arg is decimal value)
        {
            return ValueToWords(new Money(value));
        }

        return string.Empty;
    }

    public object? GetFormat(Type? formatType)
    {
        if (formatType == typeof(ICustomFormatter))
        {
            return this;
        }

        return null;
    }

    private string ValueToWords(Money value)
    {
        NumberFormatInfo nfi = new NumberFormatInfo
        {
            NumberGroupSeparator = string.Empty
        };

        string temp = Math.Abs(value.Value).ToString("N2", nfi);
        decimal numeric = decimal.Parse(temp);
        _result = new StringBuilder();

        if (Math.Abs(numeric) < 1)
        {
            _result.Append("No");
        }
        else
        {
            temp = temp.PadLeft(15, '0');

            SubProcess(0, temp, "Billion");
            SubProcess(3, temp, "Million");
            SubProcess(6, temp, "Thousand");

            int number = int.Parse(temp.Substring(9, 3));
            if (number > 0 && number < 100)
            {
                if (_result.Length > 0)
                {
                    _result.Append("And ");
                }
            }

            SubProcess(9, temp);
        }

        if (!_result.ToString().EndsWith(" ", StringComparison.Ordinal))
        {
            _result.Append(' ');
        }

        string currencyUnit = string.Equals(value.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase)
            ? "Dollar"
            : value.CurrencyCode;
        _result.Append(currencyUnit);

        if (currencyUnit == "Dollar" && decimal.Truncate(numeric) != 1)
        {
            _result.Append("s ");
        }
        else
        {
            _result.Append(' ');
        }

        int cents;
        string input = temp.Substring(temp.IndexOf(".", StringComparison.Ordinal) + 1).PadRight(2, '0');
        if (input.Length > 6)
        {
            input = input.Substring(0, 6);
        }

        cents = int.Parse(input);
        if (cents == 0)
        {
            _result.Append("And No");
        }
        else
        {
            _result.Append("And ");
            SubProcess(0, input);
        }

        if (cents == 1)
        {
            _result.Append(" Cent");
        }
        else
        {
            _result.Append(" Cents");
        }

        return _result.ToString();
    }

    private void SubProcess(int start, string value, string wordToAdd = "")
    {
        string input = value.PadLeft(3, '0').Substring(start, 3);
        int number = int.Parse(input);
        if (number != 0)
        {
            ConvertInput(input);
            if (wordToAdd.Length > 0)
            {
                if (!_result.ToString().EndsWith(" ", StringComparison.Ordinal))
                {
                    _result.Append(' ');
                }

                _result.Append(wordToAdd);
                _result.Append(' ');
            }
        }
    }

    private void ConvertInput(string input)
    {
        int hundred = int.Parse(input.PadLeft(3, '0').Substring(0, 1));
        if (hundred > 0)
        {
            AppendLiteral(hundred);
            _result.Append(" Hundred");
        }

        int tens = int.Parse(input.PadLeft(3, '0').Substring(1, 2));
        if (hundred > 0 && tens > 0)
        {
            _result.Append(" And ");
        }

        AppendLiteral(tens);
    }

    private void AppendLiteral(int index)
    {
        if (index < 20)
        {
            _result.Append(GetString(index));
        }
        else
        {
            int tens = Convert.ToInt32(index / 10) * 10;
            _result.Append(GetString(tens));
            if (index > tens)
            {
                index -= tens;
                _result.Append(' ');
                _result.Append(GetString(index));
            }
        }
    }

    private string GetString(int value)
    {
        switch (value)
        {
            case 0:
                return string.Empty;
            case < 10:
                return Choose(value, "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine");
            case >= 11 and <= 19:
                return Choose(value - 10, "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen");
            default:
                return Choose(value / 10, "Ten", "Twenty", "Thirty", "Fourty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety");
        }
    }

    private static string Choose(int index, params string[] values)
    {
        if (index <= 0 || index > values.Length)
        {
            return string.Empty;
        }

        return values[index - 1];
    }
}
