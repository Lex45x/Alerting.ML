using Alerting.ML.App.Components.TrainingCreation.FileUpload;
using Alerting.ML.App.Components.TrainingCreation.Outages;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Azure;
using Alerting.ML.Sources.Csv;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Training;

namespace Alerting.ML.App.Components.TrainingCreation.Csv;

using Alerting.ML.App.Model.Enums;

public class TrainingCreationCsvSecondStepViewModel : FileUploadViewModel, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;
    private TrainingBuilder? builderWithTimeSeriesProvider;
    public string UrlPathSegment => "csv";

    public bool IsAzureScheduledQueryRuleSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public IScreen HostScreen { get; }

    public void Continue()
    {
        if (builderWithTimeSeriesProvider == null)
        {
            throw new InvalidOperationException("Most likely CSV file is not selected.");
        }

        var updatedBuilder = this switch
        {
            { IsAzureScheduledQueryRuleSelected: true } => builderWithTimeSeriesProvider.WithAzureScheduledQueryRuleAlert(),
            _ => throw new InvalidOperationException("Csv time series provider requires a valid alert rule selection.")
        };

        HostScreen.Router.Navigate.Execute(new TrainingCreationFourthStepViewModel(HostScreen, updatedBuilder));
    }

    private async Task ConfigureBuilder(string path)
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

        var validationResult = await updatedBuilder.TimeSeriesProvider.ImportAndValidate();

        if (!validationResult.IsValid)
        {
            //todo: validation errors must be displayed to the user.
            builderWithTimeSeriesProvider = null;
        }
        else
        {
            builderWithTimeSeriesProvider = updatedBuilder;
        }
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step2;

    protected override string Title => "Upload CSV File";

    public TrainingCreationCsvSecondStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
        this.WhenAnyValue(model => model.SelectedFilePath)
            .SelectMany(s => Observable.FromAsync(() => ConfigureBuilder(s)))
            .Subscribe();
    }
}

public class TrainingCreationCsvSecondStepViewModelDesignTime : TrainingCreationCsvSecondStepViewModel
{
    public TrainingCreationCsvSecondStepViewModelDesignTime() : base(null, null)
    {
    }
}