using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.Engine.Optimizer;

public record OptimizationConfiguration(int PopulationSize,
    double SurvivorPercentage,
    double CrossoverProbability,
    double MutationProbability,
    int StallLimit, int TournamentsCount)
{
    public int SurvivorCount => (int)(PopulationSize * SurvivorPercentage);

    public static OptimizationConfiguration Default { get; } = new(100, 0.1, 0.5, 0.1, 100, 3);
}