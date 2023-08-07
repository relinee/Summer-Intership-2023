namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException(string message) :
        base(message)
    { }
}