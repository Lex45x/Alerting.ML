using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation;

public interface ITrainingCreationStepViewModel : IRoutableViewModel
{
    TrainingCreationStep CurrentStep { get; }
    void Continue();
}

public interface ITrainingCreationLastStepViewModel : IRoutableViewModel
{
    IGeneticOptimizer ConfiguredOptimizer { get; }
}