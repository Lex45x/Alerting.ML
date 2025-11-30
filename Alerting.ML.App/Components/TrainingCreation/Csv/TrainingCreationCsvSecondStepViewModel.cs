using Alerting.ML.App.Components.TrainingCreation.FileUpload;
using Alerting.ML.App.Components.TrainingCreation.Outages;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Azure;
using Alerting.ML.Sources.Csv;
using ReactiveUI;
using System;
using Alerting.ML.App.Model.Training;

namespace Alerting.ML.App.Components.TrainingCreation.Csv;

using Alerting.ML.App.Model.Enums;

public class TrainingCreationCsvSecondStepViewModel : FileUploadViewModel, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;
    public string? UrlPathSegment => "csv";

    public bool IsAzureScheduledQueryRuleSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public IScreen HostScreen { get; }

    public void Continue()
    {
        var updatedBuilder = this.builder.WithCsvTimeSeriesProvider(SelectedFilePath);
        updatedBuilder = this switch
        {
            { IsAzureScheduledQueryRuleSelected: true } => updatedBuilder.WithAzureScheduledQueryRuleAlert(),
            _ => throw new InvalidOperationException("Csv time series provider requires a valid alert rule selection.")
        };

        HostScreen.Router.Navigate.Execute(new TrainingCreationFourthStepViewModel(HostScreen, updatedBuilder));
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step2;

    protected override string Title => "Upload CSV File";

    public TrainingCreationCsvSecondStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
    }
}

public class TrainingCreationCsvSecondStepViewModelDesignTime : TrainingCreationCsvSecondStepViewModel
{
    public TrainingCreationCsvSecondStepViewModelDesignTime() : base(null, null)
    {
    }
}