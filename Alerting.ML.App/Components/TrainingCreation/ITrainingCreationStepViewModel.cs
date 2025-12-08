using System;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation;

public interface ITrainingCreationStepViewModel : IRoutableViewModel
{
    TrainingCreationStep CurrentStep { get; }
    IObservable<bool> CanContinue { get; }
    Task Continue();
}

public interface ITrainingCreationLastStepViewModel : IRoutableViewModel
{
    TrainingBuilder ConfiguredBuilder { get; }
}