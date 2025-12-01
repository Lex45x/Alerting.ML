using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

namespace Alerting.ML.App.Model.Training;

public class TrainingConfigurationBuilder : ViewModelBase
{
    public int PopulationSize
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double MutationRate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double CrossoverRate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int TotalGenerations
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public static TrainingConfigurationBuilder FromExisting(OptimizationConfiguration configuration)
    {
        return new TrainingConfigurationBuilder
        {
            PopulationSize = configuration.PopulationSize,
            TotalGenerations = configuration.TotalGenerations,
            CrossoverRate = configuration.CrossoverProbability,
            MutationRate = configuration.MutationProbability
        };
    }

    public OptimizationConfiguration Apply(OptimizationConfiguration existing)
    {
        return existing with
        {
            CrossoverProbability = CrossoverRate,
            MutationProbability = MutationRate,
            PopulationSize = PopulationSize,
            TotalGenerations = TotalGenerations
        };
    }
}