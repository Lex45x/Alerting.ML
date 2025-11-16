using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

internal class FailedPeriodsToAlertAttribute : ConfigurationParameterAttribute
{
    private readonly int step;

    public FailedPeriodsToAlertAttribute(int step)
    {
        this.step = step;
    }
    public override object GetRandomValue(IAlertConfiguration appliedTo)
    {
        var configuration = (ScheduledQueryRuleConfiguration)appliedTo;
        return Random.Shared.Next(1, configuration.NumberOfEvaluationPeriods + 1);
    }

    public override object Nudge(object value, IAlertConfiguration appliedTo)
    {
        if (Random.Shared.NextDouble() > 0.5)
        {
            return value;
        }
        var configuration = (ScheduledQueryRuleConfiguration)appliedTo;
        var newValue = (int)value + Random.Shared.Next(-step, step);
        return Math.Min(Math.Max(newValue, 1), configuration.NumberOfEvaluationPeriods);
    }

    public override bool CrossoverSensitive => true;
    public override object CrossoverRepair(object value, IAlertConfiguration appliedTo)
    {
        var configuration = (ScheduledQueryRuleConfiguration)appliedTo;
        return Math.Min(Math.Max((int)value, 1), configuration.NumberOfEvaluationPeriods);
    }
}