namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;

public interface IRecalculateCurrencyCache
{
    public Task RecalculateCurrencyCacheToNewBaseCurrencyAsync(Guid taskId, CancellationToken cancellationToken);
}