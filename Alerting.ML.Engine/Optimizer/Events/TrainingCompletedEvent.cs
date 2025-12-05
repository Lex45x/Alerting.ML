namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
/// Indicates that last generation has been computed and training is over.
/// </summary>
/// <param name="AggregateVersion"></param>
public record TrainingCompletedEvent(int AggregateVersion) : GenerationCompletedEvent(AggregateVersion);