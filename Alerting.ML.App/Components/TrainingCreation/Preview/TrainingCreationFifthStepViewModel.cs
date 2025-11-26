using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.Preview;

public class TrainingCreationFifthStepViewModel : ViewModelBase, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;

    public TrainingCreationFifthStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
    }

    public string? UrlPathSegment => "preview";
    public IScreen HostScreen { get; }

    public void Continue()
    {
        var geneticOptimizer = builder.Build();
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step5;
}