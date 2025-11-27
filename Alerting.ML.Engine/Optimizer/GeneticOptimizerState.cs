using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

public class GeneticOptimizerState<T> where T : AlertConfiguration
{
    public GeneticOptimizerState(IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, OptimizationConfiguration configuration, params IEnumerable<IEvent> events)
    {
        Alert = alert;
        TimeSeriesProvider = timeSeriesProvider;
        KnownOutagesProvider = knownOutagesProvider;
        AlertScoreCalculator = alertScoreCalculator;
        ConfigurationFactory = configurationFactory;
        Configuration = configuration;
        State = GeneticOptimizerStateEnum.RandomRepopulation;
        foreach (var @event in events)
        {
            Apply(@event);
        }
    }

    public Guid Id { get; }
    public IAlert<T> Alert { get; }
    public ITimeSeriesProvider TimeSeriesProvider { get; }
    public IKnownOutagesProvider KnownOutagesProvider { get; }
    public IAlertScoreCalculator AlertScoreCalculator { get; }
    public IConfigurationFactory<T> ConfigurationFactory { get; }
    public OptimizationConfiguration Configuration { get; }
    public GeneticOptimizerStateEnum State { get; private set; }
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
    public IEvent? LastEvent { get; private set; }

    public bool Apply<TEvent>(TEvent @event) where TEvent : IEvent
    {
        LastEvent = @event;
        return @event switch
        {
            RandomConfigurationAddedEvent<T> configurationAdded => Handle(configurationAdded),
            EvaluationCompletedEvent<T> evaluationCompleted => Handle(evaluationCompleted),
            AlertScoreComputedEvent<T> scoreComputed => Handle(scoreComputed),
            SummaryCreatedEvent summaryCreated => Handle(summaryCreated),
            SurvivorsCountedEvent<T> survivorsCounted => Handle(survivorsCounted),
            TournamentRoundCompletedEvent<T> tournamentRoundCompleted => Handle(
                tournamentRoundCompleted),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
    }

    private bool Handle(SummaryCreatedEvent summaryCreated)
    {
        GenerationIndex += 1;
        State = GeneticOptimizerStateEnum.SurvivorsCounting;
        return true;
    }

    private bool Handle(TournamentRoundCompletedEvent<T> tournamentRoundCompleted)
    {
        evaluationQueue.Enqueue(tournamentRoundCompleted.FirstWinner);

        if (evaluationQueue.Count < Configuration.PopulationSize)
        {
            evaluationQueue.Enqueue(tournamentRoundCompleted.SecondWinner);
        }

        if (evaluationQueue.Count >= Configuration.PopulationSize)
        {
            generationScores.Clear();
            State = GeneticOptimizerStateEnum.Evaluation;
        }

        if (EligibleForTournament.Count < Configuration.SurvivorCount &&
            evaluationQueue.Count >= Configuration.PopulationSize / 2)
        {
            generationScores.Clear();
            State = GeneticOptimizerStateEnum.RandomRepopulation;
        }

        return true;
    }

    private bool Handle(SurvivorsCountedEvent<T> survivorsCounted)
    {
        foreach (var survivor in survivorsCounted.Survivors)
        {
            evaluationQueue.Enqueue(survivor);
        }

        EligibleForTournament = GenerationScores.Where(scoreCard => !scoreCard.IsNotFeasible).ToList();

        State = EligibleForTournament.Count < 2
            ? GeneticOptimizerStateEnum.RandomRepopulation
            : GeneticOptimizerStateEnum.Tournament;

        return true;
    }

    private bool Handle(AlertScoreComputedEvent<T> scoreComputedEvent)
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
            State = GeneticOptimizerStateEnum.CreateSummary;
        }

        return true;
    }

    private bool Handle(EvaluationCompletedEvent<T> evaluationCompleted)
    {
        if (evaluationQueue.Dequeue() != evaluationCompleted.Configuration)
        {
            throw new InvalidOperationException(
                "Evaluated configuration is not coming from the top of the evaluation queue!");
        }

        scoreComputationQueue.Enqueue((evaluationCompleted.Configuration, evaluationCompleted.Outages));

        if (evaluationQueue.Count == 0)
        {
            State = GeneticOptimizerStateEnum.ScoreComputation;
        }

        return true;
    }

    private bool Handle(RandomConfigurationAddedEvent<T> configurationAdded)
    {
        evaluationQueue.Enqueue(configurationAdded.RandomConfiguration);
        if (evaluationQueue.Count >= Configuration.PopulationSize)
        {
            State = GeneticOptimizerStateEnum.Evaluation;
        }

        return true;
    }
}