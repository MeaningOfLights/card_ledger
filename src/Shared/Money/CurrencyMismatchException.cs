using System.Runtime.Serialization;

namespace MoneyDataType;

[Serializable]
public class CurrencyMismatchException : ApplicationException
{
    public CurrencyMismatchException()
        : this("Currencies don't match in supplied parameters.")
    {
    }

    public CurrencyMismatchException(string message)
        : this(message, null)
    {
    }

    public CurrencyMismatchException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    [Obsolete("For serialization purposes only.")]
    protected CurrencyMismatchException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
