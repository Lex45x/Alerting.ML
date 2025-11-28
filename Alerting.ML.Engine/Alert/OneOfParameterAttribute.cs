namespace Alerting.ML.Engine.Alert;

/// <summary>
/// A base class for all parameter values with small and finite list of possible values.
/// Allows descendants to define <see cref="AllowedValues"/> and get all the necessary logic out of the box.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class OneOfParameterAttribute<TValue> : ConfigurationParameterAttribute
{
    /// <inheritdoc />
    public sealed override object GetRandomValue(AlertConfiguration appliedTo)
    {
        var randomIndex = Random.Shared.Next(0, AllowedValues.Count);

        return AllowedValues[randomIndex]!;
    }

    /// <inheritdoc />
    public sealed override object Nudge(object value, AlertConfiguration appliedTo)
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

    /// <summary>
    /// A list of values a given property can take.
    /// </summary>
    protected abstract IReadOnlyList<TValue> AllowedValues { get; }
}