using System;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation;

public interface ITrainingCreationStepViewModel : IRoutableViewModel
{
    TrainingCreationStep CurrentStep { get; }
    Task Continue();
    IObservable<bool> CanContinue { get; }
}

public interface ITrainingCreationLastStepViewModel : IRoutableViewModel
{
    TrainingBuilder ConfiguredBuilder { get; }
}