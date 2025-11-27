using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

public class ScheduledQueryRuleConfiguration : AlertConfiguration
{
    public override string ToString()
    {
        return
            $"{nameof(Operator)}: {Operator}, {nameof(Threshold)}: {Threshold}, {nameof(NumberOfEvaluationPeriods)}: {NumberOfEvaluationPeriods}, {nameof(MinFailingPeriodsToAlert)}: {MinFailingPeriodsToAlert}, {nameof(TimeAggregation)}: {TimeAggregation}, {nameof(WindowSize)}: {WindowSize}, {nameof(EvaluationFrequency)}: {EvaluationFrequency}";
    }

    [EnumParameter<Operator>]
    public Operator Operator { get; init; }

    [IntParameter(-100, 100, 5)]
    public int Threshold { get; init; }

    [IntParameter(1, 20, 1)]
    public int NumberOfEvaluationPeriods { get; init; }

    [FailedPeriodsToAlert(20, Order = 1)]
    public int MinFailingPeriodsToAlert { get; init; }

    [EnumParameter<TimeAggregation>]
    public TimeAggregation TimeAggregation { get; init; }

    [WindowSizeParameter]
    public TimeSpan WindowSize { get; init; }

    [EvaluationFrequencyParameter]
    public TimeSpan EvaluationFrequency { get; init; }
}