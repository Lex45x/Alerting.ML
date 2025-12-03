using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
///     Contains a list of survivors from current generation according to
///     <see cref="OptimizationConfiguration.SurvivorPercentage" />
/// </summary>
/// <param name="Survivors">Survived configuration.</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
/// <typeparam name="T">Current alert configuration type</typeparam>
public record SurvivorsCountedEvent<T>(IReadOnlyList<T> Survivors, int AggregateVersion) : IEvent
    where T : AlertConfiguration
{
    /// <inheritdoc />
    public virtual bool Equals(SurvivorsCountedEvent<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Survivors.SequenceEqual(other.Survivors);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Survivors.GetHashCode();
    }
}