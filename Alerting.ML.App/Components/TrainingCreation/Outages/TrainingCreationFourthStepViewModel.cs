using Alerting.ML.App.Components.TrainingCreation.FileUpload;
using Alerting.ML.App.Components.TrainingCreation.Preview;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Csv;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.Outages;

public class TrainingCreationFourthStepViewModel : FileUploadViewModel, ITrainingCreationStepViewModel
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
        var updatedBuilder = builder.WithCsvOutagesProvider(SelectedFilePath);
        HostScreen.Router.Navigate.Execute(new TrainingCreationFifthStepViewModel(HostScreen, updatedBuilder));
    }


    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step4;
    protected override string Title => "Upload Outages CSV";
}

public class TrainingCreationFourthStepViewModelDesignTime : TrainingCreationFourthStepViewModel
{
    public TrainingCreationFourthStepViewModelDesignTime() : base(null, null)
    {
    }
}