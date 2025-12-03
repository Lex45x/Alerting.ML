using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Alerting.ML.App.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.FileUpload;

public abstract class FileUploadViewModel : ViewModelBase
{
    public FileUploadViewModel()
    {
        FileDroppedCommand = ReactiveCommand.Create<IEnumerable<IStorageItem>>(FileDropped);
        PickFileCommand = ReactiveCommand.CreateFromTask<Visual>(PickFile);
        this.WhenAnyValue(model => model.SelectedFilePath)
            .Subscribe(value =>
            {
                this.RaisePropertyChanged(nameof(IsFileSelected));
                this.RaisePropertyChanged(nameof(UploadTitle));
                this.RaisePropertyChanged(nameof(UploadSubTitle));
                this.RaisePropertyChanged(nameof(SelectedFileName));
            })
            .DisposeWith(Disposables);
    }

    public string? SelectedFilePath
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SelectedFileName => Path.GetFileName(SelectedFilePath);

    public bool IsFileSelected => !string.IsNullOrWhiteSpace(SelectedFilePath);

    public string UploadTitle => IsFileSelected ? SelectedFileName! : Title;
    public string UploadSubTitle => IsFileSelected ? "Click to change file" : "Drag and drop or click to browse";
    public ReactiveCommand<IEnumerable<IStorageItem>, Unit> FileDroppedCommand { get; }
    public ReactiveCommand<Visual, Unit> PickFileCommand { get; }

    protected abstract string Title { get; }

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

        SelectedFilePath = files.First().TryGetLocalPath() ??
                           throw new InvalidOperationException("Unable to get a full path to CSV file.");
    }

    private void FileDropped(IEnumerable<IStorageItem> arg)
    {
        SelectedFilePath = arg.First().Path.ToString();
    }
}

public class FileUploadViewModelDesignTime : FileUploadViewModel
{
    protected override string Title => "Design-time-title";
}