using System;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using ReactiveUI;
using System.Collections.ObjectModel;
using Alerting.ML.App.Model.Training;
using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.App.Components.TrainingCreation.Preview;

public class TrainingCreationFifthStepViewModel : ViewModelBase, ITrainingCreationStepViewModel,
    ITrainingCreationLastStepViewModel
{
    private readonly TrainingBuilder builder;

    public TrainingCreationFifthStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
        ConfiguredOptimizer = this.builder.Build();
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
        throw new InvalidOperationException("This is the last step that can't be continued further");
    }

    public ObservableCollection<PreviewSummaryItem> PreviewItems
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step5;

    public bool IsValidationPassed => true;

    public record PreviewSummaryItem(string Name, object? Value);

    public IGeneticOptimizer ConfiguredOptimizer { get; }
}

public class TrainingCreationFifthStepViewModelDesignTime : TrainingCreationFifthStepViewModel
{
    public TrainingCreationFifthStepViewModelDesignTime() : base(null, null)
    {
        PreviewItems =
        [
            new("Data Source", "CSV"),
            new("Alert Type", "Azure Scheduled Query Rule"),
            new("Outage File", "outages.csv")
        ];
    }
}