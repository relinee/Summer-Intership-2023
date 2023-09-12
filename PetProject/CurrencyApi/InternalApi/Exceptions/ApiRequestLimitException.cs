namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;

public class ApiRequestLimitException : Exception
{
    public ApiRequestLimitException(string message) :
        base(message)
    { }
}