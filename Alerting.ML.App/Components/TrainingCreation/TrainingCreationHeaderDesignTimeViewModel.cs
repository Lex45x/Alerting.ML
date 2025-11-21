using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Views.TrainingCreation;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation;

public class TrainingCreationHeaderDesignTimeViewModel : TrainingCreationViewModelBase
{
    public TrainingCreationHeaderDesignTimeViewModel() : base(null)
    {
    }

    protected override void Continue()
    {
        throw new System.NotImplementedException();
    }

    public override string? UrlPathSegment => "design-time-only";
    public override TrainingCreationStep Step => TrainingCreationStep.Step3;
}