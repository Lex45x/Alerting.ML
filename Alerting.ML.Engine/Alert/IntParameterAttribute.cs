namespace Alerting.ML.Engine.Alert;

public class IntParameterAttribute : ConfigurationParameterAttribute
{
    private readonly int min;
    private readonly int max;
    private readonly int step;

    public IntParameterAttribute(int min, int max, int step)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }

    public override object Nudge(object value, IAlertConfiguration appliedTo)
    {
        if (Random.Shared.NextDouble() > 0.5)
        {
            return value;
        }
        var newValue = (int)value + Random.Shared.Next(-step, step);
        return Math.Min(Math.Max(newValue, min), max);
    }

    public override object GetRandomValue(IAlertConfiguration appliedTo)
    {
        return Random.Shared.Next(min, max);
    }
}