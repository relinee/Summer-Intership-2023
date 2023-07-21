using System.IO.Pipes;

namespace Fuse8_ByteMinds.SummerSchool.Domain;

/// <summary>
/// Контейнер для значения, с отложенным получением
/// </summary>
public class Lazy<TValue>
{
	
	private readonly Func<TValue> _valueRetrieval;
	private TValue _value;
	private bool _isCreated;
	public Lazy(Func<TValue> func)
	{
		_valueRetrieval = func;
	}

	public TValue? Value
	{
		get
		{
			if (!_isCreated)
			{
				_value = _valueRetrieval.Invoke();
				_isCreated = true;
			}
			return _value;
		}
	}

}