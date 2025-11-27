namespace Alerting.ML.Engine.Alert;

public abstract class OneOfParameterAttribute<TValue> : ConfigurationParameterAttribute
{
    public sealed override object GetRandomValue(IAlertConfiguration appliedTo)
    {
        var randomIndex = Random.Shared.Next(0, AllowedValues.Count);

        return AllowedValues[randomIndex]!;
    }

    public sealed override object Nudge(object value, IAlertConfiguration appliedTo)
    {
        var targetIndex = 0;
        for (var i = 0; i < AllowedValues.Count; i++)
        {
            if (Equals(AllowedValues[i], (TValue)value))
            {
                targetIndex = i;
            }
        }

        var resultIndex = targetIndex + Random.Shared.NextDouble() > 0.5 ? -1 : 1;
        var clippedIndex = Math.Min(Math.Max(resultIndex, 0), AllowedValues.Count - 1);

        return AllowedValues[clippedIndex]!;
    }

    protected abstract IReadOnlyList<TValue> AllowedValues { get; }
}