using System.Reactive;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Overview;
using ExCSS;
using ReactiveUI;

namespace Alerting.ML.App.Views.TrainingCreation;

public abstract class TrainingCreationViewModelBase : ViewModelBase, IRoutableViewModel
{
    protected TrainingCreationViewModelBase(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        CancelCommand = ReactiveCommand.Create(Cancel);
        GoBackCommand = ReactiveCommand.Create(GoBack);
        ContinueCommand = ReactiveCommand.Create(Continue);
    }

    private void GoBack()
    {
        HostScreen.Router.NavigateBack.Execute();
    }

    protected abstract void Continue();

    private void Cancel()
    {
        HostScreen.Router.NavigateAndReset.Execute(new OverviewViewModel(HostScreen));
    }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    public ReactiveCommand<Unit, Unit> ContinueCommand { get; }
    public abstract string? UrlPathSegment { get; }
    public abstract TrainingCreationStep Step { get; }
    public IScreen HostScreen { get; }
}