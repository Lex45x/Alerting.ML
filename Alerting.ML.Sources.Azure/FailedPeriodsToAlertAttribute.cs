using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

internal class FailedPeriodsToAlertAttribute : ConfigurationParameterAttribute
{
    private readonly int step;

    public FailedPeriodsToAlertAttribute(int step)
    {
        this.step = step;
    }

    public override object GetRandomValue(AlertConfiguration appliedTo, TimeSeriesStatistics statistics)
    {
        var configuration = (ScheduledQueryRuleConfiguration)appliedTo;
        return Random.Shared.Next(minValue: 1, configuration.NumberOfEvaluationPeriods + 1);
    }

    public override object Nudge(object value, AlertConfiguration appliedTo, TimeSeriesStatistics statistics)
    {
        if (Random.Shared.NextDouble() > 0.5)
        {
            return value;
        }

        var configuration = (ScheduledQueryRuleConfiguration)appliedTo;
        var newValue = (int)value + Random.Shared.Next(-step, step);
        return Math.Min(Math.Max(newValue, val2: 1), configuration.NumberOfEvaluationPeriods);
    }

    public override object CrossoverRepair(object value, AlertConfiguration appliedTo)
    {
        var configuration = (ScheduledQueryRuleConfiguration)appliedTo;
        return Math.Min(Math.Max((int)value, val2: 1), configuration.NumberOfEvaluationPeriods);
    }
}