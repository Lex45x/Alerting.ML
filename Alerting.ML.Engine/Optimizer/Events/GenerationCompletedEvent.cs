using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record GenerationCompletedEvent : IEvent
{
    public GenerationCompletedEvent(int aggregateVersion)
    {
        AggregateVersion = aggregateVersion;
    }

    public int AggregateVersion { get; }
}