using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

/// <summary>
///     A configuration for scheduled query rule alert as described in
///     <a
///         href="https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/2025-01-01-preview/scheduledqueryrules?pivots=deployment-language-arm-template#condition-1">
///         Scheduled Query Rule Alert Conditions
///     </a>
/// </summary>
public class ScheduledQueryRuleConfiguration : AlertConfiguration
{
    /// <summary>
    ///     Defines a comparison operator for alert. Evaluation period is considered failed when operator execution returns
    ///     true.
    /// </summary>
    [EnumParameter<Operator>]
    public Operator Operator { get; init; }

    /// <summary>
    ///     Threshold value for comparison in evaluation period.
    /// </summary>
    [IntParameter(min: -100, max: 100, step: 2)]
    public int Threshold { get; init; }

    /// <summary>
    ///     Number of last evaluation results that will be kept for comparison.
    /// </summary>
    [IntParameter(min: 1, max: 20, step: 1)]
    public int NumberOfEvaluationPeriods { get; init; }

    /// <summary>
    ///     Amount of evaluation periods from <see cref="NumberOfEvaluationPeriods" /> that should fail to result in alert.
    /// </summary>
    [FailedPeriodsToAlert(step: 20, Order = 1)]
    public int MinFailingPeriodsToAlert { get; init; }

    /// <summary>
    ///     A function to aggregate time-series values in evaluation period.
    /// </summary>
    [EnumParameter<TimeAggregation>]
    public TimeAggregation TimeAggregation { get; init; }

    /// <summary>
    ///     The size of the time-window to be aggregated.
    /// </summary>
    [WindowSizeParameter]
    public TimeSpan WindowSize { get; init; }

    /// <summary>
    ///     Frequency of alert rule execution.
    /// </summary>
    [EvaluationFrequencyParameter]
    public TimeSpan EvaluationFrequency { get; init; }

    /// <inheritdoc />
    public override bool Equals(AlertConfiguration? other)
    {
        if (other is not ScheduledQueryRuleConfiguration otherAlert)
        {
            return false;
        }

        return Operator == otherAlert.Operator && Threshold == otherAlert.Threshold &&
               NumberOfEvaluationPeriods == otherAlert.NumberOfEvaluationPeriods &&
               MinFailingPeriodsToAlert == otherAlert.MinFailingPeriodsToAlert &&
               TimeAggregation == otherAlert.TimeAggregation && WindowSize.Equals(otherAlert.WindowSize) &&
               EvaluationFrequency.Equals(otherAlert.EvaluationFrequency);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ScheduledQueryRuleConfiguration)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Operator, Threshold, NumberOfEvaluationPeriods, MinFailingPeriodsToAlert,
            (int)TimeAggregation, WindowSize, EvaluationFrequency);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"{nameof(Operator)}: {Operator}, {nameof(Threshold)}: {Threshold}, {nameof(NumberOfEvaluationPeriods)}: {NumberOfEvaluationPeriods}, {nameof(MinFailingPeriodsToAlert)}: {MinFailingPeriodsToAlert}, {nameof(TimeAggregation)}: {TimeAggregation}, {nameof(WindowSize)}: {WindowSize}, {nameof(EvaluationFrequency)}: {EvaluationFrequency}";
    }

    /// <inheritdoc />
    public override double Distance(AlertConfiguration other)
    {
        if (other is not ScheduledQueryRuleConfiguration otherConfiguration)
        {
            throw new InvalidOperationException();
        }

        var squaresSum = 0.0;

        squaresSum += Math.Pow(Operator - otherConfiguration.Operator, y: 2);
        squaresSum += Math.Pow(Threshold - otherConfiguration.Threshold, y: 2);
        squaresSum += Math.Pow(NumberOfEvaluationPeriods - otherConfiguration.NumberOfEvaluationPeriods, y: 2);
        squaresSum += Math.Pow(MinFailingPeriodsToAlert - otherConfiguration.MinFailingPeriodsToAlert, y: 2);
        squaresSum += Math.Pow(TimeAggregation - otherConfiguration.TimeAggregation, y: 2);
        squaresSum += Math.Pow((WindowSize - otherConfiguration.WindowSize).TotalMinutes, y: 2);
        squaresSum += Math.Pow((EvaluationFrequency - otherConfiguration.EvaluationFrequency).TotalMinutes, y: 2);

        return Math.Sqrt(squaresSum);
    }
}