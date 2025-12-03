using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Components.TrainingCreation.FileUpload;
using Alerting.ML.App.Components.TrainingCreation.Preview;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Csv;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.Outages;

public class TrainingCreationFourthStepViewModel : FileUploadViewModel, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;
    private TrainingBuilder? builderWithOutagesProvider;

    public TrainingCreationFourthStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
        this.WhenAnyValue(model => model.SelectedFilePath)
            .SelectMany(s => Observable.FromAsync(() => ConfigureBuilder(s)))
            .Subscribe()
            .DisposeWith(Disposables);
    }

    protected override string Title => "Upload Outages CSV";


    public string? UrlPathSegment => "step4";
    public IScreen HostScreen { get; }

    public void Continue()
    {
        HostScreen.Router.Navigate.Execute(
            new TrainingCreationFifthStepViewModel(HostScreen,
                builderWithOutagesProvider ??
                throw new InvalidOperationException("Most likely CSV file is not selected.")));
    }


    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step4;

    private async Task ConfigureBuilder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            builderWithOutagesProvider = null;
            return;
        }

        var updatedBuilder = builder.WithCsvOutagesProvider(path);

        if (updatedBuilder.KnownOutagesProvider == null)
        {
            throw new InvalidOperationException(
                "Something very wrong happened. KnownOutagesProvider must be non-null here.");
        }

        var validationResult = await updatedBuilder.KnownOutagesProvider.ImportAndValidate();

        if (!validationResult.IsValid)
        {
            //todo: validation errors must be displayed to the user.
            builderWithOutagesProvider = null;
        }
        else
        {
            builderWithOutagesProvider = updatedBuilder;
        }
    }
}

public class TrainingCreationFourthStepViewModelDesignTime : TrainingCreationFourthStepViewModel
{
    public TrainingCreationFourthStepViewModelDesignTime() : base(null!, null!)
    {
    }
}