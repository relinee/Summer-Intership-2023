namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

public class CurrencySettingsNotFoundException : Exception
{
    public CurrencySettingsNotFoundException(string message) :
        base(message)
    {
    }
}