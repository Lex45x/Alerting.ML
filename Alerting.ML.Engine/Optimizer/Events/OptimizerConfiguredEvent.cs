using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
/// Indicates that <paramref name="Configuration"/> will be used to proceed with training.
/// </summary>
/// <param name="Configuration">Configuration to drive training process.</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
public record OptimizerConfiguredEvent(OptimizationConfiguration Configuration, int AggregateVersion)
    : IEvent;