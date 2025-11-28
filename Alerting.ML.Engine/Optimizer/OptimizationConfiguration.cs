using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Defines the way <see cref="GeneticOptimizerStateMachine{T}"/> will run the optimization process.
/// </summary>
/// <param name="PopulationSize">Count of <see cref="AlertConfiguration"/> in each generation.</param>
/// <param name="SurvivorPercentage">Percentage of best-performers that will survive to the next generation.</param>
/// <param name="CrossoverProbability">Probability of tournament winners to exchange their configuration properties.</param>
/// <param name="MutationProbability">Probability of each configuration property to mutate.</param>
/// <param name="TotalGenerations">Total duration of optimization run in generations.</param>
/// <param name="TournamentsCount">Count of score comparisons in each tournament round.</param>
public record OptimizationConfiguration(int PopulationSize,
    double SurvivorPercentage,
    double CrossoverProbability,
    double MutationProbability,
    int TotalGenerations, int TournamentsCount)
{
    /// <summary>
    /// Calculates expected count of surviving configurations based on <see cref="PopulationSize"/> and <see cref="SurvivorPercentage"/>
    /// </summary>
    public int SurvivorCount => (int)(PopulationSize * SurvivorPercentage);

    /// <summary>
    /// Provides a reasonably good default configuration for the optimization process.
    /// </summary>
    public static OptimizationConfiguration Default { get; } = new(100, 0.1, 0.5, 0.1, 100, 3);
}