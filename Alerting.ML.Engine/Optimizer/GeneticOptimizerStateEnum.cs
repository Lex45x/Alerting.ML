namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Represents all possible states of <see cref="GeneticOptimizerState{T}"/>
/// </summary>
public enum GeneticOptimizerStateEnum
{
    /// <summary>
    /// Indicates a freshly created and not configured optimization run.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Adds new random configurations into next generation until <see cref="OptimizationConfiguration.PopulationSize"/> is reached.
    /// </summary>
    RandomRepopulation = 1,

    /// <summary>
    /// Evaluates alert against the configuration population.
    /// </summary>
    Evaluation = 2,

    /// <summary>
    /// Computes score for each evaluated alert
    /// </summary>
    ScoreComputation = 3,

    /// <summary>
    /// No-op state to indicate completion of generation.
    /// </summary>
    CompletingGeneration = 4,

    /// <summary>
    /// Counts configurations that survived current generation.
    /// </summary>
    SurvivorsCounting = 5,

    /// <summary>
    /// Applies mutation and crossover to fill next generation.
    /// </summary>
    Tournament = 6,

    /// <summary>
    /// Indicates that training process has reached desired amount of generations.
    /// </summary>
    Completed = 7,
}