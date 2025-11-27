namespace Alerting.ML.Engine.Storage;

public interface IEventStore
{
    Task Write<T>(Guid aggregateId, T @event) where T: IEvent;
    IAsyncEnumerable<IEvent> GetAll(Guid aggregateId);
}