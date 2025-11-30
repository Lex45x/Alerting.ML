using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public class AlertScoreComputedEvent : IEvent
{
    public AlertScoreCard AlertScoreCard { get; }

    public AlertScoreComputedEvent(AlertScoreCard alertScoreCard)
    {
        AlertScoreCard = alertScoreCard;
    }

    public override string ToString()
    {
        return $"AlertScoreComputedEvent: {AlertScoreCard}";
    }
}

public class OptimizerConfiguredEvent : IEvent
{
    public OptimizationConfiguration Configuration { get; }

    public OptimizerConfiguredEvent(OptimizationConfiguration configuration)
    {
        Configuration = configuration;
    }

    public override string ToString()
    {
        return $"OptimizerConfiguredEvent{nameof(Configuration)}: {Configuration}";
    }
}