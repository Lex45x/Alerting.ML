using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record OptimizerConfiguredEvent : IEvent
{
    public OptimizationConfiguration Configuration { get; }

    public OptimizerConfiguredEvent(OptimizationConfiguration configuration, int aggregateVersion)
    {
        Configuration = configuration;
        AggregateVersion = aggregateVersion;
    }

    public int AggregateVersion { get; }
}