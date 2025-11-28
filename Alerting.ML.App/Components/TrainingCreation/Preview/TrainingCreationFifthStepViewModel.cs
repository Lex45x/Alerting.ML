using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Alerting.ML.App.Components.TrainingCreation.Preview;

public class TrainingCreationFifthStepViewModel : ViewModelBase, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;

    public TrainingCreationFifthStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
        PreviewItems =
        [
            new PreviewSummaryItem("Data Source", builder.TimeSeriesProvider),
            new PreviewSummaryItem("Alert Type", builder.Alert),
            new PreviewSummaryItem("Outages File", builder.KnownOutagesProvider)
        ];
    }

    public string? UrlPathSegment => "preview";
    public IScreen HostScreen { get; }

    public void Continue()
    {
        var geneticOptimizer = builder.Build();
    }

    public ObservableCollection<PreviewSummaryItem> PreviewItems
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step5;

    public bool IsValidationPassed => true;

    public record PreviewSummaryItem(string Name, object? Value);
}

public class TrainingCreationFifthStepViewModelDesignTime : TrainingCreationFifthStepViewModel
{
    public TrainingCreationFifthStepViewModelDesignTime() : base(null, TrainingBuilder.Create())
    {
        PreviewItems =
        [
            new("Data Source", "CSV"),
            new("Alert Type", "Azure Scheduled Query Rule"),
            new("Outage File", "outages.csv")
        ];
    }


}