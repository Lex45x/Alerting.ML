using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Components.TrainingCreation.Csv;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation;

public class TrainingCreationFirstStepViewModel : ViewModelBase, ITrainingCreationStepViewModel
{
    private readonly TrainingBuilder builder;

    public TrainingCreationFirstStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    {
        this.builder = builder;
        HostScreen = hostScreen;
        CanContinue = this.WhenAnyValue(model => model.IsCsvSelected);
    }

    public bool IsAzureSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsAwsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsGcpSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsCsvSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public async Task Continue()
    {
        await HostScreen.Router.Navigate.Execute(
            this switch
            {
                { IsCsvSelected: true } => new TrainingCreationCsvSecondStepViewModel(HostScreen, builder),
                _ => throw new NotImplementedException("Cloud providers are currently noot supported.")
            });
    }

    public IObservable<bool> CanContinue { get; }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step1;

    public string? UrlPathSegment => "step1";

    public IScreen HostScreen { get; }
}

public class TrainingCreationFirstStepViewModelDesignTime : TrainingCreationFirstStepViewModel
{
    public TrainingCreationFirstStepViewModelDesignTime()
        : base(null!, null!)
    {
    }
}