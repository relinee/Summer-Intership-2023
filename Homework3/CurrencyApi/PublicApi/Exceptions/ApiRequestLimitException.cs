namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

public class ApiRequestLimitException : Exception
{
    public ApiRequestLimitException(string message) :
        base(message)
    { }
}