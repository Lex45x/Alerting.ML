using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.App.Components.TrainingCreation;

using Alerting.ML.App.Model.Enums;

using ReactiveUI;

public interface ITrainingCreationStepViewModel : IRoutableViewModel
{
    void Continue();
    TrainingCreationStep CurrentStep { get; }
}

public interface ITrainingCreationLastStepViewModel : IRoutableViewModel
{
    IGeneticOptimizer ConfiguredOptimizer { get; }
}