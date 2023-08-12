namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

public class ApiRequestLimitException : Exception
{
    public ApiRequestLimitException(string message) :
        base(message)
    { }
}