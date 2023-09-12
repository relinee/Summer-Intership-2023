namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

public class CurrencyFavouriteNotFoundException : Exception
{
    public CurrencyFavouriteNotFoundException(string message) :
        base(message)
    { }
}