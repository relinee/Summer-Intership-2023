namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException(string message) :
        base(message)
    { }
}