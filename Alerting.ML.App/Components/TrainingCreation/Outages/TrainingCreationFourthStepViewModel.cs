using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.Outages;

public class TrainingCreationFourthStepViewModel:ViewModelBase, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;

    public TrainingCreationFourthStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
    }

    public string? UrlPathSegment => "step4";
    public IScreen HostScreen { get; }
    public void Continue()
    {
        throw new System.NotImplementedException();
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step4;
}