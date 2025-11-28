namespace Alerting.ML.Engine.Storage;

/// <summary>
/// Provides access to store and read <see cref="IEvent"/>s produced in engine.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Writes and event associated with <paramref name="aggregateId"/> into event store.
    /// </summary>
    /// <param name="aggregateId">Unique id of aggregate.</param>
    /// <param name="event">An event to write.</param>
    /// <typeparam name="T">Type of event.</typeparam>
    /// <returns>Task that completes when event is persisted.</returns>
    Task Write<T>(Guid aggregateId, T @event) where T: IEvent;
    
    /// <summary>
    /// Allows to get a steam of all events stored for a particular aggregate.
    /// </summary>
    /// <param name="aggregateId">Unique id of aggregate.</param>
    /// <returns>All events associated with aggregate.</returns>
    IAsyncEnumerable<IEvent> GetAll(Guid aggregateId);
}