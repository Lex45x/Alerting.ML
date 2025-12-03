using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record AlertScoreComputedEvent : IEvent
{
    public AlertScoreCard AlertScoreCard { get; }

    public AlertScoreComputedEvent(AlertScoreCard alertScoreCard, int aggregateVersion)
    {
        AlertScoreCard = alertScoreCard;
        AggregateVersion = aggregateVersion;
    }

    public int AggregateVersion { get; }
}