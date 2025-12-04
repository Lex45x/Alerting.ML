using Alerting.ML.App.DesignTimeExtensions;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Alerting.ML.App.Components.TrainingCreation.Preview;

public class TrainingCreationFifthStepViewModel : RoutableViewModelBase, ITrainingCreationStepViewModel,
    ITrainingCreationLastStepViewModel
{
    public TrainingCreationFifthStepViewModel(IScreen hostScreen, TrainingBuilder builder):base(hostScreen)
    {
        ConfiguredBuilder = builder;
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

    public TrainingBuilder ConfiguredBuilder { get; }

    public override string UrlPathSegment => "preview";

    public Task Continue()
    {
        throw new InvalidOperationException("This is the last step that can't be continued further");
    }

    public IObservable<bool> CanContinue { get; } = new Subject<bool>();

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step5;
}

public record PreviewSummaryItem(string Name, object? Value);

public class TrainingCreationFifthStepViewModelDesignTime : TrainingCreationFifthStepViewModel
{
    public TrainingCreationFifthStepViewModelDesignTime() : base(DesignTime.MockScreen, TrainingBuilder.Create())
    {
        PreviewItems =
        [
            new PreviewSummaryItem("Data Source", "CSV"),
            new PreviewSummaryItem("Alert Type", "Azure Scheduled Query Rule"),
            new PreviewSummaryItem("Outage File", "outages.csv")
        ];
    }
}