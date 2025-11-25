namespace Alerting.ML.Engine.Extensions;

public static class SpanExtensions
{
    public static double Average<T>(this Span<T> source, Func<T, double> selector)
    {
        if (source.Length == 0)
        {
            return 0;
        }

        return source.Sum(selector) / source.Length;
    }

    public static double Sum<T>(this Span<T> source, Func<T, double> selector)
    {
        var sum = 0.0;
        for (var index = 0; index < source.Length; index++)
        {
            var item = source[index];
            sum += selector(item);
        }

        return sum;
    }

    public static double Min<T>(this Span<T> source, Func<T, double> selector)
    {
        if (source.Length == 0)
        {
            throw new InvalidOperationException("Span is empty");

        }

        var min = selector(source[0]);
        for (var index = 1; index < source.Length; index++)
        {
            min = Math.Min(min, selector(source[index]));
        }

        return min;
    }

    public static double Max<T>(this Span<T> source, Func<T, double> selector)
    {
        if (source.Length == 0)
        {
            throw new InvalidOperationException("Span is empty");

        }

        var max = selector(source[0]);

        for (var index = 1; index < source.Length; index++)
        {
            max = Math.Max(max, selector(source[index]));
        }

        return max;
    }
}