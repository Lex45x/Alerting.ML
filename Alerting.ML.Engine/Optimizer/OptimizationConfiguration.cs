using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.Engine.Optimizer;

public record OptimizationConfiguration(int PopulationSize,
    double SurvivorPercentage,
    double CrossoverProbability,
    int StallLimit, AlertScoreConfiguration AlertScoreConfiguration, int TournamentsCount)
{
    public int SurvivorCount => (int)(PopulationSize * SurvivorPercentage);
}