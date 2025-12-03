using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Alerting.ML.App.Components.TrainingCreation;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Training;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

namespace Alerting.ML.App.Views.TrainingCreation;

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
            })
            .DisposeWith(Disposables);

        Router.CurrentViewModel.OfType<ITrainingCreationStepViewModel>()
            .Subscribe(model =>
            {
                ContinueCommand = ReactiveCommand.Create(model.Continue);
                Step = model.CurrentStep;
            })
            .DisposeWith(Disposables);

        Router.CurrentViewModel.OfType<ITrainingCreationLastStepViewModel>()
            .Subscribe(model => { ConfiguredOptimizer = model.ConfiguredOptimizer; })
            .DisposeWith(Disposables);

        Router.NavigateAndReset.Execute(
            new TrainingCreationFirstStepViewModel(this, trainingOrchestrator.DefaultBuilder));
    }

    public IGeneticOptimizer? ConfiguredOptimizer
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    public ReactiveCommand<Unit, Unit> StartOptimizationCommand { get; }

    public ReactiveCommand<Unit, Unit>? ContinueCommand
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TrainingCreationStep Step
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsLastStep => Step == TrainingCreationStep.Step5;
    public bool IsNotLastStep => !IsLastStep;

    public string? UrlPathSegment => "new-training";

    public IScreen HostScreen { get; }

    public RoutingState Router { get; } = new();

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
        var trainingSession = trainingOrchestrator.StartNew(ConfiguredOptimizer ??
                                                            throw new InvalidOperationException(
                                                                "Genetic optimizer is not configured yet so creation of training is not possible."));
        HostScreen.Router.NavigateBack.Execute();
        HostScreen.Router.Navigate.Execute(new TrainingViewModel(HostScreen, trainingSession));
    }

    private void Cancel()
    {
        HostScreen.Router.NavigateBack.Execute();
    }
}