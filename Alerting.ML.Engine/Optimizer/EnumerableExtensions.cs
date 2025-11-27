namespace Alerting.ML.Engine.Optimizer;

internal static class EnumerableExtensions
{
    public static IEnumerable<(int Count, double Value)> BinBy<T>(this IEnumerable<T> input, Func<T, double> selector,
        double step, int roundTo = 0)
    {
        return input.Select(selector).GroupBy(value => Math.Round(value / step, roundTo))
            .Select(doubles => (doubles.Count(), doubles.Key * step));
    }
}