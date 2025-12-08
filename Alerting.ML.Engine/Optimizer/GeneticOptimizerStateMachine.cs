using System.Collections.Concurrent;
using System.Collections.Immutable;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
///     A state-machine that operates <see cref="GeneticOptimizerState{T}" />
/// </summary>
/// <typeparam name="T">Underlying <see cref="AlertConfiguration" /></typeparam>
public class GeneticOptimizerStateMachine<T> : IGeneticOptimizer
    where T : AlertConfiguration
{
    private static readonly ImmutableArray<string> Adjectives =
    [
        "Silent",
        "Quantum",
        "Crimson",
        "Azure",
        "Neon",
        "Radiant",
        "Infinite",
        "Stellar",
        "Phantom",
        "Hyper",
        "Lunar",
        "Titanium",
        "Rapid",
        "Mystic",
        "Obsidian",
        "Electric",
        "Golden",
        "Arctic",
        "Synthetic",
        "Turbo"
    ];

    private static readonly ImmutableArray<string> Nouns =
    [
        "Falcon",
        "Nova",
        "Eclipse",
        "Specter",
        "Pulse",
        "Vertex",
        "Comet",
        "Horizon",
        "Catalyst",
        "Phoenix",
        "Reactor",
        "Matrix",
        "Cyclone",
        "Sentinel",
        "Prism",
        "Kraken",
        "Orbit",
        "Cipher",
        "Titan",
        "Vortex"
    ];

    private readonly GeneticOptimizerState<T> current;

    private readonly ConcurrentDictionary<T, WeakReference<IReadOnlyList<Outage>>> EvaluationCache = new();
    private readonly IEventStore store;

    /// <summary>
    ///     Creates a new instance of Genetic Optimizer with a new training.
    /// </summary>
    /// <param name="alert">Alert rule to be optimized.</param>
    /// <param name="timeSeriesProvider">Provider of the relevant metric.</param>
    /// <param name="knownOutagesProvider">Provider of known outages.</param>
    /// <param name="alertScoreCalculator">Calculates alert score based on detected outages.</param>
    /// <param name="configurationFactory">A relevant factory for <typeparamref name="T" /></param>
    /// <param name="store">An event store to persist all events.</param>
    public GeneticOptimizerStateMachine(IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, IEventStore store)
    {
        this.store = store;
        current = new GeneticOptimizerState<T>();

        var aggregateId = Guid.NewGuid();
        var random = Random.Shared;
        var name =
            $"{alert.ProviderName}: {Adjectives[random.Next(Adjectives.Length)]} {Nouns[random.Next(Nouns.Length)]} #{random.Next(minValue: 100, maxValue: 999)}";

        RaiseEvent(new StateInitializedEvent<T>(aggregateId, DateTime.UtcNow, name, alert.ProviderName, alert,
            timeSeriesProvider.GetTimeSeries(),
            knownOutagesProvider.GetKnownOutages(),
            alertScoreCalculator,
            configurationFactory, AggregateVersion: 0), aggregateId);
    }

    /// <summary>
    ///     Creates a new instance of GeneticOptimizer to import existing training from <see cref="store" />
    /// </summary>
    /// <param name="store"></param>
    public GeneticOptimizerStateMachine(IEventStore store)
    {
        this.store = store;
        current = new GeneticOptimizerState<T>();
    }

    /// <inheritdoc />
    public Guid Id => current.Id;

    /// <inheritdoc />
    public string Name => current?.Name ?? "Uninitialized";

    /// <inheritdoc />
    public string ProviderName => current.ProviderName ?? "Uninitialized";

    /// <inheritdoc />
    public DateTime CreatedAt => current.CreatedAt;

    /// <inheritdoc />
    public async IAsyncEnumerable<IEvent> Hydrate(Guid aggregateId)
    {
        await foreach (var @event in store.GetAll(aggregateId, CancellationToken.None))
        {
            if (!current.Apply(@event))
            {
                //todo: unless it's the last generation completion event, this might mean that terrible things has happened.
            }

            yield return @event;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IEvent> Optimize(OptimizationConfiguration configuration, CancellationToken cancellationToken)
    {
        if (current.State == GeneticOptimizerStateEnum.Completed)
        {
            yield break;
        }

        var (canContinue, bag) = Reconfigure(configuration);

        if (!canContinue)
        {
            yield break;
        }

        foreach (var @event in bag.Events)
        {
            yield return @event;
        }

        do
        {
            (canContinue, bag) = current.State switch
            {
                GeneticOptimizerStateEnum.RandomRepopulation => RepopulateWithRandom(),
                GeneticOptimizerStateEnum.Evaluation => Evaluate(cancellationToken),
                GeneticOptimizerStateEnum.ScoreComputation => ComputeScore(),
                GeneticOptimizerStateEnum.CompletingGeneration => CompleteGeneration(),
                GeneticOptimizerStateEnum.SurvivorsCounting => CountSurvivors(),
                GeneticOptimizerStateEnum.Tournament => RunTournament(),
                GeneticOptimizerStateEnum.Completed => (false, null!),
                GeneticOptimizerStateEnum.Created => throw new InvalidOperationException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (canContinue)
            {
                foreach (var @event in bag.Events)
                {
                    yield return @event!;
                }
            }
        } while (canContinue && !cancellationToken.IsCancellationRequested);
    }

    private (bool, EventBag) RaiseEvent<TEvent>(TEvent @event, Guid? aggregateId = null) where TEvent : IEvent
    {
        store.Write(aggregateId ?? current.Id, @event);
        return (current.Apply(@event), EventBag.FromSingle(@event));
    }

    private (bool, EventBag) RepopulateWithRandom()
    {
        var randomConfiguration = current.ConfigurationFactory!.CreateRandom();
        return RaiseEvent(new RandomConfigurationAddedEvent<T>(randomConfiguration, current.Version));
    }

    private (bool, EventBag) CompleteGeneration()
    {
        foreach (var (configuration, weakOutagesList) in EvaluationCache)
        {
            if (!weakOutagesList.TryGetTarget(out _))
            {
                EvaluationCache.Remove(configuration, out _);
            }
        }

        return RaiseEvent(current.Configuration?.TotalGenerations - 1 <= current.GenerationIndex
            ? new TrainingCompletedEvent(current.Version)
            : new GenerationCompletedEvent(current.Version));
    }

    private static T Tournament(OptimizationConfiguration configuration,
        IReadOnlyList<AlertScoreCard> generationScore)
    {
        var winner = generationScore[Random.Shared.Next(minValue: 0, generationScore.Count)];

        for (var i = 0; i < configuration.TournamentsCount; i++)
        {
            var challenger = generationScore[Random.Shared.Next(minValue: 0, generationScore.Count)];
            if (challenger.Score > winner.Score)
            {
                winner = challenger;
            }
        }

        return (T)winner.Configuration;
    }

    private (bool, EventBag) RunTournament()
    {
        var isEnoughProperConfigurations = current.EligibleForTournament.Count >= current.Configuration!.SurvivorCount;

        var first = Tournament(current.Configuration, isEnoughProperConfigurations
            ? current.EligibleForTournament
            : current.GenerationScores);

        var second = Tournament(current.Configuration, isEnoughProperConfigurations
            ? current.EligibleForTournament
            : current.GenerationScores);

        if (Random.Shared.NextDouble() > current.Configuration.CrossoverProbability)
        {
            (first, second) = current.ConfigurationFactory!.Crossover(first, second);
        }

        first = current.ConfigurationFactory!.Mutate(first, current.Configuration.MutationProbability);
        second = current.ConfigurationFactory.Mutate(second, current.Configuration.MutationProbability);

        return RaiseEvent(new TournamentRoundCompletedEvent<T>(first, second, current.Version));
    }

    private (bool, EventBag) CountSurvivors()
    {
        var survivors = current.GenerationScores
            .Where(card => !card.IsNotFeasible)
            .OrderBy(card => card.Score)
            .Take(current.Configuration!.SurvivorCount)
            .Select(card => (T)card.Configuration)
            .ToList();

        return RaiseEvent(new SurvivorsCountedEvent<T>(survivors, current.Version));
    }

    private (bool, EventBag) ComputeScore()
    {
        var (alertConfiguration, outages) = current.NextScoreComputation;
        var alertScoreCard = current.AlertScoreCalculator!.CalculateScore(outages, current.KnownOutages!,
            alertConfiguration);
        return RaiseEvent(new AlertScoreComputedEvent(alertScoreCard, current.Version));
    }

    private (bool, EventBag) Evaluate(CancellationToken cancellationToken)
    {
        var events = current.WaitingEvaluation
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(configuration =>
            {
                var cachedOutages = EvaluationCache.GetOrAdd(configuration,
                    alertConfiguration =>
                        new WeakReference<IReadOnlyList<Outage>>(ValueFactory(alertConfiguration).ToList()));

                if (!cachedOutages.TryGetTarget(out var outages))
                {
                    outages = ValueFactory(configuration);
                }

                return new
                {
                    Outages = outages,
                    Configuration = configuration
                };
            })
            .AsSequential()
            .Select(tuple =>
                RaiseEvent(new EvaluationCompletedEvent<T>(tuple.Configuration, tuple.Outages, current.Version)))
            .Select(tuple => tuple.Item2);

        return (true, EventBag.Merge(events));

        List<Outage> ValueFactory(T cfg)
        {
            return current.Alert!.Evaluate(current.TimeSeries!, cfg).ToList();
        }
    }

    private (bool, EventBag) Reconfigure(OptimizationConfiguration optimizationConfiguration)
    {
        return RaiseEvent(new OptimizerConfiguredEvent(optimizationConfiguration, current.Version));
    }
}

internal class EventBag
{
    public EventBag(IEnumerable<IEvent> events)
    {
        Events = events;
    }

    public IEnumerable<IEvent> Events { get; }

    public static EventBag Merge(IEnumerable<EventBag> bags)
    {
        return new EventBag(bags.SelectMany(bag => bag.Events));
    }

    public static EventBag FromSingle(IEvent @event)
    {
        return new EventBag([@event]);
    }
}