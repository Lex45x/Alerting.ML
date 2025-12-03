using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using ReactiveUI;

namespace Alerting.ML.App.Components.TrainingCreation;

using Alerting.ML.App.Components.TrainingCreation.Csv;
using System;

public class TrainingCreationFirstStepViewModel(IScreen hostScreen, TrainingBuilder builder)
    : ViewModelBase, ITrainingCreationStepViewModel
{
    public void Continue()
    {
        HostScreen.Router.Navigate.Execute(
            this switch
            {
                { IsCsvSelected: true } => new TrainingCreationCsvSecondStepViewModel(HostScreen, builder),
                _ => throw new NotImplementedException("Cloud providers are currently noot supported.")
            });
    }

    public TrainingCreationStep CurrentStep => TrainingCreationStep.Step1;

    public string? UrlPathSegment => "step1";

    public IScreen HostScreen { get; } = hostScreen;

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
}

public class TrainingCreationFirstStepViewModelDesignTime : TrainingCreationFirstStepViewModel
{
    public TrainingCreationFirstStepViewModelDesignTime()
        : base(null!, null!)
    {
    }
}