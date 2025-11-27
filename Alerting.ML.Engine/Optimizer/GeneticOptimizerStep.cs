namespace Alerting.ML.Engine.Optimizer;

public enum GeneticOptimizerStep
{
    RandomRepopulation,
    Evaluation,
    ScoreComputation,
    CreateSummary,
    SurvivorsCounting,
    Tournament
}