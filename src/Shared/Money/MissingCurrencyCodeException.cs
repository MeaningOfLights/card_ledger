using System;
using System.Runtime.Serialization;

namespace MoneyDataType;

[Serializable]
public class MissingCurrencyCodeException(string message, Exception? innerException) : ApplicationException(message, innerException)
{
    public MissingCurrencyCodeException()
        : this("Currency code is missing.")
    {
    }

    public MissingCurrencyCodeException(string message)
        : this(message, null)
    {
    }
}
