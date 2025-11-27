using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using Microsoft.Extensions.Logging;

namespace Alerting.ML.Engine.Optimizer;

public class GeneticOptimizerStateMachine<T> : IAsyncEnumerator<IEvent>, IGeneticOptimizer
    where T : AlertConfiguration<T>
{
    private readonly ILogger<GeneticOptimizerStateMachine<T>> logger;
    private readonly IEventStore store;

    public GeneticOptimizerStateMachine(IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, ILogger<GeneticOptimizerStateMachine<T>> logger, IEventStore store,
        OptimizationConfiguration configuration)
    {
        this.logger = logger;
        this.store = store;
        current = new GeneticOptimizerState<T>(alert, timeSeriesProvider, knownOutagesProvider,
            alertScoreCalculator, configurationFactory, configuration);
    }

    private async Task<bool> RaiseEvent<TEvent>(TEvent @event) where TEvent : IEvent
    {
        await store.Write(current.Id, @event);
        return current.Apply(@event);
    }

    private readonly GeneticOptimizerState<T> current;

    private Task<bool> RepopulateWithRandom()
    {
        var randomConfiguration = current.ConfigurationFactory.CreateRandom();
        return RaiseEvent(new RandomConfigurationAddedEvent<T>(randomConfiguration));
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        return current.Step switch
        {
            GeneticOptimizerStep.RandomRepopulation => await RepopulateWithRandom(),
            GeneticOptimizerStep.Evaluation => await Evaluate(),
            GeneticOptimizerStep.ScoreComputation => await ComputeScore(),
            GeneticOptimizerStep.CreateSummary => await CreateSummary(),
            GeneticOptimizerStep.SurvivorsCounting => await CountSurvivors(),
            GeneticOptimizerStep.Tournament => await RunTournament(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Task<bool> CreateSummary()
    {
        return RaiseEvent(new SummaryCreatedEvent(new GenerationSummary(current.GenerationIndex, current.GenerationScores.OrderBy(card => card.Score).ToList())));
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

    private Task<bool> RunTournament()
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

        first = current.ConfigurationFactory.Mutate(first);
        second = current.ConfigurationFactory.Mutate(second);

        return RaiseEvent(new TournamentRoundCompletedEvent<T>(first, second));
    }

    private Task<bool> CountSurvivors()
    {
        var survivors = current.GenerationScores
            .Where(card => !card.IsNotFeasible)
            .OrderBy(card => card.Score)
            .Take(current.Configuration.SurvivorCount)
            .Select(card => (T)card.Configuration)
            .ToList();

        return RaiseEvent(new SurvivorsCountedEvent<T>(survivors));
    }

    private Task<bool> ComputeScore()
    {
        var (alertConfiguration, outages) = current.NextScoreComputation;
        var alertScoreCard = current.AlertScoreCalculator.CalculateScore(outages, current.KnownOutagesProvider,
            alertConfiguration, current.Configuration.AlertScoreConfiguration);
        return RaiseEvent(new AlertScoreComputedEvent<T>(alertConfiguration, alertScoreCard));
    }

    private Task<bool> Evaluate()
    {
        var configuration = current.NextEvaluation;
        var outages = current.Alert.Evaluate(current.TimeSeriesProvider, configuration).ToList();

        return RaiseEvent(new EvaluationCompletedEvent<T>(configuration, outages));
    }

    IEvent IAsyncEnumerator<IEvent>.Current => current.LastEvent;

    public async ValueTask DisposeAsync()
    {
        //todo: do I need this? I might :D
    }

    public IAsyncEnumerator<IEvent> GetAsyncEnumerator() => this;

    public async IAsyncEnumerable<GenerationSummary> Optimize(CancellationToken cancellationToken)
    {
        await foreach (var @event in this)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
            if (@event is SummaryCreatedEvent resultsReviewed)
            {
                yield return resultsReviewed.Summary;
            }
        }
    }
}