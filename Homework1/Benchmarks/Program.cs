using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Fuse8_ByteMinds.SummerSchool.Domain;

BenchmarkRunner.Run<StringInternBenchmark>();
BenchmarkRunner.Run<AccountProcessorCalculateBenchmark>();

[MemoryDiagnoser(displayGenColumns: true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringInternBenchmark
{
    private readonly List<string> _words = new();

    public StringInternBenchmark()
    {
        foreach (var word in File.ReadLines(@".\SpellingDictionaries\ru_RU.dic"))
            _words.Add(string.Intern(word));
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(SampleData))]
    public bool WordIsExists(string word)
        => _words.Any(item => word.Equals(item, StringComparison.Ordinal));

    [Benchmark]
    [ArgumentsSource(nameof(SampleData))]
    public bool WordIsExistsIntern(string word)
    {
        var internedWord = string.Intern(word);
        return _words.Any(item => ReferenceEquals(internedWord, item));
    }

    public IEnumerable<string> SampleData()
    {
        // Из файла
        yield return new StringBuilder().Append("Чика").Append("го").ToString();
        yield return new StringBuilder().Append("пев").Append("цом").ToString();
        yield return new StringBuilder().Append("автотро").Append("фов").ToString();

        yield return "экситон/K";
        yield return "потечет";
        yield return "андорранцев";

        // Не из файла
        yield return new StringBuilder().Append("фыв").Append("фыв").ToString();
        yield return new StringBuilder().Append("выф").Append("выф").ToString();
        yield return new StringBuilder().Append("ыфв").Append("ыфв").ToString();

        yield return "ячсми";
        yield return "чсмит";
        yield return "смитт";

    }
}

/* Получившееся таблица
|             Method |        word |           Mean |        Error |       StdDev | Ratio | RatioSD | Rank |   Gen0 | Allocated | Alloc Ratio |
|------------------- |------------ |---------------:|-------------:|-------------:|------:|--------:|-----:|-------:|----------:|------------:|
| WordIsExistsIntern |  автотрофов |   990,642.8 ns | 19,493.06 ns | 20,017.94 ns |  0.77 |    0.03 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |  автотрофов | 1,286,230.8 ns | 25,694.80 ns | 32,495.69 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern | андорранцев | 1,031,415.9 ns | 17,662.32 ns | 15,657.19 ns |  0.82 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists | андорранцев | 1,261,749.5 ns | 23,514.05 ns | 20,844.59 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |      выфвыф | 1,036,806.8 ns | 18,997.74 ns | 17,770.50 ns |  0.85 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |      выфвыф | 1,221,572.5 ns | 16,153.56 ns | 15,110.05 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |      певцом |   541,993.2 ns |  7,104.08 ns |  5,932.23 ns |  0.81 |    0.01 |    1 |      - |     128 B |        1.00 |
|       WordIsExists |      певцом |   670,448.8 ns | 11,819.27 ns | 11,055.75 ns |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |     потечет |   544,139.9 ns |  9,413.25 ns |  8,805.16 ns |  0.92 |    0.02 |    1 |      - |     128 B |        1.00 |
|       WordIsExists |     потечет |   594,683.1 ns | 11,007.14 ns | 10,296.09 ns |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |       смитт | 1,033,114.8 ns | 17,432.65 ns | 16,306.51 ns |  0.84 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |       смитт | 1,225,398.1 ns | 23,494.64 ns | 25,139.00 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |      фывфыв | 1,036,144.1 ns | 20,001.19 ns | 19,643.84 ns |  0.85 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |      фывфыв | 1,219,600.9 ns | 23,678.82 ns | 23,255.77 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
|       WordIsExists |      Чикаго |       235.0 ns |      4.63 ns |      5.85 ns |  1.00 |    0.00 |    1 | 0.0076 |     128 B |        1.00 |
| WordIsExistsIntern |      Чикаго |       238.1 ns |      4.73 ns |      6.32 ns |  1.02 |    0.04 |    1 | 0.0076 |     128 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |       чсмит | 1,034,496.2 ns | 20,431.41 ns | 20,981.55 ns |  0.84 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |       чсмит | 1,236,100.0 ns | 24,401.37 ns | 25,058.41 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |      ыфвыфв | 1,032,601.9 ns | 19,644.48 ns | 16,404.03 ns |  0.85 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |      ыфвыфв | 1,220,437.5 ns | 19,754.12 ns | 16,495.59 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |   экситон/K |    44,520.0 ns |    715.44 ns |    669.23 ns |  0.76 |    0.02 |    1 |      - |     128 B |        1.00 |
|       WordIsExists |   экситон/K |    58,889.2 ns |  1,151.02 ns |  1,650.76 ns |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
|                    |             |                |              |              |       |         |      |        |           |             |
| WordIsExistsIntern |       ячсми | 1,037,093.7 ns | 14,882.97 ns | 12,427.95 ns |  0.86 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |       ячсми | 1,208,748.6 ns | 21,888.50 ns | 23,420.44 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |


Поделённая:
|             Method |        word |           Mean |        Error |       StdDev | Ratio | RatioSD | Rank |   Gen0 | Allocated | Alloc Ratio |
|------------------- |------------ |---------------:|-------------:|-------------:|------:|--------:|-----:|-------:|----------:|------------:|
// Из словаря по stringbuilder'у:
|       WordIsExists |      Чикаго |       235.0 ns |      4.63 ns |      5.85 ns |  1.00 |    0.00 |    1 | 0.0076 |     128 B |        1.00 |
| WordIsExistsIntern |      Чикаго |       238.1 ns |      4.73 ns |      6.32 ns |  1.02 |    0.04 |    1 | 0.0076 |     128 B |        1.00 |
| WordIsExistsIntern |      певцом |   541,993.2 ns |  7,104.08 ns |  5,932.23 ns |  0.81 |    0.01 |    1 |      - |     128 B |        1.00 |
|       WordIsExists |      певцом |   670,448.8 ns | 11,819.27 ns | 11,055.75 ns |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
| WordIsExistsIntern |  автотрофов |   990,642.8 ns | 19,493.06 ns | 20,017.94 ns |  0.77 |    0.03 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |  автотрофов | 1,286,230.8 ns | 25,694.80 ns | 32,495.69 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |

// Из словаря константные:
| WordIsExistsIntern |   экситон/K |    44,520.0 ns |    715.44 ns |    669.23 ns |  0.76 |    0.02 |    1 |      - |     128 B |        1.00 |
|       WordIsExists |   экситон/K |    58,889.2 ns |  1,151.02 ns |  1,650.76 ns |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
| WordIsExistsIntern |     потечет |   544,139.9 ns |  9,413.25 ns |  8,805.16 ns |  0.92 |    0.02 |    1 |      - |     128 B |        1.00 |
|       WordIsExists |     потечет |   594,683.1 ns | 11,007.14 ns | 10,296.09 ns |  1.00 |    0.00 |    2 |      - |     128 B |        1.00 |
| WordIsExistsIntern | андорранцев | 1,031,415.9 ns | 17,662.32 ns | 15,657.19 ns |  0.82 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists | андорранцев | 1,261,749.5 ns | 23,514.05 ns | 20,844.59 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |

// Не из словаря по stringbuilder'у
| WordIsExistsIntern |      фывфыв | 1,036,144.1 ns | 20,001.19 ns | 19,643.84 ns |  0.85 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |      фывфыв | 1,219,600.9 ns | 23,678.82 ns | 23,255.77 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |

// Не из словаря константные:
| WordIsExistsIntern |       смитт | 1,033,114.8 ns | 17,432.65 ns | 16,306.51 ns |  0.84 |    0.02 |    1 |      - |     129 B |        1.00 |
|       WordIsExists |       смитт | 1,225,398.1 ns | 23,494.64 ns | 25,139.00 ns |  1.00 |    0.00 |    2 |      - |     129 B |        1.00 |


Из нее я сделал некоторые выводы: 
1. Что для всех слов, кроме "Чикаго"(взято из словаря) метод с интернированием строк работает быстрее примерно на 20%.
2. Для обоих методов память выделяется одинаково.
3. Оба метода выполняются гораздо быстрее для слов из словаря, так как LINQ запрос проходит не по всему словарю. Это также подтверждается тем,
 что у слов, взятых из конца файла("автотрофов", "андорранцев") время выполнение примерно такое же, что и у слов не из словаря.
4. Разница в скорости выполнения методов достигается за счет того, что сравнение ссылок быстрее, чем строк.\
5. Разницы в скорости работы константных слов и составленных при помощи stringbuilder я не заметил.
 */

[MemoryDiagnoser(displayGenColumns: true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AccountProcessorCalculateBenchmark
{
    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(SampleData))]
    public decimal OrdinaryAccountCalculate(BankAccount bankAccount)
        => new AccountProcessor().Calculate(bankAccount);

    [Benchmark]
    [ArgumentsSource(nameof(SampleData))]
    public decimal PerformedAccountCalculate(BankAccount bankAccount)
        => new AccountProcessor().CalculatePerformed(bankAccount);

    public IEnumerable<BankAccount> SampleData()
    {
        yield return new BankAccount();
    }
}

/* Табличка:
|                    Method |          bankAccount |     Mean |   Error |   StdDev | Ratio | RatioSD | Rank |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |--------------------- |---------:|--------:|---------:|------:|--------:|-----:|-------:|----------:|------------:|
| PerformedAccountCalculate | Fuse8(...)count [47] | 302.7 ns | 6.18 ns | 10.98 ns |  0.55 |    0.03 |    1 | 0.0014 |      24 B |       0.004 |
|  OrdinaryAccountCalculate | Fuse8(...)count [47] | 541.7 ns | 4.02 ns |  3.76 ns |  1.00 |    0.00 |    2 | 0.4025 |    6744 B |       1.000 |

Выводы: 
1) Оптимизированный метод работает 45% быстрее;
2) В оптимизированном методе выделение памяти в 250 раз меньше, чем в обычном.
*/