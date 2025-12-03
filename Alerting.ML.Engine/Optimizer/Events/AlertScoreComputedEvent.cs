using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
/// Indicates that score was computed for the latest score evaluation.
/// </summary>
/// <param name="AlertScoreCard">Summary score that given configuration reached during latest optimization run.</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
public record AlertScoreComputedEvent(AlertScoreCard AlertScoreCard, int AggregateVersion) : IEvent;