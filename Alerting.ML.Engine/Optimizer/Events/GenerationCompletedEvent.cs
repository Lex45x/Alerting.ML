using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
/// Indicates that current generation processing is completed.s
/// </summary>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
public record GenerationCompletedEvent(int AggregateVersion) : IEvent;