using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Components.TrainingCreation.FileUpload;
using Alerting.ML.App.Components.TrainingCreation.Outages;
using Alerting.ML.App.DesignTimeExtensions;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Azure;
using Alerting.ML.Sources.Csv;
using Avalonia.Controls;
using Avalonia.Platform;
using FluentValidation.Results;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.Csv;

public class TrainingCreationCsvSecondStepViewModel : FileUploadViewModel, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;
    private TrainingBuilder? builderWithTimeSeriesProvider;

    public TrainingCreationCsvSecondStepViewModel(IScreen hostScreen, TrainingBuilder builder) : base(hostScreen)
    {
        this.builder = builder;
        var whenValid = this.WhenAnyValue(model => model.ImportResult,
            model => model.IsAzureScheduledQueryRuleSelected,
            (Func<ValidationResult?, bool, bool>)((validationResult, selected) =>
                validationResult is { IsValid: true } && selected));

        CanContinue = whenValid
            .CombineLatest(IsOnTopOfNavigation, (first, second) => first && second)
            .DistinctUntilChanged();

        this.WhenAnyValue(model => model.SelectedFilePath)
            .SelectMany(s => Observable.FromAsync(() => ConfigureBuilder(s)))
            .Subscribe()
            .DisposeWith(Disposables);
    }

    public bool IsAzureScheduledQueryRuleSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ValidationResult? ImportResult
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    protected override string Title => "Upload CSV File";
    public override string UrlPathSegment => "csv";

    public async Task Continue()
    {
        if (builderWithTimeSeriesProvider == null)
        {
            throw new InvalidOperationException("Most likely CSV file is not selected.");
        }

        var updatedBuilder = this switch
        {
            { IsAzureScheduledQueryRuleSelected: true } => builderWithTimeSeriesProvider
                .WithAzureScheduledQueryRuleAlert(),
            _ => throw new InvalidOperationException("Csv time series provider requires a valid alert rule selection.")
        };

        await HostScreen.Router.Navigate.Execute(new TrainingCreationFourthStepViewModel(HostScreen, updatedBuilder));
    }

    public IObservable<bool> CanContinue { get; }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step2;

    private async Task ConfigureBuilder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            builderWithTimeSeriesProvider = null;
            return;
        }

        var updatedBuilder = builder.WithCsvTimeSeriesProvider(path);

        if (updatedBuilder.TimeSeriesProvider == null)
        {
            throw new InvalidOperationException(
                "Something very wrong happened. KnownOutagesProvider must be non-null here.");
        }

        ImportResult = await updatedBuilder.TimeSeriesProvider.ImportAndValidate();

        if (!ImportResult.IsValid)
        {
            //todo: validation errors must be displayed to the user.
            builderWithTimeSeriesProvider = null;
        }
        else
        {
            builderWithTimeSeriesProvider = updatedBuilder;
        }
    }
}

public class TrainingCreationCsvSecondStepViewModelDesignTime : TrainingCreationCsvSecondStepViewModel
{
    public TrainingCreationCsvSecondStepViewModelDesignTime() : base(DesignTime.MockScreen, TrainingBuilder.Create())
    {
    }
}