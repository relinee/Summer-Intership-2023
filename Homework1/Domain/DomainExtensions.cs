namespace Fuse8_ByteMinds.SummerSchool.Domain;

public static class DomainExtensions
{
	public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
		=> collection == null || !collection.Any();

	public static string JoinToString<T>(this IEnumerable<T> collection, string separator)
		=> string.Join(separator, collection);

	public static decimal DaysCountBetween(this DateTimeOffset dtoA, DateTimeOffset dtoB)
		//=> Math.Abs(dtoA.Year + dtoA.DayOfYear - (dtoB.Year + dtoB.DayOfYear));
		=> Math.Abs((dtoA.Date - dtoB.Date).Days);
}