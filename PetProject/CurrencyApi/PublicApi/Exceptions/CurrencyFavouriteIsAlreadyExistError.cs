namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

public class CurrencyFavouriteIsAlreadyExistError : Exception
{
    public CurrencyFavouriteIsAlreadyExistError(string message) :
        base(message)
    { }
}