namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;

public class CurrencySettingsNotFoundException : Exception
{
    public CurrencySettingsNotFoundException(string message) :
        base(message)
    {
    }
}