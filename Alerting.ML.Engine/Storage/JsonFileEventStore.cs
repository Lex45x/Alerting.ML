using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Alerting.ML.Engine.Storage;

/// <summary>
///     Implements an event store and saves state of each aggregate into dedicated JSON file, allowing parallel reads and
///     writes. <br />
///     Keeps freshly written events in-memory with periodic offload of events into filesystem.
/// </summary>
public class JsonFileEventStore : IEventStore, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        TypeInfoResolver = KnownTypeInfoResolver.Instance,
        Converters = { new MetricsListConverter() }
    };

    private readonly string folder;
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<IEvent>> publishingQueue = new();

    private readonly CancellationTokenSource writingCancellationSource = new();
    private readonly Task writingTask;

    /// <summary>
    ///     Creates a new instance of <see cref="JsonFileEventStore" />
    /// </summary>
    /// <param name="folder">A folder with json files.</param>
    public JsonFileEventStore(string folder)
    {
        this.folder = folder;
        Directory.CreateDirectory(folder);
        writingTask = Task.Run(WriteEvents);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        writingCancellationSource.Cancel();
        writingCancellationSource.Dispose();
        Task.Run(Flush).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void Write<T>(Guid aggregateId, T @event) where T : IEvent
    {
        var queue = publishingQueue.GetOrAdd(aggregateId, guid => new ConcurrentQueue<IEvent>());
        queue.Enqueue(@event);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IEvent> GetAll(Guid aggregateId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var streamReader = File.OpenText(GetAggregateFileName(aggregateId));

        var eventJson = await streamReader.ReadLineAsync(cancellationToken);

        while (!string.IsNullOrWhiteSpace(eventJson) && !cancellationToken.IsCancellationRequested)
        {
            //todo: implement repair strategy. Offload all valid events into new file and purge old.
            yield return JsonSerializer.Deserialize<IEvent>(eventJson, SerializerOptions) ??
                         throw new InvalidOperationException($"Null event deserialized for aggregate {aggregateId}!");
            eventJson = await streamReader.ReadLineAsync(cancellationToken);
        }
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

                var serializedEvents =
                    DequeueAll().Select(@event => JsonSerializer.Serialize(@event, SerializerOptions));

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
            await Task.Delay(millisecondsDelay: 10_000, writingCancellationSource.Token);
            await Flush();
        }
    }

    private string GetAggregateFileName(Guid aggregateId)
    {
        return Path.Combine(folder, $"{aggregateId}.json");
    }
}