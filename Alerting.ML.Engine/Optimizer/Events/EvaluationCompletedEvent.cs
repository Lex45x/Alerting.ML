using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
///     Indicates completion of the single configuration evaluation.
/// </summary>
/// <typeparam name="T">Current alert configuration type.</typeparam>
/// <param name="Configuration">Configuration that was evaluated.</param>
/// <param name="Outages">Outages created by configuration.</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
public record EvaluationCompletedEvent<T>(T Configuration, IReadOnlyList<Outage> Outages, int AggregateVersion)
    : EvaluationCompletedEvent(AggregateVersion)
    where T : AlertConfiguration
{
}

/// <summary>
///     Indicates completion of the single configuration evaluation.
/// </summary>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
/// s
public abstract record EvaluationCompletedEvent(int AggregateVersion) : IEvent;