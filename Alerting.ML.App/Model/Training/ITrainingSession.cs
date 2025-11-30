using System;
using System.Collections.ObjectModel;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.App.Model.Training;

public interface ITrainingSession
{
    Guid Id { get; }
    string Name { get; }
    ObservableCollection<double> PopulationDiversity { get; }
    ObservableCollection<double> AverageGenerationFitness { get; }
    ObservableCollection<double> BestGenerationFitness { get; }
    ObservableCollection<AlertScoreCard> TopConfigurations { get; }
    int CurrentGeneration { get; }
    double BestFitness { get; }
    int TotalEvaluations { get; }
    TimeSpan Elapsed { get; }
    OptimizationConfiguration CurrentConfiguration { get; }
    void Start(OptimizationConfiguration configuration);
    void Stop();
    bool IsPaused { get; }
}