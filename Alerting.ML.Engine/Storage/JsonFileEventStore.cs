using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
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
        WriteIndented = false, //this setting is critical as each new line considered as an end of event json.
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
        await using var fileStream = File.OpenRead(GetAggregateFileName(aggregateId));

        var eventsStream = JsonSerializer.DeserializeAsyncEnumerable<IEvent>(fileStream, topLevelValues: true, SerializerOptions, cancellationToken);
        
        await foreach (var @event in eventsStream)
        {
            if (@event != null)
            {
                yield return @event;
            }
            else
            {
                //todo: is that possible? If so - need to handle.
            }
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

    private static readonly ReadOnlyMemory<byte> NewLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

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

                await using var eventsStream = File.Open(GetAggregateFileName(aggregateId), FileMode.Append);

                foreach (var @event in DequeueAll())
                {
                    await JsonSerializer.SerializeAsync(eventsStream, @event, SerializerOptions);
                    await eventsStream.WriteAsync(NewLineBytes);
                }

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