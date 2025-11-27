namespace Alerting.ML.Engine.Optimizer;

public enum GeneticOptimizerStateEnum
{
    RandomRepopulation = 0,
    Evaluation = 1,
    ScoreComputation = 2,
    CreateSummary = 3,
    SurvivorsCounting = 4,
    Tournament = 5
}