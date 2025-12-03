namespace Alerting.ML.Engine.Storage;

/// <summary>
///     Base interface for all events produced in Engine.
/// </summary>
public interface IEvent
{
    /// <summary>
    ///     Indicates a version of the aggregate current event was applied to.
    /// </summary>
    int AggregateVersion { get; }
}