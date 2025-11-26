using Alerting.ML.App.Components.TrainingCreation.Outages;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Azure;
using Alerting.ML.Sources.Csv;
using Avalonia;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Components.TrainingCreation.FileUpload;

namespace Alerting.ML.App.Components.TrainingCreation.Csv;

using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Avalonia.Controls;

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