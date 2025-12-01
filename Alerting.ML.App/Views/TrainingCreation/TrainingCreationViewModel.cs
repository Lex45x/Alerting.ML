using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Reactive;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.Views.Training;
using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.App.Views.TrainingCreation;

using Alerting.ML.App.Components.TrainingCreation;
using System;
using System.Reactive.Linq;

public class TrainingCreationViewModel : ViewModelBase, IRoutableViewModel, IScreen
{
    private readonly IBackgroundTrainingOrchestrator trainingOrchestrator;

    public TrainingCreationViewModel(IScreen hostScreen, IBackgroundTrainingOrchestrator trainingOrchestrator)
    {
        this.trainingOrchestrator = trainingOrchestrator;
        HostScreen = hostScreen;
        CancelCommand = ReactiveCommand.Create(Cancel);
        GoBackCommand = ReactiveCommand.Create(GoBack);
        StartOptimizationCommand = ReactiveCommand.Create(StartOptimization);

        this.WhenAnyValue(model => model.Step)
            .Subscribe(step =>
            {
                this.RaisePropertyChanged(nameof(IsLastStep));
                this.RaisePropertyChanged(nameof(IsNotLastStep));
            });

        Router.CurrentViewModel.OfType<ITrainingCreationStepViewModel>()
            .Subscribe(model =>
            {
                ContinueCommand = ReactiveCommand.Create(model.Continue);
                Step = model.CurrentStep;
            });

        Router.CurrentViewModel.OfType<ITrainingCreationLastStepViewModel>()
            .Subscribe(model =>
            {
                ConfiguredOptimizer = model.ConfiguredOptimizer;
            });

        Router.NavigateAndReset.Execute(new TrainingCreationFirstStepViewModel(this, TrainingBuilder.Create()));
    }

    private void GoBack()
    {
        if (Router.NavigationStack.Count > 1)
        {
            Router.NavigateBack.Execute();
        }
        else
        {
            Cancel();
        }
    }

    private void StartOptimization()
    {
        var trainingSession = trainingOrchestrator.StartNew(ConfiguredOptimizer);
        HostScreen.Router.NavigateBack.Execute();
        HostScreen.Router.Navigate.Execute(new TrainingViewModel(HostScreen, trainingSession));
    }

    public IGeneticOptimizer? ConfiguredOptimizer
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private void Cancel()
    {
        HostScreen.Router.NavigateBack.Execute();
    }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    public ReactiveCommand<Unit, Unit> StartOptimizationCommand { get; }

    public ReactiveCommand<Unit, Unit>? ContinueCommand
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? UrlPathSegment => "new-training";

    public TrainingCreationStep Step
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsLastStep => Step == TrainingCreationStep.Step5;
    public bool IsNotLastStep => !IsLastStep;

    public IScreen HostScreen { get; }

    public RoutingState Router { get; } = new();
}