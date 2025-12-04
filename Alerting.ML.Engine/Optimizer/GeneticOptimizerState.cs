using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
///     Json-serialization-friendly representation of the <see cref="GeneticOptimizerStateMachine{T}" /> internal state.
///     Can be mutated only by sending supported <see cref="IEvent" /> into <see cref="Apply" />
/// </summary>
/// <typeparam name="T">Target <see cref="AlertConfiguration" /> of underlying <see cref="IAlert" /></typeparam>
public class GeneticOptimizerState<T> where T : AlertConfiguration
{
    private readonly Queue<T> evaluationQueue = new();

    private readonly List<AlertScoreCard> generationScores = new();
    private readonly Queue<(T, IEnumerable<Outage>)> scoreComputationQueue = new();

    /// <summary>
    ///     Creates new instance and initializes the <see cref="State" />
    /// </summary>
    public GeneticOptimizerState()
    {
    }

    /// <summary>
    ///     An Id of the optimization run. Used for storage purposes.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Indicates current version of the state. Practically means a count of applied events.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    ///     User-friendly generated name of the session.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    ///     Name of the alert provider used in this session.
    /// </summary>
    public string? ProviderName { get; private set; }

    /// <summary>
    ///     Alert rule being optimized.
    /// </summary>
    public IAlert<T>? Alert { get; private set; }

    /// <summary>
    ///     Provider of relevant metric.
    /// </summary>
    public ITimeSeriesProvider? TimeSeriesProvider { get; private set; }

    /// <summary>
    ///     Provider of known outages.
    /// </summary>
    public IKnownOutagesProvider? KnownOutagesProvider { get; private set; }

    /// <summary>
    ///     Calculates alert score based on detected outages.
    /// </summary>
    public IAlertScoreCalculator? AlertScoreCalculator { get; private set; }

    /// <summary>
    ///     A relevant factory for <typeparamref name="T" />
    /// </summary>
    public IConfigurationFactory<T>? ConfigurationFactory { get; private set; }

    /// <summary>
    ///     Configuration of the optimization process.
    /// </summary>
    public OptimizationConfiguration? Configuration { get; private set; }

    /// <summary>
    ///     Indicates a current state for <see cref="GeneticOptimizerStateMachine{T}" />
    /// </summary>
    public GeneticOptimizerStateEnum? State { get; private set; }

    /// <summary>
    ///     Has the best score from all generations.
    /// </summary>
    public AlertScoreCard? BestScore { get; private set; }

    /// <summary>
    ///     When state is <see cref="GeneticOptimizerStateEnum.Evaluation" /> contains the next
    ///     <see cref="AlertConfiguration" /> to be evaluated.
    /// </summary>
    public T NextEvaluation => evaluationQueue.Peek();

    /// <summary>
    ///     When state is <see cref="GeneticOptimizerStateEnum.ScoreComputation" /> contains <see cref="Configuration" /> and a
    ///     list of <see cref="Outage" /> to compute score for.
    /// </summary>
    public (T, IEnumerable<Outage>) NextScoreComputation => scoreComputationQueue.Peek();

    /// <summary>
    ///     After <see cref="GeneticOptimizerStateEnum.ScoreComputation" /> will contain a list of computed scores for a given
    ///     generations. Resets when <see cref="GeneticOptimizerStateEnum.Tournament" /> is completed.
    /// </summary>
    public IReadOnlyList<AlertScoreCard> GenerationScores => generationScores;

    /// <summary>
    ///     Contains a list of !<see cref="AlertScoreCard.IsNotFeasible" /> scorecards that will participate in tournament.
    /// </summary>
    public IReadOnlyList<AlertScoreCard> EligibleForTournament { get; private set; } = [];

    /// <summary>
    ///     Indicates a current generation.
    /// </summary>
    public int GenerationIndex { get; private set; }

    /// <summary>
    ///     Datetime when optimization was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }


    /// <summary>
    ///     Applies an <see cref="IEvent" /> to state.
    /// </summary>
    /// <param name="event">Instance of event.</param>
    /// <typeparam name="TEvent">A type of event.</typeparam>
    /// <returns>Whether event was applied or not.
    ///     <value>false</value>
    ///     when critical error has occured and optimization can't continue.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">When unknown event is supplied. Indicates engineer's error.</exception>
    public bool Apply<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (Version != @event.AggregateVersion)
        {
            throw new InvalidOperationException(
                $"Out-of-order event. Current version is {Version} with event has version {@event.AggregateVersion}");
        }

        Version += 1;

        return @event switch
        {
            StateInitializedEvent<T> configurationAdded => Handle(configurationAdded),
            RandomConfigurationAddedEvent<T> configurationAdded => Handle(configurationAdded),
            EvaluationCompletedEvent<T> evaluationCompleted => Handle(evaluationCompleted),
            AlertScoreComputedEvent scoreComputed => Handle(scoreComputed),
            GenerationCompletedEvent generationCompleted => Handle(generationCompleted),
            SurvivorsCountedEvent<T> survivorsCounted => Handle(survivorsCounted),
            OptimizerConfiguredEvent optimizerConfigured => Handle(optimizerConfigured),
            TournamentRoundCompletedEvent<T> tournamentRoundCompleted => Handle(
                tournamentRoundCompleted),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, message: null)
        };
    }

    private bool Handle(StateInitializedEvent<T> stateInitialized)
    {
        Id = stateInitialized.Id;
        CreatedAt = stateInitialized.CreatedAt;
        Name = stateInitialized.Name;
        ProviderName = stateInitialized.ProviderName;
        Alert = stateInitialized.Alert;
        TimeSeriesProvider = stateInitialized.TimeSeriesProvider;
        KnownOutagesProvider = stateInitialized.KnownOutagesProvider;
        AlertScoreCalculator = stateInitialized.AlertScoreCalculator;
        ConfigurationFactory = stateInitialized.ConfigurationFactory;
        State = GeneticOptimizerStateEnum.Created;
        return true;
    }

    private bool Handle(OptimizerConfiguredEvent optimizerConfigured)
    {
        Configuration = optimizerConfigured.Configuration;

        //todo: What could go wrong in the middle of simulation?
        //todo: A lot! Reconfiguring should keep existing population and add or remove more citizens and restart the current generation.
        //todo: For now, we will pretend that nothing happened :D So, reduction of the population will be impossible for now.

        if (State == GeneticOptimizerStateEnum.Created)
        {
            State = GeneticOptimizerStateEnum.RandomRepopulation;
        }

        return true;
    }

    private bool Handle(GenerationCompletedEvent _)
    {
        if (Configuration?.TotalGenerations - 1 <= GenerationIndex)
        {
            State = GeneticOptimizerStateEnum.Completed;
            return false;
        }

        GenerationIndex += 1;
        State = GeneticOptimizerStateEnum.SurvivorsCounting;
        return true;
    }

    private bool Handle(TournamentRoundCompletedEvent<T> tournamentRoundCompleted)
    {
        evaluationQueue.Enqueue(tournamentRoundCompleted.FirstWinner);

        if (evaluationQueue.Count < Configuration?.PopulationSize)
        {
            evaluationQueue.Enqueue(tournamentRoundCompleted.SecondWinner);
        }

        if (evaluationQueue.Count >= Configuration?.PopulationSize)
        {
            generationScores.Clear();
            State = GeneticOptimizerStateEnum.Evaluation;
        }

        if (EligibleForTournament.Count < Configuration?.SurvivorCount &&
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

    private bool Handle(AlertScoreComputedEvent scoreComputedEvent)
    {
        if (!Equals(scoreComputationQueue.Dequeue().Item1, scoreComputedEvent.AlertScoreCard.Configuration))
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
            State = GeneticOptimizerStateEnum.CompletingGeneration;
        }

        return true;
    }

    private bool Handle(EvaluationCompletedEvent<T> evaluationCompleted)
    {
        if (!evaluationQueue.Dequeue().Equals(evaluationCompleted.Configuration))
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
        if (evaluationQueue.Count >= Configuration?.PopulationSize)
        {
            State = GeneticOptimizerStateEnum.Evaluation;
        }

        return true;
    }
}