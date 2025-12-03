using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Extensions;

namespace Alerting.ML.Sources.Azure
{
    /// <summary>
    /// Represent an <b>approximate</b> implementation of <a href="https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/scheduledqueryrules">Azure Scheduled Query Rule</a> based on the information inferred from bicep parameters and their description. <br/>
    /// Assumes usage of StaticThresholdCriterion, a single criteria, and no dimensions.
    /// </summary>
    public class ScheduledQueryRuleAlert : IAlert<ScheduledQueryRuleConfiguration>
    {
        /// <inheritdoc />
        public string ProviderName => "Azure";

        /// <inheritdoc />
        public IEnumerable<Outage> Evaluate(ITimeSeriesProvider provider, ScheduledQueryRuleConfiguration configuration)
        {
            var evaluationPeriods = new Queue<bool>(); //holds last NumberOfEvaluationPeriods results

            DateTime? ongoingOutageStart = null;
            var timeWindowStartIndex = 0;

            var timeSeries = provider.GetTimeSeries();
            var lastEvaluation = timeSeries[0].Timestamp;

            for (var timeWindowEndIndex = 0; timeWindowEndIndex < timeSeries.Length; timeWindowEndIndex++)
            {
                var currentMetric = timeSeries[timeWindowEndIndex];

                //shift beginning of the time window until matched configured WindowSize
                while (currentMetric.Timestamp - timeSeries[timeWindowStartIndex].Timestamp > configuration.WindowSize)
                {
                    timeWindowStartIndex++;
                }

                //wait until EvaluationFrequency interval has passed since last rule execution
                if (currentMetric.Timestamp - lastEvaluation < configuration.EvaluationFrequency)
                {
                    continue;
                }

                lastEvaluation = currentMetric.Timestamp;

                var timeSeriesSpan = timeSeries.AsSpan();

                var value = configuration.TimeAggregation switch
                {
                    TimeAggregation.Average => timeSeriesSpan[timeWindowStartIndex..timeWindowEndIndex]
                        .Average(metric => metric.Value),
                    TimeAggregation.Minimum => timeSeriesSpan[timeWindowStartIndex..timeWindowEndIndex]
                        .Min(metric => metric.Value),
                    TimeAggregation.Maximum => timeSeriesSpan[timeWindowStartIndex..timeWindowEndIndex]
                        .Max(metric => metric.Value),
                    TimeAggregation.Total => timeSeriesSpan[timeWindowStartIndex..timeWindowEndIndex]
                        .Sum(metric => metric.Value),
                    TimeAggregation.Count => timeSeriesSpan[timeWindowStartIndex..timeWindowEndIndex].Length,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var result = configuration.Operator switch
                {
                    Operator.Equals => Math.Abs(value - configuration.Threshold) < double.Epsilon,
                    Operator.GreaterThan => value > configuration.Threshold,
                    Operator.GreaterThanOrEqual => value >= configuration.Threshold,
                    Operator.LessThan => value < configuration.Threshold,
                    Operator.LessThanOrEqual => value <= configuration.Threshold,
                    _ => throw new ArgumentOutOfRangeException()
                };

                evaluationPeriods.Enqueue(result);

                while (evaluationPeriods.Count > configuration.NumberOfEvaluationPeriods)
                {
                    evaluationPeriods.Dequeue();
                }

                if (evaluationPeriods.Count(b => b) >= configuration.MinFailingPeriodsToAlert &&
                    ongoingOutageStart == null)
                {
                    ongoingOutageStart = currentMetric.Timestamp;
                }

                if (evaluationPeriods.Count(b => b) < configuration.MinFailingPeriodsToAlert &&
                    ongoingOutageStart != null)
                {
                    yield return new Outage(ongoingOutageStart.Value, currentMetric.Timestamp);
                    ongoingOutageStart = null;
                }
            }
        }
    }
}