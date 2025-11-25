using System.Reactive;
using System.Reactive.Linq;

using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Overview;
using Alerting.ML.Engine;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Alerting.ML.App.Views.TrainingCreation;

using System;
using System.Reactive.Linq;

using Alerting.ML.App.Components.TrainingCreation;

public class TrainingCreationViewModel : ViewModelBase, IRoutableViewModel, IScreen
{
    public TrainingCreationViewModel(IScreen hostScreen, ILoggerFactory loggerFactory)
    {
        HostScreen = hostScreen;
        CancelCommand = ReactiveCommand.Create(Cancel);
        GoBackCommand = ReactiveCommand.Create(GoBack);
        Router.CurrentViewModel.OfType<ITrainingCreationStepViewModel>()
            .Subscribe(model =>
                {
                    ContinueCommand = ReactiveCommand.Create(model.Continue);
                    Step = model.CurrentStep;
                });
        Router.NavigateAndReset.Execute(new TrainingCreationFirstStepViewModel(this, new TrainingBuilder(loggerFactory)));
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
    
    private void Cancel()
    {
        HostScreen.Router.NavigateBack.Execute();
    }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public ReactiveCommand<Unit, Unit> ContinueCommand
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

    public IScreen HostScreen { get; }

    public RoutingState Router { get; } = new();
}