namespace Alerting.ML.Engine.Extensions;

/// <summary>
///     Linq-like extensions for <see cref="Span{T}" />
/// </summary>
public static class SpanExtensions
{
    /// <param name="source">Source span</param>
    /// <typeparam name="T">Span element type</typeparam>
    extension<T>(ReadOnlySpan<T> source)
    {
        /// <summary>
        ///     Computes an average value in a span
        /// </summary>
        /// <param name="selector">Value selector</param>
        /// <returns></returns>
        public double Average(Func<T, double> selector)
        {
            if (source.Length == 0)
            {
                return 0;
            }

            return source.Sum(selector) / source.Length;
        }

        /// <summary>
        ///     Computes a sum of values in a span
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public double Sum(Func<T, double> selector)
        {
            var sum = 0.0;
            for (var index = 0; index < source.Length; index++)
            {
                var item = source[index];
                sum += selector(item);
            }

            return sum;
        }


        /// <summary>
        ///     Computes minimal value in a span
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public double Min(Func<T, double> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty");
            }

            var min = selector(source[index: 0]);
            for (var index = 1; index < source.Length; index++)
            {
                min = Math.Min(min, selector(source[index]));
            }

            return min;
        }

        /// <summary>
        ///     Computes maximum value in a span
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public double Max(Func<T, double> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty");
            }

            var max = selector(source[index: 0]);

            for (var index = 1; index < source.Length; index++)
            {
                max = Math.Max(max, selector(source[index]));
            }

            return max;
        }
    }
}