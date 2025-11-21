using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using ReactiveUI;

namespace Alerting.ML.App.Views.TrainingCreation;

public class TrainingCreationFirstStepViewModel : TrainingCreationViewModelBase
{
    public TrainingCreationFirstStepViewModel(IScreen hostScreen) : base(hostScreen)
    {
    }

    protected override void Continue()
    {
        throw new System.NotImplementedException();
    }

    public override string? UrlPathSegment => "overview/new/step1";
    public override TrainingCreationStep Step => TrainingCreationStep.Step1;
}

public class TrainingCreationFirstStepViewModelDesignTime : TrainingCreationFirstStepViewModel
{
    public TrainingCreationFirstStepViewModelDesignTime() : base(null)
    {
    }
}