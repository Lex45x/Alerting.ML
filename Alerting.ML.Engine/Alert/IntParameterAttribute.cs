namespace Alerting.ML.Engine.Alert;

/// <summary>
/// Represent an integer property bounded by <see cref="min"/> and <see cref="max"/> with the configurable nudge <see cref="step"/>
/// </summary>
public class IntParameterAttribute : ConfigurationParameterAttribute
{
    private readonly int min;
    private readonly int max;
    private readonly int step;

    /// <summary>
    /// Configures property with allowed <paramref name="min"/> and <paramref name="max"/> values.
    /// Defines a maximum <paramref name="step"/> for <see cref="Nudge"/>
    /// </summary>
    /// <param name="min">Minimum value of the property</param>
    /// <param name="max">Maximum value of the property</param>
    /// <param name="step">Maximum <see cref="Nudge"/> step size.</param>
    public IntParameterAttribute(int min, int max, int step)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }

    /// <inheritdoc />
    public override object Nudge(object value, AlertConfiguration appliedTo)
    {
        if (Random.Shared.NextDouble() > 0.5)
        {
            return value;
        }
        var newValue = (int)value + Random.Shared.Next(-step, step);
        return Math.Min(Math.Max(newValue, min), max);
    }

    /// <inheritdoc />
    public override object GetRandomValue(AlertConfiguration appliedTo)
    {
        return Random.Shared.Next(min, max);
    }
}