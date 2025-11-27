using System.Collections.Concurrent;

namespace Alerting.ML.Engine.Storage;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<Guid, List<IEvent>> eventsDictionary = new();

    public Task Write<T>(Guid aggregateId, T @event) where T : IEvent
    {
        eventsDictionary.GetOrAdd(aggregateId, _ => []).Add(@event);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEvent> GetAll(Guid aggregateId)
    {
        return eventsDictionary.TryGetValue(aggregateId, out var events)
            ? events.ToAsyncEnumerable()
            : Enumerable.Empty<IEvent>().ToAsyncEnumerable();
    }
}