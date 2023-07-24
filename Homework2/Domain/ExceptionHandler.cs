using System.Net;

namespace Fuse8_ByteMinds.SummerSchool.Domain;

public static class ExceptionHandler
{
	/// <summary>
	/// Обрабатывает исключение, которое может возникнуть при выполнении <paramref name="action"/>
	/// </summary>
	/// <param name="action">Действие, которое может породить исключение</param>
	/// <returns>Сообщение об ошибке</returns>
	public static string? Handle(Action action)
	{
		try
		{
			action();
			return null;
		}
		catch (NegativeRubleCountException negativeRubleCountException)
		{
			return "Число рублей не может быть отрицательным";
		}
		catch (NotValidKopekCountException notValidKopekCountException)
		{
			return "Количество копеек должно быть больше 0 и меньше 99";
		}
		catch (HttpRequestException httpRequestException)
		when(httpRequestException.StatusCode == HttpStatusCode.NotFound)
		{
			return "Ресурс не райден";

		}
		catch (HttpRequestException httpRequestException)
		{
			return httpRequestException.StatusCode.ToString();
		}
		catch (MoneyException moneyException)
		{
			return moneyException.Message;
		}
		catch (Exception exception)
		{
			return "Произошла непредвиденная ошибка";
		}
	}
}

public class MoneyException : Exception
{
	public MoneyException()
	{
	}

	public MoneyException(string? message)
		: base(message)
	{
	}
}

public class NotValidKopekCountException : MoneyException
{
	public NotValidKopekCountException()
	{
	}
}

public class NegativeRubleCountException : MoneyException
{
	public NegativeRubleCountException()
	{
	}
}