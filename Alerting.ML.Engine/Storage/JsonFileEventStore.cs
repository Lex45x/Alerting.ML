using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Alerting.ML.Engine.Storage;

/// <summary>
/// Implements an event store and saves state of each aggregate into dedicated JSON file, allowing parallel reads and writes. <br/>
/// Keeps freshly written events in-memory with periodic offload of events into filesystem.
/// </summary>
public class JsonFileEventStore : IEventStore, IDisposable
{
    private readonly string folder;
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<IEvent>> publishingQueue = new();
    private static readonly JsonSerializerSettings SerializerOptions = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly CancellationTokenSource writingCancellationSource = new();
    private readonly Task writingTask;

    /// <summary>
    /// Creates a new instance of <see cref="JsonFileEventStore"/>
    /// </summary>
    /// <param name="folder">A folder with json files.</param>
    public JsonFileEventStore(string folder)
    {
        this.folder = folder;
        Directory.CreateDirectory(folder);
        writingTask = Task.Run(WriteEvents);
    }

    private async Task Flush()
    {
        foreach (var (aggregateId, events) in publishingQueue)
        {
            try
            {
                if (events.Count == 0)
                {
                    continue;
                }

                var serializedEvents = DequeueAll().Select(@event => JsonConvert.SerializeObject(@event, SerializerOptions));

                await File.AppendAllLinesAsync(GetAggregateFileName(aggregateId), serializedEvents);

                IEnumerable<IEvent> DequeueAll()
                {
                    while (events.TryDequeue(out var @event))
                    {
                        yield return @event;
                    }
                }
            }
            catch (Exception)
            {
                //todo: panic and run in circles as writing of events has failed. ¯\_(ツ)_/¯
            }
        }
         
    }
    
    private async Task WriteEvents()
    {
        while (!writingCancellationSource.IsCancellationRequested)
        {
            await Task.Delay(10_000, writingCancellationSource.Token);
            await Flush();
        }
    }

    /// <inheritdoc />
    public void Write<T>(Guid aggregateId, T @event) where T : IEvent
    {
        var queue = publishingQueue.GetOrAdd(aggregateId, guid => new ConcurrentQueue<IEvent>());
        queue.Enqueue(@event);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEvent> GetAll(Guid aggregateId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var streamReader = File.OpenText(GetAggregateFileName(aggregateId));

        var eventJson = await streamReader.ReadLineAsync(cancellationToken); 

        while (!string.IsNullOrWhiteSpace(eventJson) && !cancellationToken.IsCancellationRequested)
        {
            //todo: implement repair strategy. Offload all valid events into new file and purge old.
            yield return JsonConvert.DeserializeObject<IEvent>(eventJson, SerializerOptions) ?? throw new InvalidOperationException($"Null event deserialized for aggregate {aggregateId}!");
            eventJson = await streamReader.ReadLineAsync(cancellationToken);
        }
    }

    private string GetAggregateFileName(Guid aggregateId)
    {
        return Path.Combine(folder, $"{aggregateId}.json");
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<Guid> GetExistingAggregates()
    {
        foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
        {
            var guidName = Path.GetFileNameWithoutExtension(file);
            yield return Guid.Parse(guidName);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        writingCancellationSource.Cancel();
        writingCancellationSource.Dispose();
        Task.Run(Flush).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}