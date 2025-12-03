using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record SurvivorsCountedEvent<T> : IEvent where T : AlertConfiguration
{
    public IReadOnlyList<T> Survivors { get; }

    public SurvivorsCountedEvent(IReadOnlyList<T> survivors, int aggregateVersion)
    {
        Survivors = survivors;
        AggregateVersion = aggregateVersion;
    }

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

        return Survivors.Equals(other.Survivors);
    }

    public override int GetHashCode()
    {
        return Survivors.GetHashCode();
    }

    public int AggregateVersion { get; }
}