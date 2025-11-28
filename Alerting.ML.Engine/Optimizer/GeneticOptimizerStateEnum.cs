namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Represents all possible states of <see cref="GeneticOptimizerState{T}"/>
/// </summary>
public enum GeneticOptimizerStateEnum
{
    /// <summary>
    /// Adds new random configurations into next generation until <see cref="OptimizationConfiguration.PopulationSize"/> is reached.
    /// </summary>
    RandomRepopulation = 0,

    /// <summary>
    /// Evaluates alert against the configuration population.
    /// </summary>
    Evaluation = 1,

    /// <summary>
    /// Computes score for each evaluated alert
    /// </summary>
    ScoreComputation = 2,

    /// <summary>
    /// No-op state to indicate completion of generation.
    /// </summary>
    CreateSummary = 3,

    /// <summary>
    /// Counts configurations that survived current generation.
    /// </summary>
    SurvivorsCounting = 4,

    /// <summary>
    /// Applies mutation and crossover to fill next generation.
    /// </summary>
    Tournament = 5
}