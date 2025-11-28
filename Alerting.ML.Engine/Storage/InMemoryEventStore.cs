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
    public Task Write<T>(Guid aggregateId, T @event) where T : IEvent
    {
        eventsDictionary.GetOrAdd(aggregateId, _ => []).Add(@event);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<IEvent> GetAll(Guid aggregateId)
    {
        return eventsDictionary.TryGetValue(aggregateId, out var events)
            ? events.ToAsyncEnumerable()
            : Enumerable.Empty<IEvent>().ToAsyncEnumerable();
    }
}