using System.Runtime.CompilerServices;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// A state-machine that operates <see cref="GeneticOptimizerState{T}"/>
/// </summary>
/// <typeparam name="T">Underlying <see cref="AlertConfiguration"/></typeparam>
public class GeneticOptimizerStateMachine<T> : IGeneticOptimizer
    where T : AlertConfiguration
{
    private readonly IEventStore store;

    /// <summary>
    /// Creates a new instance of Genetic Optimizer.
    /// </summary>
    /// <param name="alert">Alert rule to be optimized.</param>
    /// <param name="timeSeriesProvider">Provider of the relevant metric.</param>
    /// <param name="knownOutagesProvider">Provider of known outages.</param>
    /// <param name="alertScoreCalculator">Calculates alert score based on detected outages.</param>
    /// <param name="configurationFactory">A relevant factory for <typeparamref name="T"/></param>
    /// <param name="store">An event store to persist all events.</param>
    public GeneticOptimizerStateMachine(IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, IEventStore store)
    {
        this.store = store;
        current = new GeneticOptimizerState<T>(alert, timeSeriesProvider, knownOutagesProvider,
            alertScoreCalculator, configurationFactory);
    }

    private async Task<(bool, IEvent)> RaiseEvent<TEvent>(TEvent @event) where TEvent : IEvent
    {
        await store.Write(current.Id, @event);
        return (current.Apply(@event), @event);
    }

    private readonly GeneticOptimizerState<T> current;

    private Task<(bool, IEvent)> RepopulateWithRandom()
    {
        var randomConfiguration = current.ConfigurationFactory.CreateRandom();
        return RaiseEvent(new RandomConfigurationAddedEvent<T>(randomConfiguration));
    }

    private Task<(bool, IEvent)> CreateSummary()
    {
        return RaiseEvent(new GenerationCompletedEvent(new GenerationSummary(current.GenerationIndex,
            current.GenerationScores.OrderBy(card => card.Score).ToList())));
    }

    private static T Tournament(OptimizationConfiguration configuration,
        IReadOnlyList<AlertScoreCard> generationScore)
    {
        var winner = generationScore[Random.Shared.Next(0, generationScore.Count)];

        for (var i = 0; i < configuration.TournamentsCount; i++)
        {
            var challenger = generationScore[Random.Shared.Next(0, generationScore.Count)];
            if (challenger.Score > winner.Score)
            {
                winner = challenger;
            }
        }

        return (T)winner.Configuration;
    }

    private Task<(bool, IEvent)> RunTournament()
    {
        var isEnoughProperConfigurations = current.EligibleForTournament.Count >= current.Configuration.SurvivorCount;

        var first = Tournament(current.Configuration, isEnoughProperConfigurations
            ? current.EligibleForTournament
            : current.GenerationScores);

        var second = Tournament(current.Configuration, isEnoughProperConfigurations
            ? current.EligibleForTournament
            : current.GenerationScores);

        if (Random.Shared.NextDouble() > current.Configuration.CrossoverProbability)
        {
            (first, second) = current.ConfigurationFactory.Crossover(first, second);
        }

        first = current.ConfigurationFactory.Mutate(first, current.Configuration.MutationProbability);
        second = current.ConfigurationFactory.Mutate(second, current.Configuration.MutationProbability);

        return RaiseEvent(new TournamentRoundCompletedEvent<T>(first, second));
    }

    private Task<(bool, IEvent)> CountSurvivors()
    {
        var survivors = current.GenerationScores
            .Where(card => !card.IsNotFeasible)
            .OrderBy(card => card.Score)
            .Take(current.Configuration.SurvivorCount)
            .Select(card => (T)card.Configuration)
            .ToList();

        return RaiseEvent(new SurvivorsCountedEvent<T>(survivors));
    }

    private Task<(bool, IEvent)> ComputeScore()
    {
        var (alertConfiguration, outages) = current.NextScoreComputation;
        var alertScoreCard = current.AlertScoreCalculator.CalculateScore(outages, current.KnownOutagesProvider,
            alertConfiguration);
        return RaiseEvent(new AlertScoreComputedEvent(alertScoreCard));
    }

    private Task<(bool, IEvent)> Evaluate()
    {
        var configuration = current.NextEvaluation;
        var outages = current.Alert.Evaluate(current.TimeSeriesProvider, configuration).ToList();

        return RaiseEvent(new EvaluationCompletedEvent<T>(configuration, outages));
    }

    /// <inheritdoc />
    public Guid Id => current.Id;

    /// <inheritdoc />
    public async IAsyncEnumerable<IEvent> Optimize(OptimizationConfiguration configuration, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (current.State == GeneticOptimizerStateEnum.Completed)
        {
            yield break;
        }

        (var canContinue, IEvent? @event) = await Reconfigure(configuration);

        if (!canContinue)
        {
            yield break;
        }

        yield return @event;
        
        do
        {
            (canContinue, @event) = current.State switch
            {
                GeneticOptimizerStateEnum.RandomRepopulation => await RepopulateWithRandom(),
                GeneticOptimizerStateEnum.Evaluation => await Evaluate(),
                GeneticOptimizerStateEnum.ScoreComputation => await ComputeScore(),
                GeneticOptimizerStateEnum.CompletingGeneration => await CreateSummary(),
                GeneticOptimizerStateEnum.SurvivorsCounting => await CountSurvivors(),
                GeneticOptimizerStateEnum.Tournament => await RunTournament(),
                GeneticOptimizerStateEnum.Completed => (false, null),
                GeneticOptimizerStateEnum.Created => throw new InvalidOperationException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (canContinue)
            {
                yield return @event!;
            }

        } while (canContinue && !cancellationToken.IsCancellationRequested);
    }
    
    private Task<(bool canContinue, IEvent @event)> Reconfigure(OptimizationConfiguration optimizationConfiguration)
    {
        return RaiseEvent(new OptimizerConfiguredEvent(optimizationConfiguration));
    }
}