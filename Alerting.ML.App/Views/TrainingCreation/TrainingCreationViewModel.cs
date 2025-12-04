using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Components.TrainingCreation;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Training;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

namespace Alerting.ML.App.Views.TrainingCreation;

public class TrainingCreationViewModel : RoutableViewModelBase, IScreen
{
    private readonly IBackgroundTrainingOrchestrator trainingOrchestrator;

    public TrainingCreationViewModel(IScreen hostScreen, IBackgroundTrainingOrchestrator trainingOrchestrator):base(hostScreen)
    {
        this.trainingOrchestrator = trainingOrchestrator;
        CancelCommand = ReactiveCommand.CreateFromTask(Cancel, IsOnTopOfNavigation);
        GoBackCommand = ReactiveCommand.CreateFromTask(GoBack, IsOnTopOfNavigation);

        var builderConfigured = this.WhenAnyValue(model => model.ConfiguredBuilder, (Func<TrainingBuilder?, bool>)(source => source != null));

        StartOptimizationCommand = ReactiveCommand.CreateFromTask(StartOptimization, builderConfigured.CombineLatest(IsOnTopOfNavigation, (a, b) => a && b));

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
                ContinueCommand = ReactiveCommand.CreateFromTask(model.Continue, model.CanContinue);
                Step = model.CurrentStep;
            })
            .DisposeWith(Disposables);

        Router.CurrentViewModel.OfType<ITrainingCreationLastStepViewModel>()
            .Subscribe(model => { ConfiguredBuilder = model.ConfiguredBuilder; })
            .DisposeWith(Disposables);

        Router.NavigateAndReset.Execute(
            new TrainingCreationFirstStepViewModel(this, trainingOrchestrator.DefaultBuilder));
    }

    public TrainingBuilder? ConfiguredBuilder
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

    public override string UrlPathSegment => "new-training";

    public RoutingState Router { get; } = new();

    private async Task GoBack()
    {
        if (Router.NavigationStack.Count > 1)
        {
            await Router.NavigateBack.Execute();
        }
        else
        {
            await Cancel();
        }
    }

    private async Task StartOptimization()
    {
        var geneticOptimizer = ConfiguredBuilder!.Build();

        var trainingSession = trainingOrchestrator.StartNew(geneticOptimizer);
        await HostScreen.Router.NavigateBack.Execute();
        await HostScreen.Router.Navigate.Execute(new TrainingViewModel(HostScreen, trainingSession));
    }

    private async Task Cancel()
    {
        await HostScreen.Router.NavigateBack.Execute();
    }
}