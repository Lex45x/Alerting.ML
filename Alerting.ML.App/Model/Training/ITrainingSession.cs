using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.App.Model.Training;

public interface ITrainingSession
{
    Guid Id { get; }
    string? Name { get; }
    ObservableCollection<double> PopulationDiversity { get; }
    ObservableCollection<double> AverageGenerationFitness { get; }
    ObservableCollection<double> BestGenerationFitness { get; }
    ObservableCollection<AlertScoreCard> TopConfigurations { get; }
    int CurrentGeneration { get; }
    double BestFitness { get; }
    double FitnessDiff { get; }
    int TotalEvaluations { get; }
    double ProgressPercentage { get; }
    CloudProvider AlertProvider { get; }
    DateTime CreatedAt { get; }
    TimeSpan Elapsed { get; }
    double RemainingMinutes { get; }
    OptimizationConfiguration? CurrentConfiguration { get; }
    TrainingState State { get; }
    void Start(OptimizationConfiguration configuration);
    void Stop();
    Task Hydrate(Guid aggregateId);
    IBackgroundTrainingOrchestrator OwningOrchestrator { get; }
    ITrainingSession CloneAndStart();
}

public enum TrainingState
{
    Training = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Loading = 5
}