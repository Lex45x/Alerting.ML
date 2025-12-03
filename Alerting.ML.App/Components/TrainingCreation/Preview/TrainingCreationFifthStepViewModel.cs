using System;
using System.Collections.ObjectModel;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

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

    public ObservableCollection<PreviewSummaryItem> PreviewItems
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsValidationPassed => true;

    public IGeneticOptimizer ConfiguredOptimizer { get; }

    public string? UrlPathSegment => "preview";
    public IScreen HostScreen { get; }

    public void Continue()
    {
        throw new InvalidOperationException("This is the last step that can't be continued further");
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step5;
}

public record PreviewSummaryItem(string Name, object? Value);

public class TrainingCreationFifthStepViewModelDesignTime : TrainingCreationFifthStepViewModel
{
    public TrainingCreationFifthStepViewModelDesignTime() : base(null!, null!)
    {
        PreviewItems =
        [
            new PreviewSummaryItem("Data Source", "CSV"),
            new PreviewSummaryItem("Alert Type", "Azure Scheduled Query Rule"),
            new PreviewSummaryItem("Outage File", "outages.csv")
        ];
    }
}