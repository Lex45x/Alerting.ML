using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

public class GeneticOptimizerState<T> where T : AlertConfiguration<T>
{
    public GeneticOptimizerState(IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, OptimizationConfiguration configuration)
    {
        Alert = alert;
        TimeSeriesProvider = timeSeriesProvider;
        KnownOutagesProvider = knownOutagesProvider;
        AlertScoreCalculator = alertScoreCalculator;
        ConfigurationFactory = configurationFactory;
        Configuration = configuration;
        Step = GeneticOptimizerStep.RandomRepopulation;
    }

    public Guid Id { get; }
    public IAlert<T> Alert { get; }
    public ITimeSeriesProvider TimeSeriesProvider { get; }
    public IKnownOutagesProvider KnownOutagesProvider { get; }
    public IAlertScoreCalculator AlertScoreCalculator { get; }
    public IConfigurationFactory<T> ConfigurationFactory { get; }
    public OptimizationConfiguration Configuration { get; }
    public GeneticOptimizerStep Step { get; private set; }
    public AlertScoreCard? BestScore { get; private set; }

    private readonly Queue<T> evaluationQueue = new();
    private readonly Queue<(T, IEnumerable<Outage>)> scoreComputationQueue = new();

    public T NextEvaluation => evaluationQueue?.Peek() ?? throw new InvalidOperationException(
        "NextEvaluation is null! This should not happen and indicate developer error in the state-machine logic.");

    public (T, IEnumerable<Outage>) NextScoreComputation => scoreComputationQueue?.Peek() ??
                                                            throw new InvalidOperationException(
                                                                "NextScoreComputation is null! This should not happen and indicate developer error in the state-machine logic.");

    private readonly List<AlertScoreCard> generationScores = new();

    public IReadOnlyList<AlertScoreCard> GenerationScores => generationScores;

    public IReadOnlyList<AlertScoreCard> EligibleForTournament { get; private set; } = [];
    public int GenerationIndex { get; private set; }
    public IEvent LastEvent { get; private set; }

    public bool Apply<TEvent>(TEvent @event) where TEvent : IEvent
    {
        LastEvent = @event;
        return @event switch
        {
            RandomConfigurationAddedEvent<T> configurationAdded => ConfigurationAdded(configurationAdded),
            EvaluationCompletedEvent<T> evaluationCompleted => EvaluationCompleted(evaluationCompleted),
            AlertScoreComputedEvent<T> scoreComputed => ScoreComputed(scoreComputed),
            SummaryCreatedEvent _ => SummaryCreated(),
            SurvivorsCountedEvent<T> survivorsCounted => SurvivorsCounted(survivorsCounted),
            TournamentRoundCompletedEvent<T> tournamentRoundCompleted => TournamentRoundCompleted(
                tournamentRoundCompleted),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
    }

    private bool SummaryCreated()
    {
        GenerationIndex += 1;
        Step = GeneticOptimizerStep.SurvivorsCounting;
        return true;
    }

    private bool TournamentRoundCompleted(TournamentRoundCompletedEvent<T> tournamentRoundCompleted)
    {
        evaluationQueue.Enqueue(tournamentRoundCompleted.FirstWinner);

        if (evaluationQueue.Count < Configuration.PopulationSize)
        {
            evaluationQueue.Enqueue(tournamentRoundCompleted.SecondWinner);
        }

        if (evaluationQueue.Count >= Configuration.PopulationSize)
        {
            generationScores.Clear();
            Step = GeneticOptimizerStep.Evaluation;
        }

        if (EligibleForTournament.Count < Configuration.SurvivorCount &&
            evaluationQueue.Count >= Configuration.PopulationSize / 2)
        {
            generationScores.Clear();
            Step = GeneticOptimizerStep.RandomRepopulation;
        }

        return true;
    }

    private bool SurvivorsCounted(SurvivorsCountedEvent<T> survivorsCounted)
    {
        foreach (var survivor in survivorsCounted.Survivors)
        {
            evaluationQueue.Enqueue(survivor);
        }

        EligibleForTournament = GenerationScores.Where(scoreCard => !scoreCard.IsNotFeasible).ToList();

        Step = EligibleForTournament.Count < 2
            ? GeneticOptimizerStep.RandomRepopulation
            : GeneticOptimizerStep.Tournament;

        return true;
    }

    private bool ScoreComputed(AlertScoreComputedEvent<T> scoreComputedEvent)
    {
        if (scoreComputationQueue.Dequeue().Item1 != scoreComputedEvent.Configuration)
        {
            throw new InvalidOperationException(
                "Computed Score configuration is not coming from the top of the score computation queue!");
        }

        if (BestScore == null || BestScore.Score > scoreComputedEvent.AlertScoreCard.Score)
        {
            BestScore = scoreComputedEvent.AlertScoreCard;
        }

        generationScores.Add(scoreComputedEvent.AlertScoreCard);

        if (scoreComputationQueue.Count == 0)
        {
            Step = GeneticOptimizerStep.CreateSummary;
        }

        return true;
    }

    private bool EvaluationCompleted(EvaluationCompletedEvent<T> evaluationCompleted)
    {
        if (evaluationQueue.Dequeue() != evaluationCompleted.Configuration)
        {
            throw new InvalidOperationException(
                "Evaluated configuration is not coming from the top of the evaluation queue!");
        }

        scoreComputationQueue.Enqueue((evaluationCompleted.Configuration, evaluationCompleted.Outages));

        if (evaluationQueue.Count == 0)
        {
            Step = GeneticOptimizerStep.ScoreComputation;
        }

        return true;
    }

    private bool ConfigurationAdded(RandomConfigurationAddedEvent<T> configurationAdded)
    {
        evaluationQueue.Enqueue(configurationAdded.RandomConfiguration);
        if (evaluationQueue.Count >= Configuration.PopulationSize)
        {
            Step = GeneticOptimizerStep.Evaluation;
        }

        return true;
    }
}