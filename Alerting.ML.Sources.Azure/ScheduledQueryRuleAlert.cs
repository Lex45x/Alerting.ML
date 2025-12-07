using System.Collections.Immutable;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Azure;

/// <summary>
///     Represent an <b>approximate</b> implementation of
///     <a href="https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/scheduledqueryrules">
///         Azure Scheduled
///         Query Rule
///     </a>
///     based on the information inferred from bicep parameters and their description. <br />
///     Assumes usage of StaticThresholdCriterion, a single criteria, and no dimensions.
/// </summary>
public class ScheduledQueryRuleAlert : IAlert<ScheduledQueryRuleConfiguration>
{
    /// <inheritdoc />
    public string ProviderName => "Azure";


    /// <inheritdoc />
    public IEnumerable<Outage> Evaluate(ImmutableArray<Metric> timeSeries,
        ScheduledQueryRuleConfiguration configuration)
    {
        var evaluationPeriods = new Queue<bool>(); //holds last NumberOfEvaluationPeriods results

        DateTime? ongoingOutageStart = null;
        var timeWindowStartIndex = 0;
        var raisedOutages = new List<Outage>();
        ISlidingWindowCalculator calculator = configuration.TimeAggregation switch
        {
            TimeAggregation.Average => new SlidingWindowAverageCalculator(),
            TimeAggregation.Minimum => new SlidingWindowMaxCalculator(),
            TimeAggregation.Maximum => new SlidingWindowMinCalculator(),
            TimeAggregation.Total => new SlidingWindowTotalCalculator(),
            TimeAggregation.Count => new SlidingWindowCountCalculator(),
            _ => throw new ArgumentOutOfRangeException()
        };

        var timeSeriesSpan = timeSeries.AsSpan();
        var lastEvaluation = timeSeries[index: 0].Timestamp;

        for (var timeWindowEndIndex = 0; timeWindowEndIndex < timeSeries.Length; timeWindowEndIndex++)
        {
            var timeWindow = timeSeriesSpan[timeWindowStartIndex..timeWindowEndIndex];
            var currentMetric = timeSeries[timeWindowEndIndex];
            calculator.Add(currentMetric.Value, timeWindow);

            //shift beginning of the time window until matched configured WindowSize
            while (currentMetric.Timestamp - timeSeries[timeWindowStartIndex].Timestamp > configuration.WindowSize)
            {
                timeWindowStartIndex++;
                calculator.Remove(timeSeriesSpan[timeWindowStartIndex - 1].Value, timeWindow);
            }

            //wait until EvaluationFrequency interval has passed since last rule execution
            if (currentMetric.Timestamp - lastEvaluation < configuration.EvaluationFrequency)
            {
                continue;
            }

            lastEvaluation = currentMetric.Timestamp;

            var result = configuration.Operator switch
            {
                Operator.Equals => Math.Abs(calculator.Value - configuration.Threshold) < double.Epsilon,
                Operator.GreaterThan => calculator.Value > configuration.Threshold,
                Operator.GreaterThanOrEqual => calculator.Value >= configuration.Threshold,
                Operator.LessThan => calculator.Value < configuration.Threshold,
                Operator.LessThanOrEqual => calculator.Value <= configuration.Threshold,
                _ => throw new ArgumentOutOfRangeException()
            };

            evaluationPeriods.Enqueue(result);

            while (evaluationPeriods.Count > configuration.NumberOfEvaluationPeriods)
            {
                evaluationPeriods.Dequeue();
            }

            var successEvaluationPeriods = evaluationPeriods.Count(b => b);

            // condition for determining outage start.
            if (successEvaluationPeriods >= configuration.MinFailingPeriodsToAlert &&
                !ongoingOutageStart.HasValue)
            {
                ongoingOutageStart = currentMetric.Timestamp;
            }

            // condition for determining outage end.
            if (successEvaluationPeriods == 0 && ongoingOutageStart.HasValue)
            {
                raisedOutages.Add(new Outage(ongoingOutageStart.Value, currentMetric.Timestamp));
                ongoingOutageStart = null;
            }
        }

        return raisedOutages;
    }

    /// <summary>
    /// Public non-interface member for benchmarking purposes. <see cref="Evaluate"/> method is copied here to apply changes and compare outcomes with baseline.
    /// </summary>
    /// <param name="timeSeries"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public IEnumerable<Outage> EvaluateOptimized(ImmutableArray<Metric> timeSeries,
        ScheduledQueryRuleConfiguration configuration)
    {
        return Evaluate(timeSeries, configuration);
    }
}