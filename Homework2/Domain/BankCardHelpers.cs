using System.Reflection;

namespace Fuse8_ByteMinds.SummerSchool.Domain;

public static class BankCardHelpers
{
	/// <summary>
	/// Получает номер карты без маски
	/// </summary>
	/// <param name="card">Банковская карта</param>
	/// <returns>Номер карты без маски</returns>
	public static string GetUnmaskedCardNumber(BankCard card)
	{
		var unmaskedCardInfo = typeof (BankCard).GetField("_number", BindingFlags.Instance | BindingFlags.NonPublic);
		var unmaskedCardResult = (string)unmaskedCardInfo.GetValue(card);
		return unmaskedCardResult;
	}
}