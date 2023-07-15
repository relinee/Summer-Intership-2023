using System.Runtime.CompilerServices;

namespace Fuse8_ByteMinds.SummerSchool.Domain;

/// <summary>
/// Модель для хранения денег
/// </summary>
public class Money
{
	public Money(int rubles, int kopeks)
		: this(false, rubles, kopeks)
	{
	}

	public Money(bool isNegative, int rubles, int kopeks)
	{
		if (isNegative && rubles == 0 && kopeks == 0)
			throw new ArgumentException("Неверный формат данных. При нулевом количестве рублей и коппек Money не может быть отрицательным");
		if (rubles < 0)
			throw new ArgumentException("Неверный формат данных. Количество рублей должно быть неотрицательным");
		if (kopeks > 99 || kopeks < 0)
			throw new ArgumentException("Неверный формат данных. Количество копеек должно входить в отрезок [0, 99]");
		IsNegative = isNegative;
		Rubles = rubles;
		Kopeks = kopeks;
	}

	/// <summary>
	/// Отрицательное значение
	/// </summary>
	public bool IsNegative { get; }

	/// <summary>
	/// Число рублей
	/// </summary>
	public int Rubles { get; }

	/// <summary>
	/// Количество копеек
	/// </summary>
	public int Kopeks { get; }

	public static Money operator +(Money a, Money b)
	{
		if (a.IsNegative && b.IsNegative)
			return new Money(isNegative: true, rubles: a.Rubles + b.Rubles + (a.Kopeks + b.Kopeks) / 100, kopeks: (a.Kopeks + b.Kopeks) % 100);
		
		if (!a.IsNegative && !b.IsNegative)
			return new Money(isNegative: false, rubles:a.Rubles + b.Rubles + (a.Kopeks + b.Kopeks) / 100, kopeks: (a.Kopeks + b.Kopeks) % 100);

		var diffKopeks = a.Rubles * 100 + a.Kopeks - (b.Rubles * 100 + b.Kopeks);

		return (diffKopeks > 0 && a.IsNegative) || (diffKopeks < 0 && b.IsNegative) ?
			new Money(isNegative: true, rubles: Math.Abs(diffKopeks) / 100, kopeks: Math.Abs(diffKopeks) % 100) :
			new Money(isNegative: false, rubles: Math.Abs(diffKopeks) / 100, kopeks: Math.Abs(diffKopeks) % 100);
	}

	public static Money operator -(Money a, Money b) 
		=> b.IsNegative || (b.Rubles == 0 && b.Kopeks == 0) ?
			a + new Money(isNegative: false, rubles: b.Rubles, kopeks: b.Kopeks) :
			a + new Money(isNegative: true, rubles: b.Rubles, kopeks: b.Kopeks);

	public static bool operator >(Money a, Money b)
	{
		var totalKopeksA = (100 * a.Rubles + a.Kopeks) * (a.IsNegative ? -1 : 1);
		var totalKopeksB = (100 * b.Rubles + b.Kopeks) * (b.IsNegative ? -1 : 1);
		return totalKopeksA > totalKopeksB;
	}

	public static bool operator <(Money a, Money b)
	{
		var totalKopeksA = (100 * a.Rubles + a.Kopeks) * (a.IsNegative ? -1 : 1);
		var totalKopeksB = (100 * b.Rubles + b.Kopeks) * (b.IsNegative ? -1 : 1);
		return totalKopeksA < totalKopeksB;
	}
	
	public static bool operator >=(Money a, Money b)
		=> a > b || (a.IsNegative == b.IsNegative && a.Rubles == b.Rubles && a.Kopeks == b.Kopeks);
	
	public static bool operator <=(Money a, Money b)
		=> a < b || (a.IsNegative == b.IsNegative && a.Rubles == b.Rubles && a.Kopeks == b.Kopeks);

	public override string ToString()
		=> IsNegative ? $"-{Rubles} руб. {Kopeks} коп." : $"{Rubles} руб. {Kopeks} коп.";
	
	public override int GetHashCode()
		=> HashCode.Combine(IsNegative, Rubles, Kopeks);

	public override bool Equals(object? obj)
		=> Equals(obj as Money);

	public bool Equals(Money? other)
		=> other != null &&
		   other.IsNegative == IsNegative &&
		   other.Rubles == Rubles && other.Kopeks == other.Kopeks;
	
}