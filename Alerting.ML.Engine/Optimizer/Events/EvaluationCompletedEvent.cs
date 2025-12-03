using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record EvaluationCompletedEvent<T> : EvaluationCompletedEvent where T : AlertConfiguration
{
    public T Configuration { get; }
    public IReadOnlyList<Outage> Outages { get; }

    public EvaluationCompletedEvent(T configuration, IReadOnlyList<Outage> outages, int aggregateVersion) : base(
        aggregateVersion)
    {
        Configuration = configuration;
        Outages = outages;
    }

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

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Configuration, Outages);
    }
}

public record EvaluationCompletedEvent : IEvent
{
    public EvaluationCompletedEvent(int aggregateVersion)
    {
        AggregateVersion = aggregateVersion;
    }

    public int AggregateVersion { get; }
}