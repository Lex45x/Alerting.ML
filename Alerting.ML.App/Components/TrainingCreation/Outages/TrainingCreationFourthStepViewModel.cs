using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Components.TrainingCreation.FileUpload;
using Alerting.ML.App.Components.TrainingCreation.Preview;
using Alerting.ML.App.DesignTimeExtensions;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.Engine;
using Alerting.ML.Sources.Csv;
using FluentValidation.Results;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation.Outages;

public class TrainingCreationFourthStepViewModel : FileUploadViewModel, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;
    private TrainingBuilder? builderWithOutagesProvider;

    public TrainingCreationFourthStepViewModel(IScreen hostScreen, TrainingBuilder builder) : base(hostScreen)
    {
        this.builder = builder;

        var whenValid = this.WhenAnyValue(model => model.ImportResult,
            (Func<ValidationResult?, bool>)(validationResult => validationResult is { IsValid: true }));

        CanContinue = whenValid
            .CombineLatest(IsOnTopOfNavigation, (first, second) => first && second)
            .DistinctUntilChanged();

        this.WhenAnyValue(model => model.SelectedFilePath)
            .SelectMany(s => Observable.FromAsync(() => ConfigureBuilder(s)))
            .Subscribe()
            .DisposeWith(Disposables);
    }

    protected override string Title => "Upload Outages CSV";

    public ValidationResult? ImportResult
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }


    public override string UrlPathSegment => "step4";

    public async Task Continue()
    {
        await HostScreen.Router.Navigate.Execute(
            new TrainingCreationFifthStepViewModel(HostScreen,
                builderWithOutagesProvider ??
                throw new InvalidOperationException("Most likely CSV file is not selected.")));
    }

    public IObservable<bool> CanContinue { get; }

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

        ImportResult = await updatedBuilder.KnownOutagesProvider.ImportAndValidate();

        if (!ImportResult.IsValid)
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
    public TrainingCreationFourthStepViewModelDesignTime() : base(DesignTime.MockScreen, TrainingBuilder.Create())
    {
    }
}