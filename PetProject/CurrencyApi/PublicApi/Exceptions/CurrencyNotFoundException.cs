namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException(string message) :
        base(message)
    { }
}