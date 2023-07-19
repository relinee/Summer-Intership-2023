namespace Fuse8_ByteMinds.SummerSchool.Domain;

/// <summary>
/// Контейнер для значения, с отложенным получением
/// </summary>
public class Lazy<TValue>
{
	public Lazy(Func<TValue> func)
	{
		Value = func.Invoke();
	}

	public TValue? Value { get; }
}