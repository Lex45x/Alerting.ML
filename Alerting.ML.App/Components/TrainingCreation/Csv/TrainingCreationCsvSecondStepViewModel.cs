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

namespace Alerting.ML.App.Components.TrainingCreation.Csv;

using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Avalonia.Controls;

public class TrainingCreationCsvSecondStepViewModel : ViewModelBase, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;
    public string? UrlPathSegment => "csv";

    public string SelectedFilePath
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string SelectedFileName => System.IO.Path.GetFileName(SelectedFilePath);

    public bool IsAzureScheduledQueryRuleSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsFileSelected => !string.IsNullOrWhiteSpace(SelectedFilePath);

    public string UploadTitle => IsFileSelected ? SelectedFileName : "Upload CSV File";
    public string UploadSubTitle => IsFileSelected ? "Click to change file" : "Drag and drop or click to browse";

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

    public ReactiveCommand<IEnumerable<IStorageItem>, Unit> FileDroppedCommand { get; }
    public ReactiveCommand<Visual, Unit> PickFileCommand { get; }

    public TrainingCreationCsvSecondStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
        FileDroppedCommand = ReactiveCommand.Create<IEnumerable<IStorageItem>>(FileDropped);
        PickFileCommand = ReactiveCommand.CreateFromTask<Visual>(PickFile);
        this.WhenAnyValue(model => model.SelectedFilePath)
            .Subscribe(value =>
            {
                this.RaisePropertyChanged(nameof(IsFileSelected));
                this.RaisePropertyChanged(nameof(UploadTitle));
                this.RaisePropertyChanged(nameof(UploadSubTitle));
                this.RaisePropertyChanged(nameof(SelectedFileName));
            });
    }

    private async Task PickFile(Visual control)
    {
        var topLevel = TopLevel.GetTopLevel(control);

        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select a CSV file",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("CSV Files")
                    {
                        Patterns = new[] { "*.csv" }
                    }
                }
            });

        SelectedFilePath = files.First().Path.ToString();
    }

    private void FileDropped(IEnumerable<IStorageItem> arg)
    {
        SelectedFilePath = arg.First().Path.ToString();
    }
}

public class TrainingCreationCsvSecondStepViewModelDesignTime : TrainingCreationCsvSecondStepViewModel
{
    public TrainingCreationCsvSecondStepViewModelDesignTime() : base(null, null)
    {
    }
}