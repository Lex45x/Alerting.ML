using System.Collections.Concurrent;

namespace Alerting.ML.Engine.Storage;

/// <summary>
/// Implements <see cref="IEventStore"/> that keeps all events in-memory.
/// All events are destroyed after event store is collected by GC.
/// </summary>
public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<Guid, List<IEvent>> eventsDictionary = new();

    /// <inheritdoc />
    public void Write<T>(Guid aggregateId, T @event) where T : IEvent
    {
        eventsDictionary.GetOrAdd(aggregateId, _ => []).Add(@event);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<IEvent> GetAll(Guid aggregateId, CancellationToken cancellationToken)
    {
        return eventsDictionary.TryGetValue(aggregateId, out var events)
            ? events.ToAsyncEnumerable()
            : Enumerable.Empty<IEvent>().ToAsyncEnumerable();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetExistingAggregates()
    {
        return eventsDictionary.Keys.ToAsyncEnumerable();
    }
}