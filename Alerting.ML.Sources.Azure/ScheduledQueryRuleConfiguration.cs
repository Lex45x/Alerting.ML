using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

/// <summary>
/// A configuration for scheduled query rule alert as described in <a href="https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/2025-01-01-preview/scheduledqueryrules?pivots=deployment-language-arm-template#condition-1">Scheduled Query Rule Alert Conditions</a>
/// </summary>
public class ScheduledQueryRuleConfiguration : AlertConfiguration
{
    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"{nameof(Operator)}: {Operator}, {nameof(Threshold)}: {Threshold}, {nameof(NumberOfEvaluationPeriods)}: {NumberOfEvaluationPeriods}, {nameof(MinFailingPeriodsToAlert)}: {MinFailingPeriodsToAlert}, {nameof(TimeAggregation)}: {TimeAggregation}, {nameof(WindowSize)}: {WindowSize}, {nameof(EvaluationFrequency)}: {EvaluationFrequency}";
    }

    /// <summary>
    /// Defines a comparison operator for alert. Evaluation period is considered failed when operator execution returns true.
    /// </summary>
    [EnumParameter<Operator>]
    public Operator Operator { get; init; }

    /// <summary>
    /// Threshold value for comparison in evaluation period.
    /// </summary>
    [IntParameter(-100, 100, 5)]
    public int Threshold { get; init; }

    /// <summary>
    /// Number of last evaluation results that will be kept for comparison.
    /// </summary>
    [IntParameter(1, 20, 1)]
    public int NumberOfEvaluationPeriods { get; init; }

    /// <summary>
    /// Amount of evaluation periods from <see cref="NumberOfEvaluationPeriods"/> that should fail to result in alert.
    /// </summary>
    [FailedPeriodsToAlert(20, Order = 1)]
    public int MinFailingPeriodsToAlert { get; init; }

    /// <summary>
    /// A function to aggregate time-series values in evaluation period.
    /// </summary>
    [EnumParameter<TimeAggregation>]
    public TimeAggregation TimeAggregation { get; init; }

    /// <summary>
    /// The size of the time-window to be aggregated.
    /// </summary>
    [WindowSizeParameter]
    public TimeSpan WindowSize { get; init; }

    /// <summary>
    /// Frequency of alert rule execution.
    /// </summary>
    [EvaluationFrequencyParameter]
    public TimeSpan EvaluationFrequency { get; init; }
}