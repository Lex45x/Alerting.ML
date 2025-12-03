using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
///     Indicates completion of the single configuration evaluation.
/// </summary>
/// <typeparam name="T">Current alert configuration type.</typeparam>
/// <param name="Configuration">Configuration that was evaluated.</param>
/// <param name="Outages">Outages created by configuration.</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
public record EvaluationCompletedEvent<T>(T Configuration, IReadOnlyList<Outage> Outages, int AggregateVersion)
    : EvaluationCompletedEvent(AggregateVersion)
    where T : AlertConfiguration
{
    /// <inheritdoc />
    public virtual bool Equals(EvaluationCompletedEvent<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Configuration.Equals(other.Configuration) && Outages.SequenceEqual(other.Outages);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Configuration, Outages);
    }
}

/// <summary>
///     Indicates completion of the single configuration evaluation.
/// </summary>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
/// s
public abstract record EvaluationCompletedEvent(int AggregateVersion) : IEvent;