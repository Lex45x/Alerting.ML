using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Json-serialization-friendly representation of the <see cref="GeneticOptimizerStateMachine{T}"/> internal state.
/// Can be mutated only by sending supported <see cref="IEvent"/> into <see cref="Apply"/>
/// </summary>
/// <typeparam name="T">Target <see cref="AlertConfiguration"/> of underlying <see cref="IAlert"/></typeparam>
public class GeneticOptimizerState<T> where T : AlertConfiguration
{
    /// <summary>
    /// Creates new instance and initializes the <see cref="State"/>
    /// </summary>
    /// <param name="alert">Alert rule to be optimized.</param>
    /// <param name="timeSeriesProvider">Provider of the relevant metric.</param>
    /// <param name="knownOutagesProvider">Provider of known outages.</param>
    /// <param name="alertScoreCalculator">Calculates alert score based on detected outages.</param>
    /// <param name="configurationFactory">A relevant factory for <typeparamref name="T"/></param>
    /// <param name="configuration">Configuration of the optimization process.</param>
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
        State = GeneticOptimizerStateEnum.RandomRepopulation;
    }

    /// <summary>
    /// An Id of the optimization run. Used for storage purposes.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Alert rule being optimized.
    /// </summary>
    public IAlert<T> Alert { get; }

    /// <summary>
    /// Provider of relevant metric.
    /// </summary>
    public ITimeSeriesProvider TimeSeriesProvider { get; }

    /// <summary>
    /// Provider of known outages.
    /// </summary>
    public IKnownOutagesProvider KnownOutagesProvider { get; }

    /// <summary>
    /// Calculates alert score based on detected outages.
    /// </summary>
    public IAlertScoreCalculator AlertScoreCalculator { get; }

    /// <summary>
    /// A relevant factory for <typeparamref name="T"/>
    /// </summary>
    public IConfigurationFactory<T> ConfigurationFactory { get; }

    /// <summary>
    /// Configuration of the optimization process.
    /// </summary>
    public OptimizationConfiguration Configuration { get; }

    /// <summary>
    /// Indicates a current state for <see cref="GeneticOptimizerStateMachine{T}"/>
    /// </summary>
    public GeneticOptimizerStateEnum State { get; private set; }

    /// <summary>
    /// Has the best score from all generations.
    /// </summary>
    public AlertScoreCard? BestScore { get; private set; }

    private readonly Queue<T> evaluationQueue = new();
    private readonly Queue<(T, IEnumerable<Outage>)> scoreComputationQueue = new();

    /// <summary>
    /// When state is <see cref="GeneticOptimizerStateEnum.Evaluation"/> contains the next <see cref="AlertConfiguration"/> to be evaluated.
    /// </summary>
    public T NextEvaluation => evaluationQueue.Peek();

    /// <summary>
    /// When state is <see cref="GeneticOptimizerStateEnum.ScoreComputation"/> contains <see cref="Configuration"/> and a list of <see cref="Outage"/> to compute score for.
    /// </summary>
    public (T, IEnumerable<Outage>) NextScoreComputation => scoreComputationQueue.Peek();

    private readonly List<AlertScoreCard> generationScores = new();

    /// <summary>
    /// After <see cref="GeneticOptimizerStateEnum.ScoreComputation"/> will contain a list of computed scores for a given generations. Resets when <see cref="GeneticOptimizerStateEnum.Tournament"/> is completed.
    /// </summary>
    public IReadOnlyList<AlertScoreCard> GenerationScores => generationScores;

    /// <summary>
    /// Contains a list of !<see cref="AlertScoreCard.IsNotFeasible"/> scorecards that will participate in tournament.
    /// </summary>
    public IReadOnlyList<AlertScoreCard> EligibleForTournament { get; private set; } = [];

    /// <summary>
    /// Indicates a current generation.
    /// </summary>
    public int GenerationIndex { get; private set; }

    /// <summary>
    /// Has a last <see cref="IEvent"/> that has been applied.
    /// </summary>
    public IEvent? LastEvent { get; private set; }

    /// <summary>
    /// Applies an <see cref="IEvent"/> to state.
    /// </summary>
    /// <param name="event">Instance of event.</param>
    /// <typeparam name="TEvent">A type of event.</typeparam>
    /// <returns>Whether event was applied or not. <value>false</value> when critical error has occured and optimization can't continue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When unknown event is supplied. Indicates engineer's error.</exception>
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