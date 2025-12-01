using System.Collections.Immutable;
using Alerting.ML.Engine.Data;
using FluentValidation.Results;

namespace Alerting.ML.TimeSeries.Sample
{
    /// <summary>
    /// Generates a success-rate like time-series with fluctuations around known outage times.
    /// </summary>
    public class SampleTimeSeriesProvider : ITimeSeriesProvider
    {
        /// <summary>
        /// Creates a new time-series provider with respect to know outages.
        /// </summary>
        /// <param name="outagesProvider"></param>
        public SampleTimeSeriesProvider(IKnownOutagesProvider outagesProvider)
        {
            series = [
                ..Enumerable
                    .Range(0, 1_000_000)
                    .Select(i =>
                    {
                        var timestamp = Current.AddMinutes(i);

                        var isOutage = outagesProvider.GetKnownOutages()
                            .Any(outage => timestamp < outage.EndTime && timestamp >= outage.StartTime);

                        return new Metric(timestamp,
                            Random.Shared.NextDouble() * Random.Shared.NextDouble() * 5 + 95 +
                            (isOutage ? -4 : Random.Shared.NextDouble() > 0.95 ? -4 : 0));
                    })
                    .OrderBy(metric => metric.Timestamp)
            ];
        }

        private static readonly DateTime Current = DateTime.UtcNow;
        private readonly ImmutableArray<Metric> series;

        /// <inheritdoc />
        public ImmutableArray<Metric> GetTimeSeries()
        {
            return series;
        }

        /// <inheritdoc />
        public Task<ValidationResult> ImportAndValidate()
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}