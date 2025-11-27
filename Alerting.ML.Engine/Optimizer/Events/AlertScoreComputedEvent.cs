using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

internal class AlertScoreComputedEvent<T> : IEvent where T : AlertConfiguration<T>
{
    public T Configuration { get; }
    public AlertScoreCard AlertScoreCard { get; }

    public AlertScoreComputedEvent(T configuration, AlertScoreCard alertScoreCard)
    {
        Configuration = configuration;
        AlertScoreCard = alertScoreCard;
    }

    public override string ToString()
    {
        return $"AlertScoreComputedEvent: {AlertScoreCard}";
    }
}